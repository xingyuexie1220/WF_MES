using System.Globalization;

namespace WF.MES.WPF.Ui;

/// <summary>
/// 枚举/状态值 → i18n 键映射。用 <see cref="LocEnumMaps"/> 常量注册，避免 XAML 魔法串拼错。
/// </summary>
public static class LocEnum
{
    private static readonly Dictionary<string, Func<object?, Func<string, string>, string>> Maps =
        new(StringComparer.Ordinal);

    static LocEnum()
    {
        Register(LocEnumMaps.PrintStatus, (value, t) =>
            TryToInt32(value, out var status)
                ? LocalizedOptions.TranslatePrintStatus(status, t)
                : value?.ToString() ?? string.Empty);

        Register(LocEnumMaps.QaStatus, (value, t) =>
            TryToInt32(value, out var status)
                ? LocalizedOptions.TranslateQaStatus(status, t)
                : value?.ToString() ?? string.Empty);

        Register(LocEnumMaps.Enable, (value, t) =>
            TryToInt32(value, out var enable)
                ? LocalizedOptions.TranslateEnableStatus(enable, t)
                : value?.ToString() ?? string.Empty);

        Register(LocEnumMaps.Attachment, (value, t) =>
            TryToBoolean(value, out var uploaded)
                ? LocalizedOptions.TranslateAttachmentUploaded(uploaded, t)
                : value?.ToString() ?? string.Empty);
    }

    private static void Register(string mapId, Func<object?, Func<string, string>, string> translator)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            throw new ArgumentException("Enum map id is required.", nameof(mapId));
        }

        Maps[mapId] = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public static string Translate(string mapId, object? value, Func<string, string> translate)
    {
        if (string.IsNullOrWhiteSpace(mapId) || !Maps.TryGetValue(mapId, out var translator))
        {
            return value?.ToString() ?? string.Empty;
        }

        return translator(value, translate);
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
