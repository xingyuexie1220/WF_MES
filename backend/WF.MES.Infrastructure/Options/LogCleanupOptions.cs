namespace WF.MES.Infrastructure.Options;

public class LogCleanupOptions
{
    public const string SectionName = "LogCleanup";

    public int RetentionDays { get; set; } = 7;
    public string Directory { get; set; } = "logs";
}
