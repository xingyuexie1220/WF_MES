using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

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
