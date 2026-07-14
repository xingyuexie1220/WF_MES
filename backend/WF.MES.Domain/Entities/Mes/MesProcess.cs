using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>工序主档（Web 维护）</summary>
[SugarTable("Mes_Process")]
public class MesProcess : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string ProcessCode { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string ProcessName { get; set; } = string.Empty;

    public int DefaultSeq { get; set; }

    public bool Enabled { get; set; } = true;

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
