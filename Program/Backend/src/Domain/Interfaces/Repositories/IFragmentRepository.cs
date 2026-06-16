using Domain.Entity;
using Domain.Interfaces.Repositories.BaseRepository.Crud;

namespace Domain.Interfaces.Repositories;

public interface IFragmentRepository :
    IBaseCreateRepository<Fragment>,
    IBaseReadRepository<Fragment, int>,
    IBaseDeleteRepository<int>
{

}