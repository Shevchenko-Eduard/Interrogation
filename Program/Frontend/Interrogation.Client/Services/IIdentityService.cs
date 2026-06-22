using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public interface IIdentityService
{
    Task<LoginResult> LoginAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(UserSession session, CancellationToken cancellationToken = default);
    Task LogoutAsync(UserSession session, CancellationToken cancellationToken = default);
}
