namespace Interrogation.Client.Models;

public sealed record AuditRecord(
    DateTimeOffset CreatedAt,
    string UserName,
    string Action,
    string DocumentName,
    string Result)
{
    public string Header => $"{CreatedAt:HH:mm:ss} · {Action}";
    public string Details => $"{UserName} · {DocumentName}";
}
