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
        builder.HasKey(_ => _.Id)
            .HasName("fragment_id");
        #endregion

        #region property

        builder.Property(_ => _.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        builder.Property(_ => _.MarkerName)
            .HasColumnName("marker_name")
            .IsRequired();

        builder.Property(_ => _.Value)
            .HasColumnName("value")
            .IsRequired();

        #endregion

        #region fk

        builder.HasOne(_ => _.Document)
            .WithMany(_ => _.Fragments)
            .HasForeignKey(_ => _.DocumentId);

        #endregion

        #region ignore
        #endregion

        #region index
        builder.HasIndex(_ => new { _.DocumentId, _.MarkerName })
            .HasDatabaseName("uq__fragments__marker_name")
            .IsUnique();
        #endregion
    }
}
