using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Positions;
using WF.MES.Application.Positions.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/positions")]
public class SysPositionController(IPositionService positionService) : WfApiControllerBase
{
    [WfPermission("system:position:list")]
    [HttpGet]
    public async Task<ApiResult<PagedResult<PositionDto>>> GetPagedList([FromQuery] PositionQueryRequest request)
        => ApiResult<PagedResult<PositionDto>>.Ok(await positionService.GetPagedListAsync(request));

    [WfPermission("system:position:list")]
    [HttpGet("all")]
    public async Task<ApiResult<List<PositionDto>>> GetAll()
        => ApiResult<List<PositionDto>>.Ok(await positionService.GetAllAsync());

    [WfPermission("system:position:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<PositionDto>> GetById(long id)
    {
        var position = await positionService.GetByIdAsync(id);
        return position is null ? ApiResult<PositionDto>.Fail("岗位不存在", 404) : ApiResult<PositionDto>.Ok(position);
    }

    [WfPermission("system:position:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreatePositionRequest request)
        => ApiResult<long>.Ok(await positionService.CreateAsync(request, GetOperatorId()));

    [WfPermission("system:position:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdatePositionRequest request)
    {
        await positionService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:position:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await positionService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
