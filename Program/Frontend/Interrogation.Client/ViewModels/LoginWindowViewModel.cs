using Interrogation.Client.Models;
using Interrogation.Client.Services;

namespace Interrogation.Client.ViewModels;

public sealed class LoginWindowViewModel : ViewModelBase
{
    private readonly IIdentityService _identityService;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isBusy;

    public LoginWindowViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanSignIn));
                OnPropertyChanged(nameof(SignInButtonText));
            }
        }
    }

    public bool CanSignIn => !IsBusy;
    public string SignInButtonText => IsBusy ? "Ожидание Keycloak..." : "Войти через Keycloak";

    public async Task<UserSession?> SignInAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _identityService.LoginAsync();
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось выполнить вход";
                HasError = true;
            }
            return result.Session;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
