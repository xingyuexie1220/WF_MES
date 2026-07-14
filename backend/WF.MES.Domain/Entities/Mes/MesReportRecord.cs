using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>工序报工记录</summary>
[SugarTable("Mes_Report_Record")]
public class MesReportRecord : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string WorkOrderNo { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string ProcessCode { get; set; } = string.Empty;

    public int GoodQty { get; set; }

    public int DefectQty { get; set; }

    [SugarColumn(Length = 32, IsNullable = true)]
    public string? DefectCode { get; set; }

    /// <summary>pass / rework / scrap（检验工序）</summary>
    [SugarColumn(Length = 16, IsNullable = true)]
    public string? Disposition { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ReworkToProcess { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? MachineNo { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? OperatorName { get; set; }

    public DateTime ReportTime { get; set; } = DateTime.Now;

    public bool IsVoided { get; set; }
}
