using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Role_Dept")]
public class SystemRoleDept
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long DeptId { get; set; }
}
