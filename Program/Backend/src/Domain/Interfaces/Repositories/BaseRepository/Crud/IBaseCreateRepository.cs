namespace Domain.Interfaces.Repositories.BaseRepository.Crud;

public interface IBaseCreateRepository<TValue>
{
    Task AddAsync(TValue entity);
}