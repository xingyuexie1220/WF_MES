namespace WF.MES.Core.Interfaces;

/// <summary>流水号按进制与位数格式化。</summary>
public interface ISerialNumberFormatter
{
    string Format(long value, int radix, int width);

    long GetMaxValue(int radix, int width);
}
