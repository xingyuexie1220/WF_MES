using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Factory")]
public class SystemFactory : BaseEntity
{
    public long RegionId { get; set; }

    public string FactoryCode { get; set; } = string.Empty;

    public string FactoryName { get; set; } = string.Empty;

    public string? TimeZone { get; set; }

    public int Sort { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
