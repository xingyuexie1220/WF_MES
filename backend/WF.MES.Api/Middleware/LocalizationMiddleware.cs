using System.Globalization;

namespace WF.MES.Api.Middleware;

public class LocalizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var language = context.Request.Headers.AcceptLanguage.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        if (!string.IsNullOrWhiteSpace(language))
        {
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(language);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                // ignore invalid culture
            }
        }

        await next(context);
    }
}
