using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>工艺路线头（Web 维护）</summary>
[SugarTable("Mes_Routing")]
public class MesRouting : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string RoutingCode { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string RoutingName { get; set; } = string.Empty;

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? MaterialNo { get; set; }

    public bool Enabled { get; set; } = true;

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
