using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;

namespace WF.MES.WPF.Infrastructure;

/// <summary>应用启动早期（Prism 容器就绪前）使用的本地化访问点。</summary>
public static class WpfLocalization
{
    private static readonly JsonLocalizationService Fallback = new();

    public static ILocalizationService Instance { get; private set; } = Fallback;

    public static string T(string key, string? fallback = null) => Instance.T(key, fallback);

    public static void Use(ILocalizationService localization) => Instance = localization;
}
