using System.Diagnostics;

namespace WF.MES.Updater;

/// <summary>
/// 主程序退出后由 UpdateService 拉起：等待主进程结束，复制更新文件，重启 WF.MES.WPF。
/// </summary>
internal static class Program
{
    private const int MaxCopyRetries = 8;
    private const int ProcessWaitSeconds = 120;
    private const string LabelsFolderName = "Labels";
    private const string LabelTemplateExtension = ".btw";

    private static int Main(string[] args)
    {
        string? logPath = null;

        try
        {
            var options = UpdaterOptions.Parse(args);
            logPath = Path.Combine(options.Target, "Logs", $"updater-{DateTime.Now:yyyyMMdd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            Log(logPath, $"更新开始 PID={options.ProcessId} Source={options.Source} Target={options.Target}");

            WaitForProcessExit(options.ProcessId, TimeSpan.FromSeconds(ProcessWaitSeconds), logPath);
            CopyDirectory(options.Source, options.Target, logPath);

            var exePath = Path.Combine(options.Target, options.Executable);
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException("找不到主程序", exePath);
            }

            Log(logPath, $"更新完成，启动 {exePath}");
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = options.Target,
                UseShellExecute = true
            });

            return 0;
        }
        catch (Exception ex)
        {
            if (logPath != null)
            {
                Log(logPath, $"更新失败: {ex}");
            }

            return 1;
        }
    }

    private static void WaitForProcessExit(int processId, TimeSpan timeout, string logPath)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            Log(logPath, $"等待主程序退出 PID={processId}");
            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                Log(logPath, $"等待超时 ({timeout.TotalSeconds:F0}s)，继续覆盖文件");
            }
        }
        catch (ArgumentException)
        {
            Log(logPath, "主程序已退出");
        }
    }

    private static void CopyDirectory(string source, string target, string logPath)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException($"更新源目录不存在: {source}");
        }

        Directory.CreateDirectory(target);

        var skippedLabelTemplates = 0;

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(source, file);
            var destination = Path.Combine(target, relativePath);
            var destinationDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            if (ShouldPreserveExistingLabelTemplate(relativePath, destination))
            {
                skippedLabelTemplates++;
                Log(logPath, $"跳过已存在的标签模板: {relativePath}");
                continue;
            }

            CopyFileWithRetry(file, destination, logPath);
        }

        if (skippedLabelTemplates > 0)
        {
            Log(logPath, $"已保留本地标签模板 {skippedLabelTemplates} 个");
        }

        Log(logPath, "文件覆盖完成");
    }

    /// <summary>
    /// 保留工位本地微调过的 BarTender 模板：Labels 下已存在的 .btw 不覆盖。
    /// </summary>
    private static bool ShouldPreserveExistingLabelTemplate(string relativePath, string destination)
    {
        if (!File.Exists(destination))
        {
            return false;
        }

        if (!string.Equals(Path.GetExtension(relativePath), LabelTemplateExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var labelsPrefix = LabelsFolderName + Path.DirectorySeparatorChar;
        var labelsPrefixAlt = LabelsFolderName + Path.AltDirectorySeparatorChar;
        return relativePath.StartsWith(labelsPrefix, StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith(labelsPrefixAlt, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetDirectoryName(relativePath), LabelsFolderName, StringComparison.OrdinalIgnoreCase);
    }

    private static void CopyFileWithRetry(string source, string destination, string logPath)
    {
        for (var attempt = 1; attempt <= MaxCopyRetries; attempt++)
        {
            try
            {
                File.Copy(source, destination, overwrite: true);
                return;
            }
            catch (IOException ex) when (attempt < MaxCopyRetries)
            {
                Log(logPath, $"复制重试 {attempt}/{MaxCopyRetries}: {destination} ({ex.Message})");
                Thread.Sleep(500 * attempt);
            }
        }
    }

    private static void Log(string logPath, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}";
        File.AppendAllText(logPath, line + Environment.NewLine);
    }
}

internal sealed class UpdaterOptions
{
    public int ProcessId { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Executable { get; init; } = "WF.MES.WPF.exe";

    public static UpdaterOptions Parse(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"参数 {key} 缺少值");
            }

            map[key] = args[++i];
        }

        if (!map.TryGetValue("--pid", out var pidText) || !int.TryParse(pidText, out var pid))
        {
            throw new ArgumentException("缺少有效参数 --pid");
        }

        if (!map.TryGetValue("--source", out var source) || string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("缺少参数 --source");
        }

        if (!map.TryGetValue("--target", out var target) || string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("缺少参数 --target");
        }

        map.TryGetValue("--exe", out var executable);

        return new UpdaterOptions
        {
            ProcessId = pid,
            Source = Path.GetFullPath(source),
            Target = Path.GetFullPath(target),
            Executable = string.IsNullOrWhiteSpace(executable) ? "WF.MES.WPF.exe" : executable
        };
    }
}
