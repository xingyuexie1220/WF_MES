using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities.Barcode;

[SugarTable("Bcd_Customer")]
public class BcdCustomer : IFactoryScoped
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Customer_Id")]
    public int CustomerId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; }

    [SugarColumn(ColumnName = "Customer_Name")]
    public string CustomerName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Enable")]
    public int Enable { get; set; } = 1;

    [SugarColumn(ColumnName = "CreatedBy")]
    public string? CreatedBy { get; set; }

    [SugarColumn(ColumnName = "CreateDate")]
    public DateTime CreateDate { get; set; } = DateTime.Now;

    [SugarColumn(ColumnName = "UpdatedBy")]
    public string? UpdatedBy { get; set; }

    [SugarColumn(ColumnName = "UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
