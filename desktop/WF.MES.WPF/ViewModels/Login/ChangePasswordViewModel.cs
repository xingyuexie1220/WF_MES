using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Validation;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.ViewModels.Login;

/// <summary>首次登录或修改密码对话框。静态文案走 XAML <c>Loc.Key</c>。</summary>
public class ChangePasswordViewModel : LocalizedViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<PasswordChangeDto> _passwordValidator;
    private readonly string _currentPassword;

    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isBusy;

    public ChangePasswordViewModel(
        IAuthService authService,
        IValidator<PasswordChangeDto> passwordValidator,
        ILocalizationService localization,
        string currentPassword,
        string userDisplayName)
        : base(localization)
    {
        _authService = authService;
        _passwordValidator = passwordValidator;
        _currentPassword = currentPassword;
        UserDisplayName = userDisplayName;

        ConfirmCommand = new DelegateCommand(async () => await ConfirmAsync(), CanConfirm)
            .ObservesProperty(() => NewPassword)
            .ObservesProperty(() => ConfirmPassword)
            .ObservesProperty(() => IsBusy);
        CancelCommand = new DelegateCommand(RequestCancel, () => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    public event Action? RequestClose;

    public string UserDisplayName { get; }

    /// <summary>窗口 Title 与标题共用；含用户名的提示见 <see cref="HintText"/>。</summary>
    public string TitleText => L("ui.password.firstLoginTitle");

    /// <summary>含用户名的动态提示。</summary>
    public string HintText => TF("ui.password.firstLoginHint", UserDisplayName);

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool IsSuccess { get; private set; }

    public DelegateCommand ConfirmCommand { get; }

    public DelegateCommand CancelCommand { get; }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(TitleText));
        RaisePropertyChanged(nameof(HintText));
    }

    private bool CanConfirm() => !IsBusy && !string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword);

    private void RequestCancel()
    {
        IsSuccess = false;
        RequestClose?.Invoke();
    }

    private async Task ConfirmAsync()
    {
        try
        {
            _passwordValidator.ValidateRequest(new PasswordChangeDto
            {
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword,
                CurrentPassword = _currentPassword
            });
        }
        catch (InvalidOperationException ex)
        {
            HandyControl.Controls.Growl.Warning(ex.Message);
            return;
        }

        IsBusy = true;
        try
        {
            await _authService.ChangePasswordAsync(_currentPassword, NewPassword);
            IsSuccess = true;
            HandyControl.Controls.Growl.Success(L("ui.password.success"));
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
