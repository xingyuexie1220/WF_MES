namespace WF.MES.Application.Barcode;

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

public interface IBarcodeCustomerService
{
    Task<List<BarcodeCustomerDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<BarcodeCustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(SaveBarcodeCustomerRequest request, string operatorName, CancellationToken cancellationToken = default);
}
