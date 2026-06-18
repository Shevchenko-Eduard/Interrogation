namespace Infrastructure.Interfaces;

public interface IAppSecretManager
{
    Task<string> GetSecretAsync();
}