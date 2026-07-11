using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>条码明细行（Bcd_Record）。</summary>
[SugarTable("Bcd_Record")]
public class BarcodeRecord
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Record_Id")]
    public int RecordId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; } = 1;

    [SugarColumn(ColumnName = "Generate_Record_Id")]
    public int GenerateRecordId { get; set; }

    [SugarColumn(ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Barcode", Length = 200)]
    public string Barcode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Reset_Key", Length = 500)]
    public string ResetKey { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Serial_Value")]
    public long SerialValue { get; set; }

    [SugarColumn(ColumnName = "Status")]
    public int Status { get; set; } = 1;

    [SugarColumn(ColumnName = "CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
