using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using WF.MES.Core;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Shell.ViewModels;

/// <summary>主界面：侧栏权限菜单、Prism 导航、会话心跳与退出。静态文案走 XAML <c>Loc.Key</c>。</summary>
public class ShellViewModel : LocalizedViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly IAuthService _authService;
    private readonly IRegionManager _regionManager;
    private readonly ISopService _sopService;
    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly IApiHealthService _apiHealthService;
    private readonly IAppVersion _appVersion;
    private readonly int _heartbeatIntervalSeconds;

    private string _welcomeText = string.Empty;
    private string _emptyHint = string.Empty;
    private string? _emptyHintKey;
    private object[] _emptyHintArgs = [];
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
        IConfiguration configuration,
        IAppVersion appVersion)
        : base(localization)
    {
        _sessionService = sessionService;
        _authService = authService;
        _regionManager = regionManager;
        _sopService = sopService;
        _databaseHealthService = databaseHealthService;
        _apiHealthService = apiHealthService;
        _appVersion = appVersion;
        _heartbeatIntervalSeconds = Math.Max(configuration.GetValue("Session:HeartbeatIntervalSeconds", 60), 10);

        LogoutCommand = new DelegateCommand(async () => await LogoutAsync());
        OpenSopCommand = new DelegateCommand(OpenSop);
        SelectMenuCommand = new DelegateCommand<MenuNavItem?>(OnSelectMenu);

        ApplyLocalizedShellText();
        RefreshCurrentTime();
        LoadModules();
        StartHeartbeat();
        StartClockTimer();
        _ = RefreshConnectionStatusAsync();
    }

    public string CurrentFactoryText => _sessionService.CurrentUser?.FactoryName ?? L("ui.factory.current");
    public string VersionText => Lf("ui.version", _appVersion.Current);
    public string MachineNameText => Lf("ui.machineName", Environment.MachineName);

    private string MenuCountFormat => L("ui.menuCount");

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

    public DelegateCommand<MenuNavItem?> SelectMenuCommand { get; }

    private void LoadModules()
    {
        Modules.Clear();

        var countFormat = MenuCountFormat;
        foreach (var module in _sessionService.PermittedModules)
        {
            var item = new ModuleNavItem(module);
            item.RefreshTitles(Localization);
            item.RefreshSubMenuCountText(countFormat);
            Modules.Add(item);
        }

        if (Modules.Count == 0)
        {
            SetEmptyHint("ui.noPermission");
            return;
        }

        SetEmptyHint("ui.emptyHint");
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
            SetEmptyHint("ui.noMenusInModule", module.DisplayName);
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
        ClearEmptyHint();

        if (!_regionManager.Regions.ContainsRegionWithName(RegionNames.MainRegion))
        {
            Log.Warning("主内容区 {Region} 尚未注册，导航推迟", RegionNames.MainRegion);
            SetEmptyHint("ui.regionNotReady");
            return;
        }

        _regionManager.RequestNavigate(RegionNames.MainRegion, menu.ViewName, result =>
        {
            if (result.Success != true)
            {
                var error = result.Exception?.Message ?? L("ui.unknownError");
                Log.Error(result.Exception, "导航到 {ViewName} 失败: {Error}", menu.ViewName, error);
                SetEmptyHint("ui.menuLoadFailed", menu.DisplayName, error);
            }
        });
    }

    private void RefreshWelcomeText()
    {
        var user = _sessionService.CurrentUser;
        WelcomeText = user == null
            ? L("ui.welcomeDefault")
            : Lf("ui.welcomeUser", user.NickName ?? user.UserName);
        RaisePropertyChanged(nameof(CurrentFactoryText));
    }

    private void ApplyLocalizedShellText()
    {
        ApiStatusText = IsApiConnected ? L("ui.apiConnected") : L("ui.apiDisconnected");
        DatabaseStatusText = IsDatabaseConnected ? L("ui.dbConnected") : L("ui.dbDisconnected");
        ApplyEmptyHint();
        RefreshWelcomeText();
    }

    private void SetEmptyHint(string key, params object[] args)
    {
        _emptyHintKey = key;
        _emptyHintArgs = args;
        ApplyEmptyHint();
    }

    private void ClearEmptyHint()
    {
        _emptyHintKey = null;
        _emptyHintArgs = [];
        EmptyHint = string.Empty;
    }

    private void ApplyEmptyHint()
    {
        if (_emptyHintKey is null)
        {
            return;
        }

        EmptyHint = _emptyHintArgs.Length == 0
            ? L(_emptyHintKey)
            : string.Format(L(_emptyHintKey), _emptyHintArgs);
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
        HandyControl.Controls.Growl.Error(Ex(ex));
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
                ? L("ui.apiConnected")
                : L("ui.apiDisconnected");

            IsDatabaseConnected = await _databaseHealthService.CheckConnectionAsync();
            DatabaseStatusText = IsDatabaseConnected
                ? L("ui.dbConnected")
                : L("ui.dbDisconnected");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "连接状态检测失败");
            IsApiConnected = false;
            ApiStatusText = L("ui.apiDisconnected");
            DatabaseStatusText = L("ui.dbDisconnected");
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
        HandyControl.Controls.Growl.Warning(L("ui.sessionExpired"));
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
    private string _displayName = string.Empty;

    public ModuleNavItem(ModuleMenuPermissionDto source)
    {
        ModuleId = source.ModuleId;
        I18nKey = source.I18nKey;
        TitleFallback = source.TitleFallback;
        Icon = source.Icon ?? "📦";
        DisplayName = TitleFallback;

        SubMenus = new ObservableCollection<MenuNavItem>(
            source.Menus.Select(menu => new MenuNavItem(menu)));
    }

    public int ModuleId { get; }

    public string? I18nKey { get; }

    public string TitleFallback { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public string Icon { get; }

    public string SubMenuCountText
    {
        get => _subMenuCountText;
        private set => SetProperty(ref _subMenuCountText, value);
    }

    /// <summary>功能模块下的二级小菜单列表。</summary>
    public ObservableCollection<MenuNavItem> SubMenus { get; }

    public void RefreshTitles(ILocalizationService localization)
    {
        DisplayName = string.IsNullOrWhiteSpace(I18nKey)
            ? TitleFallback
            : localization.T(I18nKey, TitleFallback);

        foreach (var menu in SubMenus)
        {
            menu.RefreshTitle(localization);
        }
    }

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
    private string _displayName = string.Empty;

    public MenuNavItem(MenuPermissionDto source)
    {
        MenuId = source.MenuId;
        ModuleId = source.ModuleId;
        I18nKey = source.I18nKey;
        TitleFallback = source.TitleFallback;
        ViewName = source.ViewName;
        DisplayName = TitleFallback;
    }

    public int MenuId { get; }

    public int ModuleId { get; }

    public string? I18nKey { get; }

    public string TitleFallback { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public string ViewName { get; }

    public void RefreshTitle(ILocalizationService localization)
    {
        DisplayName = string.IsNullOrWhiteSpace(I18nKey)
            ? TitleFallback
            : localization.T(I18nKey, TitleFallback);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
