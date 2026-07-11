using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using WF.MES.WPF.Views.Login;
using Microsoft.Extensions.Configuration;
using Serilog;
using WF.MES.Core;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.WPF.ViewModels.Shell;

/// <summary>主界面：侧栏权限菜单、Prism 导航、会话心跳与退出</summary>
public class ShellViewModel : BindableBase
{
    private readonly ISessionService _sessionService;
    private readonly IAuthService _authService;
    private readonly IRegionManager _regionManager;
    private readonly ISopService _sopService;
    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly IApiHealthService _apiHealthService;
    private readonly ILocalizationService _localization;
    private readonly IMenuPermissionService _menuPermissionService;
    private readonly IAppVersion _appVersion;
    private readonly int _heartbeatIntervalSeconds;

    private string _welcomeText = string.Empty;
    private string _emptyHint = string.Empty;
    private string _selectedLocale = string.Empty;
    private string _apiStatusText = string.Empty;
    private string _databaseStatusText = string.Empty;
    private string _currentTimeText = string.Empty;
    private ModuleNavItem? _selectedModule;
    private MenuNavItem? _selectedMenu;
    private bool _isApiConnected;
    private bool _isDatabaseConnected;
    private bool _initialNavigationDone;
    private DispatcherTimer? _heartbeatTimer;
    private DispatcherTimer? _clockTimer;
    private bool _heartbeatTickRunning;
    private bool _sessionReleased;

    public ShellViewModel(
        ISessionService sessionService,
        IAuthService authService,
        IRegionManager regionManager,
        ISopService sopService,
        IDatabaseHealthService databaseHealthService,
        IApiHealthService apiHealthService,
        ILocalizationService localization,
        IMenuPermissionService menuPermissionService,
        IConfiguration configuration,
        IAppVersion appVersion)
    {
        _sessionService = sessionService;
        _authService = authService;
        _regionManager = regionManager;
        _sopService = sopService;
        _databaseHealthService = databaseHealthService;
        _apiHealthService = apiHealthService;
        _localization = localization;
        _menuPermissionService = menuPermissionService;
        _appVersion = appVersion;
        _heartbeatIntervalSeconds = Math.Max(configuration.GetValue("Session:HeartbeatIntervalSeconds", 60), 10);

        LocaleOptions = new ObservableCollection<LocaleDisplayItem>(
            _localization.LocaleOptions.Select(option =>
                new LocaleDisplayItem(option.Value, _localization.T(option.LabelKey))));
        _selectedLocale = _localization.CurrentLocale;

        LogoutCommand = new DelegateCommand(async () => await LogoutAsync());
        OpenSopCommand = new DelegateCommand(OpenSop);
        SwitchFactoryCommand = new DelegateCommand(async () => await SwitchFactoryAsync());
        SelectMenuCommand = new DelegateCommand<MenuNavItem?>(OnSelectMenu);

        _localization.LocaleChanged += async (_, _) => await OnLocaleChangedAsync();

        RefreshLocalizedTexts();
        RefreshWelcomeText();
        RefreshCurrentTime();
        LoadModules();
        StartHeartbeat();
        StartClockTimer();
        _ = RefreshConnectionStatusAsync();
    }

    public ObservableCollection<LocaleDisplayItem> LocaleOptions { get; }

