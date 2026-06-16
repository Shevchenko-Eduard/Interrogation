using Domain.Entity;
using Domain.Interfaces.Repositories.BaseRepository.Crud;

namespace Domain.Interfaces.Repositories;

public interface IEncryptionTypeRepository :
    IBaseReadRepository<Fragment, int>
{

}