namespace WF.MES.Core.Constants;

/// <summary>
/// 条码生成记录保留策略（与 dbo.sp_Barcode_PurgeExpired 及 SQL Agent 作业保持一致）。
/// </summary>
public static class BarcodeRetentionPolicy
{
    public const int RetentionDays = 30;
}
