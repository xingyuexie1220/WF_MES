using WF.MES.Application.Roles.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Roles;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetPagedListAsync(RoleQueryRequest request, CancellationToken cancellationToken = default);
    Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateRoleRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateRoleRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
