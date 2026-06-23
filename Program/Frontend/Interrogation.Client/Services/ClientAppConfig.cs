using System.Text.Json;

namespace Interrogation.Client.Services;

public sealed class ClientAppConfig
{
    public string ApiBaseUrl { get; init; } = "https://api.docker.local/";
    public string KeycloakAuthority { get; init; } = "https://auth.docker.local/realms/employee";
    public string KeycloakClientId { get; init; } = "Interrogation";
    public string BackendClientId { get; init; } = "Interrogation-api";
    public int HttpTimeoutSeconds { get; init; } = 100;
    public int MaxUploadBytes { get; init; } = 20 * 1024 * 1024;

    public static ClientAppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return new ClientAppConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<ClientAppConfig>(File.ReadAllText(path), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ClientAppConfig();
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            return new ClientAppConfig();
        }
    }
}
