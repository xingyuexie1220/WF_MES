using SqlSugar;

namespace WF.MES.Domain.Entities;

[SugarTable("System_User_Position")]
public class SystemUserPosition
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long PositionId { get; set; }
}
