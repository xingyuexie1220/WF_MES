using Microsoft.Extensions.Configuration;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Api;

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
            using var client = ApiHttpClientFactory.CreateClient(configuration);

            using var response = await client.GetAsync(
                "api/v1/auth/info",
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
