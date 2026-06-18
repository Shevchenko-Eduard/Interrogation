using System.Linq.Expressions;

namespace Domain.Interfaces.Repositories.BaseRepository.Crud;

public interface IBaseReadRepository<TValue, TValueId>
{
    Task<TValue?> GetByIdAsync(TValueId id);
    Task<IEnumerable<TValue>> GetAsync(Expression<Func<TValue, bool>> expression);
}