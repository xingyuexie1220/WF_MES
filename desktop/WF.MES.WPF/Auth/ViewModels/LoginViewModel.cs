using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using FluentValidation;
using WF.MES.Core;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Auth.Views;
using WF.MES.WPF.Shell.Views;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Auth.ViewModels;

/// <summary>登录页：更新检查、选语言、API 认证、进入 Shell。静态文案走 XAML <c>Loc.Key</c>。</summary>
public class LoginViewModel : LocalizedViewModelBase, IDisposable
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IUpdateService _updateService;
    private readonly IMenuPermissionService _menuPermissionService;
    private readonly IValidator<PasswordChangeDto> _passwordValidator;
    private readonly IRegionManager _regionManager;
    private readonly Func<ShellView> _shellViewFactory;
    private readonly IAppVersion _appVersion;
    private readonly EventHandler _localeChangedHandler;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private string _statusMessageKey = "ui.checkingUpdate";
    private object[] _statusMessageArgs = [];
    private LocaleDisplayItem? _selectedLocaleOption;
    private bool _disposed;

    public LoginViewModel(
        IAuthService authService,
        ISessionService sessionService,
        IUpdateService updateService,
        IMenuPermissionService menuPermissionService,
        IValidator<PasswordChangeDto> passwordValidator,
        IRegionManager regionManager,
        ILocalizationService localization,
        Func<ShellView> shellViewFactory,
        IAppVersion appVersion)
        : base(localization)
    {
        _authService = authService;
        _sessionService = sessionService;
        _updateService = updateService;
        _menuPermissionService = menuPermissionService;
        _passwordValidator = passwordValidator;
        _regionManager = regionManager;
        _shellViewFactory = shellViewFactory;
        _appVersion = appVersion;

        LocaleOptions = [];
        SetStatusMessage("ui.checkingUpdate");

        LoginCommand = new DelegateCommand(async () => await LoginAsync(), CanLogin)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);

        _localeChangedHandler = (_, _) => RefreshLoginTexts();
        Localization.LocaleChanged += _localeChangedHandler;
        RefreshLoginTexts();
    }

    public ObservableCollection<LocaleDisplayItem> LocaleOptions { get; }

    public LocaleDisplayItem? SelectedLocaleOption
    {
        get => _selectedLocaleOption;
        set
        {
            if (!SetProperty(ref _selectedLocaleOption, value) || value is null)
            {
                return;
            }

            Localization.SetLocale(value.Value);
        }
    }

    /// <summary>HandyControl Placeholder 无法用 Loc.Key，保留动态属性。</summary>
    public string UsernamePlaceholder => L("login.username");

    public string VersionText => Lf("ui.version", _appVersion.Current);

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public DelegateCommand LoginCommand { get; }

    public async Task InitializeAsync() => await InitializeInternalAsync();

    private void RefreshLoginTexts()
    {
        LocaleOptions.Clear();
        foreach (var option in Localization.LocaleOptions)
        {
            LocaleOptions.Add(new LocaleDisplayItem(option.Value, L(option.LabelKey)));
        }

        SyncSelectedLocaleOption();
        RaisePropertyChanged(nameof(UsernamePlaceholder));
        RaisePropertyChanged(nameof(VersionText));
        ApplyStatusMessage();
    }

    private void SyncSelectedLocaleOption()
    {
        var match = LocaleOptions.FirstOrDefault(option =>
            string.Equals(option.Value, Localization.CurrentLocale, StringComparison.OrdinalIgnoreCase));

        if (SetProperty(ref _selectedLocaleOption, match, nameof(SelectedLocaleOption)))
        {
            return;
        }

        RaisePropertyChanged(nameof(SelectedLocaleOption));
    }

    private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

    private async Task InitializeInternalAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            SetStatusMessage("ui.checkingUpdate");
            var updateInfo = await _updateService.CheckForUpdateAsync();

            if (updateInfo.HasUpdate)
            {
                SetStatusMessage("ui.updateFound", updateInfo.LatestVersion);
                if (!string.IsNullOrWhiteSpace(updateInfo.ReleaseNotes))
                {
                    Log.Information("更新说明：{ReleaseNotes}", updateInfo.ReleaseNotes);
                }

                await ApplyUpdateAsync(updateInfo);
                return;
            }

            SetStatusMessage("ui.readyToLogin", updateInfo.CurrentVersion);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "初始化登录页失败");
            SetStatusMessage("ui.updateCheckFailed");
            HandyControl.Controls.Growl.Warning(L("ui.updateCheckFailed"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyUpdateAsync(UpdateCheckResult updateInfo)
    {
        IsBusy = true;
        SetStatusMessage("ui.downloadingUpdate");

        try
        {
            var progress = new Progress<double>(value =>
                SetStatusMessage("ui.downloadingProgress", value));
            var success = await _updateService.DownloadAndApplyAsync(updateInfo, progress);

            if (success)
            {
                HandyControl.Controls.Growl.Info(L("ui.updateCompleteRestart"));
                Application.Current.Shutdown();
            }
            else
            {
                HandyControl.Controls.Growl.Error(L("ui.updateFailedContinue"));
                SetStatusMessage("ui.updateFailedContinue");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "应用更新失败");
            HandyControl.Controls.Growl.Error(L("ui.updateFailedContinue"));
            SetStatusMessage("ui.updateFailedContinue");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        SetStatusMessage("ui.loggingIn");

        try
        {
            var loginResult = await _authService.LoginAsync(UserName, Password);
            if (loginResult.NeedSelectFactory && loginResult.Factories.Count > 0)
            {
                var selectWindow = new FactorySelectWindow(loginResult.Factories, Localization)
                {
                    Owner = Application.Current.Windows.OfType<LoginView>().FirstOrDefault()
                };
                if (selectWindow.ShowDialog() != true || selectWindow.SelectedFactory == null)
                {
                    SetStatusMessage("ui.selectFactoryContinue");
                    return;
                }

                loginResult = await _authService.SelectFactoryAsync(UserName, Password, selectWindow.SelectedFactory.Id);
            }

            if (!loginResult.Success || loginResult.User is null)
            {
                HandyControl.Controls.Growl.Warning(loginResult.ErrorMessage ?? L("auth.invalid_credentials"));
                SetStatusMessage("ui.loginFailed");
                return;
            }

            var user = loginResult.User;
            var currentPassword = Password;

            if (user.MustChangePassword)
            {
                var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
                var displayName = user.NickName ?? user.UserName;
                var passwordChanged = ChangePasswordWindow.ShowDialog(
                    _authService,
                    _passwordValidator,
                    Localization,
                    currentPassword,
                    displayName,
                    loginWindow);

                if (!passwordChanged)
                {
                    await _authService.LogoutAsync();
                    SetStatusMessage("ui.mustChangePassword");
                    HandyControl.Controls.Growl.Warning(L("ui.pleaseChangePassword"));
                    return;
                }

                user.MustChangePassword = false;
            }

            _sessionService.SetUser(user);
            _sessionService.SetActionPermissions(user.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));

            var permissions = await _menuPermissionService.GetUserPermissionsAsync((int)user.Id);
            _sessionService.SetPermissions(permissions);

            if (permissions.Count == 0)
            {
                HandyControl.Controls.Growl.Warning(L("ui.noDesktopMenu"));
            }

            var shell = _shellViewFactory();
            RegionManager.SetRegionManager(shell, _regionManager);
            shell.Show();
            RegionManager.UpdateRegions();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    window.Close();
                    break;
                }
            }

            Application.Current.MainWindow = shell;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "登录异常");
            HandyControl.Controls.Growl.Error(L("ui.loginError"));
            SetStatusMessage("ui.loginError");
            await _authService.LogoutAsync();
        }
        finally
        {
            IsBusy = false;
            SetStatusMessage("ui.loginHint");
        }
    }

    private void SetStatusMessage(string key, params object[] args)
    {
        _statusMessageKey = key;
        _statusMessageArgs = args;
        ApplyStatusMessage();
    }

    private void ApplyStatusMessage()
    {
        StatusMessage = _statusMessageArgs.Length == 0
            ? L(_statusMessageKey)
            : string.Format(L(_statusMessageKey), _statusMessageArgs);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Localization.LocaleChanged -= _localeChangedHandler;
        _disposed = true;
    }
}
