using FluentValidation;
using Microsoft.Data.SqlClient;
using Serilog;
using SqlSugar;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;
using WF.MES.Infrastructure.Validation;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>
/// 条码批量生成实现。
/// 涉及 Barcode_SerialCounter、Barcode_GenerateRecord、Barcode_Record；Preview 无锁只读，Generate 单事务 UPDLOCK 占号后批量 INSERT。
/// 正式生成前校验 QA 已确认；条码唯一索引冲突时回滚并提示重试。
/// </summary>
public class BarcodeGenerateService : IBarcodeGenerateService
{
    private readonly ISqlSugarClient _db;
    private readonly IMaterialBarcodeRuleService _ruleService;
    private readonly IBarcodeBuilder _barcodeBuilder;
    private readonly ISerialNumberFormatter _formatter;
    private readonly IValidator<BarcodeGenerateRequestDto> _validator;

    public BarcodeGenerateService(
        ISqlSugarClient db,
        IMaterialBarcodeRuleService ruleService,
        IBarcodeBuilder barcodeBuilder,
        ISerialNumberFormatter formatter,
        IValidator<BarcodeGenerateRequestDto> validator)
    {
        _db = db;
        _ruleService = ruleService;
        _barcodeBuilder = barcodeBuilder;
        _formatter = formatter;
        _validator = validator;
    }

    public async Task<BarcodeGeneratePreviewDto> PreviewAsync(
        BarcodeGenerateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateRequestAsync(request, cancellationToken);

        var (rule, segments, context, serialSegment) = await LoadRuleContextAsync(request);

        var resetKey = _barcodeBuilder.BuildResetKey(segments, context);
        var start = await GetNextSerialStartAsync(rule.RuleId, resetKey);
        var end = start + request.Quantity - 1;
        ValidateSerialRange(serialSegment, end);

        var first = _barcodeBuilder.Build(segments, context, start).Barcode;
        var last = _barcodeBuilder.Build(segments, context, end).Barcode;

        var startConfig = RuleSegmentConfigMapper.FromEntity(serialSegment);

        return new BarcodeGeneratePreviewDto
        {
            ResetKey = resetKey,
            SerialRadix = startConfig.SerialRadix,
            SerialDigits = startConfig.SerialDigits,
            NextSerialStart = start,
            NextSerialEnd = end,
            FirstSerialFormatted = _formatter.Format(start, startConfig.SerialRadix, startConfig.SerialDigits),
            LastSerialFormatted = _formatter.Format(end, startConfig.SerialRadix, startConfig.SerialDigits),
            FirstBarcodeSample = first,
            LastBarcodeSample = last
        };
    }

    public async Task<BarcodeGenerateResultDto> GenerateAsync(
        BarcodeGenerateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateRequestAsync(request, cancellationToken);

        var (rule, segments, context, serialSegment) = await LoadRuleContextAsync(request);
        await _ruleService.EnsureRuleApprovedForPrintAsync(rule.RuleId, cancellationToken);

        var resetKey = _barcodeBuilder.BuildResetKey(segments, context);

        // 单事务：UPDLOCK 占号 → 写生成单/明细 → 更新计数器；任一步失败整体回滚
        await _db.Ado.BeginTranAsync();
        try
        {
            var start = await LockAndGetNextSerialStartAsync(rule.RuleId, resetKey);
            var end = start + request.Quantity - 1;
            ValidateSerialRange(serialSegment, end);

            var barcodes = BuildBarcodeRecords(rule.RuleId, resetKey, segments, context, start, end);

            var generateNo = $"BG{DateTime.Now:yyyyMMddHHmmss}";
            var generateRecordId = await _db.Insertable(new BarcodeGenerateRecord
            {
                GenerateNo = generateNo,
                RuleId = rule.RuleId,
                MaterialNo = rule.MaterialNo,
                ResetKey = resetKey,
                PrintDate = request.PrintDate.Date,
                Quantity = request.Quantity,
                SerialStart = start,
                SerialEnd = end,
                CreatedBy = request.CreatedBy
            }).ExecuteReturnIdentityAsync();

            var createdAt = DateTime.Now;
            foreach (var record in barcodes)
            {
                record.GenerateRecordId = generateRecordId;
                record.CreatedAt = createdAt;
            }

            try
            {
                await BulkInsertBarcodesAsync(barcodes, cancellationToken);
            }
            catch (Exception ex) when (IsBarcodeUniqueConstraintViolation(ex))
            {
                Log.Error(ex, "条码重复，请重试或联系管理员");
                throw new InvalidOperationException("条码重复，请重试或联系管理员。", ex);
            }

            await UpsertCounterAsync(rule.RuleId, resetKey, end);

            await _db.Ado.CommitTranAsync();
            Log.Information("料号 {MaterialNo} 生成条码 {Quantity} 条，生成单号 {GenerateNo}", rule.MaterialNo, request.Quantity, generateNo);

            return new BarcodeGenerateResultDto
            {
                GenerateRecordId = generateRecordId,
                GenerateNo = generateNo,
                Records = MapGeneratedRecords(barcodes)
            };
        }
        catch (InvalidOperationException)
        {
            await _db.Ado.RollbackTranAsync();
            throw;
        }
        catch (Exception ex) when (IsBarcodeUniqueConstraintViolation(ex))
        {
            await _db.Ado.RollbackTranAsync();
            Log.Error(ex, "条码重复，请重试或联系管理员");
            throw new InvalidOperationException("条码重复，请重试或联系管理员。", ex);
        }
        catch
        {
            await _db.Ado.RollbackTranAsync();
            throw;
        }
    }

