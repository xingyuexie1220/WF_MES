using WF.MES.Application.MasterData.Dtos;

namespace WF.MES.Application.MasterData;

public interface IMesMasterDataService
{
    Task<List<MesProcessDto>> GetProcessesAsync(CancellationToken cancellationToken = default);
    Task<long> SaveProcessAsync(SaveMesProcessRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteProcessAsync(long id, CancellationToken cancellationToken = default);

    Task<List<MesRoutingDto>> GetRoutingsAsync(CancellationToken cancellationToken = default);
    Task<MesRoutingDto?> GetRoutingAsync(long id, CancellationToken cancellationToken = default);
    Task<long> SaveRoutingAsync(SaveMesRoutingRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteRoutingAsync(long id, CancellationToken cancellationToken = default);

    Task<List<MesMaterialDto>> GetMaterialsAsync(CancellationToken cancellationToken = default);
    Task<long> SaveMaterialAsync(SaveMesMaterialRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteMaterialAsync(long id, CancellationToken cancellationToken = default);

    Task<List<MesMachineDto>> GetMachinesAsync(CancellationToken cancellationToken = default);
    Task<long> SaveMachineAsync(SaveMesMachineRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task DeleteMachineAsync(long id, CancellationToken cancellationToken = default);

    Task<List<MesDefectCodeDto>> GetDefectCodesAsync(CancellationToken cancellationToken = default);
}
