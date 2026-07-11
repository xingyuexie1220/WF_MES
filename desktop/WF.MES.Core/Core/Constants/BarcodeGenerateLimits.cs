namespace WF.MES.Core.Constants;

/// <summary>条码单次生成与批量写入、导出的数量上限。</summary>
public static class BarcodeGenerateLimits
{
    public const int MaxQuantityPerBatch = 20000;

    /// <summary>
    /// 分批 INSERT 每批行数。SQL Server 单语句参数上限约 2100；
    /// Barcode_Record 每行约 7 个字段，故每批不超过 250 行。
    /// </summary>
    public const int BulkInsertBatchSize = 250;

    /// <summary>条码明细导出时分页读取行数。</summary>
    public const int ExportPageSize = 500;
}
