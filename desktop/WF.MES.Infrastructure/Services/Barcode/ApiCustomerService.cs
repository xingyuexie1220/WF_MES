using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Api;
using WF.MES.Infrastructure.Validation;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>通过 Backend Barcode API 管理客户（不再直连数据库）。</summary>
public class ApiCustomerService(
    IBarcodeApi barcodeApi,
    IValidator<CustomerEditDto> validator) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerListDto>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        var response = await barcodeApi.GetCustomersAsync(cancellationToken);
        var data = ApiResponseHelper.EnsureData(response, "加载客户失败");
        return data.Select(Map).OrderBy(c => c.CustomerName).ToList();
    }

    public async Task<IReadOnlyList<CustomerListDto>> GetCustomerSelectionListAsync(
        int? ensureCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        var customers = (await GetCustomersAsync(cancellationToken)).Where(c => c.Enable == 1).ToList();
        if (ensureCustomerId is > 0 && customers.All(c => c.CustomerId != ensureCustomerId.Value))
        {
            var ensured = (await GetCustomersAsync(cancellationToken)).FirstOrDefault(c => c.CustomerId == ensureCustomerId.Value);
            if (ensured != null)
            {
                customers.Insert(0, ensured);
            }
        }

        return customers;
    }

    public async Task<CustomerEditDto?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = (await GetCustomersAsync(cancellationToken)).FirstOrDefault(c => c.CustomerId == customerId);
        if (customer == null)
        {
            return null;
        }

        return new CustomerEditDto
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            Enable = customer.Enable,
            CreatedBy = customer.CreatedBy,
            CreateDate = customer.CreateDate,
            UpdatedBy = customer.UpdatedBy,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<int> SaveCustomerAsync(CustomerEditDto dto, CancellationToken cancellationToken = default)
    {
        await validator.ValidateRequestAsync(dto, cancellationToken);
        var response = await barcodeApi.SaveCustomerAsync(new SaveBarcodeCustomerDto
        {
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName.Trim(),
            Enable = dto.Enable
        }, cancellationToken);
        return ApiResponseHelper.EnsureData(response, "保存客户失败");
    }

    private static CustomerListDto Map(BarcodeCustomerDto dto) => new()
    {
        CustomerId = dto.CustomerId,
        CustomerName = dto.CustomerName,
        Enable = dto.Enable,
        CreatedBy = dto.CreatedBy,
        CreateDate = dto.CreateDate,
        UpdatedBy = dto.UpdatedBy,
        UpdatedAt = dto.UpdatedAt
    };
}
