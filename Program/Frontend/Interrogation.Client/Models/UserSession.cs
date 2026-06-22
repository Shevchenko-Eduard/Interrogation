namespace Interrogation.Client.Models;

public sealed class UserSession
{
    public UserSession(string userName, string displayName, UserRole role, string accessToken,
        string refreshToken, string? idToken, DateTimeOffset expiresAt, IReadOnlySet<string> roles)
    {
        UserName = userName;
        DisplayName = displayName;
        Role = role;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IdToken = idToken;
        ExpiresAt = expiresAt;
        Roles = roles;
    }

    public string UserName { get; }
    public string DisplayName { get; }
    public UserRole Role { get; }
    public IReadOnlySet<string> Roles { get; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string? IdToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
