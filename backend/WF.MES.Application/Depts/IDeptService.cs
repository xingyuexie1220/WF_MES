using WF.MES.Application.Depts.Dtos;

namespace WF.MES.Application.Depts;

public interface IDeptService
{
    Task<List<DeptDto>> GetTreeAsync(CancellationToken cancellationToken = default);
    Task<DeptDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateDeptRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateDeptRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
