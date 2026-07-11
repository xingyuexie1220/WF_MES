namespace WF.MES.Infrastructure.Jobs;

public static class WfQuartzJobKeys
{
    public const string LogCleanupJob = "WfLogCleanupJob";
    public const string LogCleanupTrigger = "WfLogCleanupJob-trigger";
    public const string LogCleanupCron = "0 0 2 * * ?";
}
