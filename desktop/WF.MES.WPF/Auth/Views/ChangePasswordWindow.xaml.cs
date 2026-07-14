using System.Windows;
using System.Windows.Controls;
using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Auth.ViewModels;

namespace WF.MES.WPF.Auth.Views;

public partial class ChangePasswordWindow : HandyControl.Controls.Window
{
    private readonly ChangePasswordViewModel _viewModel;

    public ChangePasswordWindow(
        IAuthService authService,
        IValidator<PasswordChangeDto> passwordValidator,
        ILocalizationService localization,
        string currentPassword,
        string userDisplayName)
    {
        InitializeComponent();

        _viewModel = new ChangePasswordViewModel(authService, passwordValidator, localization, currentPassword, userDisplayName);
        _viewModel.RequestClose += OnRequestClose;
        DataContext = _viewModel;
    }

    public static bool ShowDialog(
        IAuthService authService,
        IValidator<PasswordChangeDto> passwordValidator,
        ILocalizationService localization,
        string currentPassword,
        string userDisplayName,
        Window? owner)
    {
        var window = new ChangePasswordWindow(authService, passwordValidator, localization, currentPassword, userDisplayName)
        {
            Owner = owner
        };
        window.ShowDialog();
        return window._viewModel.IsSuccess;
    }

    private void OnRequestClose()
    {
        DialogResult = _viewModel.IsSuccess;
        Close();
    }

    private void NewPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.NewPassword = passwordBox.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.ConfirmPassword = passwordBox.Password;
        }
    }
}
