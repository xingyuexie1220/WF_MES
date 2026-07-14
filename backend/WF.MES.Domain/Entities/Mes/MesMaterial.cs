using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Mes;

/// <summary>物料主档（首期本地；后期金蝶同步）</summary>
[SugarTable("Mes_Material")]
public class MesMaterial : FactoryScopedEntity
{
    [SugarColumn(Length = 64)]
    public string MaterialNo { get; set; } = string.Empty;

    [SugarColumn(Length = 256)]
    public string MaterialName { get; set; } = string.Empty;

    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Spec { get; set; }

    [SugarColumn(Length = 32, IsNullable = true)]
    public string? Unit { get; set; }

    [SugarColumn(Length = 32)]
    public string Source { get; set; } = "local";

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ErpId { get; set; }

    public bool Enabled { get; set; } = true;
}
