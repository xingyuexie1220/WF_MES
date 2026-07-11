using WF.MES.Application.Sessions.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Sessions;

public interface ISessionAdminService
{
    Task<PagedResult<SessionDto>> GetPagedListAsync(SessionQueryRequest request, CancellationToken cancellationToken = default);

    Task KickAsync(KickSessionRequest request, CancellationToken cancellationToken = default);
}
