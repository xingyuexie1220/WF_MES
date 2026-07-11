using WF.MES.Application.Positions.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Positions;

public interface IPositionService
{
    Task<PagedResult<PositionDto>> GetPagedListAsync(PositionQueryRequest request, CancellationToken cancellationToken = default);
    Task<List<PositionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PositionDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreatePositionRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdatePositionRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
