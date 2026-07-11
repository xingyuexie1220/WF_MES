using FluentValidation;
using Serilog;
using SqlSugar;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;
using WF.MES.Infrastructure.Validation;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>条码客户 CRUD，含启用状态与重复名校验。</summary>
public class CustomerService : ICustomerService
{
    private readonly ISqlSugarClient _db;
    private readonly ISessionService _sessionService;
    private readonly IValidator<CustomerEditDto> _validator;

    public CustomerService(
        ISqlSugarClient db,
        ISessionService sessionService,
        IValidator<CustomerEditDto> validator)
    {
        _db = db;
        _sessionService = sessionService;
        _validator = validator;
    }

    public async Task<IReadOnlyList<CustomerListDto>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Queryable<BarcodeCustomer>()
            .OrderBy(c => c.CustomerName)
            .Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Enable = c.Enable,
                CreatedBy = c.CreatedBy,
                CreateDate = c.CreateDate,
                UpdatedBy = c.UpdatedBy,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CustomerListDto>> GetCustomerSelectionListAsync(
        int? ensureCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        var customers = (await GetEnabledCustomersAsync(cancellationToken)).ToList();

        if (ensureCustomerId is > 0 && customers.All(c => c.CustomerId != ensureCustomerId.Value))
        {
            var customer = await GetCustomerAsync(ensureCustomerId.Value, cancellationToken);
            if (customer != null)
            {
                customers.Insert(0, new CustomerListDto
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Enable = customer.Enable
                });
            }
        }

        return customers;
    }

    private async Task<IReadOnlyList<CustomerListDto>> GetEnabledCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Queryable<BarcodeCustomer>()
            .Where(c => c.Enable == 1)
            .OrderBy(c => c.CustomerName)
            .Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Enable = c.Enable
            })
            .ToListAsync();
    }

    public async Task<CustomerEditDto?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Queryable<BarcodeCustomer>().InSingleAsync(customerId);
        return entity == null ? null : MapToEditDto(entity);
    }

    public async Task<int> SaveCustomerAsync(CustomerEditDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateRequestAsync(dto, cancellationToken);

        var operatorName = BarcodeAuditHelper.GetCurrentOperator(_sessionService);

        if (dto.CustomerId == 0)
        {
            var exists = await _db.Queryable<BarcodeCustomer>().AnyAsync(c => c.CustomerName == dto.CustomerName.Trim());
            if (exists)
            {
                throw new InvalidOperationException($"客户名称「{dto.CustomerName}」已存在");
            }

            var entity = MapToEntity(dto);
            BarcodeAuditHelper.ApplyCreateAudit(entity, operatorName);
            var id = await _db.Insertable(entity).ExecuteReturnIdentityAsync();
            Log.Information("客户 {CustomerName} 已创建", dto.CustomerName);
            return id;
        }

        var customer = await _db.Queryable<BarcodeCustomer>().InSingleAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("客户不存在");

        var duplicate = await _db.Queryable<BarcodeCustomer>()
            .AnyAsync(c => c.CustomerName == dto.CustomerName.Trim() && c.CustomerId != dto.CustomerId);
        if (duplicate)
        {
            throw new InvalidOperationException($"客户名称「{dto.CustomerName}」已存在");
        }

        customer.CustomerName = dto.CustomerName.Trim();
        customer.Enable = dto.Enable;
        BarcodeAuditHelper.ApplyUpdateAudit(customer, operatorName);
        await _db.Updateable(customer).ExecuteCommandAsync();
        Log.Information("客户 {CustomerName} 已更新", dto.CustomerName);
        return customer.CustomerId;
    }

    private static CustomerEditDto MapToEditDto(BarcodeCustomer entity) => new()
    {
        CustomerId = entity.CustomerId,
        CustomerName = entity.CustomerName,
        Enable = entity.Enable,
        CreatedBy = entity.CreatedBy,
        CreateDate = entity.CreateDate,
        UpdatedBy = entity.UpdatedBy,
        UpdatedAt = entity.UpdatedAt
    };

    private static BarcodeCustomer MapToEntity(CustomerEditDto dto) => new()
    {
        CustomerName = dto.CustomerName.Trim(),
        Enable = dto.Enable
    };
}
