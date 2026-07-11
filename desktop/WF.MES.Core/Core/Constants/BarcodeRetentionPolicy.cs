namespace WF.MES.Core.Constants;

/// <summary>
/// 条码生成记录保留策略（与 dbo.sp_Barcode_PurgeExpired 及 SQL Agent 作业保持一致）。
/// </summary>
public static class BarcodeRetentionPolicy
{
    public const int RetentionDays = 30;

    public static string DetailPageHint => $"系统仅保留近 {RetentionDays} 天内的生成记录，更早数据由数据库每日定时任务自动清理。";
}
