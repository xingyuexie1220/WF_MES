using WF.MES.Application.Jobs.Dtos;

namespace WF.MES.Application.Jobs;

public interface IJobService
{
    Task<List<JobInfoDto>> GetJobsAsync(CancellationToken cancellationToken = default);
    Task TriggerJobAsync(string jobKey, CancellationToken cancellationToken = default);
}
