using SqlSugar;
using WF.MES.Application.Positions;
using WF.MES.Application.Positions.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Common;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class PositionService(ISqlSugarClient db) : IPositionService
{
    public async Task<PagedResult<PositionDto>> GetPagedListAsync(PositionQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await db.Queryable<SystemPosition>()
            .LeftJoin<SystemDept>((p, d) => p.DeptId == d.Id)
            .Where((p, d) => !p.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(request.PositionName), (p, d) => p.PositionName.Contains(request.PositionName!))
            .WhereIF(request.DeptId.HasValue, (p, d) => p.DeptId == request.DeptId)
            .WhereIF(request.Status.HasValue, (p, d) => p.Status == request.Status)
            .OrderBy((p, d) => p.Sort)
            .Select((p, d) => new PositionDto
            {
                Id = p.Id,
                PositionCode = p.PositionCode,
                PositionName = p.PositionName,
                ProcessCode = p.ProcessCode,
                DeptId = p.DeptId,
                DeptName = d.DeptName,
                Sort = p.Sort,
                Status = p.Status,
                Remark = p.Remark
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        return new PagedResult<PositionDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<PositionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SystemPosition>()
            .LeftJoin<SystemDept>((p, d) => p.DeptId == d.Id)
            .Where((p, d) => !p.IsDeleted && p.Status == UserStatus.Enabled)
            .OrderBy((p, d) => p.Sort)
            .Select((p, d) => new PositionDto
            {
                Id = p.Id,
                PositionCode = p.PositionCode,
                PositionName = p.PositionName,
                ProcessCode = p.ProcessCode,
                DeptId = p.DeptId,
                DeptName = d.DeptName,
                Sort = p.Sort,
                Status = p.Status,
                Remark = p.Remark
            })
            .ToListAsync();
    }

    public async Task<PositionDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SystemPosition>()
            .LeftJoin<SystemDept>((p, d) => p.DeptId == d.Id)
            .Where((p, d) => p.Id == id && !p.IsDeleted)
            .Select((p, d) => new PositionDto
            {
                Id = p.Id,
                PositionCode = p.PositionCode,
                PositionName = p.PositionName,
                ProcessCode = p.ProcessCode,
                DeptId = p.DeptId,
                DeptName = d.DeptName,
                Sort = p.Sort,
                Status = p.Status,
                Remark = p.Remark
            })
            .FirstAsync();
    }

    public async Task<long> CreateAsync(CreatePositionRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SystemPosition>().AnyAsync(p => p.PositionCode == request.PositionCode && !p.IsDeleted))
        {
            throw new BusinessException("岗位编码已存在");
        }

        var position = new SystemPosition
        {
            PositionCode = request.PositionCode,
            PositionName = request.PositionName,
            ProcessCode = request.ProcessCode,
            DeptId = request.DeptId,
            Sort = request.Sort,
            Status = request.Status,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };

        return await db.Insertable(position).ExecuteReturnBigIdentityAsync();
    }

    public async Task UpdateAsync(long id, UpdatePositionRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var position = await db.Queryable<SystemPosition>().FirstAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new BusinessException("岗位不存在", 404);

        position.PositionName = request.PositionName;
        position.ProcessCode = request.ProcessCode;
        position.DeptId = request.DeptId;
        position.Sort = request.Sort;
        position.Status = request.Status;
        position.Remark = request.Remark;
        position.UpdateBy = operatorId;
        position.UpdateTime = DateTime.Now;

        await db.Updateable(position).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        if (await db.Queryable<SystemUserPosition>().AnyAsync(up => up.PositionId == id))
        {
            throw new BusinessException("岗位已分配用户，不能删除");
        }

        var position = await db.Queryable<SystemPosition>().FirstAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new BusinessException("岗位不存在", 404);

        position.IsDeleted = true;
        position.UpdateBy = operatorId;
        position.UpdateTime = DateTime.Now;
        await db.Updateable(position).ExecuteCommandAsync();
    }
}
