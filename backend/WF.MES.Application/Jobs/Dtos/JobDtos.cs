namespace WF.MES.Application.Jobs.Dtos;

public class JobInfoDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = "DEFAULT";
    public string CronExpression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Normal";
    public DateTime? NextFireTime { get; set; }
    public DateTime? PreviousFireTime { get; set; }
}
