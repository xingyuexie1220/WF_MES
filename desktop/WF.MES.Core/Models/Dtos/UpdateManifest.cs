namespace WF.MES.Models.Dtos;

/// <summary>update/version.json 远程清单结构。</summary>
public class UpdateManifest
{
    public string Version { get; set; } = string.Empty;

    public string DownloadUrl { get; set; } = string.Empty;

    public string? ReleaseNotes { get; set; }
}

/// <summary>启动时版本比对结果。</summary>
public class UpdateCheckResult
{
    public bool HasUpdate { get; init; }

    public string CurrentVersion { get; init; } = string.Empty;

    public string LatestVersion { get; init; } = string.Empty;

    public string DownloadUrl { get; init; } = string.Empty;

    public string? ReleaseNotes { get; init; }
}
