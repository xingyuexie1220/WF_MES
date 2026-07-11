using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_User_Factory")]
public class SysUserFactory
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long FactoryId { get; set; }

    public bool IsDefault { get; set; }
}
