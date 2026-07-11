using WF.MES.Application.Factories.Dtos;

namespace WF.MES.Application.Factories;

public interface IFactoryService
{
    Task<List<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken = default);
    Task<List<FactoryDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<FactoryDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateFactoryRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateFactoryRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
    Task<List<FactorySummaryDto>> GetAccessibleFactoriesAsync(long userId, CancellationToken cancellationToken = default);
    Task EnsureUserCanAccessFactoryAsync(long userId, long factoryId, CancellationToken cancellationToken = default);
}
