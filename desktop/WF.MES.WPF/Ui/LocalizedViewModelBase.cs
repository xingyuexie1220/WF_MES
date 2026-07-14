using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;

namespace WF.MES.WPF.Ui;

/// <summary>
/// ViewModel 基类：启动语言固定；登录页可切换并刷新绑定。
/// 动态文案用 <c>L</c>/<c>Lf</c>/<c>Ex</c>；下拉选项等在构造时用 <c>LocalizedOptions.*(L)</c> 构建。
/// </summary>
public abstract class LocalizedViewModelBase : BindableBase
{
    private readonly ILocalizationService _localization;

    protected LocalizedViewModelBase(ILocalizationService localization)
    {
        _localization = localization;
    }

    protected ILocalizationService Localization => _localization;

    protected string L(string key, string? fallback = null) => _localization.T(key, fallback);

    /// <summary>格式化文案。</summary>
    protected string Lf(string key, params object[] args) => string.Format(L(key), args);

    /// <summary>业务异常 → 本地化消息。</summary>
    protected string Ex(Exception ex) => BusinessMessageResolver.Resolve(Localization, ex);
}
