using Domain.Interfaces.Repositories.BaseRepository;

namespace Infrastructure.MinioRepository;

public class MinioDocumentRepository(
    IS3Repository minioRepository) : IS3DocumentRepository
{
    private readonly IS3Repository _minioRepository = minioRepository;
    private readonly string bucket = "documents";
    public Task DeleteAsync(string path) => _minioRepository.DeleteAsync(path, bucket);
    public Task<Stream> DownloadAsync(string path) => _minioRepository.DownloadAsync(path, bucket);
    public Task UploadAsync(Stream fileStream, string path) => _minioRepository.UploadAsync(fileStream, path, bucket);
}