using System.Net.Http.Headers;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Api;

/// <summary>
/// 附加 <c>Accept-Language</c>；除登录/选厂外附加 Bearer 与 <c>X-Factory-Id</c>。
/// </summary>
public sealed class AuthTokenHandler(IApiTokenStore tokenStore, ILocalizationService localization) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("Accept-Language");
        request.Headers.TryAddWithoutValidation("Accept-Language", localization.CurrentLocale);

        if (IsAnonymousAuthPath(request.RequestUri))
        {
            return base.SendAsync(request, cancellationToken);
        }

        var token = tokenStore.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (tokenStore.FactoryId is > 0)
        {
            request.Headers.Remove("X-Factory-Id");
            request.Headers.TryAddWithoutValidation("X-Factory-Id", tokenStore.FactoryId.Value.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool IsAnonymousAuthPath(Uri? requestUri)
    {
        var path = requestUri?.AbsolutePath;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        return path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/auth/select-factory", StringComparison.OrdinalIgnoreCase);
    }
}
