using Domain.Entity;
using Domain.Interfaces.Repositories.BaseRepository.Crud;

namespace Domain.Interfaces.Repositories;

public interface ISecretRepository :
    IBaseCreateRepository<Secret>,
    IBaseReadRepository<Secret, int>,
    IBaseDeleteRepository<int>
{

}