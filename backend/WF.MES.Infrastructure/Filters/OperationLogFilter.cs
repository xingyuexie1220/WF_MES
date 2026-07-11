using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Domain.Entities;

namespace WF.MES.Infrastructure.Filters;

public class OperationLogFilter(IServiceScopeFactory scopeFactory, ILogger<OperationLogFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;

        if (ShouldSkip(path, method))
        {
            await next();
            return;
        }

        var sw = Stopwatch.StartNew();
        var executed = await next();
        sw.Stop();

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

            string? operParam = null;
            if (context.ActionArguments.Count > 0)
            {
                operParam = JsonSerializer.Serialize(context.ActionArguments);
                if (operParam.Length > 4000)
                {
                    operParam = operParam[..4000];
                }
            }

            var success = executed.Exception is null && httpContext.Response.StatusCode < 400;
            var log = new SysOperationLog
            {
                Module = ResolveModule(path),
                Title = context.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
                    ? action
                    : context.ActionDescriptor.DisplayName,
                BusinessType = ResolveBusinessType(method, path),
                Method = context.ActionDescriptor.DisplayName,
                RequestMethod = method,
                OperUrl = path,
                OperIp = httpContext.Connection.RemoteIpAddress?.ToString(),
                OperParam = operParam,
                Status = success ? 1 : 0,
                ErrorMsg = executed.Exception?.Message,
                OperUserId = currentUser.UserId,
                OperUserName = currentUser.UserName,
                OperTime = DateTime.Now,
                CostTime = sw.ElapsedMilliseconds
            };

            await db.Insertable(log).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write operation log");
        }
    }

    private static bool ShouldSkip(string path, string method)
    {
        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.Contains("/system/logs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.Contains("/hubs/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static string ResolveModule(string path)
    {
        if (path.Contains("/system/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 3 ? $"system/{segments[2]}" : "system";
        }

        return "api";
    }

    private static string ResolveBusinessType(string method, string path)
    {
        if (path.Contains("/delete/", StringComparison.OrdinalIgnoreCase))
        {
            return "delete";
        }

        if (path.Contains("/update/", StringComparison.OrdinalIgnoreCase))
        {
            return "edit";
        }

        if (path.Contains("/publish/", StringComparison.OrdinalIgnoreCase))
        {
            return "publish";
        }

        if (path.Contains("/export", StringComparison.OrdinalIgnoreCase))
        {
            return "export";
        }

        if (path.Contains("/clear", StringComparison.OrdinalIgnoreCase))
        {
            return "clear";
        }

        if (path.Contains("/run/", StringComparison.OrdinalIgnoreCase))
        {
            return "run";
        }

        return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ? "add" : method.ToLowerInvariant();
    }
}
