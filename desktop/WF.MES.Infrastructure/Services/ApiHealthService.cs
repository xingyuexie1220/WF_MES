using Microsoft.Extensions.Configuration;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Services;

/// <summary>探测 API 是否在线（匿名请求 /auth/info，401 亦视为可达）。</summary>
public sealed class ApiHealthService(IConfiguration configuration) : IApiHealthService
{
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Api:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return false;
        }

        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(configuration.GetValue("Api:TimeoutSeconds", 30))
            };

            using var response = await client.GetAsync(
                $"{baseUrl.TrimEnd('/')}/api/v1/auth/info",
                cancellationToken);

            return response.IsSuccessStatusCode
                   || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }
}
