using Domain.Abstract;
using Domain.Interfaces.Repositories.BaseRepository.Crud;

namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IBaseEnumObjectAbstract<T> : IBaseReadRepository<T, int> where T : EnumObjectAbstract<T>
{

}