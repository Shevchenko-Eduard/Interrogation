using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configs.HotelConf;

public class SecretConf : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        #region table
        builder.ToTable("secrets");
        #endregion

        #region pk
        builder.HasKey(h => h.Id)
            .HasName("secret_id");
        #endregion

        #region property
        builder.Property(h => h.Value)
            .HasColumnName("value")
            .IsRequired();
        #endregion

        #region fk
        #endregion

        #region ignore
        #endregion

        #region index
        #endregion
    }
}
