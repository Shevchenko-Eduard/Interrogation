namespace Infrastructure.Database.Interfaces;

public interface IDatabaseInitialization
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}