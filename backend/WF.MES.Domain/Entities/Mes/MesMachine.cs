using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>机台档案</summary>
[SugarTable("Mes_Machine")]
public class MesMachine : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string MachineNo { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string MachineName { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
