using Domain.Interfaces;

namespace Infrastructure;

public class SecretManager : ISecretManager
{
    public string New() => Guid.NewGuid().ToString();
    
}