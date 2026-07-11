using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Operation_Log")]
public class SysOperationLog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string? Module { get; set; }
    public string? Title { get; set; }
    public string? BusinessType { get; set; }
    public string? Method { get; set; }
    public string? RequestMethod { get; set; }
    public string? OperUrl { get; set; }
    public string? OperIp { get; set; }
    public string? OperParam { get; set; }
    public int Status { get; set; } = 1;
    public string? ErrorMsg { get; set; }
    public long? OperUserId { get; set; }
    public string? OperUserName { get; set; }
    public DateTime OperTime { get; set; } = DateTime.Now;
    public long CostTime { get; set; }
}
