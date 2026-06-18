using System.Linq.Expressions;
using Domain.Entity;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository.Crud;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EfRepository;

public class EfFragmentRepository(ProgramContext context) : IFragmentRepository
{
    private readonly ProgramContext _context = context;


    public async Task AddAsync(Fragment entity) => await _context.Fragments.AddAsync(entity);
    public async Task DeleteAsync(int id) => await _context.Fragments.Where(d => d.Id == id).ExecuteDeleteAsync();
    public async Task<IEnumerable<Fragment>> GetAsync(Expression<Func<Fragment, bool>> expression) => [.. _context.Fragments.Where(expression)];
    public async Task<Fragment?> GetByIdAsync(int id) => _context.Fragments.SingleOrDefault(d => d.Id == id);
}