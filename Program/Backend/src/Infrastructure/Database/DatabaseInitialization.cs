using System.Data.Common;
using Infrastructure.Database.Interfaces;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database;

public class DatabaseInitialization(
    ProgramContext context,
    ILogger<DatabaseInitialization>? logger,
    IHostEnvironment hostEnvironment) : IDatabaseInitialization
{
    private readonly ProgramContext _context = context;
    private readonly ILogger<DatabaseInitialization>? _logger = logger;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Всегда используем миграции в Production
            if (_hostEnvironment.IsProduction())
            {
                await _context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                // Для Development можно использовать EnsureCreatedAsync для скорости
                try
                {
                    var created = await _context.Database.EnsureCreatedAsync(cancellationToken);
                    if (created && _context.Database.HasPendingModelChanges())
                    {
                        _logger?.LogWarning("Database has pending model changes. Consider creating a migration.");
                    }
                }
                catch (DbException ex) when (ex.SqlState == "42P07") // 42P07 = relation already exists
                {
                    _logger?.LogWarning(ex, "Table already exists (race condition), continuing...");
                    // Таблицы уже созданы другим экземпляром, продолжаем работу
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
