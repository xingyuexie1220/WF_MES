using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Localization;

/// <summary>将 <see cref="BusinessException.MessageCode"/> 解析为当前语言文案。</summary>
public static class BusinessMessageResolver
{
    public static string Resolve(
        ILocalizationService localization,
        Exception exception,
        string fallbackKey = "common.requestFailed")
    {
        if (TryGetBusiness(exception, out var business))
        {
            return Format(localization, business.MessageCode, business.FormatArgs);
        }

        if (!string.IsNullOrWhiteSpace(exception.Message))
        {
            return exception.Message;
        }

        return localization.T(fallbackKey);
    }

    private static bool TryGetBusiness(Exception exception, out BusinessException business)
    {
        if (exception is BusinessException direct)
        {
            business = direct;
            return true;
        }

        if (exception.InnerException is BusinessException inner)
        {
            business = inner;
            return true;
        }

        business = null!;
        return false;
    }

    private static string Format(ILocalizationService localization, string messageCode, object?[] args)
    {
        var template = localization.T(messageCode, "");
        if (string.IsNullOrWhiteSpace(template))
        {
            return messageCode;
        }

        return args.Length > 0 ? string.Format(template, args) : template;
    }
}
