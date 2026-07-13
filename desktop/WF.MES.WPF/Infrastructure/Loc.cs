using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;

namespace WF.MES.WPF.Infrastructure;

/// <summary>
/// 桌面多语言核心入口（文件内含 <see cref="Loc"/> / <see cref="WpfLocalization"/> / <see cref="LocalizationBindingSource"/>）。
/// <para>公开用法：XAML <c>infra:Loc.Key</c>；启动早期 <c>WpfLocalization.T</c>；App 注册 <c>Resources["Loc"]</c>。</para>
/// </summary>
public static class Loc
{
    private static LocalizationBindingSource? _designTimeSource;
    private static bool _designLocalePinned;

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.RegisterAttached( "Key",typeof(string),typeof(Loc),
            new PropertyMetadata(null, OnKeyChanged));

    public static readonly DependencyProperty FieldLabelKeyProperty =
        DependencyProperty.RegisterAttached( "FieldLabelKey",typeof(string),typeof(Loc),
            new PropertyMetadata(null, OnFieldLabelKeyChanged));

    public static string? GetKey(DependencyObject obj) => (string?)obj.GetValue(KeyProperty);

    public static void SetKey(DependencyObject obj, string? value) => obj.SetValue(KeyProperty, value);

    public static string? GetFieldLabelKey(DependencyObject obj) => (string?)obj.GetValue(FieldLabelKeyProperty);

    public static void SetFieldLabelKey(DependencyObject obj, string? value) => obj.SetValue(FieldLabelKeyProperty, value);

    internal static object BindingSource =>
        Application.Current?.Resources["Loc"] ?? DesignTimeSource;

    /// <summary>供 XAML <c>x:Static</c> 与 MultiBinding 语言刷新锚点使用。</summary>
    public static object? LocSource => BindingSource;

    internal static bool IsDesignMode => GetIsDesignMode();

    internal static string ResolveDesignText(string key)
    {
        EnsureDesignTimeLocale();
        return WpfLocalization.T(key);
    }

    internal static string ResolveDesignFieldLabel(string key) => ResolveDesignText(key) + "：";

    internal static Binding CreateBinding(string key) =>
        new($"[{key}]")
        {
            Source = BindingSource,
            Mode = BindingMode.OneWay
        };

    internal static Binding CreateFieldLabelBinding(string key) =>
        new($"[{key}]")
        {
            Source = BindingSource,
            Mode = BindingMode.OneWay,
            StringFormat = "{0}："
        };

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

        WpfLocalization.Instance.SetLocale("zh-CN");
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

    private static void ApplyTextBinding(DependencyObject d, Binding binding)
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

/// <summary>应用启动早期（Prism 容器就绪前）使用的本地化访问点；<see cref="App"/> 在 CreateShell 前 <see cref="Use"/> 切到 DI 单例。</summary>
public static class WpfLocalization
{
    private static readonly JsonLocalizationService Fallback = new();

    public static ILocalizationService Instance { get; private set; } = Fallback;

    public static string T(string key, string? fallback = null) => Instance.T(key, fallback);

    public static void Use(ILocalizationService localization) => Instance = localization;
}

/// <summary>全局 UI 文案绑定源（索引器 <c>[key]</c> + <see cref="Revision"/>），由 App 挂到 <c>Resources["Loc"]</c>。</summary>
public sealed class LocalizationBindingSource : INotifyPropertyChanged
{
    private readonly ILocalizationService _localization;
    private int _revision;

    public LocalizationBindingSource(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LocaleChanged += (_, _) => NotifyLocaleChanged();
    }

    /// <summary>语言切换时递增，供枚举/格式 MultiBinding 触发重算。</summary>
    public int Revision => _revision;

    public string this[string key] => _localization.T(key);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyLocaleChanged()
    {
        _revision++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Revision)));
        // WPF 索引器绑定刷新：Item[]；string.Empty 刷新全部属性
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
