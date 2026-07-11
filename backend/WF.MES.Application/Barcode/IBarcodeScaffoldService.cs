namespace WF.MES.Application.Barcode;

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
    public int Quantity { get; set; } = 1;
}

public class PrintJobDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public List<string> Barcodes { get; set; } = [];
}

public interface IBarcodeScaffoldService
{
    Task<List<BarcodeMaterialRuleDto>> GetMaterialRulesAsync(CancellationToken cancellationToken = default);
    Task<PrintJobDto> CreatePrintJobAsync(CreatePrintJobRequest request, CancellationToken cancellationToken = default);
    Task ConfirmPrintedAsync(string jobId, CancellationToken cancellationToken = default);
}