    public string SelectedLocale
    {
        get => _selectedLocale;
        set
        {
            if (!SetProperty(ref _selectedLocale, value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _localization.SetLocale(value);
        }
    }

    public string HeaderTitle => _localization.T("desktop.headerTitle");
    public string LogoutText => _localization.T("desktop.logout");
    public string OpenSopText => _localization.T("desktop.openSop");
    public string SwitchFactoryText => _localization.T("desktop.factory.switch");
    public string LocaleLabel => _localization.T("desktop.locale.label");
    public string CurrentFactoryText => _sessionService.CurrentUser?.FactoryName ?? _localization.T("desktop.factory.current");
    public bool CanSwitchFactory => (_sessionService.CurrentUser?.AccessibleFactories.Count ?? 0) > 1;
    public string VersionText => string.Format(_localization.T("desktop.version"), _appVersion.Current);
    public string MachineNameText => string.Format(_localization.T("desktop.machineName"), Environment.MachineName);
    public string MenuCountFormat => _localization.T("desktop.menuCount");

    /// <summary>Shell 窗口加载并完成 Region 注册后调用，触发首次导航。</summary>
    public void OnShellLoaded()
    {
        if (_initialNavigationDone || Modules.Count == 0)
        {
            return;
        }

        _initialNavigationDone = true;
        ActivateModule(Modules[0], selectFirstMenu: true);
    }

    /// <summary>窗口关闭时注销 API 会话。</summary>
    public async Task ReleaseSessionAsync()
    {
        if (_sessionReleased)
        {
            return;
        }

        _sessionReleased = true;
        _heartbeatTimer?.Stop();
        _clockTimer?.Stop();

        try
        {
            await _authService.LogoutAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "注销 API 会话失败");
        }

        _sessionService.Clear();
    }

    /// <summary>功能模块列表，每个模块下包含多个二级小菜单。</summary>
    public ObservableCollection<ModuleNavItem> Modules { get; } = [];

    public ModuleNavItem? SelectedModule
    {
        get => _selectedModule;
        set => SetProperty(ref _selectedModule, value);
    }

    public MenuNavItem? SelectedMenu
    {
        get => _selectedMenu;
        set => SetProperty(ref _selectedMenu, value);
    }

    public string WelcomeText
    {
        get => _welcomeText;
        private set => SetProperty(ref _welcomeText, value);
    }

    public string ApiStatusText
    {
        get => _apiStatusText;
        private set => SetProperty(ref _apiStatusText, value);
    }

    public string DatabaseStatusText
    {
        get => _databaseStatusText;
        private set => SetProperty(ref _databaseStatusText, value);
    }

    public bool IsApiConnected
    {
        get => _isApiConnected;
        private set => SetProperty(ref _isApiConnected, value);
    }

    public bool IsDatabaseConnected
    {
        get => _isDatabaseConnected;
        private set => SetProperty(ref _isDatabaseConnected, value);
    }

    public string CurrentTimeText
    {
        get => _currentTimeText;
        private set => SetProperty(ref _currentTimeText, value);
    }

    public string EmptyHint
    {
        get => _emptyHint;
        private set
        {
            if (SetProperty(ref _emptyHint, value))
            {
                RaisePropertyChanged(nameof(ShowEmptyHint));
            }
        }
    }

    public bool ShowEmptyHint => !string.IsNullOrEmpty(EmptyHint);

    public DelegateCommand LogoutCommand { get; }

    public DelegateCommand OpenSopCommand { get; }

    public DelegateCommand SwitchFactoryCommand { get; }

    public DelegateCommand<MenuNavItem?> SelectMenuCommand { get; }

    private void LoadModules()
    {
        Modules.Clear();

        var countFormat = MenuCountFormat;
        foreach (var module in _sessionService.PermittedModules)
        {
            var item = new ModuleNavItem(module);
            item.RefreshSubMenuCountText(countFormat);
            Modules.Add(item);
        }

        if (Modules.Count == 0)
        {
            EmptyHint = _localization.T("desktop.noPermission");
            return;
        }

        EmptyHint = _localization.T("desktop.emptyHint");
    }

    private void ActivateModule(ModuleNavItem module, bool selectFirstMenu)
    {
        foreach (var item in Modules)
        {
            item.IsSelected = item.ModuleId == module.ModuleId;
        }

        module.IsExpanded = true;
        SelectedModule = module;

        if (module.SubMenus.Count == 0)
        {
            SelectedMenu = null;
            EmptyHint = string.Format(_localization.T("desktop.noMenusInModule"), module.ModuleName);
            return;
        }

        if (selectFirstMenu)
        {
            SelectMenuCommand.Execute(module.SubMenus[0]);
        }
    }

    private void OnSelectMenu(MenuNavItem? menu)
    {
        if (menu == null || string.IsNullOrWhiteSpace(menu.ViewName))
        {
            return;
        }

        var parentModule = Modules.FirstOrDefault(m => m.ModuleId == menu.ModuleId);
        if (parentModule != null)
        {
            ActivateModule(parentModule, selectFirstMenu: false);
        }

        foreach (var module in Modules)
        {
            foreach (var subMenu in module.SubMenus)
            {
                subMenu.IsSelected = subMenu.MenuId == menu.MenuId;
            }
        }

        SelectedMenu = menu;
        EmptyHint = string.Empty;

        if (!_regionManager.Regions.ContainsRegionWithName(RegionNames.MainRegion))
        {
            Log.Warning("主内容区 {Region} 尚未注册，导航推迟", RegionNames.MainRegion);
            EmptyHint = _localization.T("desktop.regionNotReady");
            return;
        }

        _regionManager.RequestNavigate(RegionNames.MainRegion, menu.ViewName, result =>
        {
            if (result.Success != true)
            {
                var error = result.Exception?.Message ?? "未知错误";
                Log.Error(result.Exception, "导航到 {ViewName} 失败: {Error}", menu.ViewName, error);
                EmptyHint = string.Format(_localization.T("desktop.menuLoadFailed"), menu.MenuName, error);
            }
        });
    }

    private void RefreshWelcomeText()
    {
        var user = _sessionService.CurrentUser;
        WelcomeText = user == null
            ? _localization.T("desktop.welcomeDefault")
            : string.Format(_localization.T("desktop.welcomeUser"), user.NickName ?? user.UserName);
        RaisePropertyChanged(nameof(CurrentFactoryText));
        RaisePropertyChanged(nameof(CanSwitchFactory));
    }

    private async Task SwitchFactoryAsync()
    {
        var user = _sessionService.CurrentUser;
        if (user is null)
        {
            return;
        }

        var factories = user.AccessibleFactories;
        if (factories.Count <= 1)
        {
            HandyControl.Controls.Growl.Info(_localization.T("common.noData"));
            return;
        }

        var selectWindow = new Views.Login.FactorySelectWindow(factories, _localization)
        {
            Owner = Application.Current.MainWindow
        };

        if (selectWindow.ShowDialog() != true || selectWindow.SelectedFactory is null)
        {
            return;
        }

        if (selectWindow.SelectedFactory.Id == user.FactoryId)
        {
            return;
        }

        var result = await _authService.SwitchFactoryAsync(selectWindow.SelectedFactory.Id);
        if (!result.Success || result.User is null)
        {
            HandyControl.Controls.Growl.Warning(result.ErrorMessage ?? _localization.T("desktop.factory.switchFailed"));
            return;
        }

        _sessionService.SetUser(result.User);
        _sessionService.SetActionPermissions(result.User.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
        var permissions = await _menuPermissionService.GetUserPermissionsAsync((int)result.User.Id);
        _sessionService.SetPermissions(permissions);
        _initialNavigationDone = false;
        LoadModules();
        RefreshWelcomeText();
        if (Modules.Count > 0)
        {
            _initialNavigationDone = true;
            ActivateModule(Modules[0], selectFirstMenu: true);
        }

        HandyControl.Controls.Growl.Success(_localization.T("desktop.factory.switchSuccess"));
    }

    private async Task OnLocaleChangedAsync()
    {
        RefreshLocalizedTexts();
        RefreshWelcomeText();

        var user = _sessionService.CurrentUser;
        if (user is null)
        {
            return;
        }

        var permissions = await _menuPermissionService.GetUserPermissionsAsync((int)user.Id);
        _sessionService.SetPermissions(permissions);
        LoadModules();
    }

    private void RefreshLocalizedTexts()
    {
        if (!string.Equals(_selectedLocale, _localization.CurrentLocale, StringComparison.OrdinalIgnoreCase))
        {
            _selectedLocale = _localization.CurrentLocale;
            RaisePropertyChanged(nameof(SelectedLocale));
        }

        LocaleOptions.Clear();
        foreach (var option in _localization.LocaleOptions)
        {
            LocaleOptions.Add(new LocaleDisplayItem(option.Value, _localization.T(option.LabelKey)));
        }

        RaisePropertyChanged(nameof(HeaderTitle));
        RaisePropertyChanged(nameof(LogoutText));
        RaisePropertyChanged(nameof(OpenSopText));
        RaisePropertyChanged(nameof(SwitchFactoryText));
        RaisePropertyChanged(nameof(LocaleLabel));
        RaisePropertyChanged(nameof(CurrentFactoryText));
        RaisePropertyChanged(nameof(CanSwitchFactory));
        RaisePropertyChanged(nameof(VersionText));
        RaisePropertyChanged(nameof(MachineNameText));
        RaisePropertyChanged(nameof(MenuCountFormat));

        var countFormat = MenuCountFormat;
        foreach (var module in Modules)
        {
            module.RefreshSubMenuCountText(countFormat);
        }
    }

    private void OpenSop()
    {
        try
        {
            _sopService.OpenPdf();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "打开 SOP 文档失败");
            HandyControl.Controls.Growl.Error(ex.Message);
        }
    }

    private async Task LogoutAsync()
    {
        await ReleaseSessionAsync();
        Application.Current.Shutdown();
    }

    private void StartClockTimer()
    {
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _clockTimer.Tick += (_, _) => RefreshCurrentTime();
        _clockTimer.Start();
    }

    private void RefreshCurrentTime()
    {
        CurrentTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }

    private async Task RefreshConnectionStatusAsync()
    {
        try
        {
            IsApiConnected = await _apiHealthService.CheckConnectionAsync();
            ApiStatusText = IsApiConnected
                ? _localization.T("desktop.apiConnected")
                : _localization.T("desktop.apiDisconnected");

            IsDatabaseConnected = await _databaseHealthService.CheckConnectionAsync();
            DatabaseStatusText = IsDatabaseConnected
                ? _localization.T("desktop.dbConnected")
                : _localization.T("desktop.dbDisconnected");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "连接状态检测失败");
            IsApiConnected = false;
            ApiStatusText = _localization.T("desktop.apiDisconnected");
            DatabaseStatusText = _localization.T("desktop.dbDisconnected");
            IsDatabaseConnected = false;
        }
    }

