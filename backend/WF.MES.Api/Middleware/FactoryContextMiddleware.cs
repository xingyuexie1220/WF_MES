using Microsoft.AspNetCore.Authorization;
using WF.MES.Application.Common;
using WF.MES.Infrastructure.Security;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Api.Middleware;

/// <summary>解析当前工厂上下文，校验 JWT 与 Header 一致。</summary>
public class FactoryContextMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> BypassPaths =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/refresh",
        "/api/v1/auth/select-factory"
    ];

    public async Task InvokeAsync(HttpContext context, IFactoryContext factoryContext, ICurrentUserService currentUser)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (BypassPaths.Any(p => path.StartsWith(p, StringComparison.Ordinal)))
        {
            await next(context);
            return;
        }

        if (!currentUser.IsAuthenticated)
        {
            await next(context);
            return;
        }

        if (path.StartsWith("/api/v1/system/factories", StringComparison.Ordinal)
            || path.StartsWith("/api/v1/system/regions", StringComparison.Ordinal))
        {
            factoryContext.DisableFilter();
            await next(context);
            return;
        }

        var claimFactoryId = currentUser.FactoryId;
        if (!claimFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400, WfMessageCodes.AuthFactoryRequired);
        }

        if (context.Request.Headers.TryGetValue("X-Factory-Id", out var headerValue)
            && long.TryParse(headerValue.FirstOrDefault(), out var headerFactoryId)
            && headerFactoryId != claimFactoryId.Value)
        {
            throw new BusinessException("工厂上下文不一致", 403, WfMessageCodes.AuthFactoryForbidden);
        }

        var factoryCode = context.User.FindFirst("wf:factory_code")?.Value;
        factoryContext.SetFactory(claimFactoryId.Value, factoryCode);

        await next(context);
    }
}
