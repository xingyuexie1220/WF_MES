using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>启动时检查 version.json 并调用 WF.MES.Updater 应用更新。</summary>
public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    Task<bool> DownloadAndApplyAsync(UpdateCheckResult updateInfo, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
