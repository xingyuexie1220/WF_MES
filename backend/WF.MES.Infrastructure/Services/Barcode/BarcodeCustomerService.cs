using SqlSugar;
using WF.MES.Application.Barcode;
using WF.MES.Application.Common;
using WF.MES.Domain.Entities.Barcode;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services.Barcode;

public class BarcodeCustomerService(ISqlSugarClient db, IFactoryContext factoryContext) : IBarcodeCustomerService
{
    public async Task<List<BarcodeCustomerDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await db.Queryable<BcdCustomer>()
            .OrderBy(c => c.CustomerName)
            .Select(c => new BarcodeCustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Enable = c.Enable,
                CreatedBy = c.CreatedBy,
                CreateDate = c.CreateDate,
                UpdatedBy = c.UpdatedBy,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BarcodeCustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Queryable<BcdCustomer>()
            .Where(c => c.CustomerId == id)
            .Select(c => new BarcodeCustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Enable = c.Enable,
                CreatedBy = c.CreatedBy,
                CreateDate = c.CreateDate,
                UpdatedBy = c.UpdatedBy,
                UpdatedAt = c.UpdatedAt
            })
            .FirstAsync(cancellationToken);
    }

    public async Task<int> SaveAsync(SaveBarcodeCustomerRequest request, string operatorName, CancellationToken cancellationToken = default)
    {
        if (!factoryContext.CurrentFactoryId.HasValue)
        {
            throw new BusinessException("请先选择工厂", 400);
        }

        var factoryId = factoryContext.CurrentFactoryId.Value;
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new BusinessException("客户名称不能为空");
        }

        if (request.CustomerId > 0)
        {
            var entity = await db.Queryable<BcdCustomer>().FirstAsync(c => c.CustomerId == request.CustomerId, cancellationToken)
                ?? throw new BusinessException("客户不存在", 404);

            entity.CustomerName = request.CustomerName.Trim();
            entity.Enable = request.Enable;
            entity.UpdatedBy = operatorName;
            entity.UpdatedAt = DateTime.Now;
            await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return entity.CustomerId;
        }

        if (await db.Queryable<BcdCustomer>().AnyAsync(c => c.FactoryId == factoryId && c.CustomerName == request.CustomerName.Trim(), cancellationToken))
        {
            throw new BusinessException("客户名称已存在");
        }

        var id = await db.Insertable(new BcdCustomer
        {
            FactoryId = factoryId,
            CustomerName = request.CustomerName.Trim(),
            Enable = request.Enable,
            CreatedBy = operatorName,
            CreateDate = DateTime.Now
        }).ExecuteReturnIdentityAsync();

        return id;
    }
}
