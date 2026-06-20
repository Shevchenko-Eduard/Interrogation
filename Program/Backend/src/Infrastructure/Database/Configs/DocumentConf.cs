using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configs.HotelConf;

public class DocumentConf : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        #region table
        builder.ToTable("documents");
        #endregion

        #region pk
        builder.HasKey(_ => _.Id)
            .HasName("document_id");
        #endregion

        #region property
        builder.Property(_ => _.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(_ => _.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(_ => _.EncryptionTypeId)
            .HasColumnName("encryption_type_id")
            .IsRequired();

        builder.Property(_ => _.SecretId)
            .HasColumnName("secret_id")
            .IsRequired();

        builder.Property(_ => _.DocumentKey)
            .HasColumnName("document_key")
            .IsRequired();

        builder.Property(_ => _.CreatorId)
            .HasColumnName("creator_id")
            .IsRequired();

        builder.Property(_ => _.DateOfCreate)
            .HasColumnName("date_of_create")
            .IsRequired();

        builder.Property(_ => _.ContentType)
            .HasColumnName("content_type")
            .IsRequired();

        builder.Property(_ => _.Extension)
            .HasColumnName("extension")
            .IsRequired();

        #endregion

        #region fk
        builder.HasOne(_ => _.Secret)
            .WithOne(_ => _.Document)
            .HasForeignKey<Document>(_ => _.SecretId);

        builder.HasOne(_ => _.EncryptionType)
            .WithMany(_ => _.Documents)
            .HasForeignKey(_ => _.EncryptionTypeId);

        // builder.HasMany(_ => _.Fragments)
        //     .WithOne(_ => _.Document)
        //     .HasForeignKey(_ => _.DocumentId);
        #endregion

        #region ignore
        #endregion

        #region index
        builder.HasIndex(_ => _.Name)
            .HasDatabaseName("idx__documents__name");
        #endregion
    }
}
