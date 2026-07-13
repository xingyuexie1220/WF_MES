using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Jobs;
using WF.MES.Application.Jobs.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/jobs")]
public class SystemJobController(IJobService jobService) : WfApiControllerBase
{
    [WfPermission("system:job:list")]
    [HttpGet]
    public async Task<ApiResult<List<JobInfoDto>>> GetJobs()
        => ApiResult<List<JobInfoDto>>.Ok(await jobService.GetJobsAsync());

    [WfPermission("system:job:run")]
    [HttpPost("run/{jobKey}")]
    public async Task<ApiResult> RunJob(string jobKey)
    {
        await jobService.TriggerJobAsync(jobKey);
        return ApiResult.Ok();
    }
}
