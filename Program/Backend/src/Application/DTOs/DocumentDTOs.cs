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
                    documentKeyGenerator: documentKeyGenerator,
                    clock: clock
                );
        }
        public record Delete(
            int Id
        );
        public record ReadById(
            int Id
        );
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
                    DateOfCreate: document.DateOfCreate
                );
        }
        public record Read(
            int Id,
            int EncryptionTypeId,
            int SecretId,
            string CreatorId,
            string Name,
            string? Description,
            string ContentType,
            string Extension,
            string DocumentKey,
            DateTimeOffset DateOfCreate,
            Stream Stream
        )
        {
            public static Read FromDocument(Document document, Stream stream) => new(
                    Id: document.Id,
                    EncryptionTypeId: document.EncryptionTypeId,
                    SecretId: document.SecretId,
                    CreatorId: document.CreatorId,
                    Name: document.Name,
                    Description: document.Description,
                    ContentType: document.ContentType,
                    Extension: document.Extension,
                    DocumentKey: document.DocumentKey,
                    DateOfCreate: document.DateOfCreate,
                    Stream: stream
                );
        }
    }
}
