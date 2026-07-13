using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using WF.MES.Core;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure;
using WF.MES.WPF.Infrastructure;
using WF.MES.WPF.Modules.Mes.ViewModels;
using WF.MES.WPF.Modules.Mes.Views;
using WF.MES.WPF.Modules.Equipment.ViewModels;
using WF.MES.WPF.Modules.Equipment.Views;
using WF.MES.WPF.DeviceAdapters;
using WF.MES.WPF.Modules.Barcode.ViewModels;
using WF.MES.WPF.Modules.Barcode.Views;
using WF.MES.WPF.ViewModels.Login;
using WF.MES.WPF.ViewModels.Shell;
using WF.MES.WPF.Views.Login;
using WF.MES.WPF.Views.Shell;

namespace WF.MES.WPF;

/// <summary>
/// 应用入口：Prism 容器注册、单实例、全局异常兜底、Serilog 日志。
/// </summary>
public partial class App : PrismApplication
{
    private static readonly IConfiguration AppConfiguration = BuildConfiguration();
    private static readonly IAppVersion ApplicationVersion = new EntryAssemblyAppVersion();
    private SingleInstanceGuard? _singleInstanceGuard;

    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigureLogging();

        // 工位客户端一机一进程，避免多开引发条码/BarTender/会话冲突（与会话表互补）
        if (!SingleInstanceGuard.TryAcquire(out _singleInstanceGuard))
        {
            Log.Warning("检测到重复启动，已拒绝");
            MessageBox.Show(WpfLocalization.T("ui.app.singleInstance"), WpfLocalization.T("ui.app.tip"), MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // 兜底 ViewModel async void、fire-and-forget 等未捕获异常，避免静默崩溃
        RegisterGlobalExceptionHandlers();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("WF MES 退出");
        _singleInstanceGuard?.Dispose();
        _singleInstanceGuard = null;
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    protected override Window CreateShell()
    {
        // Prism 先 CreateShell 再 OnInitialized；必须在解析 LoginView 前挂好 Loc，
        // 否则 Loc.Key 会绑到 DesignTime Fallback，语言切换只更新 VM、不更新 XAML。
        EnsureUiLocalization();
        return Container.Resolve<LoginView>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        InfrastructureServiceRegistration.RegisterServices(containerRegistry, AppConfiguration, ApplicationVersion);
        containerRegistry.RegisterSingleton<DeviceAdapterRegistry>();

        containerRegistry.Register<LoginView>();
        containerRegistry.Register<ShellView>();
        containerRegistry.Register<LoginViewModel>();
        containerRegistry.Register<ShellViewModel>();
        containerRegistry.Register<Func<ShellView>>(container => () => container.Resolve<ShellView>());

        RegisterModuleViews(containerRegistry);
    }

    protected override void OnInitialized()
    {
        EnsureUiLocalization();
        base.OnInitialized();
    }

    private void EnsureUiLocalization()
    {
        WpfLocalization.Use(Container.Resolve<ILocalizationService>());
        if (Resources["Loc"] is not LocalizationBindingSource)
        {
            Resources["Loc"] = new LocalizationBindingSource(WpfLocalization.Instance);
        }
    }

    /// <summary>
    /// Prism 导航注册；Component 须与 System_Menu.Component 一致。
    /// </summary>
    private static void RegisterModuleViews(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<WorkOrderScanView, WorkOrderScanViewModel>("Mes.WorkOrderScan");
        containerRegistry.RegisterForNavigation<BoxReworkView, BoxReworkViewModel>("Mes.BoxRework");
        containerRegistry.RegisterForNavigation<AssemblyView, AssemblyViewModel>("Mes.Assembly");
        containerRegistry.RegisterForNavigation<MaterialMaintainView, MaterialMaintainViewModel>("Material.Maintain");
        containerRegistry.RegisterForNavigation<MaterialScanView, MaterialScanViewModel>("Material.Scan");

        containerRegistry.RegisterForNavigation<CustomerManageView, CustomerManageViewModel>("Barcode.Customer");
        containerRegistry.RegisterForNavigation<MaterialRuleView, MaterialRuleViewModel>("Barcode.MaterialRule");
        containerRegistry.RegisterForNavigation<BarcodePrintView, BarcodePrintViewModel>("Barcode.Print");
        containerRegistry.RegisterForNavigation<BarcodeDetailView, BarcodeDetailViewModel>("Barcode.Detail");
        containerRegistry.RegisterForNavigation<BarcodeReprintView, BarcodeReprintViewModel>("Barcode.Reprint");
        containerRegistry.RegisterForNavigation<BarcodeQaReviewView, BarcodeQaReviewViewModel>("Barcode.QaReview");
        containerRegistry.RegisterForNavigation<EquipmentTestView, EquipmentTestViewModel>("Equipment.Test");
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#else
            .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
#endif
            .Build();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(AppConfiguration)
            .CreateLogger();

        Log.Information("WF MES 启动，版本 {Version}", ApplicationVersion.Current);
    }

    /// <summary>
    /// 注册 UI / 进程 / Task 三层未捕获异常处理。
    /// </summary>
    private static void RegisterGlobalExceptionHandlers()
    {
        Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// UI 线程未捕获异常（含 async void 延续抛出的异常）。
    /// </summary>
    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "UI 线程未捕获异常");
        NotifyUser(e.Exception, WpfLocalization.T("ui.app.runtimeError"));
        // 标记已处理，避免 WPF 默认行为直接终止进程
        e.Handled = true;
    }

    /// <summary>
    /// 非 UI 线程或未进入 Dispatcher 的致命异常。
    /// </summary>
    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "进程级未捕获异常，IsTerminating={IsTerminating}", e.IsTerminating);
            TryNotifyOnUiThread(ex, WpfLocalization.T("ui.app.fatalError"));
        }
        else
        {
            Log.Fatal(
                "进程级未捕获异常: {ExceptionObject}, IsTerminating={IsTerminating}",
                e.ExceptionObject,
                e.IsTerminating);
        }
    }

    /// <summary>
    /// fire-and-forget 任务（如 _ = SomeAsync()）中未观察到的异常。
    /// </summary>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "后台任务未观察到的异常");
        // 防止 .NET 因未观察 Task 异常而终止进程
        e.SetObserved();
        TryNotifyOnUiThread(e.Exception, WpfLocalization.T("ui.app.backgroundError"));
    }

    private static void TryNotifyOnUiThread(Exception exception, string title)
    {
        var dispatcher = Current?.Dispatcher;
        if (dispatcher == null || dispatcher.HasShutdownStarted)
        {
            return;
        }

        if (dispatcher.CheckAccess())
        {
            NotifyUser(exception, title);
        }
        else
        {
            dispatcher.BeginInvoke(() => NotifyUser(exception, title));
        }
    }

    private static void NotifyUser(Exception exception, string title)
    {
        var message = string.Format(WpfLocalization.T("ui.app.errorDetail"), title, exception.Message);
        try
        {
            HandyControl.Controls.Growl.Error(new HandyControl.Data.GrowlInfo
            {
                Message = message,
                WaitTime = 5
            });
        }
        catch
        {
            MessageBox.Show(message, "WF MES", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
