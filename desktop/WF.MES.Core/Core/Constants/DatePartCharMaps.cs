namespace WF.MES.Core.Constants;

/// <summary>
/// 年月日一位对照：1~9 为数字，月起 10 月为 A~C，日起 10 日为字母且不含 I/O/Q。
/// </summary>
public static class DatePartCharMaps
{
    private static readonly char[] DayOneCharMap = BuildDayOneCharMap();

    public static string FormatMonthOneChar(int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "月份必须在 1~12 之间");
        }

        return month <= 9 ? month.ToString() : ((char)('A' + month - 10)).ToString();
    }

    public static string FormatDayOneChar(int day)
    {
        if (day is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(day), day, "日期必须在 1~31 之间");
        }

        return day <= 9 ? day.ToString() : DayOneCharMap[day - 10].ToString();
    }

    private static char[] BuildDayOneCharMap()
    {
        ReadOnlySpan<char> excluded = ['I', 'O', 'Q'];
        var chars = new char[22];
        var index = 0;
        for (var c = 'A'; index < chars.Length; c++)
        {
            if (excluded.Contains(c))
            {
                continue;
            }

            chars[index++] = c;
        }

        return chars;
    }
}
