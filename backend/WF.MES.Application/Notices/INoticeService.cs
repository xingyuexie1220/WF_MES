using WF.MES.Application.Notices.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Notices;

public interface INoticeService
{
    Task<PagedResult<NoticeDto>> GetPagedListAsync(NoticeQueryRequest request, CancellationToken cancellationToken = default);
    Task<List<NoticePushDto>> GetPublishedRecentAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<NoticeDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateNoticeRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, UpdateNoticeRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default);
    Task<NoticePushDto> PublishAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
