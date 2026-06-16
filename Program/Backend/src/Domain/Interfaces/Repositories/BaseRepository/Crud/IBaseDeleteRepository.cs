namespace Domain.Interfaces.Repositories.BaseRepository.Crud;

public interface IBaseDeleteRepository<TValueId>
{
	Task DeleteAsync(TValueId id);
}