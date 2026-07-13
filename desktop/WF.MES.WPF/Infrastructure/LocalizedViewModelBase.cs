using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;

namespace WF.MES.WPF.Infrastructure;

/// <summary>订阅语言切换并刷新绑定文案的 ViewModel 基类。标准动态文案入口：<c>L</c> / <c>TF</c> / <c>EX</c>。</summary>
public abstract class LocalizedViewModelBase : BindableBase
{
    private readonly ILocalizationService _localization;

    protected LocalizedViewModelBase(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LocaleChanged += OnLocaleChanged;
    }

    protected ILocalizationService Localization => _localization;

    protected string L(string key, string? fallback = null) => _localization.T(key, fallback);

    protected string TF(string key, params object[] args) => string.Format(L(key), args);

    protected string EX(Exception ex) => BusinessMessageResolver.Resolve(Localization, ex);

    private void OnLocaleChanged(object? sender, EventArgs e) => RefreshLocalizedProperties();

    protected virtual void RefreshLocalizedProperties()
    {
    }
}
