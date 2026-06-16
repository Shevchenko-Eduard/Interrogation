namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IS3Repository
{
    Task UploadAsync(Stream fileStream, string path, string bucket);
    Task<Stream> DownloadAsync(string path, string bucket);
    Task DeleteAsync(string path, string bucket);
}