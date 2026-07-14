using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>生产工单（首期本地；后期金蝶同步）</summary>
[SugarTable("Mes_Work_Order")]
public class MesWorkOrder : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string WorkOrderNo { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string MaterialNo { get; set; } = string.Empty;

    public long RoutingId { get; set; }

    public int PlanQty { get; set; }

    public DateTime DueDate { get; set; }

    /// <summary>open / closed</summary>
    [SugarColumn(Length = 16)]
    public string Status { get; set; } = "open";

    [SugarColumn(Length = 32)]
    public string Source { get; set; } = "local";

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ErpBillId { get; set; }

    public DateTime SyncedAt { get; set; }

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
