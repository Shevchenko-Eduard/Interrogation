using System.Linq.Expressions;
using Domain.Entity;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository.Crud;
using Infrastructure.Database;

namespace Infrastructure.EfRepository;

public sealed class EfEncryptionTypeRepository(ProgramContext context) : IEncryptionTypeRepository
{
    private readonly ProgramContext _context = context;

    public async Task<List<EncryptionType>> GetAllAsync() => [.. _context.EncryptionTypes.Select(d => d)];
    public async Task<IEnumerable<EncryptionType>> GetAsync(Expression<Func<EncryptionType, bool>> expression) => [.. _context.EncryptionTypes.Where(expression)];
    async Task<EncryptionType?> IBaseReadRepository<EncryptionType, int>.GetByIdAsync(int id) => _context.EncryptionTypes.SingleOrDefault(d => d.Id == id);
}