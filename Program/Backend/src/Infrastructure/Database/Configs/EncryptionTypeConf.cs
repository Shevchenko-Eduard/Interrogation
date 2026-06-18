using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configs.HotelConf;

public class EncryptionTypeConf : IEntityTypeConfiguration<EncryptionType>
{
    public void Configure(EntityTypeBuilder<EncryptionType> builder)
    {
        #region table
        builder.ToTable("encryption_types");
        #endregion

        #region pk
        builder.HasKey(h => h.Id)
            .HasName("encryption_type_id");
        #endregion

        #region property
        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        #endregion

        #region fk
        #endregion

        #region ignore
        #endregion

        #region index
        builder.HasIndex(h => h.Name)
            .HasDatabaseName("uq__encryption_types__name")
            .IsUnique();
        #endregion

        builder.HasData(EncryptionType.All);
    }
}
