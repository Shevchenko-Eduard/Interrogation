using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class InterrogationApiClient : IInterrogationApiClient
{
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
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var headerName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
        var fileName = string.IsNullOrWhiteSpace(headerName)
            ? fallbackName + extension
            : headerName;
        return new(fileName, response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream", bytes);
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
        throw new HttpRequestException($"API {(int)response.StatusCode}: {details}", null, response.StatusCode);
    }
}
