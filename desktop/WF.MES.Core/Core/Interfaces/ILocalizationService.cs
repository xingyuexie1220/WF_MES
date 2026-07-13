using System.ComponentModel;

namespace WF.MES.Core.Interfaces;

public sealed class LocaleOption
{
    public string Value { get; init; } = "zh-CN";
    public string LabelKey { get; init; } = "locale.zhCN";
}

/// <summary>桌面端 UI 文案包（desktop/WF.MES.WPF/i18n）多语言服务；API 错误键与 i18n/api-codes 协议一致。</summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLocale { get; }

    IReadOnlyList<LocaleOption> LocaleOptions { get; }

    string T(string key, string? fallback = null);

    void SetLocale(string locale);

    event EventHandler? LocaleChanged;
}
