using WF.MES.Application.Menus.Dtos;
using WF.MES.Shared.Enums;

namespace WF.MES.Application.Menus;

public interface IMenuService
{
    Task<List<MenuDto>> GetTreeAsync(ClientType? clientType = null, CancellationToken cancellationToken = default);
    Task<MenuDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateMenuRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateMenuRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
