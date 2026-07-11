using Refit;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

public interface IBarcodeApi
{
    [Get("/api/v1/barcode/customers")]
    Task<ApiResultDto<List<BarcodeCustomerDto>>> GetCustomersAsync(CancellationToken cancellationToken = default);

    [Post("/api/v1/barcode/customers")]
    Task<ApiResultDto<int>> SaveCustomerAsync([Body] SaveBarcodeCustomerDto request, CancellationToken cancellationToken = default);
}

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

public class SaveBarcodeCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Enable { get; set; } = 1;
}
