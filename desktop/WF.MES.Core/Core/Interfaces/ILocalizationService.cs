using System.ComponentModel;

namespace WF.MES.Core.Interfaces;

public sealed class LocaleOption
{
    public string Value { get; init; } = "zh-CN";
    public string LabelKey { get; init; } = "locale.zhCN";
}

/// <summary>共享 i18n/messages JSON 的多语言服务（与 Web / Mobile 同源）。</summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLocale { get; }

    IReadOnlyList<LocaleOption> LocaleOptions { get; }

    string T(string key, string? fallback = null);

    void SetLocale(string locale);

    event EventHandler? LocaleChanged;
}
