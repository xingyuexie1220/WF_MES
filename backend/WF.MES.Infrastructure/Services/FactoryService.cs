using SqlSugar;
using WF.MES.Application.Factories;
using WF.MES.Application.Factories.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class FactoryService(ISqlSugarClient db) : IFactoryService
{
    public async Task<List<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SysRegion>()
            .Where(r => !r.IsDeleted && r.Status == UserStatus.Enabled)
            .OrderBy(r => r.Sort)
            .Select(r => new RegionDto
            {
                Id = r.Id,
                RegionCode = r.RegionCode,
                RegionName = r.RegionName,
                Sort = r.Sort,
                Status = r.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FactoryDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SysFactory>()
            .LeftJoin<SysRegion>((f, r) => f.RegionId == r.Id)
            .Where((f, r) => !f.IsDeleted)
            .OrderBy((f, r) => f.Sort)
            .Select((f, r) => new FactoryDto
            {
                Id = f.Id,
                RegionId = f.RegionId,
                RegionName = r.RegionName,
                FactoryCode = f.FactoryCode,
                FactoryName = f.FactoryName,
                TimeZone = f.TimeZone,
                Sort = f.Sort,
                Status = f.Status,
                Remark = f.Remark,
                CreateTime = f.CreateTime
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FactoryDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SysFactory>()
            .LeftJoin<SysRegion>((f, r) => f.RegionId == r.Id)
            .Where((f, r) => f.Id == id && !f.IsDeleted)
            .Select((f, r) => new FactoryDto
            {
                Id = f.Id,
                RegionId = f.RegionId,
                RegionName = r.RegionName,
                FactoryCode = f.FactoryCode,
                FactoryName = f.FactoryName,
                TimeZone = f.TimeZone,
                Sort = f.Sort,
                Status = f.Status,
                Remark = f.Remark,
                CreateTime = f.CreateTime
            })
            .FirstAsync(cancellationToken);
    }

    public async Task<long> CreateAsync(CreateFactoryRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SysFactory>().AnyAsync(f => f.FactoryCode == request.FactoryCode && !f.IsDeleted, cancellationToken))
        {
            throw new BusinessException("工厂编码已存在", 400, WfMessageCodes.FactoryCodeExists);
        }

        var regionExists = await db.Queryable<SysRegion>().AnyAsync(r => r.Id == request.RegionId && !r.IsDeleted, cancellationToken);
        if (!regionExists)
        {
            throw new BusinessException("地区不存在", 404);
        }

        var factory = new SysFactory
        {
            RegionId = request.RegionId,
            FactoryCode = request.FactoryCode.Trim(),
            FactoryName = request.FactoryName.Trim(),
            TimeZone = request.TimeZone,
            Sort = request.Sort,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        return await db.Insertable(factory).ExecuteReturnBigIdentityAsync();
    }

    public async Task UpdateAsync(long id, UpdateFactoryRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factory = await db.Queryable<SysFactory>().FirstAsync(f => f.Id == id && !f.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工厂不存在", 404, WfMessageCodes.FactoryNotFound);

        if (await db.Queryable<SysFactory>().AnyAsync(f => f.FactoryCode == request.FactoryCode && f.Id != id && !f.IsDeleted, cancellationToken))
        {
            throw new BusinessException("工厂编码已存在", 400, WfMessageCodes.FactoryCodeExists);
        }

        factory.RegionId = request.RegionId;
        factory.FactoryCode = request.FactoryCode.Trim();
        factory.FactoryName = request.FactoryName.Trim();
        factory.TimeZone = request.TimeZone;
        factory.Sort = request.Sort;
        factory.Status = request.Status;
        factory.Remark = request.Remark;
        factory.UpdateBy = operatorId;
        factory.UpdateTime = DateTime.Now;

        await db.Updateable(factory).ExecuteCommandAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SysDept>().AnyAsync(d => d.FactoryId == id && !d.IsDeleted, cancellationToken))
        {
            throw new BusinessException("工厂下存在组织，不能删除");
        }

        var factory = await db.Queryable<SysFactory>().FirstAsync(f => f.Id == id && !f.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工厂不存在", 404, WfMessageCodes.FactoryNotFound);

        factory.IsDeleted = true;
        factory.UpdateBy = operatorId;
        factory.UpdateTime = DateTime.Now;
        await db.Updateable(factory).ExecuteCommandAsync(cancellationToken);
    }

    public async Task<List<FactorySummaryDto>> GetAccessibleFactoriesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var isAdmin = await db.Queryable<SysUserRole>()
            .InnerJoin<SysRole>((ur, r) => ur.RoleId == r.Id)
            .Where((ur, r) => ur.UserId == userId && r.RoleCode == "admin" && !r.IsDeleted)
            .AnyAsync(cancellationToken);

        if (isAdmin)
        {
            var user = await db.Queryable<SysUser>().FirstAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
            return await db.Queryable<SysFactory>()
                .Where(f => !f.IsDeleted && f.Status == UserStatus.Enabled)
                .OrderBy(f => f.Sort)
                .Select(f => new FactorySummaryDto
                {
                    Id = f.Id,
                    FactoryCode = f.FactoryCode,
                    FactoryName = f.FactoryName,
                    IsDefault = user != null && user.DefaultFactoryId == f.Id
                })
                .ToListAsync(cancellationToken);
        }

        return await db.Queryable<SysUserFactory>()
            .InnerJoin<SysFactory>((uf, f) => uf.FactoryId == f.Id)
            .Where((uf, f) => uf.UserId == userId && !f.IsDeleted && f.Status == UserStatus.Enabled)
            .OrderBy((uf, f) => f.Sort)
            .Select((uf, f) => new FactorySummaryDto
            {
                Id = f.Id,
                FactoryCode = f.FactoryCode,
                FactoryName = f.FactoryName,
                IsDefault = uf.IsDefault
            })
            .ToListAsync(cancellationToken);
    }

    public async Task EnsureUserCanAccessFactoryAsync(long userId, long factoryId, CancellationToken cancellationToken = default)
    {
        var factories = await GetAccessibleFactoriesAsync(userId, cancellationToken);
        if (factories.All(f => f.Id != factoryId))
        {
            throw new BusinessException("无权访问该工厂", 403, WfMessageCodes.AuthFactoryForbidden);
        }

        var factory = await db.Queryable<SysFactory>().FirstAsync(f => f.Id == factoryId && !f.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工厂不存在", 404, WfMessageCodes.FactoryNotFound);

        if (factory.Status != UserStatus.Enabled)
        {
            throw new BusinessException("工厂已禁用", 403);
        }
    }
}
