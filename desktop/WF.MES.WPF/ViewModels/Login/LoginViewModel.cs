using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using FluentValidation;
using WF.MES.Core;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Views.Shell;

namespace WF.MES.WPF.ViewModels.Login;

/// <summary>登录页：更新检查、API 认证、进入 Shell。</summary>
public class LoginViewModel : BindableBase
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IUpdateService _updateService;
    private readonly IMenuPermissionService _menuPermissionService;
    private readonly IValidator<PasswordChangeDto> _passwordValidator;
    private readonly IRegionManager _regionManager;
    private readonly ILocalizationService _localization;
    private readonly Func<ShellView> _shellViewFactory;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "正在检查更新...";

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
    {
        _authService = authService;
        _sessionService = sessionService;
        _updateService = updateService;
        _menuPermissionService = menuPermissionService;
        _passwordValidator = passwordValidator;
        _regionManager = regionManager;
        _localization = localization;
        _shellViewFactory = shellViewFactory;

        VersionText = string.Format(_localization.T("desktop.version"), appVersion.Current);
        LocaleOptions = new ObservableCollection<Views.Login.LocaleDisplayItem>(
            _localization.LocaleOptions.Select(option =>
                new Views.Login.LocaleDisplayItem(option.Value, _localization.T(option.LabelKey))));

        _selectedLocale = _localization.CurrentLocale;
        _localization.LocaleChanged += (_, _) => RefreshLocalizedTexts();

        LoginCommand = new DelegateCommand(async () => await LoginAsync(), CanLogin)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);

        RefreshLocalizedTexts();
    }

    public ObservableCollection<Views.Login.LocaleDisplayItem> LocaleOptions { get; }

    private string _selectedLocale;

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

    public string BrandTitle => _localization.T("desktop.brandTitle");
    public string BrandSubtitle => _localization.T("desktop.brandSubtitle");
    public string Feature1Text => $"✓  {_localization.T("desktop.feature1")}";
    public string Feature2Text => $"✓  {_localization.T("desktop.feature2")}";
    public string Feature3Text => $"✓  {_localization.T("desktop.feature3")}";
    public string WelcomeTitle => _localization.T("login.welcome");
    public string LoginHint => _localization.T("desktop.loginHint");
    public string UsernameLabel => _localization.T("login.usernameLabel");
    public string PasswordLabel => _localization.T("login.passwordLabel");
    public string UsernamePlaceholder => _localization.T("login.username");
    public string PasswordPlaceholder => _localization.T("login.password");
    public string LoginButtonText => _localization.T("login.submit");
    public string LocaleLabel => _localization.T("desktop.locale.label");

    public string VersionText { get; }

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

    public async Task InitializeAsync()
    {
        await InitializeInternalAsync();
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
            StatusMessage = _localization.T("desktop.checkingUpdate");
            var updateInfo = await _updateService.CheckForUpdateAsync();

            if (updateInfo.HasUpdate)
            {
                StatusMessage = string.Format(_localization.T("desktop.updateFound"), updateInfo.LatestVersion);
                if (!string.IsNullOrWhiteSpace(updateInfo.ReleaseNotes))
                {
                    Log.Information("更新说明：{ReleaseNotes}", updateInfo.ReleaseNotes);
                }

                await ApplyUpdateAsync(updateInfo);
                return;
            }

            StatusMessage = string.Format(_localization.T("desktop.readyToLogin"), updateInfo.CurrentVersion);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "初始化登录页失败");
            StatusMessage = _localization.T("desktop.updateCheckFailed");
            HandyControl.Controls.Growl.Warning(_localization.T("desktop.updateCheckFailed"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyUpdateAsync(UpdateCheckResult updateInfo)
    {
        IsBusy = true;
        StatusMessage = _localization.T("desktop.downloadingUpdate");

        try
        {
            var progress = new Progress<double>(value =>
                StatusMessage = string.Format(_localization.T("desktop.downloadingProgress"), value));
            var success = await _updateService.DownloadAndApplyAsync(updateInfo, progress);

            if (success)
            {
                HandyControl.Controls.Growl.Info(_localization.T("desktop.updateCompleteRestart"));
                Application.Current.Shutdown();
            }
            else
            {
                HandyControl.Controls.Growl.Error(_localization.T("desktop.updateFailedContinue"));
                StatusMessage = _localization.T("desktop.updateFailedContinue");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "应用更新失败");
            HandyControl.Controls.Growl.Error(_localization.T("desktop.updateFailedContinue"));
            StatusMessage = _localization.T("desktop.updateFailedContinue");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        StatusMessage = _localization.T("desktop.loggingIn");

        try
        {
            var loginResult = await _authService.LoginAsync(UserName, Password);
            if (loginResult.NeedSelectFactory && loginResult.Factories.Count > 0)
            {
                var selectWindow = new Views.Login.FactorySelectWindow(loginResult.Factories, _localization)
                {
                    Owner = Application.Current.Windows.OfType<Views.Login.LoginView>().FirstOrDefault()
                };
                if (selectWindow.ShowDialog() != true || selectWindow.SelectedFactory == null)
                {
                    StatusMessage = _localization.T("desktop.selectFactoryContinue");
                    return;
                }

                loginResult = await _authService.SelectFactoryAsync(UserName, Password, selectWindow.SelectedFactory.Id);
            }

            if (!loginResult.Success || loginResult.User is null)
            {
                HandyControl.Controls.Growl.Warning(loginResult.ErrorMessage ?? _localization.T("auth.invalid_credentials"));
                StatusMessage = _localization.T("desktop.loginFailed");
                return;
            }

            var user = loginResult.User;
            var currentPassword = Password;

            if (user.MustChangePassword)
            {
                var loginWindow = Application.Current.Windows.OfType<Views.Login.LoginView>().FirstOrDefault();
                var displayName = user.NickName ?? user.UserName;
                var passwordChanged = Views.Login.ChangePasswordWindow.ShowDialog(
                    _authService,
                    _passwordValidator,
                    _localization,
                    currentPassword,
                    displayName,
                    loginWindow);

                if (!passwordChanged)
                {
                    await _authService.LogoutAsync();
                    StatusMessage = _localization.T("desktop.mustChangePassword");
                    HandyControl.Controls.Growl.Warning(_localization.T("desktop.pleaseChangePassword"));
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
                HandyControl.Controls.Growl.Warning(_localization.T("desktop.noDesktopMenu"));
            }

            var shell = _shellViewFactory();
            RegionManager.SetRegionManager(shell, _regionManager);
            shell.Show();
            RegionManager.UpdateRegions();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.Login.LoginView)
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
            HandyControl.Controls.Growl.Error(_localization.T("desktop.loginError"));
            StatusMessage = _localization.T("desktop.loginError");
            await _authService.LogoutAsync();
        }
        finally
        {
            IsBusy = false;
            StatusMessage = _localization.T("desktop.loginHint");
        }
    }

    private void RefreshLocalizedTexts()
    {
        LocaleOptions.Clear();
        foreach (var option in _localization.LocaleOptions)
        {
            LocaleOptions.Add(new Views.Login.LocaleDisplayItem(option.Value, _localization.T(option.LabelKey)));
        }

        RaisePropertyChanged(nameof(BrandTitle));
        RaisePropertyChanged(nameof(BrandSubtitle));
        RaisePropertyChanged(nameof(Feature1Text));
        RaisePropertyChanged(nameof(Feature2Text));
        RaisePropertyChanged(nameof(Feature3Text));
        RaisePropertyChanged(nameof(WelcomeTitle));
        RaisePropertyChanged(nameof(LoginHint));
        RaisePropertyChanged(nameof(UsernameLabel));
        RaisePropertyChanged(nameof(PasswordLabel));
        RaisePropertyChanged(nameof(UsernamePlaceholder));
        RaisePropertyChanged(nameof(PasswordPlaceholder));
        RaisePropertyChanged(nameof(LoginButtonText));
        RaisePropertyChanged(nameof(LocaleLabel));
    }
}
