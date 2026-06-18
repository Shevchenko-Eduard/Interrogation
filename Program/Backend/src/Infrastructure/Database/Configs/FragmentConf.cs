using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configs.HotelConf;

public class FragmentConf : IEntityTypeConfiguration<Fragment>
{
    public void Configure(EntityTypeBuilder<Fragment> builder)
    {
        #region table
        builder.ToTable("fragments");
        #endregion

        #region pk
        builder.HasKey(h => h.Id)
            .HasName("fragment_id");
        #endregion

        #region property

        builder.Property(h => h.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        builder.Property(h => h.MarkerName)
            .HasColumnName("marker_name")
            .IsRequired();

        builder.Property(h => h.Value)
            .HasColumnName("value")
            .IsRequired();

        #endregion

        #region fk

        builder.HasOne(h => h.Document)
            .WithMany(h => h.Fragments)
            .HasForeignKey(h => h.DocumentId);

        #endregion

        #region ignore
        #endregion

        #region index
        builder.HasIndex(h => h.MarkerName)
            .HasDatabaseName("uq__fragments__marker_name")
            .IsUnique();
        #endregion
    }
}
