using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>流水号计数器，按 ResetKey 递增（Barcode_SerialCounter）。</summary>
[SugarTable("Barcode_SerialCounter")]
public class BarcodeSerialCounter
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Id")]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Reset_Key", Length = 500)]
    public string ResetKey { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Current_Value")]
    public long CurrentValue { get; set; }

    [SugarColumn(ColumnName = "UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
