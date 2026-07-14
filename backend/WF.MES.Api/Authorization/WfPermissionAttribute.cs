using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WF.MES.Application.Common;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class WfPermissionAttribute(string permission) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (context.HttpContext.User.IsInRole("admin"))
        {
            return;
        }

        var permissions = context.HttpContext.User.FindAll(WfClaimTypes.Permission).Select(c => c.Value).ToList();
        if (!permissions.Contains(permission))
        {
            context.Result = new ForbidResult();
        }
    }
}

public abstract class WfApiControllerBase : ControllerBase
{
    protected long GetOperatorId()
    {
        var currentUser = HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        return currentUser.UserId ?? throw new BusinessException("未登录", 401);
    }

    protected string GetOperatorName()
    {
        var currentUser = HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        return currentUser.UserName ?? "unknown";
    }
}
