using Domain.Interfaces.Repositories.BaseRepository;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.MinioRepository;

public class MinioRepository(
    IMinioClient minioClient) : IS3Repository
{
    private readonly IMinioClient _minioClient = minioClient;
    public async Task DeleteAsync(string path, string bucket)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(path);

        await _minioClient.RemoveObjectAsync(removeArgs);
    }

    public async Task<Stream> DownloadAsync(string path, string bucket)
    {
        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(path)
            .WithCallbackStream(stream => stream.CopyToAsync(memoryStream));

        await _minioClient.GetObjectAsync(getObjectArgs);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task UploadAsync(Stream fileStream, string path, string bucket)
    {
        if (fileStream.Length == 0)
        {
            throw new ArgumentException("Поток не содержит данных");
        }

        if (!fileStream.CanRead)
        {
            throw new InvalidOperationException("Поток нельзя прочитать");
        }

        // Проверяем существование бакета
        bool bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket));

        if (!bucketExists)
        {
            // Создаём бакет, если его нет
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket));
        }

        if (fileStream.CanSeek)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(path)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length);

        await _minioClient.PutObjectAsync(putObjectArgs);
    }
}