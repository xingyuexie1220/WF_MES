using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Depts;
using WF.MES.Application.Depts.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/depts")]
public class SysDeptController(IDeptService deptService) : WfApiControllerBase
{
    [WfPermission("system:dept:list")]
    [HttpGet("tree")]
    public async Task<ApiResult<List<DeptDto>>> GetTree()
        => ApiResult<List<DeptDto>>.Ok(await deptService.GetTreeAsync());

    [WfPermission("system:dept:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<DeptDto>> GetById(long id)
    {
        var dept = await deptService.GetByIdAsync(id);
        return dept is null ? ApiResult<DeptDto>.Fail("部门不存在", 404) : ApiResult<DeptDto>.Ok(dept);
    }

    [WfPermission("system:dept:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreateDeptRequest request)
        => ApiResult<long>.Ok(await deptService.CreateAsync(request, GetOperatorId()));

    [WfPermission("system:dept:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateDeptRequest request)
    {
        await deptService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:dept:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await deptService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
