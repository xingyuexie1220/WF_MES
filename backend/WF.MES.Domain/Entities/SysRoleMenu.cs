using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Role_Menu")]
public class SysRoleMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long MenuId { get; set; }
}
