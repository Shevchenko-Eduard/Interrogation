using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Interrogation.Client.Models;
using Interrogation.Client.Services;
using Interrogation.Client.ViewModels;
using Interrogation.Client.Views;

namespace Interrogation.Client;

public partial class App : Application
{
    private readonly IIdentityService _identityService = new KeycloakIdentityService();
    private readonly ICryptographyService _cryptographyService = new OpenSslCryptographyService();
    private readonly IAuditLogService _auditLogService = new JsonLineAuditLogService();
    private readonly IContainerIntegrityService _integrityService = new ContainerIntegrityService();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ShowLogin(desktop, null);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop, MainWindow? currentWindow)
    {
        var loginWindow = new LoginWindow
        {
            DataContext = new LoginWindowViewModel(_identityService)
        };

        loginWindow.Authenticated += session => ShowMainWindow(desktop, loginWindow, session);
        desktop.MainWindow = loginWindow;
        loginWindow.Show();
        currentWindow?.Close();
    }

    private void ShowMainWindow(
        IClassicDesktopStyleApplicationLifetime desktop,
        LoginWindow loginWindow,
        UserSession session)
    {
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(
                new InterrogationApiClient(_identityService, session),
                _cryptographyService,
                _auditLogService,
                _integrityService,
                session)
        };

        mainWindow.LogoutRequested += async () =>
        {
            try { await _identityService.LogoutAsync(session); }
            catch (HttpRequestException) { }
            ShowLogin(desktop, mainWindow);
        };
        mainWindow.Opened += async (_, _) =>
            await ((MainWindowViewModel)mainWindow.DataContext).InitializeAsync();
        desktop.MainWindow = mainWindow;
        mainWindow.Show();
        loginWindow.Close();
    }
}
