using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Logs;
using WF.MES.Application.Logs.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/logs")]
public class SystemLogController(ILogService logService) : WfApiControllerBase
{
    [WfPermission("system:log:list")]
    [HttpGet("operations")]
    public async Task<ApiResult<PagedResult<OperationLogDto>>> GetOperationPagedList([FromQuery] OperationLogQueryRequest request)
        => ApiResult<PagedResult<OperationLogDto>>.Ok(await logService.GetOperationPagedListAsync(request));

    [WfPermission("system:log:list")]
    [HttpGet("exceptions")]
    public async Task<ApiResult<PagedResult<ExceptionLogDto>>> GetExceptionPagedList([FromQuery] ExceptionLogQueryRequest request)
        => ApiResult<PagedResult<ExceptionLogDto>>.Ok(await logService.GetExceptionPagedListAsync(request));

    [WfPermission("system:log:export")]
    [HttpGet("operations/export")]
    public async Task<ApiResult<List<OperationLogDto>>> ExportOperationLogs([FromQuery] OperationLogQueryRequest request)
        => ApiResult<List<OperationLogDto>>.Ok(await logService.ExportOperationLogsAsync(request));

    [WfPermission("system:log:export")]
    [HttpGet("exceptions/export")]
    public async Task<ApiResult<List<ExceptionLogDto>>> ExportExceptionLogs([FromQuery] ExceptionLogQueryRequest request)
        => ApiResult<List<ExceptionLogDto>>.Ok(await logService.ExportExceptionLogsAsync(request));

    [WfPermission("system:log:clear")]
    [HttpPost("operations/clear")]
    public async Task<ApiResult> ClearOperationLogs([FromBody] ClearLogRequest? request)
    {
        await logService.ClearOperationLogsAsync(request ?? new ClearLogRequest());
        return ApiResult.Ok();
    }

    [WfPermission("system:log:clear")]
    [HttpPost("exceptions/clear")]
    public async Task<ApiResult> ClearExceptionLogs([FromBody] ClearLogRequest? request)
    {
        await logService.ClearExceptionLogsAsync(request ?? new ClearLogRequest());
        return ApiResult.Ok();
    }
}
