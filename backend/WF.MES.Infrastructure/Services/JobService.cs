using Quartz;
using Quartz.Impl.Matchers;
using WF.MES.Application.Jobs;
using WF.MES.Application.Jobs.Dtos;
using WF.MES.Infrastructure.Jobs;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class JobService(ISchedulerFactory schedulerFactory) : IJobService
{
    private static readonly Dictionary<string, (string Name, string Description)> JobMeta = new()
    {
        [WfQuartzJobKeys.LogCleanupJob] = ("日志文件清理", "清理7天前本地 Serilog 日志文件（wf-mes-*.log）")
    };

    public async Task<List<JobInfoDto>> GetJobsAsync(CancellationToken cancellationToken = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);
        var result = new List<JobInfoDto>();

        foreach (var jobKey in jobKeys)
        {
            var detail = await scheduler.GetJobDetail(jobKey, cancellationToken);
            var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);
            var trigger = triggers.FirstOrDefault();
            var meta = JobMeta.GetValueOrDefault(jobKey.Name);

            result.Add(new JobInfoDto
            {
                JobKey = jobKey.Name,
                JobName = meta.Name ?? jobKey.Name,
                JobGroup = jobKey.Group,
                CronExpression = trigger is ICronTrigger cron ? cron.CronExpressionString ?? string.Empty : string.Empty,
                Description = meta.Description ?? detail?.Description ?? string.Empty,
                Status = "Normal",
                NextFireTime = trigger?.GetNextFireTimeUtc()?.LocalDateTime,
                PreviousFireTime = trigger?.GetPreviousFireTimeUtc()?.LocalDateTime
            });
        }

        return result.OrderBy(x => x.JobKey).ToList();
    }

    public async Task TriggerJobAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        if (!JobMeta.ContainsKey(jobKey))
        {
            throw new BusinessException("任务不存在");
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var key = new JobKey(jobKey);
        if (!await scheduler.CheckExists(key, cancellationToken))
        {
            throw new BusinessException("任务未注册");
        }

        await scheduler.TriggerJob(key, cancellationToken);
    }
}
