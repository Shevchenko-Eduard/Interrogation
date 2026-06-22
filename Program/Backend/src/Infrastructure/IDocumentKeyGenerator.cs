using Domain.Interfaces;

namespace Infrastructure;

public class DocumentKeyManager : IDocumentKeyManager
{
    public string Create() => Guid.NewGuid().ToString();
    
}