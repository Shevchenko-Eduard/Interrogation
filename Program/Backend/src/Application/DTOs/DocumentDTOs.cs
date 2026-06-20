using Domain.Entity;
using Domain.Interfaces;

namespace Application.DTOs;

public static class DocumentDTOs
{
    public static class Inner
    {
        public record Create(
            int EncryptionTypeId,
            int SecretId,
            string CreatorId,
            string Name,
            string? Description,
            string ContentType,
            string Extension,
            string? EncryptionAlgorithm,
            Stream Stream
        )
        {
            public Document GetDocument(IDocumentKeyManager documentKeyGenerator, IClock clock) => new(
                    encryptionTypeId: EncryptionTypeId,
                    secretId: SecretId,
                    creatorId: CreatorId,
                    name: Name,
                    description: Description,
                    contentType: ContentType,
                    extension: Extension,
                    encryptionAlgorithm: EncryptionAlgorithm,
                    documentKeyGenerator: documentKeyGenerator,
                    clock: clock);
        }
        public record Delete(
            int Id
        );
        public record ReadById(
            int Id
        );
        public record DownloadById(
            int Id
        );
        public record Read();
        public record Update(
            int Id,
            string? Name,
            string? Description
        )
        {
            public Document GetDocument(Document document)
            {
                if (Name is null && Description is null)
                {
                    throw new Exception("Нет полей для обновления.");
                }
                else
                {
                    if (Name != null) { document.Name = Name; }
                    if (Description != null) { document.Description = Description; }
                }
                return document;
            }
        }
    }
    public static class Request
    {
        public record Create(
            int EncryptionTypeId,
            int SecretId,
            string Name,
            string? Description,
            string? EncryptionAlgorithm
        );
        public record Update(
            string? Name,
            string? Description
        );
    }
    public static class Response
    {
        public record Create(
            int Id,
            int EncryptionTypeId,
            int SecretId,
            string CreatorId,
            string Name,
            string? Description,
            string ContentType,
            string Extension,
            string DocumentKey,
            string? EncryptionAlgorithm,
            DateTimeOffset DateOfCreate
        )
        {
            public static Create FromDocument(Document document) => new(
                    Id: document.Id,
                    EncryptionTypeId: document.EncryptionTypeId,
                    SecretId: document.SecretId,
                    CreatorId: document.CreatorId,
                    Name: document.Name,
                    Description: document.Description,
                    ContentType: document.ContentType,
                    Extension: document.Extension,
                    DocumentKey: document.DocumentKey,
                    EncryptionAlgorithm: document.EncryptionAlgorithm,
                    DateOfCreate: document.DateOfCreate);
        }
        public record Read(
            int Id,
            int EncryptionTypeId,
            string CreatorId,
            string Name,
            string? Description,
            string ContentType,
            string Extension,
            string? EncryptionAlgorithm,
            DateTimeOffset DateOfCreate
        )
        {
            public static Read FromDocument(Document document) => new(
                    Id: document.Id,
                    EncryptionTypeId: document.EncryptionTypeId,
                    CreatorId: document.CreatorId,
                    Name: document.Name,
                    Description: document.Description,
                    ContentType: document.ContentType,
                    Extension: document.Extension,
                    EncryptionAlgorithm: document.EncryptionAlgorithm,
                    DateOfCreate: document.DateOfCreate
                );
        }
        public record DownloadById(
            string Name,
            string ContentType,
            string Extension,
            Stream Stream
        )
        {
            public static DownloadById FromDocument(Document document, Stream stream) => new(
                    Name: document.Name,
                    ContentType: document.ContentType,
                    Extension: document.Extension,
                    Stream: stream
                );
        }
    }
}
