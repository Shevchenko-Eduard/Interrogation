using System.Text.Json;

namespace API.Schema;

public class KeycloakSchema(IConfiguration configuration)
{
    private readonly IConfigurationSection _kcSettings = configuration.GetSection("Keycloak");

    public string AuthServerUrl => _kcSettings["AuthServerUrl"] ?? throw new InvalidOperationException("Keycloak:AuthServerUrl is missing");
    public string Realm => _kcSettings["Realm"] ?? throw new InvalidOperationException("Keycloak:Realm is missing");
    public string Resource => _kcSettings["Resource"] ?? throw new InvalidOperationException("Keycloak:Resource is missing");
    public string Secret => _kcSettings["Secret"] ?? throw new InvalidOperationException("Keycloak:Secret is missing");
    public string SslRequired => _kcSettings["SslRequired"] ?? "none";
    public bool VerifyTokenAudience => bool.TryParse(_kcSettings["VerifyTokenAudience"], out var verify) && verify;
    public string MetadataAddress => $"{AuthServerUrl}/realms/{Realm}/.well-known/openid-configuration";
    public const string RoleClaimType = "roles";

    public async Task<string> AuthorizationEndpoint()
    {
        return (await OidcConfigurationAsync())["authorization_endpoint"].ToString()
            ?? throw new Exception("authorization_endpoint is missing in OIDC configuration");
    }
    public async Task<string> TokenEndpoint()
    {
        return (await OidcConfigurationAsync())["token_endpoint"].ToString()
            ?? throw new Exception("token_endpoint is missing in OIDC configuration");
    }

    public async Task<string> IssuerEndpoint()
    {
        return (await OidcConfigurationAsync())["issuer"].ToString()
            ?? throw new Exception("issuer is missing in OIDC configuration");
    }

    private Dictionary<string, object>? _oidcConfig = null;

    public async Task<Dictionary<string, object>> OidcConfigurationAsync()
    {
        if (_oidcConfig != null)
        {
            return _oidcConfig;
        }

        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using HttpClient httpClient = new(handler);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(MetadataAddress);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString)
                    ?? throw new Exception("Failed to deserialize OIDC configuration");

                _oidcConfig = dict;
                return dict;
            }
            catch (HttpRequestException) when (attempt < 3)
            {
                await Task.Delay(1000 * attempt);
            }
        }

        throw new Exception("Failed to fetch OIDC configuration after 3 attempts");
    }
}