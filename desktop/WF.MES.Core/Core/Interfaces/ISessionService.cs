using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>当前登录用户、菜单权限（进程内内存态）。</summary>
public interface ISessionService
{
    UserInfoDto? CurrentUser { get; }

    IReadOnlyList<ModuleMenuPermissionDto> PermittedModules { get; }

    /// <summary>当前用户 API 权限码（含按钮 Permission）。</summary>
    IReadOnlySet<string> PermittedActionCodes { get; }

    string? CurrentOperatorName { get; }

    void SetUser(UserInfoDto user);

    void SetPermissions(IReadOnlyList<ModuleMenuPermissionDto> modules);

    void SetActionPermissions(IReadOnlySet<string> actionCodes);

    void Clear();
}
