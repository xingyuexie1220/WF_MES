using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WF.MES.Core.Constants;
using WF.MES.WPF.Modules.Barcode.ViewModels;

namespace WF.MES.WPF.Infrastructure;

/// <summary>
/// 转换器与选项映射（本文件：Enum/Format Converter、LocalizedOptions）。
/// 日常优先用 <c>L</c>/<c>TF</c>；列表枚举列用 <see cref="LocDataGridEnumColumn"/>。
/// </summary>
public sealed class LocEnumConverter : IMultiValueConverter
{
    public static readonly LocEnumConverter Instance = new();

    private LocEnumConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || IsUnset(values[0]))
        {
            return string.Empty;
        }

        var mapId = parameter as string;
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return values[0].ToString() ?? string.Empty;
        }

        return LocalizedOptions.TranslateEnumMap(mapId, values[0], key => WpfLocalization.T(key));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static bool IsUnset(object? value) =>
        value is null || value == DependencyProperty.UnsetValue;
}

/// <summary>构造带语言刷新触发的枚举翻译 MultiBinding。</summary>
public static class LocEnumBinding
{
    public static MultiBinding Create(Binding valueBinding, string enumMap)
    {
        var multi = new MultiBinding
        {
            Converter = LocEnumConverter.Instance,
            ConverterParameter = enumMap
        };
        multi.Bindings.Add(valueBinding);
        multi.Bindings.Add(new Binding(nameof(LocalizationBindingSource.Revision))
        {
            Source = Loc.BindingSource,
            Mode = BindingMode.OneWay
        });
        return multi;
    }
}

/// <summary>【高级】整句格式化；日常优先 ViewModel <c>TF()</c>。</summary>
public sealed class LocFormatConverter : IMultiValueConverter
{
    public static readonly LocFormatConverter Instance = new();

    private LocFormatConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || parameter is not string key || string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var args = values[..^1];
        if (args.All(static v => v is null || v == DependencyProperty.UnsetValue || v is ""))
        {
            return string.Empty;
        }

        var formattedArgs = args
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

    public static string TranslateEnumMap(string mapId, object? value, Func<string, string> translate) => mapId switch
    {
        LocEnumMaps.PrintStatus when TryToInt32(value, out var printStatus)
            => TranslatePrintStatus(printStatus, translate),
        LocEnumMaps.QaStatus when TryToInt32(value, out var qaStatus)
            => TranslateQaStatus(qaStatus, translate),
        LocEnumMaps.Enable when TryToInt32(value, out var enable)
            => TranslateEnableStatus(enable, translate),
        LocEnumMaps.Attachment when TryToBoolean(value, out var uploaded)
            => TranslateAttachmentUploaded(uploaded, translate),
        _ => value?.ToString() ?? string.Empty
    };

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

    private static bool TryToInt32(object? value, out int result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l and >= int.MinValue and <= int.MaxValue:
                result = (int)l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result):
                return true;
            case IConvertible convertible:
                try
                {
                    result = convertible.ToInt32(CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    break;
                }
        }

        result = 0;
        return false;
    }

    private static bool TryToBoolean(object? value, out bool result)
    {
        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case int i:
                result = i != 0;
                return true;
            case string text when bool.TryParse(text, out result):
                return true;
            case IConvertible convertible:
                try
                {
                    result = convertible.ToBoolean(CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    break;
                }
        }

        result = false;
        return false;
    }
}
