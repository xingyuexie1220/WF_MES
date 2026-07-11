using System.Globalization;
using WF.MES.Core.Constants;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>将规则段中的日期格式码转为实际字符串。</summary>
public static class DatePartFormatter
{
    public static string Format(DateTime date, string format)
    {
        if (!DatePartFormats.IsValid(format))
        {
            throw new InvalidOperationException($"不支持的日期格式: {format}");
        }

        return format switch
        {
            "YYYY" => date.ToString("yyyy"),
            "YY" => date.ToString("yy"),
            "Y" => (date.Year % 10).ToString(),
            "MM" => date.ToString("MM"),
            "M" => DatePartCharMaps.FormatMonthOneChar(date.Month),
            "DD" => date.ToString("dd"),
            "D" => DatePartCharMaps.FormatDayOneChar(date.Day),
            "WW" => ISOWeek.GetWeekOfYear(date).ToString("D2"),
            _ => throw new InvalidOperationException($"不支持的日期格式: {format}")
        };
    }
}
