namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IS3DocumentRepository
{
    Task UploadAsync(Stream fileStream, string path);
    Task<Stream> DownloadAsync(string path);
    Task DeleteAsync(string path);
}