    private void StartHeartbeat()
    {
        if (_sessionService.CurrentUser is null)
        {
            return;
        }

        _heartbeatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_heartbeatIntervalSeconds)
        };
        _heartbeatTimer.Tick += async (_, _) => await OnHeartbeatTickAsync();
        _heartbeatTimer.Start();
    }

    private async Task OnHeartbeatTickAsync()
    {
        if (_heartbeatTickRunning || _sessionReleased)
        {
            return;
        }

        if (_sessionService.CurrentUser is null)
        {
            return;
        }

        _heartbeatTickRunning = true;
        _heartbeatTimer?.Stop();

        try
        {
            var ok = await _authService.ValidateSessionAsync();
            if (!ok)
            {
                HandleSessionExpired();
                return;
            }

            await RefreshConnectionStatusAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "会话心跳异常");
        }
        finally
        {
            _heartbeatTickRunning = false;
            if (!_sessionReleased)
            {
                _heartbeatTimer?.Start();
            }
        }
    }

    /// <summary>心跳失败（被踢、超时或已在别处登录）：提示后清空会话并退出程序。</summary>
    private void HandleSessionExpired()
    {
        _heartbeatTimer?.Stop();
        _clockTimer?.Stop();
        HandyControl.Controls.Growl.Warning(_localization.T("desktop.sessionExpired"));
        _sessionService.Clear();
        Application.Current.Shutdown();
    }
}

