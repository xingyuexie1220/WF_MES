using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("System_User_Role")]
public class SystemUserRole
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long RoleId { get; set; }
}
