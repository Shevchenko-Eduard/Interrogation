using System.Linq.Expressions;
using Domain.Entity;
using Domain.Interfaces.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EfRepository;

public class EfSecretRepository(ProgramContext context) : ISecretRepository
{
    private readonly ProgramContext _context = context;

    public async Task AddAsync(Secret entity) => await _context.Secrets.AddAsync(entity);
    public async Task DeleteAsync(int id) => await _context.Secrets.Where(d => d.Id == id).ExecuteDeleteAsync();
    public async Task<IEnumerable<Secret>> GetAsync(Expression<Func<Secret, bool>> expression) => [.. _context.Secrets.Where(expression)];
    public async Task<Secret?> GetByIdAsync(int id) => _context.Secrets.SingleOrDefault(d => d.Id == id);
}