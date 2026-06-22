namespace Interrogation.Client.Models;

public sealed record LoginResult(UserSession? Session, string? ErrorMessage)
{
    public bool IsSuccess => Session is not null;
}
