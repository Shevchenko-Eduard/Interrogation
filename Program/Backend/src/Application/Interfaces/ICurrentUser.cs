namespace Application.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    bool IsAuthenticated { get; }
    public string? Email { get; }
    public string? Name { get; }
}