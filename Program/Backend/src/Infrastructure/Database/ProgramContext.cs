using Infrastructure.Database.Converter;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public partial class ProgramContext : DbContext
{
    public ProgramContext(DbContextOptions<ProgramContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProgramContext).Assembly);

        // Глобальный конвертер
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        FactoryConverter.UseConverter(entityTypes);

        base.OnModelCreating(modelBuilder);
    }
}