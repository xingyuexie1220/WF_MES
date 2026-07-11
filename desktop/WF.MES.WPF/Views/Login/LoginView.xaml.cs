using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WF.MES.WPF.Views.Login;

public partial class LoginView : HandyControl.Controls.Window
{
    public LoginView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Login.LoginViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Login.LoginViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void PasswordBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not ViewModels.Login.LoginViewModel viewModel || !viewModel.LoginCommand.CanExecute())
        {
            return;
        }

        viewModel.LoginCommand.Execute();
        e.Handled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
