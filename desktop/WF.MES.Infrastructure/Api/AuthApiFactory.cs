using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Refit;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Api;

/// <summary>创建带 Token / 工厂 / Accept-Language 的 Refit <see cref="IAuthApi"/>。</summary>
public static class AuthApiFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IAuthApi Create(
        IConfiguration configuration,
        IApiTokenStore tokenStore,
        ILocalizationService localization)
    {
        var handler = new AuthTokenHandler(tokenStore, localization)
        {
            InnerHandler = ApiHttpClientFactory.CreatePrimaryHandler()
        };

        var httpClient = ApiHttpClientFactory.CreateClient(configuration, handler);
        return RestService.For<IAuthApi>(httpClient, new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions)
        });
    }
}
