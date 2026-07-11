namespace WF.MES.Core.Constants;

/// <summary>条码规则段类型：固定符号、日期、流水号。</summary>
public static class BarcodeSegmentTypes
{
    public const string Literal = "Literal";

    public const string Date = "Date";

    public const string Serial = "Serial";

    public const long SerialStartValue = 1;

    public static IReadOnlyList<BarcodeSegmentTypeOption> TypeOptions { get; } =
    [
        new(Literal, GetDisplayName(Literal)),
        new(Date, GetDisplayName(Date)),
        new(Serial, GetDisplayName(Serial))
    ];

    public static string GetDisplayName(string segmentType) => segmentType switch
    {
        Literal => "固定符号",
        Date => "日期",
        Serial => "流水号",
        _ => segmentType
    };
}

public sealed record BarcodeSegmentTypeOption(string Value, string Display);
