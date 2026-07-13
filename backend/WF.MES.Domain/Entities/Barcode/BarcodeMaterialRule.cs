using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Barcode;

[SugarTable("Barcode_MaterialRule")]
public class BarcodeMaterialRule : IFactoryScoped
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; }

    [SugarColumn(ColumnName = "Customer_Id")]
    public int CustomerId { get; set; }

    [SugarColumn(ColumnName = "Material_No")]
    public string MaterialNo { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Barcode_Length")]
    public int BarcodeLength { get; set; }

    [SugarColumn(ColumnName = "Qa_Status")]
    public int QaStatus { get; set; }
}
