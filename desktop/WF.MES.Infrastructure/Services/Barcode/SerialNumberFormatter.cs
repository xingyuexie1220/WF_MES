using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>流水号按进制/位数格式化及最大值校验。</summary>
public class SerialNumberFormatter : ISerialNumberFormatter
{
    public string Format(long value, int radix, int width)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "流水号不能为负数");
        }

        var text = radix switch
        {
            10 => value.ToString(),
            16 => value.ToString("X"),
            32 => ToCustomBase(value, SerialRadixDefinitions.Base32Chars),
            34 => ToCustomBase(value, SerialRadixDefinitions.Base34Chars),
            36 => ToCustomBase(value, SerialRadixDefinitions.Base36Chars),
            _ => throw new NotSupportedException($"不支持的进制: {radix}")
        };

        if (text.Length > width)
        {
            throw new InvalidOperationException($"流水号 {value} 超出宽度 {width}（{radix}进制）");
        }

        return text.PadLeft(width, '0');
    }

    public long GetMaxValue(int radix, int width)
    {
        return radix switch
        {
            10 => (long)Math.Pow(10, width) - 1,
            16 => (long)Math.Pow(16, width) - 1,
            32 => (long)Math.Pow(SerialRadixDefinitions.Base32Chars.Length, width) - 1,
            34 => (long)Math.Pow(SerialRadixDefinitions.Base34Chars.Length, width) - 1,
            36 => (long)Math.Pow(SerialRadixDefinitions.Base36Chars.Length, width) - 1,
            _ => throw new NotSupportedException($"不支持的进制: {radix}")
        };
    }

    private static string ToCustomBase(long value, char[] chars)
    {
        if (value == 0)
        {
            return "0";
        }

        var baseSize = chars.Length;
        var result = new Stack<char>();
        while (value > 0)
        {
            result.Push(chars[(int)(value % baseSize)]);
            value /= baseSize;
        }

        return new string(result.ToArray());
    }
}
