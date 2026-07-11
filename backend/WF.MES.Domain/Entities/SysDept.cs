using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Dept")]
public class SysDept : BaseEntity
{
    public long FactoryId { get; set; }

    public long ParentId { get; set; }

    public string DeptCode { get; set; } = string.Empty;

    public string DeptName { get; set; } = string.Empty;

    public DeptType DeptType { get; set; }

    public int Sort { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
