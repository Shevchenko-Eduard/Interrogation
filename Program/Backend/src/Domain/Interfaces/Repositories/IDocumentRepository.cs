using Domain.Entity;
using Domain.Interfaces.Repositories.BaseRepository;

namespace Domain.Interfaces.Repositories;

public interface IDocumentRepository : IBaseCrudRepository<Document, int>
{
    Task<string?> GetDocumentKeyByIdAsync(int id);
}