using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>条码客户主数据 CRUD。</summary>
public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListDto>> GetCustomersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerListDto>> GetCustomerSelectionListAsync(int? ensureCustomerId = null, CancellationToken cancellationToken = default);

    Task<CustomerEditDto?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<int> SaveCustomerAsync(CustomerEditDto dto, CancellationToken cancellationToken = default);
}
