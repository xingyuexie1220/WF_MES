using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Dict_Data")]
public class SystemDictData : BaseEntity
{
    public long DictTypeId { get; set; }
    public string DictType { get; set; } = string.Empty;
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}
