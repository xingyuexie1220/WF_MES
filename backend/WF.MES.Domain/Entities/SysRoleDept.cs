using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Role_Dept")]
public class SysRoleDept
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long DeptId { get; set; }
}
