namespace WF.MES.Core;

/// <summary>宿主程序版本号（由 WPF/Web 等入口注入，Core 不自行读取程序集）。</summary>
public interface IAppVersion
{
    string Current { get; }
}
