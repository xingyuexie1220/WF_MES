using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>料号条码规则及规则段维护。</summary>
public interface IMaterialBarcodeRuleService
{
    Task<IReadOnlyList<MaterialRuleListDto>> GetRulesAsync(CancellationToken cancellationToken = default);

    Task<MaterialRuleEditDto?> GetRuleAsync(int ruleId, CancellationToken cancellationToken = default);

    Task<int> SaveRuleAsync(MaterialRuleEditDto dto, CancellationToken cancellationToken = default);

    Task<bool> HasGenerationHistoryAsync(int ruleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaterialRuleListDto>> GetApprovedRulesAsync(CancellationToken cancellationToken = default);

    Task EnsureRuleApprovedForPrintAsync(int ruleId, CancellationToken cancellationToken = default);
}
