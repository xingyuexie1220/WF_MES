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
public class MasterDataController(IMasterDataScaffoldService service) : WfApiControllerBase
{
    [HttpGet("materials")]
    [WfPermission("master:material:list")]
    public async Task<ApiResult<object>> GetMaterials() => ApiResult<object>.Ok(await service.GetMaterialsAsync());

    [HttpGet("routes")]
    [WfPermission("master:route:list")]
    public async Task<ApiResult<object>> GetRoutes() => ApiResult<object>.Ok(await service.GetRoutesAsync());

    [HttpGet("stations")]
    [WfPermission("master:station:list")]
    public async Task<ApiResult<object>> GetStations() => ApiResult<object>.Ok(await service.GetStationsAsync());

    [HttpGet("work-centers")]
    [WfPermission("master:workcenter:list")]
    public async Task<ApiResult<object>> GetWorkCenters() => ApiResult<object>.Ok(await service.GetWorkCentersAsync());
}

[Authorize]
[ApiController]
[Route("api/v1/production")]
public class ProductionController(IProductionScaffoldService service) : WfApiControllerBase
{
    [HttpGet("work-orders")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> GetWorkOrders() => ApiResult<object>.Ok(await service.GetWorkOrdersAsync());

    [HttpPost("pass-records")]
    [WfPermission("mes:workorder:scan")]
    public async Task<ApiResult<object>> PassStation([FromBody] object request) => ApiResult<object>.Ok(await service.PassStationAsync(request));
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
