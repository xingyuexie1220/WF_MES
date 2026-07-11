using SqlSugar;
using WF.MES.Application.Users;
using WF.MES.Application.Users.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Infrastructure.Security;
using WF.MES.Shared.Common;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class UserService(ISqlSugarClient db) : IUserService
{
    public async Task<PagedResult<UserDto>> GetPagedListAsync(UserQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var query = db.Queryable<SysUser>()
            .LeftJoin<SysDept>((u, d) => u.DeptId == d.Id)
            .Where((u, d) => !u.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(request.UserName), (u, d) => u.UserName.Contains(request.UserName!))
            .WhereIF(request.Status.HasValue, (u, d) => u.Status == request.Status)
            .WhereIF(request.DeptId.HasValue, (u, d) => u.DeptId == request.DeptId)
            .OrderBy((u, d) => u.CreateTime, OrderByType.Desc);

        var items = await query.Select((u, d) => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            NickName = u.NickName,
            Email = u.Email,
            DeptId = u.DeptId,
            DeptName = d.DeptName,
            Status = u.Status,
            Remark = u.Remark,
            CreateTime = u.CreateTime,
            CreateBy = u.CreateBy,
            UpdateTime = u.UpdateTime,
            UpdateBy = u.UpdateBy
        }).ToPageListAsync(request.PageIndex, request.PageSize, total);

        foreach (var item in items)
        {
            item.RoleIds = await db.Queryable<SysUserRole>().Where(x => x.UserId == item.Id).Select(x => x.RoleId).ToListAsync();
            item.PositionIds = await db.Queryable<SysUserPosition>().Where(x => x.UserId == item.Id).Select(x => x.PositionId).ToListAsync();
        }

        await FillUserAuditNamesAsync(items, cancellationToken);

        return new PagedResult<UserDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<UserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SysUser>()
            .LeftJoin<SysDept>((u, d) => u.DeptId == d.Id)
            .Where((u, d) => u.Id == id && !u.IsDeleted)
            .Select((u, d) => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                NickName = u.NickName,
                Email = u.Email,
                DeptId = u.DeptId,
                DeptName = d.DeptName,
                Status = u.Status,
                Remark = u.Remark,
                CreateTime = u.CreateTime,
                CreateBy = u.CreateBy,
                UpdateTime = u.UpdateTime,
                UpdateBy = u.UpdateBy
            })
            .FirstAsync();

        if (user is null)
        {
            return null;
        }

        user.RoleIds = await db.Queryable<SysUserRole>().Where(x => x.UserId == id).Select(x => x.RoleId).ToListAsync();
        user.PositionIds = await db.Queryable<SysUserPosition>().Where(x => x.UserId == id).Select(x => x.PositionId).ToListAsync();
        await FillUserAuditNamesAsync([user], cancellationToken);
        return user;
    }

    public async Task<long> CreateAsync(CreateUserRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SysUser>().AnyAsync(u => u.UserName == request.UserName && !u.IsDeleted))
        {
            throw new BusinessException("用户名已存在");
        }

        var user = new SysUser
        {
            UserName = request.UserName,
            PasswordHash = PasswordHasher.Hash(request.Password),
            NickName = request.NickName,
            Email = request.Email,
            DeptId = request.DeptId,
            Status = request.Status,
            Remark = request.Remark,
            MustChangePassword = true,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        var userId = await db.Insertable(user).ExecuteReturnBigIdentityAsync();
        await SaveUserRelationsAsync(userId, request.RoleIds, request.PositionIds);
        return userId;
    }

    public async Task UpdateAsync(long id, UpdateUserRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SysUser>().FirstAsync(u => u.Id == id && !u.IsDeleted)
            ?? throw new BusinessException("用户不存在", 404);

        user.NickName = request.NickName;
        user.Email = request.Email;
        user.DeptId = request.DeptId;
        user.Status = request.Status;
        user.Remark = request.Remark;
        user.UpdateBy = operatorId;
        user.UpdateTime = DateTime.Now;

        await db.Updateable(user).ExecuteCommandAsync();
        await db.Deleteable<SysUserRole>().Where(x => x.UserId == id).ExecuteCommandAsync();
        await db.Deleteable<SysUserPosition>().Where(x => x.UserId == id).ExecuteCommandAsync();
        await SaveUserRelationsAsync(id, request.RoleIds, request.PositionIds);
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SysUser>().FirstAsync(u => u.Id == id && !u.IsDeleted)
            ?? throw new BusinessException("用户不存在", 404);

        if (user.UserName == "admin")
        {
            throw new BusinessException("不能删除超级管理员");
        }

        user.IsDeleted = true;
        user.UpdateBy = operatorId;
        user.UpdateTime = DateTime.Now;
        await db.Updateable(user).ExecuteCommandAsync();
    }

    public async Task ResetPasswordAsync(long id, ResetPasswordRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SysUser>().FirstAsync(u => u.Id == id && !u.IsDeleted)
            ?? throw new BusinessException("用户不存在", 404);

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.MustChangePassword = true;
        user.UpdateBy = operatorId;
        user.UpdateTime = DateTime.Now;
        await db.Updateable(user).UpdateColumns(u => new { u.PasswordHash, u.MustChangePassword, u.UpdateBy, u.UpdateTime }).ExecuteCommandAsync();
    }

    private async Task SaveUserRelationsAsync(long userId, List<long> roleIds, List<long> positionIds)
    {
        if (roleIds.Count > 0)
        {
            await db.Insertable(roleIds.Select(roleId => new SysUserRole { UserId = userId, RoleId = roleId }).ToList()).ExecuteCommandAsync();
        }

        if (positionIds.Count > 0)
        {
            await db.Insertable(positionIds.Select(positionId => new SysUserPosition { UserId = userId, PositionId = positionId }).ToList()).ExecuteCommandAsync();
        }
    }

    private async Task FillUserAuditNamesAsync(List<UserDto> users, CancellationToken cancellationToken = default)
    {
        if (users.Count == 0)
        {
            return;
        }

        var userIds = users
            .SelectMany(u => new[] { u.CreateBy, u.UpdateBy })
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

        foreach (var user in users)
        {
            if (user.CreateBy.HasValue && nameMap.TryGetValue(user.CreateBy.Value, out var createByName))
            {
                user.CreateByName = createByName;
            }

            if (user.UpdateBy.HasValue && nameMap.TryGetValue(user.UpdateBy.Value, out var updateByName))
            {
                user.UpdateByName = updateByName;
            }
        }
    }
}
