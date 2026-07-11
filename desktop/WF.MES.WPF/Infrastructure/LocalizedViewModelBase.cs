using WF.MES.Core.Interfaces;

namespace WF.MES.WPF.Infrastructure;

/// <summary>订阅语言切换并刷新绑定文案的 ViewModel 基类。</summary>
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

    private void OnLocaleChanged(object? sender, EventArgs e) => RefreshLocalizedProperties();

    protected virtual void RefreshLocalizedProperties()
    {
    }
}
