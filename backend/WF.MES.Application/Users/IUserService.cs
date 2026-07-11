using WF.MES.Application.Users.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Users;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedListAsync(UserQueryRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateUserRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateUserRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(long id, ResetPasswordRequest request, long operatorId, CancellationToken cancellationToken = default);
}
