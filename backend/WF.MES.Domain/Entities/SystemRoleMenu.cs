using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Role_Menu")]
public class SystemRoleMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long MenuId { get; set; }
}
