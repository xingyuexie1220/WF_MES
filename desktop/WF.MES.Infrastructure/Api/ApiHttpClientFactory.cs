using WF.MES.Core.Exceptions;
using Microsoft.Extensions.Configuration;

namespace WF.MES.Infrastructure.Api;

/// <summary>桌面端匿名/探测用 HttpClient 与主 Handler 工厂（证书、BaseUrl、超时）。</summary>
internal static class ApiHttpClientFactory
{
    public static HttpClientHandler CreatePrimaryHandler()
    {
        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;
#endif
        return handler;
    }

    public static HttpClient CreateClient(IConfiguration configuration, HttpMessageHandler? handler = null)
    {
        return new HttpClient(handler ?? CreatePrimaryHandler())
        {
            BaseAddress = new Uri(GetBaseUrl(configuration)),
            Timeout = GetTimeout(configuration)
        };
    }

    public static string GetBaseUrl(IConfiguration configuration)
    {
        var baseUrl = configuration["Api:BaseUrl"]
            ?? throw new BusinessException("err.apiBaseUrlNotConfigured");
        return baseUrl.TrimEnd('/') + "/";
    }

    public static TimeSpan GetTimeout(IConfiguration configuration)
        => TimeSpan.FromSeconds(configuration.GetValue("Api:TimeoutSeconds", 30));
}
