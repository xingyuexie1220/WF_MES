using Serilog;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Api;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services;

/// <summary>从 /auth/desktop-menus 构建侧栏模组/菜单树（保留 I18nKey，标题在 UI 渲染时翻译）。</summary>
public sealed class ApiMenuPermissionService(IAuthApi authApi) : IMenuPermissionService
{
    private const int DesktopRootMenuId = 300;
    private const int MenuTypeDirectory = 1;
    private const int MenuTypeMenu = 2;

    public async Task<IReadOnlyList<ModuleMenuPermissionDto>> GetUserPermissionsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var response = await authApi.GetDesktopMenusAsync(cancellationToken);
        var tree = ApiResponseHelper.EnsureData(response, "err.loadMenusFailed");
        var modules = MapToModules(tree);

        Log.Information("用户 {UserId} 加载 API 菜单：{ModuleCount} 个模组，{MenuCount} 个页面",
            userId, modules.Count, modules.Sum(m => m.Menus.Count));

        return modules;
    }

    private static List<ModuleMenuPermissionDto> MapToModules(IReadOnlyList<ClientMenuDto> tree)
    {
        if (tree.Count == 0)
        {
            return [];
        }

        var desktopRoot = tree.FirstOrDefault(node => node.Id == DesktopRootMenuId) ?? tree[0];
        var moduleDirs = desktopRoot.Children
            .Where(node => node.MenuType == MenuTypeDirectory && node.Visible)
            .OrderBy(node => node.Sort);

        return moduleDirs
            .Select(dir => new ModuleMenuPermissionDto
            {
                ModuleId = (int)dir.Id,
                I18nKey = dir.I18nKey,
                TitleFallback = dir.Title,
                Icon = dir.Icon,
                Menus = dir.Children
                    .Where(menu => menu.MenuType == MenuTypeMenu
                                   && menu.Visible
                                   && !string.IsNullOrWhiteSpace(menu.Component))
                    .OrderBy(menu => menu.Sort)
                    .Select(menu => new MenuPermissionDto
                    {
                        MenuId = (int)menu.Id,
                        ModuleId = (int)dir.Id,
                        I18nKey = menu.I18nKey,
                        TitleFallback = menu.Title,
                        ViewName = menu.Component!
                    })
                    .ToList()
            })
            .Where(module => module.Menus.Count > 0)
            .ToList();
    }
}
