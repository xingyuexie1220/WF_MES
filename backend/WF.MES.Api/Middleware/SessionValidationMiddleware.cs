using Microsoft.AspNetCore.Authorization;
using WF.MES.Application.Common;
using WF.MES.Infrastructure.Security;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Api.Middleware;

/// <summary>校验 JWT 中的 session_id 是否为该用户当前端的有效会话（按端单设备）。</summary>
public class SessionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ISessionStore sessionStore, ICurrentUserService currentUser)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            await next(context);
            return;
        }

        var sessionId = currentUser.SessionId;
        var clientType = currentUser.ClientType;
        if (string.IsNullOrWhiteSpace(sessionId) || clientType is null)
        {
            await next(context);
            return;
        }

        var valid = await sessionStore.IsActiveSessionAsync(
            currentUser.UserId.Value,
            clientType.Value,
            sessionId,
            currentUser.FactoryId);
        if (!valid)
        {
            throw new BusinessException("账号已在其他设备登录", 401, WfMessageCodes.SessionReplaced);
        }

        await next(context);
    }
}
