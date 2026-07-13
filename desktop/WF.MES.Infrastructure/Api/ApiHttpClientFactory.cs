using WF.MES.Core.Exceptions;
using Microsoft.Extensions.Configuration;

namespace WF.MES.Infrastructure.Api;

internal static class ApiHttpClientFactory
{
    public static HttpClientHandler CreateHandler()
    {
        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;
#endif
        return handler;
    }

    public static HttpClient CreateClient(IConfiguration configuration)
    {
        var baseUrl = configuration["Api:BaseUrl"]
            ?? throw new BusinessException("err.apiBaseUrlNotConfigured");

        return new HttpClient(CreateHandler())
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(configuration.GetValue("Api:TimeoutSeconds", 30))
        };
    }
}
