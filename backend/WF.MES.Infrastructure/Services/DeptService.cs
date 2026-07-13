using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Application.Depts;
using WF.MES.Application.Depts.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class DeptService(ISqlSugarClient db, IFactoryContext factoryContext) : IDeptService
{
    public async Task<List<DeptDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        if (!factoryContext.CurrentFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400);
        }

        var factoryId = factoryContext.CurrentFactoryId.Value;
        var depts = await db.Queryable<SystemDept>()
            .Where(d => !d.IsDeleted && d.FactoryId == factoryId)
            .OrderBy(d => d.Sort)
            .Select(d => new DeptDto
            {
                Id = d.Id,
                FactoryId = d.FactoryId,
                ParentId = d.ParentId,
                DeptCode = d.DeptCode,
                DeptName = d.DeptName,
                DeptType = d.DeptType,
                Sort = d.Sort,
                Status = d.Status,
                Remark = d.Remark
            })
            .ToListAsync(cancellationToken);

        return BuildTree(depts, 0);
    }

    public async Task<DeptDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var dept = await db.Queryable<SystemDept>()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DeptDto
            {
                Id = d.Id,
                FactoryId = d.FactoryId,
                ParentId = d.ParentId,
                DeptCode = d.DeptCode,
                DeptName = d.DeptName,
                DeptType = d.DeptType,
                Sort = d.Sort,
                Status = d.Status,
                Remark = d.Remark,
                CreateTime = d.CreateTime,
                CreateBy = d.CreateBy,
                UpdateTime = d.UpdateTime,
                UpdateBy = d.UpdateBy
            })
            .FirstAsync(cancellationToken);

        dept.CreateByName = await ResolveUserNameAsync(dept.CreateBy, cancellationToken);
        dept.UpdateByName = await ResolveUserNameAsync(dept.UpdateBy, cancellationToken);
        return dept;
    }

    private async Task<string?> ResolveUserNameAsync(long? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value <= 0)
        {
            return null;
        }

        var users = await db.Queryable<SystemUser>()
            .Where(u => u.Id == userId.Value && !u.IsDeleted)
            .Select(u => new { u.NickName, u.UserName })
            .Take(1)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return null;
        }

        var user = users[0];
        return string.IsNullOrWhiteSpace(user.NickName) ? user.UserName : user.NickName;
    }

    public async Task<long> CreateAsync(CreateDeptRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (!factoryContext.CurrentFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400);
        }

        var factoryId = factoryContext.CurrentFactoryId.Value;

        if (await db.Queryable<SystemDept>().AnyAsync(d => d.FactoryId == factoryId && d.DeptCode == request.DeptCode && !d.IsDeleted, cancellationToken))
        {
            throw new BusinessException("部门编码已存在");
        }

        var parentType = await GetParentDeptTypeAsync(request.ParentId, factoryId, cancellationToken);
        ValidateOrgStructure(request.ParentId, request.DeptType, parentType);

        var dept = new SystemDept
        {
            FactoryId = factoryId,
            ParentId = request.ParentId,
            DeptCode = request.DeptCode,
            DeptName = request.DeptName,
            DeptType = request.DeptType,
            Sort = request.Sort,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        return await db.Insertable(dept).ExecuteReturnBigIdentityAsync();
    }

    public async Task UpdateAsync(long id, UpdateDeptRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (!factoryContext.CurrentFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400);
        }

        if (id == request.ParentId)
        {
            throw new BusinessException("上级部门不能是自己");
        }

        var factoryId = factoryContext.CurrentFactoryId.Value;
        var dept = await db.Queryable<SystemDept>().FirstAsync(d => d.Id == id && d.FactoryId == factoryId && !d.IsDeleted, cancellationToken)
            ?? throw new BusinessException("部门不存在", 404);

        if (await db.Queryable<SystemDept>().AnyAsync(d => d.FactoryId == factoryId && d.DeptCode == request.DeptCode && d.Id != id && !d.IsDeleted, cancellationToken))
        {
            throw new BusinessException("部门编码已存在");
        }

        var parentType = await GetParentDeptTypeAsync(request.ParentId, factoryId, cancellationToken);
        ValidateOrgStructure(request.ParentId, request.DeptType, parentType);

        dept.ParentId = request.ParentId;
        dept.DeptCode = request.DeptCode;
        dept.DeptName = request.DeptName;
        dept.DeptType = request.DeptType;
        dept.Sort = request.Sort;
        dept.Status = request.Status;
        dept.Remark = request.Remark;
        dept.UpdateBy = operatorId;
        dept.UpdateTime = DateTime.Now;

        await db.Updateable(dept).ExecuteCommandAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        if (!factoryContext.CurrentFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400);
        }

        var factoryId = factoryContext.CurrentFactoryId.Value;

        if (await db.Queryable<SystemDept>().AnyAsync(d => d.ParentId == id && !d.IsDeleted, cancellationToken))
        {
            throw new BusinessException("存在下级部门，不能删除");
        }

        if (await db.Queryable<SystemUser>().AnyAsync(u => u.DeptId == id && !u.IsDeleted, cancellationToken))
        {
            throw new BusinessException("部门下存在用户，不能删除");
        }

        var dept = await db.Queryable<SystemDept>().FirstAsync(d => d.Id == id && d.FactoryId == factoryId && !d.IsDeleted, cancellationToken)
            ?? throw new BusinessException("部门不存在", 404);

        dept.IsDeleted = true;
        dept.UpdateBy = operatorId;
        dept.UpdateTime = DateTime.Now;
        await db.Updateable(dept).ExecuteCommandAsync(cancellationToken);
    }

    private static List<DeptDto> BuildTree(List<DeptDto> depts, long parentId)
    {
        return depts
            .Where(d => d.ParentId == parentId)
            .Select(d =>
            {
                d.Children = BuildTree(depts, d.Id);
                return d;
            })
            .ToList();
    }

    private async Task<DeptType?> GetParentDeptTypeAsync(long parentId, long factoryId, CancellationToken cancellationToken)
    {
        if (parentId == 0)
        {
            return null;
        }

        return await db.Queryable<SystemDept>()
            .Where(d => d.Id == parentId && d.FactoryId == factoryId && !d.IsDeleted)
            .Select(d => d.DeptType)
            .FirstAsync(cancellationToken);
    }

    /// <summary>工厂内组织：车间 → 产线 → 班组</summary>
    private static void ValidateOrgStructure(long parentId, DeptType deptType, DeptType? parentType)
    {
        if (parentId == 0)
        {
            if (deptType != DeptType.Workshop)
            {
                throw new BusinessException("顶级节点必须为车间");
            }
            return;
        }

        if (parentType is null)
        {
            throw new BusinessException("上级组织不存在");
        }

        switch (parentType)
        {
            case DeptType.Workshop when deptType != DeptType.Line:
                throw new BusinessException("车间下只能创建产线");
            case DeptType.Line when deptType != DeptType.Team:
                throw new BusinessException("产线下只能创建班组");
            case DeptType.Team:
                throw new BusinessException("班组为最小组织单位，不能再创建下级");
            default:
                throw new BusinessException("不支持的组织层级");
        }
    }
}
