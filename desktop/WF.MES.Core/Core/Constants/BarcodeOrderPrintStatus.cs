namespace WF.MES.Core.Constants;

/// <summary>生成单打印状态：未打印 / 已打印 / 已补打。</summary>
public static class BarcodeOrderPrintStatus
{
    public const int NotPrinted = 0;
    public const int Printed = 1;
    public const int Reprinted = 2;

    public static string GetText(int status) => status switch
    {
        NotPrinted => "未打印",
        Printed => "已打印",
        Reprinted => "已补打",
        _ => "未知"
    };

    public static bool CanReprint(int status) => status == Printed; // 已补打不可再次补打
}
