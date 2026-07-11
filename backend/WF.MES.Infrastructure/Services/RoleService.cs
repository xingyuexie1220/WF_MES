using SqlSugar;
using WF.MES.Application.Roles;
using WF.MES.Application.Roles.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Common;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class RoleService(ISqlSugarClient db) : IRoleService
{
    public async Task<PagedResult<RoleDto>> GetPagedListAsync(RoleQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await db.Queryable<SysRole>()
            .Where(r => !r.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(request.RoleName), r => r.RoleName.Contains(request.RoleName!))
            .WhereIF(request.Status.HasValue, r => r.Status == request.Status)
            .OrderBy(r => r.Sort)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Sort = r.Sort,
                DataScope = r.DataScope,
                Status = r.Status,
                Remark = r.Remark,
                CreateBy = r.CreateBy,
                CreateTime = r.CreateTime,
                UpdateBy = r.UpdateBy,
                UpdateTime = r.UpdateTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        foreach (var item in items)
        {
            item.MenuIds = await db.Queryable<SysRoleMenu>().Where(x => x.RoleId == item.Id).Select(x => x.MenuId).ToListAsync();
            item.DeptIds = await db.Queryable<SysRoleDept>().Where(x => x.RoleId == item.Id).Select(x => x.DeptId).ToListAsync();
        }

        await FillRoleAuditNamesAsync(items, cancellationToken);

        return new PagedResult<RoleDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Queryable<SysRole>()
            .Where(r => !r.IsDeleted && r.Status == UserStatus.Enabled)
            .OrderBy(r => r.Sort)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Sort = r.Sort,
                DataScope = r.DataScope,
                Status = r.Status,
                Remark = r.Remark,
                CreateBy = r.CreateBy,
                CreateTime = r.CreateTime,
                UpdateBy = r.UpdateBy,
                UpdateTime = r.UpdateTime
            })
            .ToListAsync();

        await FillRoleAuditNamesAsync(items, cancellationToken);

        return items;
    }

    public async Task<RoleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await db.Queryable<SysRole>()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Sort = r.Sort,
                DataScope = r.DataScope,
                Status = r.Status,
                Remark = r.Remark,
                CreateBy = r.CreateBy,
                CreateTime = r.CreateTime,
                UpdateBy = r.UpdateBy,
                UpdateTime = r.UpdateTime
            })
            .FirstAsync();

        if (role is null)
        {
            return null;
        }

        role.MenuIds = await db.Queryable<SysRoleMenu>().Where(x => x.RoleId == id).Select(x => x.MenuId).ToListAsync();
        role.DeptIds = await db.Queryable<SysRoleDept>().Where(x => x.RoleId == id).Select(x => x.DeptId).ToListAsync();
        await FillRoleAuditNamesAsync([role], cancellationToken);
        return role;
    }

    public async Task<long> CreateAsync(CreateRoleRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SysRole>().AnyAsync(r => r.RoleCode == request.RoleCode && !r.IsDeleted))
        {
            throw new BusinessException("角色编码已存在");
        }

        var role = new SysRole
        {
            RoleCode = request.RoleCode,
            RoleName = request.RoleName,
            Sort = request.Sort,
            DataScope = request.DataScope,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        var roleId = await db.Insertable(role).ExecuteReturnBigIdentityAsync();
        await SaveRoleRelationsAsync(roleId, request.MenuIds, request.DeptIds);
        return roleId;
    }

    public async Task UpdateAsync(long id, UpdateRoleRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var role = await db.Queryable<SysRole>().FirstAsync(r => r.Id == id && !r.IsDeleted)
            ?? throw new BusinessException("角色不存在", 404);

        role.RoleName = request.RoleName;
        role.Sort = request.Sort;
        role.DataScope = request.DataScope;
        role.Status = request.Status;
        role.Remark = request.Remark;
        role.UpdateBy = operatorId;
        role.UpdateTime = DateTime.Now;

        await db.Updateable(role).ExecuteCommandAsync();
        await db.Deleteable<SysRoleMenu>().Where(x => x.RoleId == id).ExecuteCommandAsync();
        await db.Deleteable<SysRoleDept>().Where(x => x.RoleId == id).ExecuteCommandAsync();
        await SaveRoleRelationsAsync(id, request.MenuIds, request.DeptIds);
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var role = await db.Queryable<SysRole>().FirstAsync(r => r.Id == id && !r.IsDeleted)
            ?? throw new BusinessException("角色不存在", 404);

        if (role.RoleCode == "admin")
        {
            throw new BusinessException("不能删除系统管理员角色");
        }

        role.IsDeleted = true;
        role.UpdateBy = operatorId;
        role.UpdateTime = DateTime.Now;
        await db.Updateable(role).ExecuteCommandAsync();
    }

    private async Task SaveRoleRelationsAsync(long roleId, List<long> menuIds, List<long> deptIds)
    {
        if (menuIds.Count > 0)
        {
            var parentMap = await db.Queryable<SysMenu>()
                .Where(m => !m.IsDeleted && m.MenuType != MenuType.Button)
                .Select(m => new { m.Id, m.ParentId })
                .ToListAsync();

            var expandedMenuIds = MenuPermissionHelper.ExpandWithAncestors(menuIds, parentMap.ToDictionary(m => m.Id, m => m.ParentId));
            await db.Insertable(expandedMenuIds.Select(menuId => new SysRoleMenu { RoleId = roleId, MenuId = menuId }).ToList()).ExecuteCommandAsync();
        }

        if (deptIds.Count > 0)
        {
            await db.Insertable(deptIds.Select(deptId => new SysRoleDept { RoleId = roleId, DeptId = deptId }).ToList()).ExecuteCommandAsync();
        }
    }

    private async Task FillRoleAuditNamesAsync(List<RoleDto> roles, CancellationToken cancellationToken = default)
    {
        if (roles.Count == 0)
        {
            return;
        }

        var userIds = roles
            .SelectMany(r => new[] { r.CreateBy, r.UpdateBy })
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

        foreach (var role in roles)
        {
            if (role.CreateBy.HasValue && nameMap.TryGetValue(role.CreateBy.Value, out var createByName))
            {
                role.CreateByName = createByName;
            }

            if (role.UpdateBy.HasValue && nameMap.TryGetValue(role.UpdateBy.Value, out var updateByName))
            {
                role.UpdateByName = updateByName;
            }
        }
    }
}
