using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Barcode;
using WF.MES.Application.Common;
using WF.MES.Application.Dashboard;
using WF.MES.Application.Equipment;
using WF.MES.Application.MasterData;
using WF.MES.Application.Production;
using WF.MES.Application.Warehouse;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/master-data")]
public class MasterDataController(IMesMasterDataService service) : WfApiControllerBase
{
    [HttpGet("materials")]
    [WfPermission("master:material:list")]
    public async Task<ApiResult<object>> GetMaterials()
        => ApiResult<object>.Ok(await service.GetMaterialsAsync());

    [HttpPost("materials")]
    [WfPermission("master:material:list")]
    public async Task<ApiResult<long>> SaveMaterial([FromBody] Application.MasterData.Dtos.SaveMesMaterialRequest request)
        => ApiResult<long>.Ok(await service.SaveMaterialAsync(request, GetOperatorId()));

    [HttpPost("materials/delete/{id:long}")]
    [WfPermission("master:material:list")]
    public async Task<ApiResult<object>> DeleteMaterial(long id)
    {
        await service.DeleteMaterialAsync(id);
        return ApiResult<object>.Ok(new { id });
    }

    [HttpGet("routes")]
    [WfPermission("master:route:list")]
    public async Task<ApiResult<object>> GetRoutes()
        => ApiResult<object>.Ok(await service.GetRoutingsAsync());

    [HttpGet("routes/{id:long}")]
    [WfPermission("master:route:list")]
    public async Task<ApiResult<object>> GetRoute(long id)
        => ApiResult<object>.Ok(await service.GetRoutingAsync(id));

    [HttpPost("routes")]
    [WfPermission("master:route:list")]
    public async Task<ApiResult<long>> SaveRoute([FromBody] Application.MasterData.Dtos.SaveMesRoutingRequest request)
        => ApiResult<long>.Ok(await service.SaveRoutingAsync(request, GetOperatorId()));

    [HttpPost("routes/delete/{id:long}")]
    [WfPermission("master:route:list")]
    public async Task<ApiResult<object>> DeleteRoute(long id)
    {
        await service.DeleteRoutingAsync(id);
        return ApiResult<object>.Ok(new { id });
    }

    /// <summary>工序（沿用 stations 路径）</summary>
    [HttpGet("stations")]
    [WfPermission("master:station:list")]
    public async Task<ApiResult<object>> GetStations()
        => ApiResult<object>.Ok(await service.GetProcessesAsync());

    [HttpPost("stations")]
    [WfPermission("master:station:list")]
    public async Task<ApiResult<long>> SaveStation([FromBody] Application.MasterData.Dtos.SaveMesProcessRequest request)
        => ApiResult<long>.Ok(await service.SaveProcessAsync(request, GetOperatorId()));

    [HttpPost("stations/delete/{id:long}")]
    [WfPermission("master:station:list")]
    public async Task<ApiResult<object>> DeleteStation(long id)
    {
        await service.DeleteProcessAsync(id);
        return ApiResult<object>.Ok(new { id });
    }

    /// <summary>机台（沿用 work-centers 路径）</summary>
    [HttpGet("work-centers")]
    [WfPermission("master:workcenter:list")]
    public async Task<ApiResult<object>> GetWorkCenters()
        => ApiResult<object>.Ok(await service.GetMachinesAsync());

    [HttpPost("work-centers")]
    [WfPermission("master:workcenter:list")]
    public async Task<ApiResult<long>> SaveWorkCenter([FromBody] Application.MasterData.Dtos.SaveMesMachineRequest request)
        => ApiResult<long>.Ok(await service.SaveMachineAsync(request, GetOperatorId()));

    [HttpPost("work-centers/delete/{id:long}")]
    [WfPermission("master:workcenter:list")]
    public async Task<ApiResult<object>> DeleteWorkCenter(long id)
    {
        await service.DeleteMachineAsync(id);
        return ApiResult<object>.Ok(new { id });
    }

    [HttpGet("defect-codes")]
    [WfPermission("master:station:list")]
    public async Task<ApiResult<object>> GetDefectCodes()
        => ApiResult<object>.Ok(await service.GetDefectCodesAsync());
}