/// <summary>功能模块导航项，包含多个二级小菜单。</summary>
public class ModuleNavItem : BindableBase
{
    private bool _isSelected;
    private bool _isExpanded = true;
    private string _subMenuCountText = string.Empty;

    public ModuleNavItem(ModuleMenuPermissionDto source)
    {
        ModuleId = source.ModuleId;
        ModuleName = source.ModuleName;
        Icon = source.Icon ?? "📦";

        SubMenus = new ObservableCollection<MenuNavItem>(
            source.Menus.Select(menu => new MenuNavItem(menu)));
    }

    public int ModuleId { get; }

    public string ModuleName { get; }

    public string Icon { get; }

    public string SubMenuCountText
    {
        get => _subMenuCountText;
        private set => SetProperty(ref _subMenuCountText, value);
    }

    /// <summary>功能模块下的二级小菜单列表。</summary>
    public ObservableCollection<MenuNavItem> SubMenus { get; }

    public void RefreshSubMenuCountText(string format)
        => SubMenuCountText = string.Format(format, SubMenus.Count);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}

/// <summary>二级小菜单导航项。</summary>
public class MenuNavItem : BindableBase
{
    private bool _isSelected;

    public MenuNavItem(MenuPermissionDto source)
    {
        MenuId = source.MenuId;
        ModuleId = source.ModuleId;
        MenuName = source.MenuName;
        ViewName = source.ViewName;
    }

    public int MenuId { get; }

    public int ModuleId { get; }

    public string MenuName { get; }

    public string ViewName { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
