using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Application.MasterData;
using WF.MES.Application.MasterData.Dtos;
using WF.MES.Domain.Entities.Mes;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services.Mes;

public class MesMasterDataService(ISqlSugarClient db, IFactoryContext factoryContext) : IMesMasterDataService
{
    private long RequireFactoryId()
        => factoryContext.CurrentFactoryId ?? throw new BusinessException("请先选择工厂", 400);

    public async Task<List<MesProcessDto>> GetProcessesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<MesProcess>()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.DefaultSeq)
            .Select(p => new MesProcessDto
            {
                Id = p.Id,
                ProcessCode = p.ProcessCode,
                ProcessName = p.ProcessName,
                DefaultSeq = p.DefaultSeq,
                Enabled = p.Enabled,
                Remark = p.Remark
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<long> SaveProcessAsync(SaveMesProcessRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();
        if (string.IsNullOrWhiteSpace(request.ProcessCode) || string.IsNullOrWhiteSpace(request.ProcessName))
            throw new BusinessException("工序编码和名称不能为空");

        if (request.Id > 0)
        {
            var entity = await db.Queryable<MesProcess>().FirstAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
                ?? throw new BusinessException("工序不存在", 404);
            entity.ProcessName = request.ProcessName.Trim();
            entity.DefaultSeq = request.DefaultSeq;
            entity.Enabled = request.Enabled;
            entity.Remark = request.Remark;
            entity.UpdateBy = operatorId;
            entity.UpdateTime = DateTime.Now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return entity.Id;
        }

        if (await db.Queryable<MesProcess>().AnyAsync(p => p.ProcessCode == request.ProcessCode.Trim() && !p.IsDeleted, cancellationToken))
            throw new BusinessException("工序编码已存在");

        return await db.Insertable(new MesProcess
        {
            FactoryId = factoryId,
            ProcessCode = request.ProcessCode.Trim(),
            ProcessName = request.ProcessName.Trim(),
            DefaultSeq = request.DefaultSeq,
            Enabled = request.Enabled,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        }).ExecuteReturnBigIdentityAsync(cancellationToken);
    }

    public async Task DeleteProcessAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<MesProcess>().FirstAsync(p => p.Id == id && !p.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工序不存在", 404);
        entity.IsDeleted = true;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
    }

    public async Task<List<MesRoutingDto>> GetRoutingsAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.Queryable<MesRouting>()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
        var result = new List<MesRoutingDto>();
        foreach (var r in list)
        {
            result.Add(await MapRoutingAsync(r, cancellationToken));
        }
        return result;
    }

    public async Task<MesRoutingDto?> GetRoutingAsync(long id, CancellationToken cancellationToken = default)
    {
        var list = await db.Queryable<MesRouting>()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Take(1)
            .ToListAsync(cancellationToken);
        var entity = list.FirstOrDefault();
        return entity is null ? null : await MapRoutingAsync(entity, cancellationToken);
    }

    private async Task<MesRoutingDto> MapRoutingAsync(MesRouting r, CancellationToken cancellationToken)
    {
        var steps = await db.Queryable<MesRoutingStep>()
            .LeftJoin<MesProcess>((s, p) => s.ProcessCode == p.ProcessCode && !p.IsDeleted)
            .Where((s, p) => s.RoutingId == r.Id && !s.IsDeleted)
            .OrderBy((s, p) => s.Seq)
            .Select((s, p) => new MesRoutingStepDto
            {
                ProcessCode = s.ProcessCode,
                ProcessName = p.ProcessName,
                Seq = s.Seq
            })
            .ToListAsync(cancellationToken);

        return new MesRoutingDto
        {
            Id = r.Id,
            RoutingCode = r.RoutingCode,
            RoutingName = r.RoutingName,
            MaterialNo = r.MaterialNo,
            Enabled = r.Enabled,
            Remark = r.Remark,
            Steps = steps
        };
    }

    public async Task<long> SaveRoutingAsync(SaveMesRoutingRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();
        if (string.IsNullOrWhiteSpace(request.RoutingCode) || string.IsNullOrWhiteSpace(request.RoutingName))
            throw new BusinessException("工艺编码和名称不能为空");
        if (request.Steps is null || request.Steps.Count == 0)
            throw new BusinessException("请至少配置一道工序");

        long routingId;
        if (request.Id > 0)
        {
            var entity = await db.Queryable<MesRouting>().FirstAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
                ?? throw new BusinessException("工艺不存在", 404);
            entity.RoutingName = request.RoutingName.Trim();
            entity.MaterialNo = request.MaterialNo;
            entity.Enabled = request.Enabled;
            entity.Remark = request.Remark;
            entity.UpdateBy = operatorId;
            entity.UpdateTime = DateTime.Now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            routingId = entity.Id;

            await db.Updateable<MesRoutingStep>()
                .SetColumns(s => new MesRoutingStep { IsDeleted = true, UpdateTime = DateTime.Now })
                .Where(s => s.RoutingId == routingId && !s.IsDeleted)
                .ExecuteCommandAsync(cancellationToken);
        }
        else
        {
            if (await db.Queryable<MesRouting>().AnyAsync(r => r.RoutingCode == request.RoutingCode.Trim() && !r.IsDeleted, cancellationToken))
                throw new BusinessException("工艺编码已存在");

            routingId = await db.Insertable(new MesRouting
            {
                FactoryId = factoryId,
                RoutingCode = request.RoutingCode.Trim(),
                RoutingName = request.RoutingName.Trim(),
                MaterialNo = request.MaterialNo,
                Enabled = request.Enabled,
                Remark = request.Remark,
                CreateBy = operatorId,
                CreateTime = DateTime.Now
            }).ExecuteReturnBigIdentityAsync(cancellationToken);
        }

        var steps = request.Steps
            .OrderBy(s => s.Seq)
            .Select(s => new MesRoutingStep
            {
                FactoryId = factoryId,
                RoutingId = routingId,
                ProcessCode = s.ProcessCode.Trim(),
                Seq = s.Seq,
                CreateBy = operatorId,
                CreateTime = DateTime.Now
            }).ToList();
        await db.Insertable(steps).ExecuteCommandAsync(cancellationToken);
        return routingId;
    }

    public async Task DeleteRoutingAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<MesRouting>().FirstAsync(r => r.Id == id && !r.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工艺不存在", 404);
        entity.IsDeleted = true;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
        await db.Updateable<MesRoutingStep>()
            .SetColumns(s => new MesRoutingStep { IsDeleted = true, UpdateTime = DateTime.Now })
            .Where(s => s.RoutingId == id && !s.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }

    public async Task<List<MesMaterialDto>> GetMaterialsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<MesMaterial>()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.MaterialNo)
            .Select(m => new MesMaterialDto
            {
                Id = m.Id,
                MaterialNo = m.MaterialNo,
                MaterialName = m.MaterialName,
                Spec = m.Spec,
                Unit = m.Unit,
                Source = m.Source,
                Enabled = m.Enabled
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<long> SaveMaterialAsync(SaveMesMaterialRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();
        if (string.IsNullOrWhiteSpace(request.MaterialNo) || string.IsNullOrWhiteSpace(request.MaterialName))
            throw new BusinessException("物料编码和名称不能为空");

        if (request.Id > 0)
        {
            var entity = await db.Queryable<MesMaterial>().FirstAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken)
                ?? throw new BusinessException("物料不存在", 404);
            entity.MaterialName = request.MaterialName.Trim();
            entity.Spec = request.Spec;
            entity.Unit = request.Unit;
            entity.Enabled = request.Enabled;
            entity.UpdateBy = operatorId;
            entity.UpdateTime = DateTime.Now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return entity.Id;
        }

        if (await db.Queryable<MesMaterial>().AnyAsync(m => m.MaterialNo == request.MaterialNo.Trim() && !m.IsDeleted, cancellationToken))
            throw new BusinessException("物料编码已存在");

        return await db.Insertable(new MesMaterial
        {
            FactoryId = factoryId,
            MaterialNo = request.MaterialNo.Trim(),
            MaterialName = request.MaterialName.Trim(),
            Spec = request.Spec,
            Unit = request.Unit,
            Source = "local",
            Enabled = request.Enabled,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        }).ExecuteReturnBigIdentityAsync(cancellationToken);
    }

    public async Task DeleteMaterialAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<MesMaterial>().FirstAsync(m => m.Id == id && !m.IsDeleted, cancellationToken)
            ?? throw new BusinessException("物料不存在", 404);
        entity.IsDeleted = true;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
    }

    public async Task<List<MesMachineDto>> GetMachinesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<MesMachine>()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.MachineNo)
            .Select(m => new MesMachineDto
            {
                Id = m.Id,
                MachineNo = m.MachineNo,
                MachineName = m.MachineName,
                Enabled = m.Enabled,
                Remark = m.Remark
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<long> SaveMachineAsync(SaveMesMachineRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();
        if (string.IsNullOrWhiteSpace(request.MachineNo) || string.IsNullOrWhiteSpace(request.MachineName))
            throw new BusinessException("机台编号和名称不能为空");

        if (request.Id > 0)
        {
            var entity = await db.Queryable<MesMachine>().FirstAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken)
                ?? throw new BusinessException("机台不存在", 404);
            entity.MachineName = request.MachineName.Trim();
            entity.Enabled = request.Enabled;
            entity.Remark = request.Remark;
            entity.UpdateBy = operatorId;
            entity.UpdateTime = DateTime.Now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return entity.Id;
        }

        if (await db.Queryable<MesMachine>().AnyAsync(m => m.MachineNo == request.MachineNo.Trim() && !m.IsDeleted, cancellationToken))
            throw new BusinessException("机台编号已存在");

        return await db.Insertable(new MesMachine
        {
            FactoryId = factoryId,
            MachineNo = request.MachineNo.Trim(),
            MachineName = request.MachineName.Trim(),
            Enabled = request.Enabled,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        }).ExecuteReturnBigIdentityAsync(cancellationToken);
    }

    public async Task DeleteMachineAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<MesMachine>().FirstAsync(m => m.Id == id && !m.IsDeleted, cancellationToken)
            ?? throw new BusinessException("机台不存在", 404);
        entity.IsDeleted = true;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
    }

    public async Task<List<MesDefectCodeDto>> GetDefectCodesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<MesDefectCode>()
            .Where(d => !d.IsDeleted && d.Enabled)
            .OrderBy(d => d.Sort)
            .Select(d => new MesDefectCodeDto
            {
                Id = d.Id,
                DefectCode = d.DefectCode,
                DefectName = d.DefectName,
                Sort = d.Sort,
                Enabled = d.Enabled
            })
            .ToListAsync(cancellationToken);
    }
}
