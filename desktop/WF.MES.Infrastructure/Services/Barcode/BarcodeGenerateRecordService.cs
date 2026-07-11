using System.IO;
using System.Text;
using SqlSugar;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>
/// 生成单查询、分页流式导出 CSV、打印/补打状态更新。
/// 导出与取条码值分页读取 Barcode_Record，避免大单占满内存；补打仅允许「已打印→已补打」一次。
/// </summary>
public class BarcodeGenerateRecordService : IBarcodeGenerateRecordService
{
    private readonly ISqlSugarClient _db;

    public BarcodeGenerateRecordService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task MarkPrintedAsync(int generateRecordId, CancellationToken cancellationToken = default)
    {
        if (generateRecordId <= 0)
        {
            throw new InvalidOperationException("生成单无效。");
        }

        var updated = await _db.Updateable<BarcodeGenerateRecord>()
            .SetColumns(r => new BarcodeGenerateRecord
            {
                PrintStatus = BarcodeOrderPrintStatus.Printed
            })
            .Where(r => r.GenerateRecordId == generateRecordId)
            .ExecuteCommandAsync();

        if (updated == 0)
        {
            throw new InvalidOperationException("未找到对应的生成单。");
        }
    }

    public async Task MarkReprintedAsync(
        int generateRecordId,
        string reprintedBy,
        CancellationToken cancellationToken = default)
    {
        if (generateRecordId <= 0)
        {
            throw new InvalidOperationException("生成单无效。");
        }

        if (string.IsNullOrWhiteSpace(reprintedBy))
        {
            throw new InvalidOperationException("补打操作人不能为空。");
        }

        var updated = await _db.Updateable<BarcodeGenerateRecord>()
            .SetColumns(r => new BarcodeGenerateRecord
            {
                PrintStatus = BarcodeOrderPrintStatus.Reprinted,
                LastReprintedAt = DateTime.Now,
                LastReprintedBy = reprintedBy.Trim()
            })
            // 仅「已打印」可补打一次，补打后变为「已补打」
            .Where(r => r.GenerateRecordId == generateRecordId && r.PrintStatus == BarcodeOrderPrintStatus.Printed)
            .ExecuteCommandAsync();

        if (updated == 0)
        {
            throw new InvalidOperationException("该生成单不可补打（可能已补打或尚未打印）。");
        }
    }

    public async Task<IReadOnlyList<BarcodeGenerateRecordListDto>> GetGenerateRecordsAsync(
        BarcodeGenerateRecordQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var materialNo = query?.MaterialNo?.Trim();
        var customerId = query?.CustomerId;
        var generateNo = query?.GenerateNo?.Trim();
        var createdFrom = query?.CreatedFrom?.Date;
        var printStatuses = query?.PrintStatuses;

        var rows = await _db.Queryable<BarcodeGenerateRecord, BarcodeMaterialRule, BarcodeCustomer>(
                (record, rule, customer) => new JoinQueryInfos(
                    JoinType.Inner, record.RuleId == rule.RuleId,
                    JoinType.Inner, rule.CustomerId == customer.CustomerId))
            .WhereIF(customerId is > 0,
                (record, rule, customer) => rule.CustomerId == customerId)
            .WhereIF(!string.IsNullOrWhiteSpace(materialNo),
                (record, rule, customer) => record.MaterialNo.Contains(materialNo!))
            .WhereIF(!string.IsNullOrWhiteSpace(generateNo),
                (record, rule, customer) => record.GenerateNo.Contains(generateNo!))
            .WhereIF(createdFrom != null,
                (record, rule, customer) => record.CreatedAt >= createdFrom)
            .WhereIF(printStatuses is { Count: > 0 },
                (record, rule, customer) => printStatuses!.Contains(record.PrintStatus))
            .OrderBy((record, rule, customer) => record.CreatedAt, OrderByType.Desc)
            .Select((record, rule, customer) => new
            {
                record.GenerateRecordId,
                record.RuleId,
                record.GenerateNo,
                customer.CustomerName,
                record.MaterialNo,
                record.PrintDate,
                record.Quantity,
                record.SerialStart,
                record.SerialEnd,
                record.ResetKey,
                record.CreatedBy,
                record.CreatedAt,
                record.PrintStatus,
                record.LastReprintedAt,
                record.LastReprintedBy
            })
            .ToListAsync();

        return rows.Select(row => new BarcodeGenerateRecordListDto
        {
            GenerateRecordId = row.GenerateRecordId,
            RuleId = row.RuleId,
            GenerateNo = row.GenerateNo,
            CustomerName = row.CustomerName,
            MaterialNo = row.MaterialNo,
            PrintDate = row.PrintDate,
            Quantity = row.Quantity,
            SerialStart = row.SerialStart,
            SerialEnd = row.SerialEnd,
            ResetKey = row.ResetKey,
            CreatedBy = row.CreatedBy,
            CreatedAt = row.CreatedAt,
            PrintStatus = row.PrintStatus,
            LastReprintedAt = row.LastReprintedAt,
            LastReprintedBy = row.LastReprintedBy
        }).ToList();
    }

