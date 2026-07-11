using FluentValidation;
using Serilog;
using SqlSugar;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;
using WF.MES.Infrastructure.Validation;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>
/// 料号规则 CRUD 实现。保存时全量替换规则段并重置 QA；HasGenerationHistoryAsync 供 UI 判断 ResetKey 变更是否需确认。
/// </summary>
public class MaterialBarcodeRuleService : IMaterialBarcodeRuleService
{
    private readonly ISqlSugarClient _db;
    private readonly ISessionService _sessionService;
    private readonly IValidator<MaterialRuleEditDto> _validator;

    public MaterialBarcodeRuleService(
        ISqlSugarClient db,
        ISessionService sessionService,
        IValidator<MaterialRuleEditDto> validator)
    {
        _db = db;
        _sessionService = sessionService;
        _validator = validator;
    }

    public async Task<IReadOnlyList<MaterialRuleListDto>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        return await QueryRulesAsync(approvedOnly: false);
    }

    public async Task<IReadOnlyList<MaterialRuleListDto>> GetApprovedRulesAsync(CancellationToken cancellationToken = default)
    {
        return await QueryRulesAsync(approvedOnly: true);
    }

    public async Task EnsureRuleApprovedForPrintAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(ruleId);
        if (rule == null || !BarcodeQaStatus.IsApprovedForPrint(rule.QaStatus))
        {
            var materialNo = rule?.MaterialNo ?? ruleId.ToString();
            throw new InvalidOperationException($"料号 {materialNo} 待 QA 确认");
        }
    }

    /// <summary>列表查询；approvedOnly 为 true 时仅返回 QA 已确认规则（打印页下拉）。</summary>
    private async Task<IReadOnlyList<MaterialRuleListDto>> QueryRulesAsync(bool approvedOnly)
    {
        var query = _db.Queryable<BarcodeMaterialRule, BarcodeCustomer>(
                (rule, customer) => new JoinQueryInfos(
                    JoinType.Inner, rule.CustomerId == customer.CustomerId))
            .Where((rule, customer) => customer.Enable == 1);

        if (approvedOnly)
        {
           
            query = query.Where((rule, customer) => rule.QaStatus == BarcodeQaStatus.Approved);
        }

        var rules = await query
            .OrderBy((rule, customer) => rule.MaterialNo)
            .Select((rule, customer) => new { rule, customer })
            .ToListAsync();

        var ruleIds = rules.Select(r => r.rule.RuleId).ToList();
        var segments = ruleIds.Count == 0
            ? []
            : await _db.Queryable<BarcodeRuleSegment>()
                .Where(s => ruleIds.Contains(s.RuleId))
                .ToListAsync();

        return rules.Select(r =>
        {
            var ruleSegments = segments.Where(s => s.RuleId == r.rule.RuleId).OrderBy(s => s.SortOrder).ToList();
            return new MaterialRuleListDto
            {
                RuleId = r.rule.RuleId,
                CustomerId = r.rule.CustomerId,
                CustomerName = r.customer.CustomerName,
                MaterialNo = r.rule.MaterialNo,
                SegmentSummary = BuildSummary(ruleSegments),
                BarcodeLength = r.rule.BarcodeLength,
                CreatedBy = r.rule.CreatedBy,
                CreateDate = r.rule.CreateDate,
                UpdatedBy = r.rule.UpdatedBy,
                UpdatedAt = r.rule.UpdatedAt
            };
        }).ToList();
    }

    public async Task<MaterialRuleEditDto?> GetRuleAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(ruleId);
        if (rule == null)
        {
            return null;
        }

        var segments = await GetSegmentsAsync(ruleId);
        return new MaterialRuleEditDto
        {
            RuleId = rule.RuleId,
            CustomerId = rule.CustomerId,
            MaterialNo = rule.MaterialNo,
            BarcodeLength = rule.BarcodeLength,
            Segments = segments.ToList(),
            CreatedBy = rule.CreatedBy,
            CreateDate = rule.CreateDate,
            UpdatedBy = rule.UpdatedBy,
            UpdatedAt = rule.UpdatedAt
        };
    }

    private async Task<IReadOnlyList<RuleSegmentEditDto>> GetSegmentsAsync(int ruleId)
    {
        var segments = await _db.Queryable<BarcodeRuleSegment>()
            .Where(s => s.RuleId == ruleId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        return segments.Select(RuleSegmentConfigMapper.FromEntity).ToList();
    }

    public async Task<bool> HasGenerationHistoryAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        if (ruleId <= 0)
        {
            return false;
        }

        var hasGenerateRecords = await _db.Queryable<BarcodeGenerateRecord>()
            .AnyAsync(r => r.RuleId == ruleId);

        if (hasGenerateRecords)
        {
            return true;
        }

        return await _db.Queryable<BarcodeSerialCounter>()
            .AnyAsync(c => c.RuleId == ruleId);
    }

    public async Task<int> SaveRuleAsync(MaterialRuleEditDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateRequestAsync(dto, cancellationToken);

        var operatorName = BarcodeAuditHelper.GetCurrentOperator(_sessionService);

        await _db.Ado.BeginTranAsync();
        try
        {
            var ruleId = dto.RuleId;
            if (dto.RuleId == 0)
            {
                var exists = await _db.Queryable<BarcodeMaterialRule>()
                    .AnyAsync(r => r.CustomerId == dto.CustomerId && r.MaterialNo == dto.MaterialNo.Trim());
                if (exists)
                {
                    throw new InvalidOperationException($"该客户下料号「{dto.MaterialNo}」规则已存在");
                }

                var rule = new BarcodeMaterialRule
                {
                    CustomerId = dto.CustomerId,
                    MaterialNo = dto.MaterialNo.Trim(),
                    BarcodeLength = dto.BarcodeLength
                };
                BarcodeAuditHelper.ApplyCreateAudit(rule, operatorName);

                ruleId = await _db.Insertable(rule).ExecuteReturnIdentityAsync();
            }
            else
            {
                var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(dto.RuleId)
                    ?? throw new InvalidOperationException("料号条码规则不存在");

                var duplicate = await _db.Queryable<BarcodeMaterialRule>()
                    .AnyAsync(r => r.CustomerId == dto.CustomerId
                                   && r.MaterialNo == dto.MaterialNo.Trim()
                                   && r.RuleId != dto.RuleId);
                if (duplicate)
                {
                    throw new InvalidOperationException($"该客户下料号「{dto.MaterialNo}」规则已存在");
                }

                rule.CustomerId = dto.CustomerId;
                rule.MaterialNo = dto.MaterialNo.Trim();
                rule.BarcodeLength = dto.BarcodeLength;
                BarcodeAuditHelper.ResetQaApproval(rule);
                BarcodeAuditHelper.ApplyUpdateAudit(rule, operatorName);
                await _db.Updateable(rule).ExecuteCommandAsync();
                ruleId = rule.RuleId;
            }

            await _db.Deleteable<BarcodeRuleSegment>().Where(s => s.RuleId == ruleId).ExecuteCommandAsync();

            var order = 1;
            foreach (var segment in dto.Segments.OrderBy(s => s.SortOrder))
            {
                segment.SortOrder = order++;
                await _db.Insertable(new BarcodeRuleSegment
                {
                    RuleId = ruleId,
                    SortOrder = segment.SortOrder,
                    SegmentType = segment.SegmentType,
                    ConfigJson = RuleSegmentConfigHelper.ToConfigJson(segment),
                    IncludeInResetKey = segment.SegmentType == BarcodeSegmentTypes.Serial ? 0 : segment.IncludeInResetKey ? 1 : 0
                }).ExecuteCommandAsync();
            }

            await _db.Ado.CommitTranAsync();
            Log.Information("料号条码规则 {MaterialNo} 已保存", dto.MaterialNo);
            return ruleId;
        }
        catch
        {
            await _db.Ado.RollbackTranAsync();
            throw;
        }
    }

    private static string BuildSummary(IReadOnlyList<BarcodeRuleSegment> segments)
    {
        return string.Join("+", segments.OrderBy(s => s.SortOrder).Select(s => s.SegmentType switch
        {
            BarcodeSegmentTypes.Serial => FormatSerialSummary(s),
            _ => BarcodeSegmentTypes.GetDisplayName(s.SegmentType)
        }));
    }

    private static string FormatSerialSummary(BarcodeRuleSegment segment)
    {
        var config = RuleSegmentConfigMapper.FromEntity(segment);
        return $"流水号({config.SerialRadix}进制,{config.SerialDigits}位)";
    }
}
