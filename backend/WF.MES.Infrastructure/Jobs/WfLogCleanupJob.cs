using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using WF.MES.Infrastructure.Options;

namespace WF.MES.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class WfLogCleanupJob(
    ILogger<WfLogCleanupJob> logger,
    IOptions<LogCleanupOptions> options,
    IHostEnvironment hostEnvironment) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var cfg = options.Value;
        var retentionDays = cfg.RetentionDays <= 0 ? 7 : cfg.RetentionDays;
        var logDir = string.IsNullOrWhiteSpace(cfg.Directory) ? "logs" : cfg.Directory;
        var fullPath = Path.IsPathRooted(logDir)
            ? logDir
            : Path.Combine(hostEnvironment.ContentRootPath, logDir);

        if (!Directory.Exists(fullPath))
        {
            logger.LogWarning("[Quartz] Log directory not found: {Path}", fullPath);
            return Task.CompletedTask;
        }

        var cutoff = DateTime.Now.AddDays(-retentionDays);
        var deleted = 0;

        foreach (var file in Directory.EnumerateFiles(fullPath, "wf-mes-*.log"))
        {
            try
            {
                var info = new FileInfo(file);
                if (info.LastWriteTime < cutoff)
                {
                    File.Delete(file);
                    deleted++;
                    logger.LogInformation("[Quartz] Deleted expired log file: {File}", file);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Quartz] Failed to delete log file: {File}", file);
            }
        }

        logger.LogInformation(
            "[Quartz] Log cleanup finished. Deleted {Count} file(s) older than {Days} day(s) in {Path}",
            deleted,
            retentionDays,
            fullPath);

        return Task.CompletedTask;
    }
}
