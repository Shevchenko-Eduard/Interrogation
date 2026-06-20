using Infrastructure.Database.Interfaces;

namespace API.Services;

public class DbInitService(IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitialization>();
            await dbInit.InitializeAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}