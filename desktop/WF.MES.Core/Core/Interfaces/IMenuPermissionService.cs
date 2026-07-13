using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>按用户角色加载可访问的模组与菜单（侧栏导航数据源）。</summary>
public interface IMenuPermissionService
{
    Task<IReadOnlyList<ModuleMenuPermissionDto>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}
