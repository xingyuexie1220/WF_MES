using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Localization;

public sealed class JsonLocalizationService : ILocalizationService
{
    private const string LocaleStorageFile = "wf_locale.txt";

    private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLocale = "zh-CN";

    public JsonLocalizationService()
    {
        LocaleOptions =
        [
            new LocaleOption { Value = "zh-CN", LabelKey = "locale.zhCN" },
            new LocaleOption { Value = "zh-TW", LabelKey = "locale.zhTW" },
            new LocaleOption { Value = "en", LabelKey = "locale.en" }
        ];

        foreach (var option in LocaleOptions)
        {
            LoadCatalog(option.Value);
        }

        _currentLocale = LoadSavedLocale();
        if (!_catalogs.ContainsKey(_currentLocale))
        {
            _currentLocale = "zh-CN";
        }
    }

    public string CurrentLocale => _currentLocale;

    public IReadOnlyList<LocaleOption> LocaleOptions { get; }

    public event EventHandler? LocaleChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string T(string key, string? fallback = null)
    {
        if (_catalogs.TryGetValue(_currentLocale, out var map)
            && map.TryGetValue(key, out var value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (_catalogs.TryGetValue("zh-CN", out var fallbackMap)
            && fallbackMap.TryGetValue(key, out var zhValue)
            && !string.IsNullOrWhiteSpace(zhValue))
        {
            return zhValue;
        }

        return fallback ?? key;
    }

    public void SetLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale)
            || string.Equals(_currentLocale, locale, StringComparison.OrdinalIgnoreCase)
            || !LocaleOptions.Any(option => option.Value.Equals(locale, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _currentLocale = locale;
        SaveLocale(locale);
        LocaleChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(CurrentLocale));
    }

    private void LoadCatalog(string locale)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "i18n", $"{locale}.json");
        if (!File.Exists(path))
        {
            _catalogs[locale] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);
        var flat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Flatten(document.RootElement, string.Empty, flat);
        _catalogs[locale] = flat;
    }

    private static void Flatten(JsonElement element, string prefix, IDictionary<string, string> target)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Flatten(property.Value, key, target);
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                target[key] = property.Value.GetString() ?? string.Empty;
            }
        }
    }

    private static string LoadSavedLocale()
    {
        var path = GetLocaleStoragePath();
        if (!File.Exists(path))
        {
            return "zh-CN";
        }

        var saved = File.ReadAllText(path).Trim();
        return string.IsNullOrWhiteSpace(saved) ? "zh-CN" : saved;
    }

    private static void SaveLocale(string locale)
    {
        var path = GetLocaleStoragePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, locale);
    }

    private static string GetLocaleStoragePath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WF.MES", LocaleStorageFile);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
