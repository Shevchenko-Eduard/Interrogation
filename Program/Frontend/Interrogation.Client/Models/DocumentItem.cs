using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Interrogation.Client.Models;

public sealed class DocumentItem : INotifyPropertyChanged
{
    private string _status = string.Empty;
    private string _content = string.Empty;
    private DateTimeOffset _updatedAt;

    public int Id { get; init; }
    public required string Name { get; init; }
    public required string CaseNumber { get; init; }
    public required string Owner { get; init; }
    public string InvestigationActionType { get; set; } = "Иное следственное действие";

    public required string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public required string Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public DateTimeOffset UpdatedAt
    {
        get => _updatedAt;
        set => SetField(ref _updatedAt, value);
    }

    public string? EncryptedPayload { get; set; }
    public string? IntegritySalt { get; set; }
    public string? IntegrityTag { get; set; }
    public bool IsRemote { get; init; }
    public string? RemoteExtension { get; init; }
    public string? FileLocation { get; set; }
    public byte[]? OriginalFileBytes { get; set; }
    public string? OriginalText { get; set; }
    public string? SourceFormat { get; set; }

    public string Summary => $"{CaseNumber} · {Owner}";
    public string StorageText => IsRemote ? "Сервер API" : FileLocation ?? "Локальное хранилище";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
