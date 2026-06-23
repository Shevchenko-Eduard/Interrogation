using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class KeycloakIdentityService : IIdentityService
{
    private static readonly string SessionPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "InterrogationClient",
        "session.json");
    private readonly ClientAppConfig _config;
    private readonly HttpClient _httpClient;

    public KeycloakIdentityService(ClientAppConfig config)
    {
        _config = config;
        _httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
                errors == System.Net.Security.SslPolicyErrors.None ||
                request.RequestUri?.Host.EndsWith(".docker.local", StringComparison.OrdinalIgnoreCase) == true
        })
        {
            Timeout = TimeSpan.FromSeconds(config.HttpTimeoutSeconds)
        };
    }

    public async Task<LoginResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var redirectUri = $"http://127.0.0.1:{port}/callback/";
            var state = Base64Url(RandomNumberGenerator.GetBytes(24));
            var verifier = Base64Url(RandomNumberGenerator.GetBytes(48));
            var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
            var authorizationUrl = _config.KeycloakAuthority + "/protocol/openid-connect/auth?" + BuildQuery(new Dictionary<string, string>
            {
                ["client_id"] = _config.KeycloakClientId,
                ["response_type"] = "code",
                ["scope"] = "openid profile roles",
                ["redirect_uri"] = redirectUri,
                ["state"] = state,
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256"
            });

            Process.Start(new ProcessStartInfo(authorizationUrl) { UseShellExecute = true });
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMinutes(3));
            using var client = await listener.AcceptTcpClientAsync(timeout.Token);
            var callback = await ReadCallbackAsync(client, timeout.Token);
            await WriteBrowserResponseAsync(client, callback.ContainsKey("code"), timeout.Token);
            if (!callback.TryGetValue("state", out var returnedState) || returnedState != state)
            {
                return new(null, "Keycloak вернул неверный параметр state");
            }
            if (!callback.TryGetValue("code", out var code))
            {
                return new(null, callback.GetValueOrDefault("error_description") ?? "Вход отменён");
            }

            using var tokenResponse = await _httpClient.PostAsync(
                _config.KeycloakAuthority + "/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = _config.KeycloakClientId,
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["code_verifier"] = verifier
                }), timeout.Token);
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(timeout.Token);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return new(null, $"Keycloak отклонил код авторизации: {(int)tokenResponse.StatusCode}");
            }
            var token = JsonSerializer.Deserialize<TokenResponse>(tokenJson)
                ?? throw new InvalidOperationException("Пустой ответ Keycloak");
            var session = CreateSession(token);
            SaveSession(session);
            return new(session, null);
        }
        catch (OperationCanceledException)
        {
            return new(null, "Время ожидания входа истекло");
        }
        catch (Exception exception) when (exception is HttpRequestException or SocketException or InvalidOperationException)
        {
            return new(null, $"Не удалось подключиться к Keycloak: {exception.Message}");
        }
    }

    public async Task RefreshAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(_config.KeycloakAuthority + "/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _config.KeycloakClientId,
                ["refresh_token"] = session.RefreshToken
            }), cancellationToken);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Пустой ответ обновления токена");
        session.AccessToken = token.AccessToken;
        session.RefreshToken = token.RefreshToken ?? session.RefreshToken;
        session.IdToken = token.IdToken ?? session.IdToken;
        session.ExpiresAt = DateTimeOffset.Now.AddSeconds(token.ExpiresIn);
        SaveSession(session);
    }

    public async Task LogoutAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(_config.KeycloakAuthority + "/protocol/openid-connect/logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _config.KeycloakClientId,
                ["refresh_token"] = session.RefreshToken
            }), cancellationToken);
        DeleteSavedSession();
        response.EnsureSuccessStatusCode();
    }

    public async Task<UserSession?> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(SessionPath)) return null;
            var saved = JsonSerializer.Deserialize<SavedSession>(await File.ReadAllTextAsync(SessionPath, cancellationToken));
            if (saved is null || string.IsNullOrWhiteSpace(saved.RefreshToken)) return null;
            var session = new UserSession(
                saved.UserName,
                saved.DisplayName,
                saved.Role,
                saved.AccessToken,
                saved.RefreshToken,
                saved.IdToken,
                saved.ExpiresAt,
                saved.Roles.ToHashSet(StringComparer.OrdinalIgnoreCase));
            await RefreshAsync(session, cancellationToken);
            return session;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or HttpRequestException or InvalidOperationException)
        {
            DeleteSavedSession();
            return null;
        }
    }

    private UserSession CreateSession(TokenResponse token)
    {
        var accessToken = token.AccessToken;
        var parts = accessToken.Split('.');
        if (parts.Length < 2) throw new InvalidOperationException("Некорректный access token");
        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = payload.RootElement;
        var userName = GetString(root, "preferred_username", GetString(root, "sub", "user"));
        var displayName = GetString(root, "name", userName);
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("resource_access", out var resources) &&
            resources.TryGetProperty(_config.BackendClientId, out var backend) &&
            backend.TryGetProperty("roles", out var roleArray))
        {
            foreach (var role in roleArray.EnumerateArray())
            {
                if (role.GetString() is { } value) roles.Add(value);
            }
        }
        var roleType = roles.Contains("SecretReadById") || roles.Contains("DocumentDelete")
            ? UserRole.Admin : UserRole.Employee;
        return new(userName, displayName, roleType, accessToken,
            token.RefreshToken ?? string.Empty, token.IdToken,
            DateTimeOffset.Now.AddSeconds(token.ExpiresIn), roles);
    }

    private static void SaveSession(UserSession session)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SessionPath)!);
        var saved = new SavedSession(
            session.UserName,
            session.DisplayName,
            session.Role,
            session.AccessToken,
            session.RefreshToken,
            session.IdToken,
            session.ExpiresAt,
            session.Roles.ToArray());
        File.WriteAllText(SessionPath, JsonSerializer.Serialize(saved, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void DeleteSavedSession()
    {
        try { if (File.Exists(SessionPath)) File.Delete(SessionPath); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static async Task<Dictionary<string, string>> ReadCallbackAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(client.GetStream(), Encoding.ASCII, leaveOpen: true);
        var requestLine = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
        while (!string.IsNullOrEmpty(await reader.ReadLineAsync(cancellationToken))) { }
        var target = requestLine.Split(' ').ElementAtOrDefault(1) ?? "/";
        var query = new Uri("http://127.0.0.1" + target).Query.TrimStart('?');
        return query.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(part => Uri.UnescapeDataString(part[0]), part => Uri.UnescapeDataString(part.ElementAtOrDefault(1) ?? string.Empty));
    }

    private static async Task WriteBrowserResponseAsync(TcpClient client, bool success, CancellationToken cancellationToken)
    {
        var body = success
            ? "<script>window.close();</script><h2>Вход выполнен</h2><p>Эту вкладку можно закрыть.</p>"
            : "<h2>Вход отменён</h2>";
        var bytes = Encoding.UTF8.GetBytes($"<html><body>{body}</body></html>");
        var header = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {bytes.Length}\r\nConnection: close\r\n\r\n");
        await client.GetStream().WriteAsync(header, cancellationToken);
        await client.GetStream().WriteAsync(bytes, cancellationToken);
    }

    private static string BuildQuery(IEnumerable<KeyValuePair<string, string>> values) =>
        string.Join("&", values.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
    private static string GetString(JsonElement element, string name, string fallback) =>
        element.TryGetProperty(name, out var value) ? value.GetString() ?? fallback : fallback;
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }
    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("id_token")] string? IdToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record SavedSession(
        string UserName,
        string DisplayName,
        UserRole Role,
        string AccessToken,
        string RefreshToken,
        string? IdToken,
        DateTimeOffset ExpiresAt,
        string[] Roles);
}
