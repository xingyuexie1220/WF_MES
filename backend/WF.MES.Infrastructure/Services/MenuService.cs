using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Application.Menus;
using WF.MES.Application.Menus.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class MenuService(ISqlSugarClient db, Application.Common.ICacheService cache) : IMenuService
{
    public async Task<List<MenuDto>> GetTreeAsync(ClientType? clientType = null, CancellationToken cancellationToken = default)
    {
        var menus = await db.Queryable<SysMenu>()
            .Where(m => !m.IsDeleted)
            .WhereIF(clientType.HasValue, m => m.ClientType == clientType)
            .OrderBy(m => m.Sort)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                MenuName = m.MenuName,
                MenuType = m.MenuType,
                ClientType = m.ClientType,
                I18nKey = m.I18nKey,
                Path = m.Path,
                Component = m.Component,
                Permission = m.Permission,
                Icon = m.Icon,
                Sort = m.Sort,
                Visible = m.Visible,
                Status = m.Status,
                Remark = m.Remark,
                CreateTime = m.CreateTime,
                CreateBy = m.CreateBy,
                UpdateTime = m.UpdateTime,
                UpdateBy = m.UpdateBy
            })
            .ToListAsync(cancellationToken);

        await FillMenuAuditNamesAsync(menus, cancellationToken);

        if (clientType.HasValue)
        {
            return BuildTree(menus, 0);
        }

        var roots = menus.Where(m => m.ParentId == 0).OrderBy(m => m.Sort).ToList();
        return roots.Select(root =>
        {
            root.Children = BuildTree(menus, root.Id);
            return root;
        }).ToList();
    }

    public async Task<MenuDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var menu = await db.Queryable<SysMenu>()
            .Where(m => m.Id == id && !m.IsDeleted)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                MenuName = m.MenuName,
                MenuType = m.MenuType,
                ClientType = m.ClientType,
                I18nKey = m.I18nKey,
                Path = m.Path,
                Component = m.Component,
                Permission = m.Permission,
                Icon = m.Icon,
                Sort = m.Sort,
                Visible = m.Visible,
                Status = m.Status,
                Remark = m.Remark,
                CreateTime = m.CreateTime,
                CreateBy = m.CreateBy,
                UpdateTime = m.UpdateTime,
                UpdateBy = m.UpdateBy
            })
            .FirstAsync(cancellationToken);

        if (menu is null)
        {
            return null;
        }

        await FillMenuAuditNamesAsync([menu], cancellationToken);
        return menu;
    }

    public async Task<long> CreateAsync(CreateMenuRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        await ValidateParentClientTypeAsync(request.ParentId, request.ClientType, cancellationToken);

        var menu = new SysMenu
        {
            ParentId = request.ParentId,
            MenuName = request.MenuName,
            MenuType = request.MenuType,
            ClientType = request.ClientType,
            I18nKey = request.I18nKey,
            Path = request.Path,
            Component = request.Component,
            Permission = request.Permission,
            Icon = request.Icon,
            Sort = request.Sort,
            Visible = request.Visible,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        var id = await db.Insertable(menu).ExecuteReturnBigIdentityAsync();
        await cache.RemoveCategoryAsync("menu");
        return id;
    }

    public async Task UpdateAsync(long id, UpdateMenuRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (id == request.ParentId)
        {
            throw new BusinessException("上级菜单不能是自己");
        }

        await ValidateParentClientTypeAsync(request.ParentId, request.ClientType, cancellationToken);

        var menu = await db.Queryable<SysMenu>().FirstAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new BusinessException("菜单不存在", 404);

        menu.ParentId = request.ParentId;
        menu.MenuName = request.MenuName;
        menu.MenuType = request.MenuType;
        menu.ClientType = request.ClientType;
        menu.I18nKey = request.I18nKey;
        menu.Path = request.Path;
        menu.Component = request.Component;
        menu.Permission = request.Permission;
        menu.Icon = request.Icon;
        menu.Sort = request.Sort;
        menu.Visible = request.Visible;
        menu.Status = request.Status;
        menu.Remark = request.Remark;
        menu.UpdateBy = operatorId;
        menu.UpdateTime = DateTime.Now;

        await db.Updateable(menu).ExecuteCommandAsync();
        await cache.RemoveCategoryAsync("menu");
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SysMenu>().AnyAsync(m => m.ParentId == id && !m.IsDeleted))
        {
            throw new BusinessException("存在子菜单，不能删除");
        }

        var menu = await db.Queryable<SysMenu>().FirstAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new BusinessException("菜单不存在", 404);

        menu.IsDeleted = true;
        menu.UpdateBy = operatorId;
        menu.UpdateTime = DateTime.Now;
        await db.Updateable(menu).ExecuteCommandAsync();
        await cache.RemoveCategoryAsync("menu");
    }

    private async Task ValidateParentClientTypeAsync(long parentId, ClientType clientType, CancellationToken cancellationToken)
    {
        if (parentId <= 0)
        {
            return;
        }

        var parent = await db.Queryable<SysMenu>()
            .Where(m => m.Id == parentId && !m.IsDeleted)
            .Select(m => new { m.ClientType })
            .FirstAsync(cancellationToken)
            ?? throw new BusinessException("上级菜单不存在");

        if (parent.ClientType != clientType)
        {
            throw new BusinessException("上级菜单与当前终端类型不一致");
        }
    }

    private static List<MenuDto> BuildTree(List<MenuDto> menus, long parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m =>
            {
                m.Children = BuildTree(menus, m.Id);
                return m;
            })
            .ToList();
    }

    private async Task FillMenuAuditNamesAsync(List<MenuDto> menus, CancellationToken cancellationToken = default)
    {
        if (menus.Count == 0)
        {
            return;
        }

        var userIds = menus
            .SelectMany(m => new[] { m.CreateBy, m.UpdateBy })
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var operators = await db.Queryable<SysUser>()
            .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
            .Select(u => new { u.Id, u.NickName, u.UserName })
            .ToListAsync(cancellationToken);

        var nameMap = operators.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.NickName) ? u.UserName : u.NickName);

        foreach (var menu in menus)
        {
            if (menu.CreateBy.HasValue && nameMap.TryGetValue(menu.CreateBy.Value, out var createByName))
            {
                menu.CreateByName = createByName;
            }

            if (menu.UpdateBy.HasValue && nameMap.TryGetValue(menu.UpdateBy.Value, out var updateByName))
            {
                menu.UpdateByName = updateByName;
            }
        }
    }
}
