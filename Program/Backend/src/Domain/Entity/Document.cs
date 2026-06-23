using Domain.Interfaces;

namespace Domain.Entity;

public sealed class Document
{
    public int Id { get; init; }
    public int EncryptionTypeId { get; init; }
    public int? SecretId { get; init; }
    public string DocumentKey { get; init; }
    public string CreatorId { get; init; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset DateOfCreate { get; init; }
    public string ContentType { get; init; }
    public string Extension { get; init; }
    public string? EncryptionAlgorithm { get; init; }
    public Secret? Secret { get; set; }
    public EncryptionType? EncryptionType { get; set; }
    public ICollection<Fragment>? Fragments { get; private set; }
    private readonly IDocumentKeyManager _documentKeyGenerator;
    private readonly IClock _clock;
#pragma warning disable CS9264, CS8618
    private Document() { }
#pragma warning restore CS9264, CS8618
    public Document(
        int encryptionTypeId,
        int? secretId,
        string creatorId,
        string name,
        string? description,
        string contentType,
        string extension,
        string? encryptionAlgorithm,
        IDocumentKeyManager documentKeyGenerator,
        IClock clock
    )
    {
        _documentKeyGenerator = documentKeyGenerator;
        _clock = clock;

        EncryptionTypeId = encryptionTypeId;
        SecretId = secretId;
        DocumentKey = _documentKeyGenerator.Create();
        CreatorId = creatorId;
        Name = name;
        Description = description;
        DateOfCreate = _clock.Now;
        EncryptionAlgorithm = encryptionAlgorithm;
        ContentType = contentType;
        Extension = extension;
    }
}