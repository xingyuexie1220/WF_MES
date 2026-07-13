using WF.MES.Core.Exceptions;

namespace WF.MES.Core.Constants;

/// <summary>
/// 年月日一位对照：1~9 为数字，月起 10 月为 A~C，日起 10 日为字母且不含 I/O/Q。
/// </summary>
public static class DatePartCharMaps
{
    /// <summary>10~31 日对应字母（A~Z 去掉 I、O、Q，共 22 个）。</summary>
    private static readonly char[] DayOneCharMap = "ABCDEFGHJKLMNPRSTUVWXY".ToCharArray();

    public static string FormatMonthOneChar(int month)
    {
        if (month is < 1 or > 12)
        {
            throw new BusinessException("err.monthOutOfRange", month);
        }

        return month <= 9 ? month.ToString() : ((char)('A' + month - 10)).ToString();
    }

    public static string FormatDayOneChar(int day)
    {
        if (day is < 1 or > 31)
        {
            throw new BusinessException("err.dayOutOfRange", day);
        }

        return day <= 9 ? day.ToString() : DayOneCharMap[day - 10].ToString();
    }
}
