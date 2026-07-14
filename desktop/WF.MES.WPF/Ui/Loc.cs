using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;

namespace WF.MES.WPF.Ui;

/// <summary>
/// 桌面多语言公开入口。
/// XAML：<c>Loc.Key</c>；带冒号标签用 <c>Loc.FieldLabelKey</c>（标点走 <c>ui.punct.colon</c>）；
/// DataGrid：<c>LocDataGrid*</c>；VM：<c>L</c>/<c>Lf</c>/<c>Ex</c>；启动早期：<c>WpfLocalization.T</c>。
/// </summary>
public static class Loc
{
    private static LocalizationBindingSource? _designTimeSource;
    private static bool _designLocalePinned;
    public const string ColonKey = "ui.punct.colon";

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.RegisterAttached(
            "Key",
            typeof(string),
            typeof(Loc),
            new PropertyMetadata(null, OnKeyChanged));

    /// <summary>字段标签（文案 + 本地化冒号 <c>ui.punct.colon</c>）。新静态标签优先 <see cref="KeyProperty"/>。</summary>
    public static readonly DependencyProperty FieldLabelKeyProperty =
        DependencyProperty.RegisterAttached(
            "FieldLabelKey",
            typeof(string),
            typeof(Loc),
            new PropertyMetadata(null, OnFieldLabelKeyChanged));

    public static string? GetKey(DependencyObject obj) => (string?)obj.GetValue(KeyProperty);

    public static void SetKey(DependencyObject obj, string? value) => obj.SetValue(KeyProperty, value);

    public static string? GetFieldLabelKey(DependencyObject obj) => (string?)obj.GetValue(FieldLabelKeyProperty);

    public static void SetFieldLabelKey(DependencyObject obj, string? value) => obj.SetValue(FieldLabelKeyProperty, value);

    internal static object BindingSource =>
        Application.Current?.TryFindResource("Loc") as LocalizationBindingSource
        ?? DesignTimeSource;

    /// <summary>供 XAML 绑定文案源（索引器）。</summary>
    public static object LocSource => BindingSource;

    internal static bool IsDesignMode => GetIsDesignMode();

    internal static string ResolveDesignText(string key)
    {
        EnsureDesignTimeLocale();
        return WpfLocalization.T(key);
    }

    internal static string ResolveDesignFieldLabel(string key) =>
        ResolveDesignText(key) + ResolveDesignText(ColonKey);

    internal static Binding CreateBinding(string key) =>
        new($"[{key}]")
        {
            Source = BindingSource,
            Mode = BindingMode.OneWay
        };

    internal static MultiBinding CreateFieldLabelBinding(string key)
    {
        var multi = new MultiBinding
        {
            Converter = LocFieldLabelConverter.Instance,
            Mode = BindingMode.OneWay
        };
        multi.Bindings.Add(CreateBinding(key));
        multi.Bindings.Add(CreateBinding(ColonKey));
        return multi;
    }

    private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string key || string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (IsDesignMode)
        {
            ApplyDesignText(d, ResolveDesignText(key));
            return;
        }

        ApplyTextBinding(d, CreateBinding(key));
    }

    private static void OnFieldLabelKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string key || string.IsNullOrWhiteSpace(key) || d is not TextBlock textBlock)
        {
            return;
        }

        if (IsDesignMode)
        {
            textBlock.Text = ResolveDesignFieldLabel(key);
            return;
        }

        BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, CreateFieldLabelBinding(key));
    }

    private static LocalizationBindingSource DesignTimeSource =>
        _designTimeSource ??= new LocalizationBindingSource(WpfLocalization.Instance);

    private static void EnsureDesignTimeLocale()
    {
        if (_designLocalePinned)
        {
            return;
        }

        try
        {
            _ = WpfLocalization.Instance;
        }
        catch (InvalidOperationException)
        {
            WpfLocalization.Use(new JsonLocalizationService());
        }

        _designLocalePinned = true;
    }

    private static bool GetIsDesignMode()
    {
        try
        {
            return DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyDesignText(DependencyObject d, string text)
    {
        switch (d)
        {
            case TextBlock textBlock:
                textBlock.Text = text;
                break;
            case HeaderedContentControl headered:
                headered.Header = text;
                break;
            case ContentControl contentControl:
                contentControl.Content = text;
                break;
        }
    }

    private static void ApplyTextBinding(DependencyObject d, BindingBase binding)
    {
        switch (d)
        {
            case TextBlock textBlock:
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, binding);
                break;
            case HeaderedContentControl headered:
                BindingOperations.SetBinding(headered, HeaderedContentControl.HeaderProperty, binding);
                break;
            case ContentControl contentControl:
                BindingOperations.SetBinding(contentControl, ContentControl.ContentProperty, binding);
                break;
        }
    }
}

/// <summary>应用级本地化单例入口。App 静态构造即绑定同一 <see cref="ILocalizationService"/>，不再有 Fallback/替换。</summary>
public static class WpfLocalization
{
    private static ILocalizationService? _instance;

    public static ILocalizationService Instance =>
        _instance ?? throw new InvalidOperationException("WpfLocalization has not been initialized. Call Use() from App startup.");

    public static string T(string key, string? fallback = null) => Instance.T(key, fallback);

    public static void Use(ILocalizationService localization) =>
        _instance = localization ?? throw new ArgumentNullException(nameof(localization));
}

/// <summary>全局 UI 文案绑定源（索引器 <c>[key]</c>）。登录页切语言时通知绑定刷新；壳层不提供切换。</summary>
public sealed class LocalizationBindingSource : INotifyPropertyChanged
{
    private readonly ILocalizationService _localization;

    public LocalizationBindingSource(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LocaleChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] => _localization.T(key);
}
