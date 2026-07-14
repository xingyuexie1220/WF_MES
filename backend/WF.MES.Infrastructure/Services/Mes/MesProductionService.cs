using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Application.Production;
using WF.MES.Application.Production.Dtos;
using WF.MES.Domain.Entities.Mes;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services.Mes;

public class MesProductionService(
    ISqlSugarClient db,
    IFactoryContext factoryContext) : IMesProductionService
{
    private long RequireFactoryId()
        => factoryContext.CurrentFactoryId ?? throw new BusinessException("请先选择工厂", 400);

    public async Task<List<MesWorkOrderDto>> GetWorkOrdersAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = db.Queryable<MesWorkOrder>().Where(w => !w.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.Status == status);

        var orders = await query.OrderByDescending(w => w.Id).ToListAsync(cancellationToken);
        var result = new List<MesWorkOrderDto>();
        foreach (var order in orders)
        {
            result.Add(await MapWorkOrderAsync(order, cancellationToken));
        }
        return result;
    }

    public async Task<MesWorkOrderDto?> GetWorkOrderByNoAsync(string workOrderNo, CancellationToken cancellationToken = default)
    {
        var orders = await db.Queryable<MesWorkOrder>()
            .Where(w => w.WorkOrderNo == workOrderNo.Trim() && !w.IsDeleted)
            .Take(1)
            .ToListAsync(cancellationToken);
        var order = orders.FirstOrDefault();
        return order is null ? null : await MapWorkOrderAsync(order, cancellationToken);
    }

    private async Task<MesWorkOrderDto> MapWorkOrderAsync(MesWorkOrder order, CancellationToken cancellationToken)
    {
        var materialName = (await db.Queryable<MesMaterial>()
            .Where(m => m.MaterialNo == order.MaterialNo && !m.IsDeleted)
            .Select(m => m.MaterialName)
            .Take(1)
            .ToListAsync(cancellationToken)).FirstOrDefault();

        string? routingName = null;
        List<MesRoutingStep> steps = [];
        if (order.RoutingId > 0)
        {
            routingName = (await db.Queryable<MesRouting>()
                .Where(r => r.Id == order.RoutingId && !r.IsDeleted)
                .Select(r => r.RoutingName)
                .Take(1)
                .ToListAsync(cancellationToken)).FirstOrDefault();
            steps = await db.Queryable<MesRoutingStep>()
                .Where(s => s.RoutingId == order.RoutingId && !s.IsDeleted)
                .OrderBy(s => s.Seq)
                .ToListAsync(cancellationToken);
        }

        var reports = await db.Queryable<MesReportRecord>()
            .Where(r => r.WorkOrderNo == order.WorkOrderNo && !r.IsDeleted && !r.IsVoided)
            .ToListAsync(cancellationToken);

        var processNames = await db.Queryable<MesProcess>()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);
        var nameMap = processNames.ToDictionary(p => p.ProcessCode, p => p.ProcessName);

        var progress = new List<MesProcessProgressDto>();
        int prevGood = order.PlanQty;
        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var good = reports.Where(r => r.ProcessCode == step.ProcessCode).Sum(r => r.GoodQty);
            var defect = reports.Where(r => r.ProcessCode == step.ProcessCode).Sum(r => r.DefectQty);
            var reported = good + defect;
            var remain = Math.Max(0, prevGood - reported);
            progress.Add(new MesProcessProgressDto
            {
                ProcessCode = step.ProcessCode,
                ProcessName = nameMap.GetValueOrDefault(step.ProcessCode),
                Seq = step.Seq,
                GoodQty = good,
                DefectQty = defect,
                RemainQty = remain
            });
            prevGood = good;
        }

        return new MesWorkOrderDto
        {
            WorkOrderId = order.Id,
            WorkOrderNo = order.WorkOrderNo,
            MaterialNo = order.MaterialNo,
            MaterialName = materialName,
            RoutingId = order.RoutingId,
            RoutingName = routingName,
            PlanQty = order.PlanQty,
            DueDate = order.DueDate,
            Status = order.Status,
            Source = order.Source,
            Progress = progress
        };
    }

    public async Task<long> SaveWorkOrderAsync(SaveMesWorkOrderRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();
        if (string.IsNullOrWhiteSpace(request.WorkOrderNo) || string.IsNullOrWhiteSpace(request.MaterialNo))
            throw new BusinessException("工单号和物料编码不能为空");
        if (request.PlanQty <= 0)
            throw new BusinessException("计划数量必须大于 0");

        long? routingId = request.RoutingId;
        if (!routingId.HasValue || routingId <= 0)
        {
            var defaultRouting = (await db.Queryable<MesRouting>()
                .Where(r => !r.IsDeleted && r.Enabled)
                .Where(r => r.MaterialNo == request.MaterialNo.Trim() || r.MaterialNo == null || r.MaterialNo == "")
                .OrderBy(r => r.Id)
                .Take(1)
                .ToListAsync(cancellationToken)).FirstOrDefault();
            routingId = defaultRouting?.Id;
        }

        if (!routingId.HasValue)
            throw new BusinessException("请先在 Web 维护工艺路线并绑定到工单");

        var routingIdValue = routingId.Value;
        var dueDate = request.DueDate ?? DateTime.Today.AddDays(7);
        var now = DateTime.Now;

        if (request.Id > 0)
        {
            var entity = await db.Queryable<MesWorkOrder>().FirstAsync(w => w.Id == request.Id && !w.IsDeleted, cancellationToken)
                ?? throw new BusinessException("工单不存在", 404);
            if (entity.Status == "closed")
                throw new BusinessException("已关闭工单不可修改");
            entity.MaterialNo = request.MaterialNo.Trim();
            entity.RoutingId = routingIdValue;
            entity.PlanQty = request.PlanQty;
            entity.DueDate = dueDate;
            entity.Remark = request.Remark;
            entity.SyncedAt = now;
            entity.UpdateBy = operatorId;
            entity.UpdateTime = now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return entity.Id;
        }

        if (await db.Queryable<MesWorkOrder>().AnyAsync(w => w.WorkOrderNo == request.WorkOrderNo.Trim() && !w.IsDeleted, cancellationToken))
            throw new BusinessException("工单号已存在");

        return await db.Insertable(new MesWorkOrder
        {
            FactoryId = factoryId,
            WorkOrderNo = request.WorkOrderNo.Trim(),
            MaterialNo = request.MaterialNo.Trim(),
            RoutingId = routingIdValue,
            PlanQty = request.PlanQty,
            DueDate = dueDate,
            Status = "open",
            Source = "local",
            SyncedAt = now,
            Remark = request.Remark,
            CreateBy = operatorId,
            CreateTime = now
        }).ExecuteReturnBigIdentityAsync(cancellationToken);
    }

    public async Task CloseWorkOrderAsync(long id, string? remark, long operatorId, CancellationToken cancellationToken = default)
    {
        var entity = await db.Queryable<MesWorkOrder>().FirstAsync(w => w.Id == id && !w.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工单不存在", 404);
        entity.Status = "closed";
        if (!string.IsNullOrWhiteSpace(remark))
            entity.Remark = remark;
        entity.UpdateBy = operatorId;
        entity.UpdateTime = DateTime.Now;
        await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
    }

    public async Task<MesReportResultDto> SubmitReportAsync(
        MesReportRequest request,
        string operatorName,
        long operatorId,
        CancellationToken cancellationToken = default)
    {
        var factoryId = RequireFactoryId();

        var workOrderNo = (request.WorkOrderNo ?? request.Barcode ?? string.Empty).Trim();
        var processCode = (request.ProcessCode ?? request.StationCode ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(workOrderNo))
            throw new BusinessException("请扫描或输入工单号");

        var order = await db.Queryable<MesWorkOrder>()
            .FirstAsync(w => w.WorkOrderNo == workOrderNo && !w.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工单不存在", 404);
        if (order.Status != "open")
            throw new BusinessException("工单已关闭，无法报工");
        if (order.RoutingId <= 0)
            throw new BusinessException("工单未绑定工艺路线");

        var dto = await MapWorkOrderAsync(order, cancellationToken);

        // 兼容旧简单过站：只传 barcode 时，自动报第一道有余额的工序良品 1
        if (string.IsNullOrWhiteSpace(processCode)
            && !string.IsNullOrWhiteSpace(request.Barcode)
            && request.GoodQty == 0 && request.DefectQty == 0)
        {
            var auto = dto.Progress.FirstOrDefault(p => p.RemainQty > 0)
                ?? throw new BusinessException("本工单已无可报工序");
            processCode = auto.ProcessCode;
            request.GoodQty = 1;
        }

        if (string.IsNullOrWhiteSpace(processCode))
            throw new BusinessException("请选择工序");
        if (request.GoodQty < 0 || request.DefectQty < 0 || request.GoodQty + request.DefectQty <= 0)
            throw new BusinessException("良品/不良数量不合法");
        if (request.DefectQty > 0 && string.IsNullOrWhiteSpace(request.DefectCode))
            throw new BusinessException("有不良时请选择不良原因");

        var progress = dto.Progress.FirstOrDefault(p => p.ProcessCode == processCode)
            ?? throw new BusinessException("该工序不在本工单工艺中");
        var submitQty = request.GoodQty + request.DefectQty;
        if (submitQty > progress.RemainQty)
            throw new BusinessException($"超报：本工序剩余可报 {progress.RemainQty}，本次 {submitQty}");

        var entity = new MesReportRecord
        {
            FactoryId = factoryId,
            WorkOrderNo = workOrderNo,
            ProcessCode = processCode,
            GoodQty = request.GoodQty,
            DefectQty = request.DefectQty,
            DefectCode = request.DefectCode,
            Disposition = request.Disposition,
            ReworkToProcess = request.ReworkToProcess,
            MachineNo = request.MachineNo,
            OperatorName = operatorName,
            ReportTime = DateTime.Now,
            CreateBy = operatorId,
            CreateTime = DateTime.Now
        };
        var id = await db.Insertable(entity).ExecuteReturnBigIdentityAsync(cancellationToken);

        var refreshed = await MapWorkOrderAsync(order, cancellationToken);
        var remain = refreshed.Progress.FirstOrDefault(p => p.ProcessCode == processCode)?.RemainQty ?? 0;

        return new MesReportResultDto
        {
            Id = id,
            WorkOrderNo = workOrderNo,
            ProcessCode = processCode,
            GoodQty = request.GoodQty,
            DefectQty = request.DefectQty,
            RemainQty = remain,
            ReportTime = entity.ReportTime
        };
    }

    public async Task<List<MesReportRecordDto>> GetRecentReportsAsync(
        string? workOrderNo = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.Queryable<MesReportRecord>().Where(r => !r.IsDeleted && !r.IsVoided);
        if (!string.IsNullOrWhiteSpace(workOrderNo))
            query = query.Where(r => r.WorkOrderNo == workOrderNo.Trim());

        return await query.OrderByDescending(r => r.Id)
            .Take(Math.Clamp(take, 1, 200))
            .Select(r => new MesReportRecordDto
            {
                Id = r.Id,
                WorkOrderNo = r.WorkOrderNo,
                ProcessCode = r.ProcessCode,
                GoodQty = r.GoodQty,
                DefectQty = r.DefectQty,
                DefectCode = r.DefectCode,
                MachineNo = r.MachineNo,
                OperatorName = r.OperatorName,
                ReportTime = r.ReportTime
            })
            .ToListAsync(cancellationToken);
    }
}
