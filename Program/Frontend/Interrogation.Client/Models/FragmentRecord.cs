namespace Interrogation.Client.Models;

public sealed class FragmentRecord
{
    public required string Marker { get; init; }
    public required string DocumentName { get; init; }
    public required string Preview { get; init; }
    public required string EncryptedPayload { get; init; }
    public int Length { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public string Details => $"{DocumentName} · {Length} симв. · {CreatedAt:HH:mm}";
}
