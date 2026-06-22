namespace Interrogation.Client.Models;

public sealed record ApiDocument(
    int Id,
    int EncryptionTypeId,
    string CreatorId,
    string Name,
    string? Description,
    string ContentType,
    string Extension,
    string? EncryptionAlgorithm,
    DateTimeOffset DateOfCreate);

public sealed record DownloadedDocument(string FileName, string ContentType, byte[] Content);

public sealed record ApiDocumentDetails(int Id, int EncryptionTypeId, int SecretId, string Name, string? Description, string ContentType, string Extension, string? EncryptionAlgorithm);
public sealed record ApiSecret(int Id, string Value);
public sealed record ApiFragment(int Id, int DocumentId, string MarkerName, string Value);
public sealed record CreatedApiDocument(int Id, int EncryptionTypeId, int SecretId, string Name);
public sealed record DocumentUpload(string FileName, string ContentType, byte[] Content, int EncryptionTypeId, int SecretId, string Name, string? Description, string? EncryptionAlgorithm);
