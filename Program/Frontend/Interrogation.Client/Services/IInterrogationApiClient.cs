using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public interface IInterrogationApiClient
{
    Task<IReadOnlyList<ApiDocument>> GetDocumentsAsync(CancellationToken cancellationToken = default);
    Task<DownloadedDocument> DownloadDocumentAsync(int id, string fallbackName, string extension, CancellationToken cancellationToken = default);
}