[Authorize]
[ApiController]
[Route("api/v1/production")]
public class ProductionController(IMesProductionService service) : WfApiControllerBase
{
    [HttpGet("work-orders")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetWorkOrders([FromQuery] string? status = null)
        => ApiResult<object>.Ok(await service.GetWorkOrdersAsync(status));

    [HttpGet("work-orders/{workOrderNo}")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetWorkOrder(string workOrderNo)
        => ApiResult<object>.Ok(await service.GetWorkOrderByNoAsync(workOrderNo));

    [HttpPost("work-orders")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<long>> SaveWorkOrder([FromBody] Application.Production.Dtos.SaveMesWorkOrderRequest request)
        => ApiResult<long>.Ok(await service.SaveWorkOrderAsync(request, GetOperatorId()));

    [HttpPost("work-orders/{id:long}/close")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> CloseWorkOrder(long id, [FromBody] Application.Production.Dtos.SaveMesWorkOrderRequest? request)
    {
        await service.CloseWorkOrderAsync(id, request?.Remark, GetOperatorId());
        return ApiResult<object>.Ok(new { id });
    }

    [HttpPost("pass-records")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> PassStation([FromBody] Application.Production.Dtos.MesReportRequest request)
        => ApiResult<object>.Ok(await service.SubmitReportAsync(request, GetOperatorName(), GetOperatorId()));

    [HttpGet("pass-records")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetPassRecords([FromQuery] string? workOrderNo = null, [FromQuery] int take = 50)
        => ApiResult<object>.Ok(await service.GetRecentReportsAsync(workOrderNo, take));

    [HttpGet("machines")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetMachines([FromServices] IMesMasterDataService master)
        => ApiResult<object>.Ok(await master.GetMachinesAsync());

    [HttpGet("defect-codes")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetDefectCodes([FromServices] IMesMasterDataService master)
        => ApiResult<object>.Ok(await master.GetDefectCodesAsync());
}

[Authorize]
[ApiController]
[Route("api/v1/warehouse")]
public class WarehouseController(IWarehouseScaffoldService service) : WfApiControllerBase
{
    [HttpGet("inbound")]
    [WfPermission("mobile:warehouse:scan:list")]
    public async Task<ApiResult<object>> GetInbound() => ApiResult<object>.Ok(await service.GetInboundListAsync());

    [HttpPost("inbound/scan")]
    [WfPermission("mobile:warehouse:scan:submit")]
    public async Task<ApiResult<object>> SubmitScan([FromBody] object request) => ApiResult<object>.Ok(await service.SubmitScanAsync(request));
}

[Authorize]
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController(IDashboardScaffoldService service) : WfApiControllerBase
{
    [HttpGet("reports/overview")]
    [WfPermission("dashboard:report:view")]
    public async Task<ApiResult<object>> ReportOverview() => ApiResult<object>.Ok(await service.GetReportOverviewAsync());

    [HttpGet("bigscreen/overview")]
    [WfPermission("dashboard:bigscreen:view")]
    public async Task<ApiResult<object>> BigScreenOverview() => ApiResult<object>.Ok(await service.GetBigScreenOverviewAsync());
}

[Authorize]
[ApiController]
[Route("api/v1/equipment")]
public class EquipmentController(IEquipmentScaffoldService service) : WfApiControllerBase
{
    [HttpGet("test-records")]
    [WfPermission("equipment:test:submit")]
    public async Task<ApiResult<object>> GetTestRecords() => ApiResult<object>.Ok(await service.GetTestRecordsAsync());

    [HttpPost("test-records")]
    [WfPermission("equipment:test:submit")]
    public async Task<ApiResult<object>> SubmitTest([FromBody] object request) => ApiResult<object>.Ok(await service.SubmitTestAsync(request));
}

[Authorize]
[ApiController]
[Route("api/v1/barcode")]
public class BarcodeController(
    IBarcodeCustomerService customerService,
    IBarcodeScaffoldService barcodeScaffoldService,
    ICurrentUserService currentUser) : WfApiControllerBase
{
    [HttpGet("customers")]
    [WfPermission("barcode:customer:list")]
    public async Task<ApiResult<List<BarcodeCustomerDto>>> GetCustomers()
        => ApiResult<List<BarcodeCustomerDto>>.Ok(await customerService.GetListAsync());

    [HttpGet("customers/{id:int}")]
    [WfPermission("barcode:customer:list")]
    public async Task<ApiResult<BarcodeCustomerDto?>> GetCustomer(int id)
        => ApiResult<BarcodeCustomerDto?>.Ok(await customerService.GetByIdAsync(id));

    [HttpPost("customers")]
    [WfPermission("barcode:customer:list")]
    public async Task<ApiResult<int>> SaveCustomer([FromBody] SaveBarcodeCustomerRequest request)
        => ApiResult<int>.Ok(await customerService.SaveAsync(request, currentUser.UserName ?? "system"));

    [HttpGet("material-rules")]
    [WfPermission("barcode:rule:list")]
    public async Task<ApiResult<List<BarcodeMaterialRuleDto>>> GetMaterialRules()
        => ApiResult<List<BarcodeMaterialRuleDto>>.Ok(await barcodeScaffoldService.GetMaterialRulesAsync());

    [HttpPost("print-jobs")]
    [WfPermission("barcode:print:list")]
    public async Task<ApiResult<PrintJobDto>> CreatePrintJob([FromBody] CreatePrintJobRequest request)
        => ApiResult<PrintJobDto>.Ok(await barcodeScaffoldService.CreatePrintJobAsync(request));

    [HttpPost("print-jobs/{jobId}/confirm")]
    [WfPermission("barcode:print:list")]
    public async Task<ApiResult<object>> ConfirmPrinted(string jobId)
    {
        await barcodeScaffoldService.ConfirmPrintedAsync(jobId);
        return ApiResult<object>.Ok(new { jobId, status = "printed" });
    }
}
