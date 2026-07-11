using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Role")]
public class SysRole : BaseEntity
{
    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public int Sort { get; set; }

    public DataScopeType DataScope { get; set; } = DataScopeType.Dept;

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
