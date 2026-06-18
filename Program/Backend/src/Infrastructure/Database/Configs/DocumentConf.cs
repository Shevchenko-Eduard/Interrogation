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
        builder.HasKey(h => h.Id)
            .HasName("document_id");
        #endregion

        #region property
        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(h => h.EncryptionTypeId)
            .HasColumnName("encryption_type_id")
            .IsRequired();

        builder.Property(h => h.SecretId)
            .HasColumnName("secret_id")
            .IsRequired();

        builder.Property(h => h.DocumentKey)
            .HasColumnName("document_key")
            .IsRequired();

        builder.Property(h => h.CreatorId)
            .HasColumnName("creator_id")
            .IsRequired();

        builder.Property(h => h.DateOfCreate)
            .HasColumnName("date_of_create")
            .IsRequired();

        builder.Property(h => h.ContentType)
            .HasColumnName("content_type")
            .IsRequired();

        builder.Property(h => h.Extension)
            .HasColumnName("extension")
            .IsRequired();

        #endregion

        #region fk
        builder.HasOne(h => h.Secret)
            .WithOne(h => h.Document)
            .HasForeignKey<Document>(h => h.SecretId);

        builder.HasOne(h => h.EncryptionType)
            .WithMany(h => h.Documents)
            .HasForeignKey(h => h.EncryptionTypeId);

        // builder.HasMany(h => h.Fragments)
        //     .WithOne(h => h.Document)
        //     .HasForeignKey(h => h.DocumentId);
        #endregion

        #region ignore
        #endregion

        #region index
        builder.HasIndex(h => h.Name)
            .HasDatabaseName("idx__documents__name");
        #endregion
    }
}
