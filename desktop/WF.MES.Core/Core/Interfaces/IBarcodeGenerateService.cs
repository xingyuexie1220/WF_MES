using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>条码批量生成（事务、流水号锁定、写入生成单与明细）。</summary>
public interface IBarcodeGenerateService
{
    /// <summary>预览流水号区间与样例条码；只读计数器，不加锁、不写库。</summary>
    Task<BarcodeGeneratePreviewDto> PreviewAsync(BarcodeGenerateRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>正式生成；须 QA 已确认，单事务内 UPDLOCK 占号后写入生成单与明细。</summary>
    Task<BarcodeGenerateResultDto> GenerateAsync(BarcodeGenerateRequestDto request, CancellationToken cancellationToken = default);
}
