namespace WF.MES.Infrastructure.Api;

public static class ApiErrorHelper
{
    public static string ToUserMessage(Exception ex, string? apiBaseUrl = null)
    {
        var inner = ex;
        while (inner.InnerException is not null)
        {
            inner = inner.InnerException;
        }

        var message = inner.Message;
        if (inner is HttpRequestException or System.Net.Sockets.SocketException
            || message.Contains("积极拒绝", StringComparison.Ordinal)
            || message.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
            || message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase))
        {
            var target = string.IsNullOrWhiteSpace(apiBaseUrl) ? "API 服务" : $"API 服务 ({apiBaseUrl})";
            return $"无法连接 {target}，请确认后端已启动且地址端口正确";
        }

        if (message.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || message.Contains("超时", StringComparison.Ordinal))
        {
            return "连接 API 超时，请检查网络或增大 Api:TimeoutSeconds";
        }

        return string.IsNullOrWhiteSpace(message) ? "登录失败，请稍后重试" : message;
    }
}
