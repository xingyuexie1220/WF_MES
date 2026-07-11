using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>BarTender 标签打印（COM，模板在 Labels/{料号}.btw）。</summary>
public interface ILabelPrintService
{
    IReadOnlyList<string> GetInstalledPrinters();

    /// <summary>在资源管理器中打开 Labels/{materialNo}.btw 所在目录。</summary>
    void OpenTemplateFolder(string materialNo);

    /// <summary>按料号模板与条码列表构造 BarTender 打印请求。</summary>
    LabelPrintRequestDto CreatePrintRequest(string materialNo, string printerName, IEnumerable<string> barcodes);

    Task<LabelPrintResultDto> PrintAsync(
        LabelPrintRequestDto request,
        IProgress<LabelPrintProgressDto>? progress = null,
        CancellationToken cancellationToken = default);
}
