using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

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
