using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>工艺路线步骤</summary>
[SugarTable("Mes_Routing_Step")]
public class MesRoutingStep : FactoryScopedEntity
{
    public long RoutingId { get; set; }

    [SugarColumn(Length = 64)]
    public string ProcessCode { get; set; } = string.Empty;

    public int Seq { get; set; }
}
