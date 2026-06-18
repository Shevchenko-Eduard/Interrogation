using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public partial class ProgramContext
{
    public DbSet<Domain.Entity.Document> Documents { get; set; }
    public DbSet<Domain.Entity.EncryptionType> EncryptionTypes { get; set; }
    public DbSet<Domain.Entity.Fragment> Fragments { get; set; }
    public DbSet<Domain.Entity.Secret> Secrets { get; set; }
}