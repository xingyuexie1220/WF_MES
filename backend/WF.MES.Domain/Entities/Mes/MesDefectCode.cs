using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>不良原因</summary>
[SugarTable("Mes_Defect_Code")]
public class MesDefectCode : FactoryScopedEntity
{
    [SugarColumn(Length = 32)]
    public string DefectCode { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string DefectName { get; set; } = string.Empty;

    public int Sort { get; set; }

    public bool Enabled { get; set; } = true;
}
