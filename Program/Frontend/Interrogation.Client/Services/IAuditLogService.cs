using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public interface IAuditLogService
{
    IReadOnlyList<AuditRecord> LoadRecent(int limit);
    void Append(AuditRecord record);
}
