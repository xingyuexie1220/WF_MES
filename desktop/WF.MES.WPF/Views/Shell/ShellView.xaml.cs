using System.Windows;
using WF.MES.WPF.ViewModels.Shell;

namespace WF.MES.WPF.Views.Shell;

/// <summary>主窗口：承载 MainRegion，关闭时注销 API 会话。</summary>
public partial class ShellView : HandyControl.Controls.Window
{
    private readonly IRegionManager _regionManager;

    public ShellView(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
        {
            await viewModel.ReleaseSessionAsync();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (RegionManager.GetRegionManager(this) == null)
        {
            RegionManager.SetRegionManager(this, _regionManager);
        }

        RegionManager.UpdateRegions();

        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.OnShellLoaded();
        }
    }
}
