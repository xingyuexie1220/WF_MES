using WF.MES.Application.Dicts.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Application.Dicts;

public interface IDictService
{
    Task<PagedResult<DictTypeDto>> GetTypePagedListAsync(DictTypeQueryRequest request, CancellationToken cancellationToken = default);
    Task<List<DictTypeDto>> GetAllTypesAsync(CancellationToken cancellationToken = default);
    Task<DictTypeDto?> GetTypeByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateTypeAsync(CreateDictTypeRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateTypeAsync(long id, UpdateDictTypeRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteTypeAsync(long id, long operatorId, CancellationToken cancellationToken = default);

    Task<PagedResult<DictDataDto>> GetDataPagedListAsync(DictDataQueryRequest request, CancellationToken cancellationToken = default);
    Task<List<DictDataDto>> GetDataByTypeAsync(string dictType, CancellationToken cancellationToken = default);
    Task<DictDataDto?> GetDataByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateDataAsync(CreateDictDataRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task UpdateDataAsync(long id, UpdateDictDataRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteDataAsync(long id, long operatorId, CancellationToken cancellationToken = default);
}
