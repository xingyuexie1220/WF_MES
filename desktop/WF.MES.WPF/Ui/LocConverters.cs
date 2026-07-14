using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WF.MES.Core.Constants;
using WF.MES.WPF.Modules.Barcode.ViewModels;

namespace WF.MES.WPF.Ui;

/// <summary>字段标签：文案 + 本地化冒号（<c>ui.punct.colon</c>）。</summary>
public sealed class LocFieldLabelConverter : IMultiValueConverter
{
    public static readonly LocFieldLabelConverter Instance = new();

    private LocFieldLabelConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var label = values.ElementAtOrDefault(0)?.ToString() ?? string.Empty;
        var colon = values.ElementAtOrDefault(1)?.ToString() ?? "：";
        return string.IsNullOrEmpty(label) ? string.Empty : label + colon;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// 转换器与选项映射（本文件：Enum/Format Converter、LocalizedOptions）。
/// 日常优先用 <c>L</c>/<c>Lf</c>；列表枚举列用 <see cref="LocDataGridEnumColumn"/>。
/// </summary>
public sealed class LocEnumConverter : IValueConverter
{
    public static readonly LocEnumConverter Instance = new();

    private LocEnumConverter()
    {
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (IsUnset(value))
        {
            return string.Empty;
        }

        var mapId = parameter as string;
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return value?.ToString() ?? string.Empty;
        }

        return LocEnum.Translate(mapId, value, key => WpfLocalization.T(key));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static bool IsUnset(object? value) =>
        value is null || value == DependencyProperty.UnsetValue;
}

/// <summary>构造枚举翻译 Binding（语言启动时固定，无需 Revision）。</summary>
public static class LocEnumBinding
{
    public static Binding Create(Binding valueBinding, string enumMap)
    {
        valueBinding.Converter = LocEnumConverter.Instance;
        valueBinding.ConverterParameter = enumMap;
        return valueBinding;
    }
}

/// <summary>【高级】XAML 整句格式化；日常优先 ViewModel <c>Lf()</c>。</summary>
public sealed class LocFormatConverter : IMultiValueConverter
{
    public static readonly LocFormatConverter Instance = new();

    private LocFormatConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string key || string.IsNullOrWhiteSpace(key) || values.Length == 0)
        {
            return string.Empty;
        }

        if (values.All(static v => v is null || v == DependencyProperty.UnsetValue || v is ""))
        {
            return string.Empty;
        }

        var formattedArgs = values
            .Select(static v => v is null || v == DependencyProperty.UnsetValue ? string.Empty : v)
            .ToArray();
        return string.Format(WpfLocalization.T(key), formattedArgs);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>枚举/选项 → i18n 键；DTO 只存原始值。</summary>
public static class LocalizedOptions
{
    public static IReadOnlyList<BarcodeSegmentTypeOption> SegmentTypes(Func<string, string> translate) =>
    [
        new(BarcodeSegmentTypes.Literal, translate("ui.barcode.segmentType.literal")),
        new(BarcodeSegmentTypes.Date, translate("ui.barcode.segmentType.date")),
        new(BarcodeSegmentTypes.Serial, translate("ui.barcode.segmentType.serial"))
    ];

    public static IReadOnlyList<DateFormatOption> DateFormats(Func<string, string> translate) =>
        DatePartFormats.AllOptions
            .Select(format => new DateFormatOption(format, translate($"ui.barcode.dateFormatOption.{format}")))
            .ToList();

    public static IReadOnlyList<SerialRadixOption> SerialRadices(Func<string, string> translate) =>
        SerialRadixDefinitions.SupportedRadices
            .Select(radix => new SerialRadixOption(
                radix,
                translate($"ui.barcode.serialRadixOption.{radix}"),
                translate($"ui.barcode.serialRadixDesc.{radix}")))
            .ToList();

    public static string TranslatePrintStatus(int status, Func<string, string> translate) => status switch
    {
        BarcodeOrderPrintStatus.NotPrinted => translate("ui.barcode.orderPrintStatus.notPrinted"),
        BarcodeOrderPrintStatus.Printed => translate("ui.barcode.orderPrintStatus.printed"),
        BarcodeOrderPrintStatus.Reprinted => translate("ui.barcode.orderPrintStatus.reprinted"),
        _ => translate("ui.barcode.orderPrintStatus.unknown")
    };

    public static string TranslateQaStatus(int status, Func<string, string> translate) => status switch
    {
        BarcodeQaStatus.PendingUpload => translate("ui.barcode.qaStatus.pendingUpload"),
        BarcodeQaStatus.PendingReview => translate("ui.barcode.qaStatus.pendingReview"),
        BarcodeQaStatus.Approved => translate("ui.barcode.qaStatus.approved"),
        BarcodeQaStatus.Rejected => translate("ui.barcode.qaStatus.rejected"),
        _ => translate("ui.barcode.qaStatus.unknown")
    };

    public static string TranslateEnableStatus(int enable, Func<string, string> translate) =>
        enable == 1 ? translate("ui.actions.enable") : translate("ui.actions.disabled");

    public static string TranslateAttachmentUploaded(bool uploaded, Func<string, string> translate) =>
        translate(uploaded ? "ui.barcode.attachmentUploaded" : "ui.barcode.attachmentNotUploaded");

    /// <summary>将 Infrastructure 返回的段摘要翻译为当前语言。</summary>
    public static string TranslateSegmentSummary(string summary, Func<string, string> translate)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return summary;
        }

        return string.Join("+", summary.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => TranslateSegmentSummaryPart(part, translate)));
    }

    private static string TranslateSegmentSummaryPart(string part, Func<string, string> translate)
    {
        if (part.StartsWith($"{BarcodeSegmentTypes.Serial}:", StringComparison.Ordinal))
        {
            var args = part[(BarcodeSegmentTypes.Serial.Length + 1)..].Split(',');
            if (args.Length == 2
                && int.TryParse(args[0], out var radix)
                && int.TryParse(args[1], out var digits))
            {
                return string.Format(
                    translate("ui.barcode.serialConfigShort"),
                    translate($"ui.barcode.serialRadixOption.{radix}"),
                    digits);
            }
        }

        return part switch
        {
            BarcodeSegmentTypes.Literal => translate("ui.barcode.segmentType.literal"),
            BarcodeSegmentTypes.Date => translate("ui.barcode.segmentType.date"),
            BarcodeSegmentTypes.Serial => translate("ui.barcode.segmentType.serial"),
            _ => part
        };
    }
}
