namespace WF.MES.Core.Constants;

/// <summary>日期段可选格式（与 DatePartFormatter / DatePartCharMaps 对应）。</summary>
public static class DatePartFormats
{
    public static readonly string[] AllOptions = ["YYYY", "YY", "Y", "MM", "M", "DD", "D", "WW"];

    public static readonly DateFormatOption[] AllOptionItems =
    [
        new("YYYY", "YYYY（四位年）"),
        new("YY", "YY（两位年）"),
        new("Y", "Y（年份个位）"),
        new("MM", "MM（两位月）"),
        new("M", "M（月份一位：1~9，10~12 为 A~C）"),
        new("DD", "DD（两位日）"),
        new("D", "D（日期一位：1~9，10~31 为字母且不含 I/O/Q）"),
        new("WW", "WW（周别）")
    ];

    public static string Default => "YYYY";

    public static bool IsValid(string format) => AllOptions.Contains(format);
}

public sealed record DateFormatOption(string Value, string Display);
