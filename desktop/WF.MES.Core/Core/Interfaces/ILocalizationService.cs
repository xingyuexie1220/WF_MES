namespace WF.MES.Core.Interfaces;

public sealed class LocaleOption
{
    public string Value { get; init; } = "zh-CN";
    public string LabelKey { get; init; } = "locale.zhCN";
}

/// <summary>
/// 桌面端 UI 文案包（desktop/WF.MES.WPF/i18n）。
/// 语言可在<strong>登录页</strong>切换并写入 <c>wf_locale.txt</c>；进入主界面后不再提供语言切换。
/// API 错误键与 i18n/api-codes 协议一致。
/// </summary>
public interface ILocalizationService
{
    string CurrentLocale { get; }

    IReadOnlyList<LocaleOption> LocaleOptions { get; }

    string T(string key, string? fallback = null);

    /// <summary>切换语言并持久化；登录页选择用，进入壳层后勿再调用。</summary>
    void SetLocale(string locale);

    event EventHandler? LocaleChanged;
}
