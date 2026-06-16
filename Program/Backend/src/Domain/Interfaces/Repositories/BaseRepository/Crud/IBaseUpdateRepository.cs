namespace Domain.Interfaces.Repositories.BaseRepository.Crud;

public interface IBaseUpdateRepository<TValue>
{
	Task UpdateAsync(TValue entity);
}