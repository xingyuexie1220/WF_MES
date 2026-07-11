using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>条码客户（Bcd_Customer）。</summary>
[SugarTable("Bcd_Customer")]
public class BarcodeCustomer
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Customer_Id")]
    public int CustomerId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; } = 1;

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
