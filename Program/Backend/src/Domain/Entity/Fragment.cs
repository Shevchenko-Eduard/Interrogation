namespace Domain.Entity;

public sealed class Fragment
{
    public int Id { get; init; }
    public int DocumentId { get; init; }
    public string MarkerName { get; init; }
    public string Value { get; init; }
    public Document? Document { get; set; }
#pragma warning disable CS9264, CS8618
    private Fragment() { }
#pragma warning restore CS9264, CS8618
    public Fragment(
        int documentId,
        string markerName,
        string value
    )
    {
        DocumentId = documentId;
        MarkerName = markerName;
        Value = value;
    }
}