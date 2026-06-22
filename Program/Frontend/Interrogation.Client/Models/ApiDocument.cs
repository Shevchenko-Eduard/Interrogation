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
