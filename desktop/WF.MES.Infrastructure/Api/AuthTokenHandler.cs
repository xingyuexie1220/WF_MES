using System.Net.Http.Headers;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Api;

/// <summary>为 API 请求附加 Bearer Token、X-Factory-Id 与 Accept-Language（登录接口除外）。</summary>
public sealed class AuthTokenHandler(IApiTokenStore tokenStore, ILocalizationService localization) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("Accept-Language");
        request.Headers.TryAddWithoutValidation("Accept-Language", localization.CurrentLocale);

        if (request.RequestUri is not null
            && !request.RequestUri.AbsolutePath.Contains("/auth/login", StringComparison.OrdinalIgnoreCase)
            && !request.RequestUri.AbsolutePath.Contains("/auth/select-factory", StringComparison.OrdinalIgnoreCase))
        {
            var token = tokenStore.AccessToken;
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (tokenStore.FactoryId.HasValue && tokenStore.FactoryId.Value > 0)
            {
                request.Headers.Remove("X-Factory-Id");
                request.Headers.TryAddWithoutValidation("X-Factory-Id", tokenStore.FactoryId.Value.ToString());
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
