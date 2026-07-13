namespace WF.MES.Core.Constants;

/// <summary>日期段可选格式（与 DatePartFormatter / DatePartCharMaps 对应）。</summary>
public static class DatePartFormats
{
    public static readonly string[] AllOptions = ["YYYY", "YY", "Y", "MM", "M", "DD", "D", "WW"];

    public static string Default => "YYYY";

    public static bool IsValid(string format) => AllOptions.Contains(format);
}

public sealed record DateFormatOption(string Value, string Display);
