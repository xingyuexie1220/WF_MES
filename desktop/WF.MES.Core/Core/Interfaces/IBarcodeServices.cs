namespace WF.MES.Core.Interfaces;

using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

/// <summary>条码客户主数据 CRUD。</summary>
public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListDto>> GetCustomersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerListDto>> GetCustomerSelectionListAsync(int? ensureCustomerId = null, CancellationToken cancellationToken = default);

    Task<CustomerEditDto?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<int> SaveCustomerAsync(CustomerEditDto dto, CancellationToken cancellationToken = default);
}

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

/// <summary>
/// 条码资料审核。状态：待上传 → 待审核 → 已确认/已驳回；已确认后才允许打印。
/// </summary>
public interface IBarcodeQaReviewService
{
    Task<IReadOnlyList<BarcodeQaReviewListDto>> GetListAsync(
        int? qaStatusFilter = null,
        int? customerIdFilter = null,
        string? materialNoFilter = null,
        CancellationToken cancellationToken = default);

    Task<BarcodeQaReviewDetailDto?> GetDetailAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>待上传/已驳回可上传；图纸与实物图齐全后转为待审核。</summary>
    Task SaveAttachmentsAsync(BarcodeQaReviewSaveAttachmentsDto dto, CancellationToken cancellationToken = default);

    Task SaveAttachmentsFromFilesAsync(
        int ruleId,
        string? drawingFilePath,
        string? printSampleFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>待审核且双图齐全时确认通过。</summary>
    Task ApproveAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>待审核时驳回，状态变为已驳回。</summary>
    Task RejectAsync(BarcodeQaReviewRejectDto dto, CancellationToken cancellationToken = default);
}

/// <summary>按规则段拼接条码、ResetKey 与长度计算（无 DB 依赖）。</summary>
public interface IBarcodeBuilder
{
    void ValidateSegments(IReadOnlyList<BarcodeRuleSegment> segments);

    BarcodeBuildResult Build(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context, long serialValue);

    string BuildResetKey(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context);

    string PreviewSample(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context);

    int CalculateBarcodeLength(IReadOnlyList<BarcodeRuleSegment> segments);
}

/// <summary>流水号按进制与位数格式化。</summary>
public interface ISerialNumberFormatter
{
    string Format(long value, int radix, int width);

    long GetMaxValue(int radix, int width);
}

/// <summary>条码批量生成（事务、流水号锁定、写入生成单与明细）。</summary>
public interface IBarcodeGenerateService
{
    /// <summary>预览流水号区间与样例条码；只读计数器，不加锁、不写库。</summary>
    Task<BarcodeGeneratePreviewDto> PreviewAsync(BarcodeGenerateRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>正式生成；须 QA 已确认，单事务内 UPDLOCK 占号后写入生成单与明细。</summary>
    Task<BarcodeGenerateResultDto> GenerateAsync(BarcodeGenerateRequestDto request, CancellationToken cancellationToken = default);
}

/// <summary>生成单查询、导出、打印/补打状态更新。</summary>
public interface IBarcodeGenerateRecordService
{
    Task<IReadOnlyList<BarcodeGenerateRecordListDto>> GetGenerateRecordsAsync(
        BarcodeGenerateRecordQueryDto? query = null,
        CancellationToken cancellationToken = default);

    Task<int> ExportGenerateRecordBarcodesAsync(
        int generateRecordId,
        BarcodeGenerateRecordListDto header,
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>标记生成单为已打印。</summary>
    Task MarkPrintedAsync(int generateRecordId, CancellationToken cancellationToken = default);

    /// <summary>标记生成单为已补打并记录操作员与时间。</summary>
    Task MarkReprintedAsync(int generateRecordId, string reprintedBy, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetGenerateRecordBarcodeValuesAsync(int generateRecordId, CancellationToken cancellationToken = default);
}
