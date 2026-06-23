using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public interface IInterrogationApiClient
{
    int MaxUploadBytes { get; }
    Task<IReadOnlyList<ApiDocument>> GetDocumentsAsync(CancellationToken cancellationToken = default);
    Task<DownloadedDocument> DownloadDocumentAsync(int id, string fallbackName, string extension, CancellationToken cancellationToken = default);
    Task<ApiDocumentDetails> GetDocumentAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiSecret> CreateSecretAsync(CancellationToken cancellationToken = default);
    Task<ApiSecret> GetSecretAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteSecretAsync(int id, CancellationToken cancellationToken = default);
    Task<CreatedApiDocument> UploadDocumentAsync(DocumentUpload upload, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(int id, CancellationToken cancellationToken = default);
    Task CreateFragmentAsync(int documentId, string marker, string value, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiFragment>> GetFragmentsAsync(int documentId, CancellationToken cancellationToken = default);
}
