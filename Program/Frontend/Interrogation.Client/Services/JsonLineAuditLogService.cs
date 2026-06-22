using System.Text.Json;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class JsonLineAuditLogService : IAuditLogService
{
    private readonly string _filePath;
    private readonly object _syncRoot = new();

    public JsonLineAuditLogService() : this(GetDefaultFilePath())
    {
    }

    public JsonLineAuditLogService(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath)
            ?? throw new ArgumentException("Для журнала требуется полный путь", nameof(filePath));
        Directory.CreateDirectory(directory);
        _filePath = filePath;
    }

    private static string GetDefaultFilePath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InterrogationClient");
        return Path.Combine(directory, "audit-log.jsonl");
    }

    public IReadOnlyList<AuditRecord> LoadRecent(int limit)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        return File.ReadLines(_filePath)
            .Select(TryDeserialize)
            .Where(record => record is not null)
            .Cast<AuditRecord>()
            .TakeLast(limit)
            .Reverse()
            .ToArray();
    }

    public void Append(AuditRecord record)
    {
        var line = JsonSerializer.Serialize(record) + Environment.NewLine;
        lock (_syncRoot)
        {
            File.AppendAllText(_filePath, line);
        }
    }

    private static AuditRecord? TryDeserialize(string line)
    {
        try
        {
            return JsonSerializer.Deserialize<AuditRecord>(line);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
