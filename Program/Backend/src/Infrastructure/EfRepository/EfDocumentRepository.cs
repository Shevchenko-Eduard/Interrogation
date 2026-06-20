using System.Linq.Expressions;
using Domain.Entity;
using Domain.Interfaces.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EfRepository;

public class EfDocumentRepository(ProgramContext context) : IDocumentRepository
{
    private readonly ProgramContext _context = context;


    public async Task AddAsync(Document entity) => await _context.Documents.AddAsync(entity);
    public async Task DeleteAsync(int id) => await _context.Documents.Where(d => d.Id == id).ExecuteDeleteAsync();
    public async Task<IEnumerable<Document>> GetAsync(Expression<Func<Document, bool>> expression) => [.. _context.Documents.Where(expression)];
    public async Task<string?> GetDocumentKeyByIdAsync(int id) => (await _context.Documents.Select(d => new {d.DocumentKey, d.Id}).SingleOrDefaultAsync(d => d.Id == id))?.DocumentKey;
    public async Task UpdateAsync(Document entity) => _context.Documents.Update(entity);
    public async Task<Document?> GetByIdAsync(int id) => _context.Documents.SingleOrDefault(d => d.Id == id);
}