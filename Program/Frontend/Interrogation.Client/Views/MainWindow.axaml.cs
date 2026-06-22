using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Interrogation.Client.Services;
using Interrogation.Client.ViewModels;

namespace Interrogation.Client.Views;

public partial class MainWindow : Window
{
    private readonly DocumentContentReader _documentContentReader = new();
    private readonly DocumentExportService _documentExportService = new();
    private string _cachedSelectedText = string.Empty;
    private int _cachedSelectionStart = -1;
    private int _cachedSelectionLength;
    private readonly DispatcherTimer _inactivityTimer = new() { Interval = TimeSpan.FromMinutes(15) };

    private static readonly FilePickerFileType TextDocuments = new("Текстовые документы")
    {
        Patterns = ["*.txt", "*.md", "*.docx", "*.odt", "*.json", "*.enc", "*.enc.json"]
    };

    private static readonly FilePickerFileType EncryptedContainer = new("Зашифрованный контейнер")
    {
        Patterns = ["*.enc", "*.enc.json"]
    };
    private static readonly FilePickerFileType WordDocument = new("Microsoft Word") { Patterns = ["*.docx"] };
    private static readonly FilePickerFileType OpenDocument = new("OpenDocument") { Patterns = ["*.odt"] };

    public MainWindow()
    {
        InitializeComponent();
        PointerPressed += (_, _) => ResetInactivityTimer();
        KeyDown += (_, _) => ResetInactivityTimer();
        Opened += (_, _) => ResetInactivityTimer();
        Closed += (_, _) => _inactivityTimer.Stop();
        _inactivityTimer.Tick += InactivityTimer_OnTick;
    }

    public event Action? LogoutRequested;

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void ResetInactivityTimer()
    {
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }

    private void InactivityTimer_OnTick(object? sender, EventArgs e)
    {
        _inactivityTimer.Stop();
        ViewModel?.RecordAudit("Автоматический выход", "Сеанс завершён после 15 минут бездействия", "Система");
        LogoutRequested?.Invoke();
    }

