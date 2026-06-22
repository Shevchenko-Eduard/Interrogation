using System.Security.Claims;
using Application.Interfaces;

namespace LibWeb;

/// <summary>
/// Предварительно надо добавить builder.Services.AddHttpContextAccessor();
/// </summary>
#pragma warning disable CA1812 // Избегайте внутренних классов, не имеющих экземпляры
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    // public string? Id => User?.FindFirstValue("sub");
    public string? Id => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? Name => User?.FindFirstValue(ClaimTypes.Name);
}
