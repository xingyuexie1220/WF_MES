using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>条码生成单（Barcode_GenerateRecord）。</summary>
[SugarTable("Barcode_GenerateRecord")]
public class BarcodeGenerateRecord
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Generate_Record_Id")]
    public int GenerateRecordId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; } = 1;

    [SugarColumn(ColumnName = "Generate_No")]
    public string GenerateNo { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Material_No")]
    public string MaterialNo { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Reset_Key", Length = 500)]
    public string ResetKey { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Print_Date")]
    public DateTime PrintDate { get; set; }

    [SugarColumn(ColumnName = "Quantity")]
    public int Quantity { get; set; }

    [SugarColumn(ColumnName = "Serial_Start")]
    public long SerialStart { get; set; }

    [SugarColumn(ColumnName = "Serial_End")]
    public long SerialEnd { get; set; }

    [SugarColumn(ColumnName = "Print_Status")]
    public int PrintStatus { get; set; }

    [SugarColumn(ColumnName = "Last_Reprinted_At")]
    public DateTime? LastReprintedAt { get; set; }

    [SugarColumn(ColumnName = "Last_Reprinted_By")]
    public string? LastReprintedBy { get; set; }

    [SugarColumn(ColumnName = "CreatedBy")]
    public string? CreatedBy { get; set; }

    [SugarColumn(ColumnName = "CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