    private void DocumentEditor_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CacheEditorSelection();
    }

    private void DocumentEditor_OnKeyUp(object? sender, KeyEventArgs e)
    {
        CacheEditorSelection();
    }

    private void CacheEditorSelection()
    {
        var text = DocumentEditor.Text;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var start = Math.Min(DocumentEditor.SelectionStart, DocumentEditor.SelectionEnd);
        var end = Math.Max(DocumentEditor.SelectionStart, DocumentEditor.SelectionEnd);
        if (end <= start || end > text.Length)
        {
            return;
        }

        _cachedSelectionStart = start;
        _cachedSelectionLength = end - start;
        _cachedSelectedText = text.Substring(start, _cachedSelectionLength);
    }

    private void LogoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        LogoutRequested?.Invoke();
    }

    private async void DocumentList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is not null)
        {
            await ViewModel.LoadSelectedDocumentAsync();
        }
    }

    private void TakeSelectionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_cachedSelectedText) || _cachedSelectionStart < 0)
        {
            ViewModel.ClearSelectedFragment();
            ViewModel.StatusMessage = "В редакторе нет выделенного текста";
            return;
        }

        ViewModel.SetSelectedFragment(
            _cachedSelectedText,
            _cachedSelectionStart,
            _cachedSelectionLength);
        ViewModel.StatusMessage = "Фрагмент подготовлен к шифрованию";

        _cachedSelectedText = string.Empty;
        _cachedSelectionStart = -1;
        _cachedSelectionLength = 0;
    }

    private async void EncryptFragmentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var password = await RequestPasswordAsync("Шифрование фрагмента", confirmPassword: true);
        if (password is not null)
        {
            await ViewModel.EncryptSelectedFragmentAsync(password);
        }
        _cachedSelectedText = string.Empty;
        _cachedSelectionStart = -1;
        _cachedSelectionLength = 0;
    }

    private async void EncryptFullButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var password = await RequestPasswordAsync("Шифрование документа", confirmPassword: true);
        if (password is not null)
        {
            await ViewModel.EncryptFullDocumentAsync(password);
        }
    }

    private async void DecryptButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var password = await RequestPasswordAsync("Расшифрование документа", confirmPassword: false);
        if (password is not null)
        {
            await ViewModel.DecryptDocumentAsync(password);
        }
    }

    private void SaveDraftButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SaveDraft();
    }

    private async void RefreshDocumentsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not null) await ViewModel.InitializeAsync();
    }

    private async void UploadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите текстовый документ",
            AllowMultiple = false,
            FileTypeFilter = [TextDocuments, FilePickerFileTypes.All]
        });

        var file = files.FirstOrDefault();
        if (file is null)
        {
            ViewModel.StatusMessage = "Загрузка отменена";
            return;
        }

        string content;
        byte[] fileBytes;
        try
        {
            await using var stream = await file.OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            fileBytes = memory.ToArray();
            memory.Position = 0;
            content = await _documentContentReader.ReadAsync(file.Name, memory);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
            ViewModel.StatusMessage = $"Не удалось открыть документ: {exception.Message}";
            ViewModel.RecordAudit("Открытие документа", $"Ошибка: {exception.Message}", file.Name);
            return;
        }
        if (IsEncryptedContainer(file.Name, content))
        {
            ViewModel.ImportEncryptedContainer(file.Name, content);
            return;
        }

        var sourceFormat = Path.GetExtension(file.Name).TrimStart('.').ToLowerInvariant();
        var preserveSource = sourceFormat is "docx" or "odt";
        ViewModel.ImportDocument(
            file.Name,
            content,
            preserveSource ? fileBytes : null,
            preserveSource ? sourceFormat : null);
    }

    private async void ExportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var password = await RequestPasswordAsync("Защита контейнера", confirmPassword: true);
        if (password is null)
        {
            return;
        }

        string containerJson;
        try
        {
            containerJson = await ViewModel.BuildEncryptedContainerJsonAsync(password);
        }
        catch (InvalidOperationException exception)
        {
            ViewModel.StatusMessage = exception.Message;
            ViewModel.RecordAudit("Экспорт контейнера", $"Ошибка: {exception.Message}");
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить контейнер",
            SuggestedFileName = "document.enc",
            FileTypeChoices = [EncryptedContainer, FilePickerFileTypes.All],
            DefaultExtension = "enc"
        });

        if (file is null)
        {
            ViewModel.StatusMessage = "Экспорт отменен";
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(containerJson);
        ViewModel.StatusMessage = $"Контейнер сохранен: {file.Name}";
        ViewModel.RecordAudit("Экспорт контейнера", "Успешно");
    }

    private Task<string?> RequestPasswordAsync(string title, bool confirmPassword)
    {
        var dialog = new CryptoPasswordDialog(title, confirmPassword);
        return dialog.ShowDialog<string?>(this);
    }

    private async void ExportDocxButton_OnClick(object? sender, RoutedEventArgs e) => await ExportOfficeAsync("docx");
    private async void ExportOdtButton_OnClick(object? sender, RoutedEventArgs e) => await ExportOfficeAsync("odt");

    private async Task ExportOfficeAsync(string format)
    {
        var document = ViewModel?.SelectedDocument;
        if (ViewModel is null || document is null) return;
        if (!string.IsNullOrWhiteSpace(document.EncryptedPayload))
        {
            ViewModel.StatusMessage = "Сначала расшифруйте документ";
            return;
        }
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = format == "docx" ? "Экспорт в Microsoft Word" : "Экспорт в OpenDocument",
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(document.Name)}.{format}",
            FileTypeChoices = format == "docx" ? [WordDocument] : [OpenDocument],
            DefaultExtension = format
        });
        if (file is null) return;
        var bytes = format == "docx"
            ? _documentExportService.ExportDocx(ViewModel.DocumentText, document)
            : _documentExportService.ExportOdt(ViewModel.DocumentText, document);
        await using var stream = await file.OpenWriteAsync();
        stream.SetLength(0);
        await stream.WriteAsync(bytes);
        ViewModel.StatusMessage = $"Документ экспортирован: {file.Name}";
        ViewModel.RecordAudit($"Экспорт {format.ToUpperInvariant()}", "Успешно");
    }

    private static bool IsEncryptedContainer(string fileName, string content)
    {
        if (fileName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".enc.json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var trimmedContent = content.AsSpan().TrimStart();
        return trimmedContent.StartsWith("{".AsSpan(), StringComparison.Ordinal) &&
               content.Contains("\"format\"", StringComparison.Ordinal) &&
               (content.Contains("interrogation.enc.v2", StringComparison.Ordinal) ||
                content.Contains("interrogation.enc.v3", StringComparison.Ordinal));
    }
}
