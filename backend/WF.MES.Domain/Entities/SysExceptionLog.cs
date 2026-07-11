using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Exception_Log")]
public class SysExceptionLog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string? Module { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? RequestUrl { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestParam { get; set; }
    public string? OperIp { get; set; }
    public long? OperUserId { get; set; }
    public string? OperUserName { get; set; }
    public DateTime ExceptionTime { get; set; } = DateTime.Now;
}
