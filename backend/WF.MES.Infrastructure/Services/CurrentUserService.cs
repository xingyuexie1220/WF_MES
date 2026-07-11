using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WF.MES.Application.Common;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;

namespace WF.MES.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public long? UserId
    {
        get
        {
            var value = User?.FindFirstValue(WfClaimTypes.UserId);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirstValue(WfClaimTypes.UserName);

    public long? DeptId
    {
        get
        {
            var value = User?.FindFirstValue(WfClaimTypes.DeptId);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public ClientType? ClientType
    {
        get
        {
            var value = User?.FindFirstValue(WfClaimTypes.ClientType);
            return int.TryParse(value, out var type) && Enum.IsDefined(typeof(ClientType), type)
                ? (ClientType)type
                : null;
        }
    }

    public long? FactoryId
    {
        get
        {
            var value = User?.FindFirstValue(WfClaimTypes.FactoryId);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? SessionId => User?.FindFirstValue(WfClaimTypes.SessionId);
}
