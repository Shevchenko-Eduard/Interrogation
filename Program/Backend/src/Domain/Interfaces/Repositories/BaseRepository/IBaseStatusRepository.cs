using Domain.Abstract;

namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IBaseStatusObjectRepository<T> : IBaseEnumObjectAbstract<T> where T : StatusObjectAbstract<T>
{
    Task<T?> GetByNameAsync(string name);
}