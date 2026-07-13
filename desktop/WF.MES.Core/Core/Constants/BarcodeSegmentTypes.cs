namespace WF.MES.Core.Constants;

/// <summary>条码规则段类型：固定符号、日期、流水号。</summary>
public static class BarcodeSegmentTypes
{
    public const string Literal = "Literal";

    public const string Date = "Date";

    public const string Serial = "Serial";

    public const long SerialStartValue = 1;

    public static readonly string[] AllTypes = [Literal, Date, Serial];

    public static bool IsValid(string? segmentType) =>
        !string.IsNullOrWhiteSpace(segmentType) && AllTypes.Contains(segmentType);
}

public sealed record BarcodeSegmentTypeOption(string Value, string Display);
