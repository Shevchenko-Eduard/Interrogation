using Domain.Interfaces.Repositories.BaseRepository.Crud;

namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IBaseCrudRepository<TValue, TValueId> :
    IBaseCreateRepository<TValue>,
    IBaseReadRepository<TValue, TValueId>,
    IBaseUpdateRepository<TValue>,
    IBaseDeleteRepository<TValueId>
{

}