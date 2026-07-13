using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using WF.MES.Core;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services;

/// <summary>从 update/version.json 拉取清单，下载 zip 至 staging 后启动 WF.MES.Updater 覆盖主程序。</summary>
public class UpdateService : IUpdateService
{
    private const string UpdaterExeName = "WF.MES.Updater.exe";
    private const string MainExeName = "WF.MES.WPF.exe";
    private const string MainDllName = "WF.MES.WPF.dll";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly IAppVersion _appVersion;
    private readonly HttpClient _httpClient;

    public UpdateService(IConfiguration configuration, IAppVersion appVersion)
    {
        _configuration = configuration;
        _appVersion = appVersion;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
    }

    public async Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = _appVersion.Current;

        var checkUrl = _configuration["Update:CheckUrl"];

        if (string.IsNullOrWhiteSpace(checkUrl))
        {
            checkUrl = await TryLoadLocalManifestAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(checkUrl))
        {
            Log.Warning("未配置更新检查地址 Update:CheckUrl");
            return NoUpdate(currentVersion);
        }

        try
        {
            UpdateManifest manifest;

            if (checkUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && File.Exists(checkUrl))
            {
                var localJson = await File.ReadAllTextAsync(checkUrl, cancellationToken);
                manifest = JsonSerializer.Deserialize<UpdateManifest>(localJson, JsonOptions)
                    ?? throw new BusinessException("err.updateManifestParseFailed");
            }
            else if (Uri.IsWellFormedUriString(checkUrl, UriKind.Absolute))
            {
                var remoteJson = await _httpClient.GetStringAsync(checkUrl, cancellationToken);
                manifest = JsonSerializer.Deserialize<UpdateManifest>(remoteJson, JsonOptions)
                    ?? throw new BusinessException("err.updateManifestParseFailed");
            }
            else
            {
                return NoUpdate(currentVersion);
            }

            if (Version.TryParse(manifest.Version, out var latest) && Version.TryParse(currentVersion, out var current) && latest > current)
            {
                Log.Information("发现新版本 {LatestVersion}，当前版本 {CurrentVersion}", manifest.Version, currentVersion);
                return new UpdateCheckResult
                {
                    HasUpdate = true,
                    CurrentVersion = currentVersion,
                    LatestVersion = manifest.Version,
                    DownloadUrl = manifest.DownloadUrl,
                    ReleaseNotes = manifest.ReleaseNotes
                };
            }

            return NoUpdate(currentVersion);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "检查更新失败");
            return NoUpdate(currentVersion);
        }
    }

    public async Task<bool> DownloadAndApplyAsync(
        UpdateCheckResult updateInfo,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!updateInfo.HasUpdate || string.IsNullOrWhiteSpace(updateInfo.DownloadUrl))
        {
            return false;
        }

        try
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "WF.MES.WPF", updateInfo.LatestVersion);
            Directory.CreateDirectory(tempRoot);

            var packagePath = Path.Combine(tempRoot, "update.zip");
            var extractPath = Path.Combine(tempRoot, "extract");

            Log.Information("开始下载更新包 {DownloadUrl}", updateInfo.DownloadUrl);

            await DownloadFileAsync(updateInfo.DownloadUrl, packagePath, progress, cancellationToken);

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            ZipFile.ExtractToDirectory(packagePath, extractPath);

            var packageRoot = ResolvePackageRoot(extractPath);

            var appPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var updaterTempDir = Path.Combine(tempRoot, "updater");
            if (!TryStageUpdater(appPath, packageRoot, updaterTempDir))
            {
                Log.Error("未找到 {UpdaterExe}，无法应用更新", UpdaterExeName);
                return false;
            }

            var updaterExe = Path.Combine(updaterTempDir, UpdaterExeName);
            var arguments = $"--pid {Environment.ProcessId} --source \"{packageRoot}\" --target \"{appPath}\" --exe {MainExeName}";

            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExe,
                Arguments = arguments,
                WorkingDirectory = updaterTempDir,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            Log.Information("更新程序已启动，应用程序即将退出");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "下载或应用更新失败");
            return false;
        }
    }

    private static bool TryStageUpdater(string appPath, string extractPath, string updaterTempDir)
    {
        string? sourceDir = null;
        if (File.Exists(Path.Combine(appPath, UpdaterExeName)))
        {
            sourceDir = appPath;
        }
        else if (File.Exists(Path.Combine(extractPath, UpdaterExeName)))
        {
            sourceDir = extractPath;
        }

        if (sourceDir == null)
        {
            return false;
        }

        Directory.CreateDirectory(updaterTempDir);

        foreach (var file in Directory.EnumerateFiles(sourceDir, "WF.MES.Updater*"))
        {
            File.Copy(file, Path.Combine(updaterTempDir, Path.GetFileName(file)), overwrite: true);
        }

        return File.Exists(Path.Combine(updaterTempDir, UpdaterExeName));
    }

    private static string ResolvePackageRoot(string extractPath)
    {
        if (File.Exists(Path.Combine(extractPath, MainDllName)))
        {
            return extractPath;
        }

        var subdirectories = Directory.GetDirectories(extractPath);
        if (subdirectories.Length == 1 &&
            File.Exists(Path.Combine(subdirectories[0], MainDllName)))
        {
            return subdirectories[0];
        }

        return extractPath;
    }

    private static UpdateCheckResult NoUpdate(string currentVersion) =>
        new()
        {
            HasUpdate = false,
            CurrentVersion = currentVersion,
            LatestVersion = currentVersion
        };

    private static async Task<string?> TryLoadLocalManifestAsync(CancellationToken cancellationToken)
    {
        var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update", "version.json");
        if (!File.Exists(localPath))
        {
            return null;
        }

        await Task.CompletedTask;
        return localPath;
    }

    private async Task DownloadFileAsync(
        string url,
        string destination,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1;
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = File.Create(destination);

        var buffer = new byte[81920];
        long downloaded = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            downloaded += read;

            if (total > 0)
            {
                progress?.Report(downloaded * 100d / total);
            }
        }
    }
}
