using System.Reflection;
using WF.MES.Core;

namespace WF.MES.WPF.Ui;

/// <summary>从 WPF 入口程序集读取 InformationalVersion。</summary>
public sealed class EntryAssemblyAppVersion : IAppVersion
{
    public const string Default = "1.0.0.0";

    public EntryAssemblyAppVersion()
    {
        Current = ResolveVersion();
    }

    public string Current { get; }

    private static string ResolveVersion()
    {
        var entry = Assembly.GetEntryAssembly();
        if (entry == null)
        {
            return Default;
        }

        var informational = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plusIndex = informational.IndexOf('+', StringComparison.Ordinal);
            return plusIndex >= 0 ? informational[..plusIndex] : informational;
        }

        return entry.GetName().Version?.ToString() ?? Default;
    }
}
