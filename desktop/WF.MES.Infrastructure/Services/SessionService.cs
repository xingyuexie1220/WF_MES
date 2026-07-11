using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services;

/// <summary>ISessionService 内存实现；登录成功后写入用户、菜单与操作权限。</summary>
public class SessionService : ISessionService
{
    public UserInfoDto? CurrentUser { get; private set; }

    public IReadOnlyList<ModuleMenuPermissionDto> PermittedModules { get; private set; } = [];

    public IReadOnlySet<string> PermittedActionCodes { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public string? CurrentOperatorName => CurrentUser?.NickName ?? CurrentUser?.UserName;

    public void SetUser(UserInfoDto user)
    {
        CurrentUser = user;
    }

    public void SetPermissions(IReadOnlyList<ModuleMenuPermissionDto> modules)
    {
        PermittedModules = modules;
    }

    public void SetActionPermissions(IReadOnlySet<string> actionCodes)
    {
        PermittedActionCodes = actionCodes;
    }

    public void Clear()
    {
        CurrentUser = null;
        PermittedModules = [];
        PermittedActionCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
