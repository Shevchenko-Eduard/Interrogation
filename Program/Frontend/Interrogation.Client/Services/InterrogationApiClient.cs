using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class InterrogationApiClient : IInterrogationApiClient
{
    public const int MaxUploadBytes = 20 * 1024 * 1024;
    private readonly HttpClient _httpClient;
    private readonly IIdentityService _identityService;
    private readonly UserSession _session;

    public InterrogationApiClient(IIdentityService identityService, UserSession session)
    {
        _identityService = identityService;
        _session = session;
        _httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
                errors == System.Net.Security.SslPolicyErrors.None ||
                request.RequestUri?.Host.EndsWith(".docker.local", StringComparison.OrdinalIgnoreCase) == true
        })
        {
            BaseAddress = new Uri("https://api.docker.local/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
    }

    public async Task<IReadOnlyList<ApiDocument>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.GetAsync("v1/documents", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiDocument[]>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken) ?? [];
    }

    public async Task<DownloadedDocument> DownloadDocumentAsync(
        int id,
        string fallbackName,
        string extension,
        CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.GetAsync($"v1/documents/{id}/download", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var target = new MemoryStream();
        await source.CopyToAsync(target, cancellationToken);
        var bytes = target.ToArray();
        var headerName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
        var fileName = string.IsNullOrWhiteSpace(headerName)
            ? fallbackName + extension
            : headerName;
        return new(fileName, response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream", bytes);
    }

    public async Task<ApiDocumentDetails> GetDocumentAsync(int id, CancellationToken cancellationToken = default) =>
        await GetJsonAsync<ApiDocumentDetails>($"v1/documents/{id}", cancellationToken);

    public async Task<ApiSecret> CreateSecretAsync(CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.PostAsJsonAsync("v1/documents/encryption/secrets", new { numberOfBytes = 32 }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<ApiSecret>(cancellationToken: cancellationToken))!;
    }

    public Task<ApiSecret> GetSecretAsync(int id, CancellationToken cancellationToken = default) =>
        GetJsonAsync<ApiSecret>($"v1/documents/encryption/secrets/{id}", cancellationToken);

    public async Task DeleteSecretAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.DeleteAsync($"v1/documents/encryption/secrets/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<CreatedApiDocument> UploadDocumentAsync(DocumentUpload upload, CancellationToken cancellationToken = default)
    {
        if (upload.Content.Length > MaxUploadBytes)
            throw new InvalidOperationException("Размер файла превышает серверный лимит 20 МБ");
        await EnsureTokenAsync(cancellationToken);
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(upload.Content);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(upload.ContentType);
        form.Add(file, "file", upload.FileName);
        form.Add(new StringContent(upload.EncryptionTypeId.ToString()), "EncryptionTypeId");
        form.Add(new StringContent(upload.SecretId.ToString()), "SecretId");
        form.Add(new StringContent(upload.Name), "Name");
        form.Add(new StringContent(upload.Description ?? string.Empty), "Description");
        form.Add(new StringContent(upload.EncryptionAlgorithm ?? string.Empty), "EncryptionAlgorithm");
        using var response = await _httpClient.PostAsync("v1/documents", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<CreatedApiDocument>(cancellationToken: cancellationToken))!;
    }

    public async Task DeleteDocumentAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.DeleteAsync($"v1/documents/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CreateFragmentAsync(int documentId, string marker, string value, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.PostAsJsonAsync("v1/documents/fragment", new { documentId, markerName = marker, value }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiFragment>> GetFragmentsAsync(int documentId, CancellationToken cancellationToken = default) =>
        await GetJsonAsync<ApiFragment[]>($"v1/documents/{documentId}/fragment", cancellationToken);

    private async Task<T> GetJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        await EnsureTokenAsync(cancellationToken);
        using var response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken))!;
    }

    private async Task EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (_session.ExpiresAt <= DateTimeOffset.Now.AddSeconds(30))
        {
            await _identityService.RefreshAsync(_session, cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        var details = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge
            ? "Файл превышает серверный лимит 20 МБ"
            : $"API {(int)response.StatusCode}: {details}";
        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
