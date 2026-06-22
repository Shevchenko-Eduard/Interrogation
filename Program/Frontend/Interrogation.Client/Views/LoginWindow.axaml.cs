using Avalonia.Controls;
using Avalonia.Interactivity;
using Interrogation.Client.Models;
using Interrogation.Client.ViewModels;

namespace Interrogation.Client.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public event Action<UserSession>? Authenticated;

    private LoginWindowViewModel? ViewModel => DataContext as LoginWindowViewModel;

    private async void SignInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var session = await (ViewModel?.SignInAsync() ?? Task.FromResult<UserSession?>(null));
        if (session is not null)
        {
            Authenticated?.Invoke(session);
        }
    }
}
