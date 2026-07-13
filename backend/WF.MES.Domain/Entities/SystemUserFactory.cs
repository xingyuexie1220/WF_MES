using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("System_User_Factory")]
public class SystemUserFactory
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long FactoryId { get; set; }

    public bool IsDefault { get; set; }
}