    private List<BarcodeRecord> BuildBarcodeRecords(int ruleId,
        string resetKey,
        List<BarcodeRuleSegment> segments,
        BarcodeBuildContext context,
        long start,
        long end)
    {
        var count = (int)(end - start + 1);
        var barcodes = new List<BarcodeRecord>(count);

        for (var serial = start; serial <= end; serial++)
        {
            var built = _barcodeBuilder.Build(segments, context, serial);
            barcodes.Add(new BarcodeRecord
            {
                RuleId = ruleId,
                Barcode = built.Barcode,
                ResetKey = resetKey,
                SerialValue = serial,
                Status = 1
            });
        }

        return barcodes;
    }

    private async Task BulkInsertBarcodesAsync(List<BarcodeRecord> barcodes, CancellationToken cancellationToken)
    {
        if (barcodes.Count == 0)
        {
            return;
        }

        // 分批多行 INSERT（每批控制在 SQL Server 参数上限内），与 BeginTran 共用同一事务
        var batchSize = BarcodeGenerateLimits.BulkInsertBatchSize;
        for (var offset = 0; offset < barcodes.Count; offset += batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = barcodes.Skip(offset).Take(batchSize).ToList();
            await _db.Insertable(batch).ExecuteCommandAsync();
        }
    }

    private static List<BarcodeRecordDto> MapGeneratedRecords(IReadOnlyList<BarcodeRecord> barcodes)
    {
        return barcodes
            .Select(record => new BarcodeRecordDto
            {
                Barcode = record.Barcode,
                SerialValue = record.SerialValue
            })
            .ToList();
    }

    private static bool IsBarcodeUniqueConstraintViolation(Exception ex)
    {
        for (var current = ex; current != null; current = current.InnerException)
        {
            if (current is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>加载规则段、构建打印上下文；校验段配置合法且含流水号段。</summary>
    private async Task<(BarcodeMaterialRule rule, List<BarcodeRuleSegment> segments, BarcodeBuildContext context, BarcodeRuleSegment serialSegment)>
        LoadRuleContextAsync(BarcodeGenerateRequestDto request)
    {
        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(request.RuleId)
            ?? throw new InvalidOperationException("料号条码规则不存在");

        var segments = await _db.Queryable<BarcodeRuleSegment>()
            .Where(s => s.RuleId == rule.RuleId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        _barcodeBuilder.ValidateSegments(segments);

        var serialSegment = segments.First(s => s.SegmentType == BarcodeSegmentTypes.Serial);
        var context = new BarcodeBuildContext
        {
            PrintDate = request.PrintDate.Date
        };

        return (rule, segments, context, serialSegment);
    }

    /// <summary>Preview 用：无锁读取计数器，推算下一流水号起点（并发下可能与 Generate 结果不一致）。</summary>
    private async Task<long> GetNextSerialStartAsync(int ruleId, string resetKey)
    {
        var counters = await _db.Queryable<BarcodeSerialCounter>()
            .Where(c => c.RuleId == ruleId && c.ResetKey == resetKey)
            .ToListAsync();
        var counter = counters.FirstOrDefault();

        if (counter == null)
        {
            return BarcodeSegmentTypes.SerialStartValue;
        }

        return counter.CurrentValue + 1;
    }

    /// <summary>Generate 用：在已开启的事务内 UPDLOCK 锁定计数器行后取下一流水号。</summary>
    private async Task<long> LockAndGetNextSerialStartAsync(int ruleId, string resetKey)
    {
        // UPDLOCK + ROWLOCK：同工位/多工位并发生成时串行占号，避免流水号重复
        var sql = """
                  SELECT Id, Rule_Id, Reset_Key, Current_Value, UpdatedAt
                  FROM dbo.Barcode_SerialCounter WITH (UPDLOCK, ROWLOCK)
                  WHERE Rule_Id = @RuleId AND Reset_Key = @ResetKey
                  """;
        var counter = await _db.Ado.SqlQueryAsync<BarcodeSerialCounter>(sql, new { RuleId = ruleId, ResetKey = resetKey });
        var row = counter.FirstOrDefault();

        if (row == null)
        {
            return BarcodeSegmentTypes.SerialStartValue;
        }

        return row.CurrentValue + 1;
    }

    /// <summary>生成成功后 upsert 计数器，将 CurrentValue 更新为本批最大流水号。</summary>
    private async Task UpsertCounterAsync(int ruleId, string resetKey, long endValue)
    {
        var existingList = await _db.Queryable<BarcodeSerialCounter>()
            .Where(c => c.RuleId == ruleId && c.ResetKey == resetKey)
            .ToListAsync();
        var existing = existingList.FirstOrDefault();

        if (existing == null)
        {
            await _db.Insertable(new BarcodeSerialCounter
            {
                RuleId = ruleId,
                ResetKey = resetKey,
                CurrentValue = endValue,
                UpdatedAt = DateTime.Now
            }).ExecuteCommandAsync();
            return;
        }

        existing.CurrentValue = endValue;
        existing.UpdatedAt = DateTime.Now;
        await _db.Updateable(existing).ExecuteCommandAsync();
    }

    private void ValidateSerialRange(BarcodeRuleSegment serialSegment, long endValue)
    {
        var config = RuleSegmentConfigMapper.FromEntity(serialSegment);
        var max = _formatter.GetMaxValue(config.SerialRadix, config.SerialDigits);
        if (endValue > max)
        {
            throw new InvalidOperationException($"流水号超出最大范围（{config.SerialRadix}进制 {config.SerialDigits} 位）");
        }
    }
}
