using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Api;

/// <summary>将 API messageCode 解析为当前语言文案；message 仅作 fallback。</summary>
public static class ApiMessageResolver
{
    public static string Resolve(
        ILocalizationService localization,
        string? messageCode,
        string? message = null,
        string fallbackKey = "common.requestFailed")
    {
        if (!string.IsNullOrWhiteSpace(messageCode))
        {
            var translated = localization.T(messageCode, "");
            if (!string.IsNullOrWhiteSpace(translated))
            {
                return translated;
            }
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return localization.T(fallbackKey);
    }
}
