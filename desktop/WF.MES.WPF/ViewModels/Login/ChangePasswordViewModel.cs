using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Validation;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.ViewModels.Login;

/// <summary>首次登录或修改密码对话框逻辑。</summary>
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

    public string TitleText => L("desktop.password.firstLoginTitle");
    public string HintText => string.Format(L("desktop.password.firstLoginHint"), UserDisplayName);
    public string NewPasswordLabel => L("mobile.password.new");
    public string ConfirmPasswordLabel => L("mobile.password.confirm");
    public string RuleHintText => L("desktop.password.ruleHint");
    public string CancelText => L("common.cancel");
    public string ConfirmText => L("desktop.password.confirmChange");

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
        RaisePropertyChanged(nameof(NewPasswordLabel));
        RaisePropertyChanged(nameof(ConfirmPasswordLabel));
        RaisePropertyChanged(nameof(RuleHintText));
        RaisePropertyChanged(nameof(CancelText));
        RaisePropertyChanged(nameof(ConfirmText));
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
            HandyControl.Controls.Growl.Success(L("mobile.password.success"));
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
