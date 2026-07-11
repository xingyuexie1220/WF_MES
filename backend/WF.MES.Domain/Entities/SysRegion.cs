using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Region")]
public class SysRegion : BaseEntity
{
    public string RegionCode { get; set; } = string.Empty;

    public string RegionName { get; set; } = string.Empty;

    public int Sort { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
