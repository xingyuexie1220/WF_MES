using Microsoft.Extensions.Options;
using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Application.Dicts;
using WF.MES.Application.Dicts.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Infrastructure.Options;
using WF.MES.Shared.Common;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class DictService(ISqlSugarClient db, Application.Common.ICacheService cache, IOptions<CacheOptions> cacheOptions) : IDictService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;
    public async Task<PagedResult<DictTypeDto>> GetTypePagedListAsync(DictTypeQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await db.Queryable<SysDictType>()
            .Where(t => !t.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(request.DictName), t => t.DictName.Contains(request.DictName!))
            .WhereIF(!string.IsNullOrWhiteSpace(request.DictType), t => t.DictType.Contains(request.DictType!))
            .WhereIF(request.Status.HasValue, t => t.Status == request.Status)
            .OrderBy(t => t.DictType)
            .Select(t => new DictTypeDto
            {
                Id = t.Id,
                DictName = t.DictName,
                DictType = t.DictType,
                Status = t.Status,
                Remark = t.Remark,
                CreateBy = t.CreateBy,
                CreateTime = t.CreateTime,
                UpdateBy = t.UpdateBy,
                UpdateTime = t.UpdateTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        await FillTypeAuditNamesAsync(items, cancellationToken);

        return new PagedResult<DictTypeDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<DictTypeDto>> GetAllTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Queryable<SysDictType>()
            .Where(t => !t.IsDeleted && t.Status == 1)
            .OrderBy(t => t.DictType)
            .Select(t => new DictTypeDto
            {
                Id = t.Id,
                DictName = t.DictName,
                DictType = t.DictType,
                Status = t.Status,
                Remark = t.Remark,
                CreateBy = t.CreateBy,
                CreateTime = t.CreateTime,
                UpdateBy = t.UpdateBy,
                UpdateTime = t.UpdateTime
            })
            .ToListAsync();

        await FillTypeAuditNamesAsync(items, cancellationToken);
        return items;
    }

    public async Task<DictTypeDto?> GetTypeByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await db.Queryable<SysDictType>()
            .Where(t => t.Id == id && !t.IsDeleted)
            .Select(t => new DictTypeDto
            {
                Id = t.Id,
                DictName = t.DictName,
                DictType = t.DictType,
                Status = t.Status,
                Remark = t.Remark,
                CreateBy = t.CreateBy,
                CreateTime = t.CreateTime,
                UpdateBy = t.UpdateBy,
                UpdateTime = t.UpdateTime
            })
            .FirstAsync();

        if (item is null)
        {
            return null;
        }

        await FillTypeAuditNamesAsync([item], cancellationToken);
        return item;
    }

    public async Task<long> CreateTypeAsync(CreateDictTypeRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var dictType = request.DictType.Trim();
        if (await db.Queryable<SysDictType>().AnyAsync(t => t.DictType == dictType && !t.IsDeleted))
        {
            throw new BusinessException("字典类型编码已存在");
        }

        var entity = new SysDictType
        {
            DictName = request.DictName.Trim(),
            DictType = dictType,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        var id = await db.Insertable(entity).ExecuteReturnIdentityAsync();
        await InvalidateDictCacheAsync(dictType);
        return id;
    }

    public async Task UpdateTypeAsync(long id, UpdateDictTypeRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<SysDictType>().FirstAsync(t => t.Id == id && !t.IsDeleted)
            ?? throw new BusinessException("字典类型不存在");

        var oldDictType = entity.DictType;
        entity.DictName = request.DictName.Trim();
        entity.Status = request.Status;
        entity.Remark = request.Remark;
        entity.UpdateBy = operatorId;
        entity.UpdateTime = DateTime.Now;

        await db.Updateable(entity).ExecuteCommandAsync();
        await InvalidateDictCacheAsync(oldDictType);
    }

    public async Task DeleteTypeAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<SysDictType>().FirstAsync(t => t.Id == id && !t.IsDeleted)
            ?? throw new BusinessException("字典类型不存在");

        if (await db.Queryable<SysDictData>().AnyAsync(d => d.DictTypeId == id && !d.IsDeleted))
        {
            throw new BusinessException("请先删除该类型下的字典数据");
        }

        entity.IsDeleted = true;
        entity.UpdateBy = operatorId;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync();
        await InvalidateDictCacheAsync(entity.DictType);
    }

    public async Task<PagedResult<DictDataDto>> GetDataPagedListAsync(DictDataQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await db.Queryable<SysDictData>()
            .Where(d => !d.IsDeleted)
            .WhereIF(request.DictTypeId.HasValue, d => d.DictTypeId == request.DictTypeId)
            .WhereIF(!string.IsNullOrWhiteSpace(request.DictType), d => d.DictType == request.DictType)
            .WhereIF(!string.IsNullOrWhiteSpace(request.DictLabel), d => d.DictLabel.Contains(request.DictLabel!))
            .WhereIF(request.Status.HasValue, d => d.Status == request.Status)
            .OrderBy(d => d.Sort)
            .Select(d => new DictDataDto
            {
                Id = d.Id,
                DictTypeId = d.DictTypeId,
                DictType = d.DictType,
                DictLabel = d.DictLabel,
                DictValue = d.DictValue,
                Sort = d.Sort,
                Status = d.Status,
                Remark = d.Remark,
                CreateBy = d.CreateBy,
                CreateTime = d.CreateTime,
                UpdateBy = d.UpdateBy,
                UpdateTime = d.UpdateTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        await FillDataAuditNamesAsync(items, cancellationToken);

        return new PagedResult<DictDataDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<DictDataDto>> GetDataByTypeAsync(string dictType, CancellationToken cancellationToken = default)
    {
        var cacheKey = dictType.Trim();
        var cached = await cache.GetAsync<List<DictDataDto>>("dict", cacheKey, cancellationToken: cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var items = await db.Queryable<SysDictData>()
            .Where(d => !d.IsDeleted && d.DictType == dictType && d.Status == 1)
            .OrderBy(d => d.Sort)
            .Select(d => new DictDataDto
            {
                Id = d.Id,
                DictTypeId = d.DictTypeId,
                DictType = d.DictType,
                DictLabel = d.DictLabel,
                DictValue = d.DictValue,
                Sort = d.Sort,
                Status = d.Status,
                Remark = d.Remark
            })
            .ToListAsync();

        await cache.SetAsync(
            "dict",
            cacheKey,
            items,
            TimeSpan.FromMinutes(_cacheOptions.DictTtlMinutes),
            cancellationToken: cancellationToken);

        return items;
    }

    public async Task<DictDataDto?> GetDataByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await db.Queryable<SysDictData>()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DictDataDto
            {
                Id = d.Id,
                DictTypeId = d.DictTypeId,
                DictType = d.DictType,
                DictLabel = d.DictLabel,
                DictValue = d.DictValue,
                Sort = d.Sort,
                Status = d.Status,
                Remark = d.Remark,
                CreateBy = d.CreateBy,
                CreateTime = d.CreateTime,
                UpdateBy = d.UpdateBy,
                UpdateTime = d.UpdateTime
            })
            .FirstAsync();

        if (item is null)
        {
            return null;
        }

        await FillDataAuditNamesAsync([item], cancellationToken);
        return item;
    }

    public async Task<long> CreateDataAsync(CreateDictDataRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var type = await db.Queryable<SysDictType>().FirstAsync(t => t.Id == request.DictTypeId && !t.IsDeleted)
            ?? throw new BusinessException("字典类型不存在");

        var entity = new SysDictData
        {
            DictTypeId = type.Id,
            DictType = type.DictType,
            DictLabel = request.DictLabel.Trim(),
            DictValue = request.DictValue.Trim(),
            Sort = request.Sort,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        var id = await db.Insertable(entity).ExecuteReturnIdentityAsync();
        await InvalidateDictCacheAsync(type.DictType);
        return id;
    }

    public async Task UpdateDataAsync(long id, UpdateDictDataRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<SysDictData>().FirstAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new BusinessException("字典数据不存在");

        entity.DictLabel = request.DictLabel.Trim();
        entity.DictValue = request.DictValue.Trim();
        entity.Sort = request.Sort;
        entity.Status = request.Status;
        entity.Remark = request.Remark;
        entity.UpdateBy = operatorId;
        entity.UpdateTime = DateTime.Now;

        await db.Updateable(entity).ExecuteCommandAsync();
        await InvalidateDictCacheAsync(entity.DictType);
    }

    public async Task DeleteDataAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<SysDictData>().FirstAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new BusinessException("字典数据不存在");

        entity.IsDeleted = true;
        entity.UpdateBy = operatorId;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync();
        await InvalidateDictCacheAsync(entity.DictType);
    }

    private async Task FillTypeAuditNamesAsync(List<DictTypeDto> items, CancellationToken cancellationToken)
    {
        var userIds = items
            .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var users = await db.Queryable<SysUser>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.NickName, u.UserName })
            .ToListAsync();

        var map = users.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.NickName) ? u.UserName : u.NickName);

        foreach (var item in items)
        {
            if (item.CreateBy.HasValue && map.TryGetValue(item.CreateBy.Value, out var createName))
            {
                item.CreateByName = createName;
            }

            if (item.UpdateBy.HasValue && map.TryGetValue(item.UpdateBy.Value, out var updateName))
            {
                item.UpdateByName = updateName;
            }
        }
    }

    private async Task FillDataAuditNamesAsync(List<DictDataDto> items, CancellationToken cancellationToken)
    {
        var userIds = items
            .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var users = await db.Queryable<SysUser>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.NickName, u.UserName })
            .ToListAsync();

        var map = users.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.NickName) ? u.UserName : u.NickName);

        foreach (var item in items)
        {
            if (item.CreateBy.HasValue && map.TryGetValue(item.CreateBy.Value, out var createName))
            {
                item.CreateByName = createName;
            }

            if (item.UpdateBy.HasValue && map.TryGetValue(item.UpdateBy.Value, out var updateName))
            {
                item.UpdateByName = updateName;
            }
        }
    }

    private Task InvalidateDictCacheAsync(string? dictType = null)
        => string.IsNullOrWhiteSpace(dictType)
            ? cache.RemoveCategoryAsync("dict")
            : cache.RemoveAsync("dict", dictType.Trim());
}
