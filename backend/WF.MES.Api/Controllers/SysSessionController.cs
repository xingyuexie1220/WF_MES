using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Sessions;
using WF.MES.Application.Sessions.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/sessions")]
public class SysSessionController(ISessionAdminService sessionAdminService) : WfApiControllerBase
{
    [WfPermission("system:session:list")]
    [HttpGet]
    public async Task<ApiResult<PagedResult<SessionDto>>> GetPagedList([FromQuery] SessionQueryRequest request)
        => ApiResult<PagedResult<SessionDto>>.Ok(await sessionAdminService.GetPagedListAsync(request));

    [WfPermission("system:session:kick")]
    [HttpPost("kick")]
    public async Task<ApiResult> Kick([FromBody] KickSessionRequest request)
    {
        await sessionAdminService.KickAsync(request);
        return ApiResult.Ok();
    }
}
