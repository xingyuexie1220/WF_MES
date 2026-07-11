using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Position")]
public class SysPosition : BaseEntity
{
    public string PositionCode { get; set; } = string.Empty;

    public string PositionName { get; set; } = string.Empty;

    /// <summary>
    /// 关联工序编码（MES 工序权限）
    /// </summary>
    public string? ProcessCode { get; set; }

    public long? DeptId { get; set; }

    public int Sort { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
