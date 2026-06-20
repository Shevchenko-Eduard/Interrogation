using Domain.Interfaces;

namespace Infrastructure;

public class DocumentKeyManager : IDocumentKeyManager
{
    public string New() => Guid.NewGuid().ToString();
    
}