    public async Task<int> ExportGenerateRecordBarcodesAsync(
        int generateRecordId,
        BarcodeGenerateRecordListDto header,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (generateRecordId <= 0)
        {
            throw new InvalidOperationException("生成单无效。");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException("导出路径无效。");
        }

        await using var writer = new StreamWriter(
            filePath,
            append: false,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync($"生成单号,{EscapeCsv(header.GenerateNo)}");
        await writer.WriteLineAsync($"客户,{EscapeCsv(header.CustomerName)}");
        await writer.WriteLineAsync($"料号,{EscapeCsv(header.MaterialNo)}");
        await writer.WriteLineAsync($"打印日期,{header.PrintDate:yyyy-MM-dd}");
        await writer.WriteLineAsync($"数量,{header.Quantity}");
        await writer.WriteLineAsync($"流水范围,{EscapeCsv(header.SerialRangeText)}");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync("流水值,完整条码");

        var pageIndex = 1;
        var total = 0;

        // 分页写入 CSV，单页大小见 BarcodeGenerateLimits.ExportPageSize
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rows = await _db.Queryable<BarcodeRecord>()
                .Where(r => r.GenerateRecordId == generateRecordId && r.Status == 1)
                .OrderBy(r => r.SerialValue)
                .Select(r => new
                {
                    r.SerialValue,
                    r.Barcode
                })
                .ToPageListAsync(pageIndex, BarcodeGenerateLimits.ExportPageSize);

            if (rows.Count == 0)
            {
                break;
            }

            foreach (var row in rows)
            {
                await writer.WriteLineAsync($"{row.SerialValue},{EscapeCsv(row.Barcode)}");
            }

            total += rows.Count;
            if (rows.Count < BarcodeGenerateLimits.ExportPageSize)
            {
                break;
            }

            pageIndex++;
        }

        return total;
    }

    public async Task<IReadOnlyList<string>> GetGenerateRecordBarcodeValuesAsync(
        int generateRecordId,
        CancellationToken cancellationToken = default)
    {
        if (generateRecordId <= 0)
        {
            throw new InvalidOperationException("生成单无效。");
        }

        var result = new List<string>();
        var pageIndex = 1;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rows = await _db.Queryable<BarcodeRecord>()
                .Where(r => r.GenerateRecordId == generateRecordId && r.Status == 1)
                .OrderBy(r => r.SerialValue)
                .Select(r => r.Barcode)
                .ToPageListAsync(pageIndex, BarcodeGenerateLimits.ExportPageSize);

            if (rows.Count == 0)
            {
                break;
            }

            result.AddRange(rows);
            if (rows.Count < BarcodeGenerateLimits.ExportPageSize)
            {
                break;
            }

            pageIndex++;
        }

        return result;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
