namespace WF.MES.Core.Constants;

/// <summary>条码资料审核状态（Barcode_MaterialRule.Qa_Status）。</summary>
public static class BarcodeQaStatus
{
    public const int PendingUpload = 0;

    public const int PendingReview = 1;

    public const int Approved = 2;

    public const int Rejected = 3;

    public static bool CanReview(int status) => status == PendingReview;

    public static bool CanUpload(int status) => status is PendingUpload or Rejected;

    /// <summary>是否允许打印（仅 Qa_Status=已确认）。</summary>
    public static bool IsApprovedForPrint(int status) => status == Approved;
}
