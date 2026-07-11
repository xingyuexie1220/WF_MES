namespace WF.MES.Contracts.Barcode;

public class BarcodeCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Enable { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaveBarcodeCustomerRequest
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Enable { get; set; } = 1;
}

public class BarcodeMaterialRuleDto
{
    public int RuleId { get; set; }
    public int CustomerId { get; set; }
    public string MaterialNo { get; set; } = string.Empty;
    public int BarcodeLength { get; set; }
    public int QaStatus { get; set; }
}

public class CreatePrintJobRequest
{
    public int RuleId { get; set; }
    public int Quantity { get; set; }
}

public class PrintJobDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public List<string> Barcodes { get; set; } = [];
}
