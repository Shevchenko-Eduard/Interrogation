namespace Domain.Interfaces.Repositories.BaseRepository;

public interface IDocumentS3Repository
{
    Task UploadAsync(Stream fileStream, string path);
    Task<Stream> DownloadAsync(string path);
    Task DeleteAsync(string path);
}