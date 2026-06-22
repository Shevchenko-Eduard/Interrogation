using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Interrogation.Client.Views;

public partial class CryptoPasswordDialog : Window
{
    private readonly bool _confirmPassword;

    public CryptoPasswordDialog() : this("Криптографическая операция", confirmPassword: false)
    {
    }

    public CryptoPasswordDialog(string title, bool confirmPassword)
    {
        InitializeComponent();
        _confirmPassword = confirmPassword;
        TitleText.Text = title;
        ConfirmPanel.IsVisible = confirmPassword;
        Opened += (_, _) => PasswordBox.Focus();
    }

    private void ConfirmButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var password = PasswordBox.Text ?? string.Empty;
        if (password.Length < 10)
        {
            ShowError("Пароль должен содержать не менее 10 символов");
            return;
        }

        if (_confirmPassword && password != ConfirmPasswordBox.Text)
        {
            ShowError("Введённые пароли не совпадают");
            return;
        }

        Close(password);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close(null);

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = true;
    }
}
