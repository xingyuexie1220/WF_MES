using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Dict_Type")]
public class SystemDictType : BaseEntity
{
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}
