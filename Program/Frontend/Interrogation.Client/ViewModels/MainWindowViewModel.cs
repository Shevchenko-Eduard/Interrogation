using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Interrogation.Client.Models;
using Interrogation.Client.Services;

namespace Interrogation.Client.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private static readonly string LocalStorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "InterrogationClient",
        "local-documents.json");
    private readonly IInterrogationApiClient _apiClient;
    private readonly DocumentContentReader _documentContentReader = new();
    private readonly DocumentExportService _documentExportService = new();
    private readonly ICryptographyService _cryptographyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContainerIntegrityService _integrityService;
    private DocumentItem? _selectedDocument;
    private string _documentText = string.Empty;
    private string _selectedFragmentText = string.Empty;
    private int _selectedFragmentStart = -1;
    private int _selectedFragmentLength;
    private string _statusMessage = "Готово к работе";
    private UserRole _currentRole = UserRole.Employee;
    private int _nextDocumentId = 100;
    private bool _isBusy;
    private CancellationTokenSource? _documentLoadCancellation;
    private readonly List<AuditRecord> _auditHistory;
    private string _auditSearchText = string.Empty;
    private string _auditResultFilter = "Все";
    private string _selectedServerMode = "Без шифрования (None)";

    public MainWindowViewModel(
        IInterrogationApiClient apiClient,
        ICryptographyService cryptographyService,
        IAuditLogService auditLogService,
        IContainerIntegrityService integrityService,
        UserSession session)
    {
        _apiClient = apiClient;
        _cryptographyService = cryptographyService;
        _auditLogService = auditLogService;
        _integrityService = integrityService;
        CurrentRole = session.Role;
        CurrentUserName = session.DisplayName;
        Documents = new ObservableCollection<DocumentItem>();
        Fragments = new ObservableCollection<FragmentRecord>();
        _auditHistory = new List<AuditRecord>(_auditLogService.LoadRecent(300));
        AuditLog = new ObservableCollection<AuditRecord>(_auditHistory);
        LoadLocalDocuments();
        RecordAudit("Вход в систему", "Успешно", "Система");
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        StatusMessage = "Загрузка документов с сервера...";
        try
        {
            var documents = await _apiClient.GetDocumentsAsync();
            var localDocuments = Documents.Where(document => !document.IsRemote).ToArray();
            Documents.Clear();
            foreach (var document in documents)
            {
                Documents.Add(new DocumentItem
                {
                    Id = document.Id,
                    Name = document.Name,
                    CaseNumber = document.Description ?? $"Документ API #{document.Id}",
                    Owner = document.CreatorId,
                    Status = document.EncryptionAlgorithm ?? "Без шифрования",
                    UpdatedAt = document.DateOfCreate,
                    Content = "Документ хранится на сервере. Выберите его для загрузки содержимого.",
                    IsRemote = true,
                    RemoteExtension = document.Extension,
                    FileLocation = "Сервер API"
                });
            }
            foreach (var document in localDocuments)
            {
                Documents.Add(document);
            }
            SelectedDocument = Documents.FirstOrDefault();
            StatusMessage = $"Загружено документов: {Documents.Count}";
            RecordAudit("Синхронизация с API", "Успешно", "Система");
        }
        catch (HttpRequestException exception)
        {
            StatusMessage = $"API недоступен: {exception.Message}";
            RecordAudit("Синхронизация с API", $"Ошибка: {exception.Message}", "Система");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadSelectedDocumentAsync()
    {
        var document = SelectedDocument;
        if (document is null || !document.IsRemote)
        {
            return;
        }
        _documentLoadCancellation?.Cancel();
        _documentLoadCancellation?.Dispose();
        _documentLoadCancellation = new CancellationTokenSource();
        var cancellationToken = _documentLoadCancellation.Token;
        IsBusy = true;
        StatusMessage = $"Скачивание документа: {document.Name}";
        try
        {
            var details = await _apiClient.GetDocumentAsync(document.Id, cancellationToken);
            var downloaded = await _apiClient.DownloadDocumentAsync(
                document.Id,
                document.Name,
                document.RemoteExtension ?? string.Empty,
                cancellationToken);
            if (details.EncryptionTypeId == 1)
            {
                var secret = await _apiClient.GetSecretAsync(details.SecretId, cancellationToken);
                using var container = JsonDocument.Parse(downloaded.Content);
                var payload = container.RootElement.GetProperty("encryptedPayload").GetString()
                    ?? throw new InvalidDataException("В контейнере отсутствует шифротекст");
                DocumentText = await _cryptographyService.DecryptAsync(payload, secret.Value);
            }
            else
            {
                using var stream = new MemoryStream(downloaded.Content);
                DocumentText = await Task.Run(() => _documentContentReader.ReadAsync(downloaded.FileName, stream), cancellationToken);
                if (details.EncryptionTypeId == 2)
                {
                    var secret = await _apiClient.GetSecretAsync(details.SecretId, cancellationToken);
                    var fragments = await _apiClient.GetFragmentsAsync(document.Id, cancellationToken);
                    foreach (var fragment in fragments)
                    {
                        var plainText = await _cryptographyService.DecryptAsync(fragment.Value, secret.Value);
                        DocumentText = DocumentText.Replace(fragment.MarkerName, plainText, StringComparison.Ordinal);
                    }
                }
            }
            document.Content = DocumentText;
            var sourceFormat = Path.GetExtension(downloaded.FileName).TrimStart('.').ToLowerInvariant();
            if (sourceFormat is "docx" or "odt")
            {
                document.OriginalFileBytes = downloaded.Content;
                document.OriginalText = DocumentText;
                document.SourceFormat = sourceFormat;
            }
            StatusMessage = $"Документ загружен: {downloaded.FileName}";
            RecordAudit("Скачивание документа", "Успешно", document.Name);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception) when (exception is HttpRequestException or IOException or InvalidDataException or InvalidOperationException or JsonException)
        {
            StatusMessage = $"Не удалось скачать документ: {exception.Message}";
            RecordAudit("Скачивание документа", $"Ошибка: {exception.Message}", document.Name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public ObservableCollection<DocumentItem> Documents { get; }

    public ObservableCollection<FragmentRecord> Fragments { get; }

    public ObservableCollection<AuditRecord> AuditLog { get; }

    public IReadOnlyList<string> ServerModes { get; } = ["Без шифрования (None)", "Полное шифрование (Full)", "Частичное шифрование (Part)"];
    public string SelectedServerMode
    {
        get => _selectedServerMode;
        set => SetProperty(ref _selectedServerMode, value);
    }

    public IReadOnlyList<string> AuditResultFilters { get; } = ["Все", "Успешно", "Ошибки"];

    public string AuditSearchText
    {
        get => _auditSearchText;
        set
        {
            if (SetProperty(ref _auditSearchText, value)) ApplyAuditFilter();
        }
    }

    public string AuditResultFilter
    {
        get => _auditResultFilter;
        set
        {
            if (SetProperty(ref _auditResultFilter, value)) ApplyAuditFilter();
        }
    }

    public string CurrentUserName { get; }

    public bool IsAdmin => CurrentRole == UserRole.Admin;

    public string CryptoEngineText => _cryptographyService.EngineDescription;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEncrypt));
                OnPropertyChanged(nameof(CanDecrypt));
            }
        }
    }

    public DocumentItem? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value))
            {
                DocumentText = value?.Content ?? string.Empty;
                ClearSelectedFragment();
                StatusMessage = value is null
                    ? "Документ не выбран"
                    : $"Открыт документ: {value.Name}";
                OnPropertyChanged(nameof(DocumentInfo));
                OnPropertyChanged(nameof(DocumentLocationText));
                OnPropertyChanged(nameof(HasSelectedDocument));
                OnPropertyChanged(nameof(CanEncrypt));
                OnPropertyChanged(nameof(CanDecrypt));
            }
        }
    }

    public string DocumentText
    {
        get => _documentText;
        set => SetProperty(ref _documentText, value);
    }

    public string SelectedFragmentText
    {
        get => _selectedFragmentText;
        set => SetProperty(ref _selectedFragmentText, value);
    }

    public void SetSelectedFragment(string text, int start, int length)
    {
        SelectedFragmentText = text;
        _selectedFragmentStart = start;
        _selectedFragmentLength = length;
    }

    public void ClearSelectedFragment()
    {
        SelectedFragmentText = string.Empty;
        _selectedFragmentStart = -1;
        _selectedFragmentLength = 0;
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public UserRole CurrentRole
    {
        get => _currentRole;
        set
        {
            if (SetProperty(ref _currentRole, value))
            {
                OnPropertyChanged(nameof(CurrentRoleText));
                OnPropertyChanged(nameof(CanDecrypt));
            }
        }
    }

    public string CurrentRoleText => CurrentRole == UserRole.Admin ? "Администратор" : "Сотрудник";

    public bool HasSelectedDocument => SelectedDocument is not null;

    public bool CanEncrypt => HasSelectedDocument && _cryptographyService.IsAvailable && !IsBusy;

    public bool CanDecrypt => CurrentRole == UserRole.Admin && HasSelectedDocument && _cryptographyService.IsAvailable && !IsBusy;

    public string DocumentInfo
    {
        get
        {
            if (SelectedDocument is null)
            {
                return "Нет выбранного документа";
            }

            return $"{SelectedDocument.CaseNumber}\nВладелец: {SelectedDocument.Owner}\nСтатус: {SelectedDocument.Status}\nОбновлен: {SelectedDocument.UpdatedAt:dd.MM.yyyy HH:mm}";
        }
    }

    public string DocumentLocationText => SelectedDocument is null
        ? "Место хранения: документ не выбран"
        : $"Место хранения: {SelectedDocument.StorageText}";

    public void ImportDocument(string fileName, string content, byte[]? originalBytes = null, string? sourceFormat = null, string? fileLocation = null)
    {
        var document = new DocumentItem
        {
            Id = _nextDocumentId++,
            Name = fileName,
            CaseNumber = $"Локальный файл {DateTimeOffset.Now:ddMMyy-HHmm}",
            Owner = CurrentRoleText,
            Status = "Без шифрования",
            UpdatedAt = DateTimeOffset.Now,
            Content = content,
            OriginalFileBytes = originalBytes,
            OriginalText = originalBytes is null ? null : content,
            SourceFormat = sourceFormat,
            FileLocation = fileLocation ?? "Локальное хранилище"
        };

        Documents.Insert(0, document);
        SelectedDocument = document;
        StatusMessage = $"Файл загружен: {fileName}";
        SaveLocalDocuments();
        RecordAudit("Открытие документа", "Успешно", fileName);
    }

    public void ImportEncryptedContainer(string fileName, string json)
    {
        try
        {
            using var container = JsonDocument.Parse(json);
            var root = container.RootElement;
            var format = root.GetProperty("format").GetString();
            if (format is not ("interrogation.enc.v2" or "interrogation.enc.v3"))
            {
                StatusMessage = "Формат контейнера не поддерживается";
                return;
            }

            var metadata = root.GetProperty("document");
            var payload = root.GetProperty("encryptedPayload").GetString();
            if (string.IsNullOrWhiteSpace(payload))
            {
                StatusMessage = "В контейнере отсутствуют зашифрованные данные";
                return;
            }

            var documentName = metadata.TryGetProperty("Name", out var name)
                ? name.GetString() ?? fileName
                : fileName;
            var document = new DocumentItem
            {
                Id = _nextDocumentId++,
                Name = documentName,
                CaseNumber = GetString(metadata, "CaseNumber", "Импортированный контейнер"),
                Owner = GetString(metadata, "Owner", "Не указан"),
                Status = "Зашифрованный контейнер",
                UpdatedAt = DateTimeOffset.Now,
                Content = "[ДОКУМЕНТ ЗАШИФРОВАН]\n\nДля просмотра содержимого выберите роль администратора и выполните расшифровку.",
                EncryptedPayload = payload,
                FileLocation = fileName
            };

            if (format == "interrogation.enc.v3")
            {
                if (!root.TryGetProperty("integrity", out var integrity))
                {
                    StatusMessage = "Контейнер v3 повреждён: отсутствует контроль целостности";
                    RecordAudit("Открытие контейнера", "Ошибка: отсутствует контроль целостности", fileName);
                    return;
                }

                document.IntegritySalt = GetString(integrity, "salt", string.Empty);
                document.IntegrityTag = GetString(integrity, "tag", string.Empty);
                if (string.IsNullOrWhiteSpace(document.IntegritySalt) || string.IsNullOrWhiteSpace(document.IntegrityTag))
                {
                    StatusMessage = "Контейнер v3 повреждён: неверные параметры целостности";
                    RecordAudit("Открытие контейнера", "Ошибка: неверные параметры целостности", fileName);
                    return;
                }
            }

            if (root.TryGetProperty("fragments", out var fragments))
            {
                foreach (var fragment in fragments.EnumerateArray())
                {
                    var fragmentPayload = GetString(fragment, "encryptedPayload", string.Empty);
                    if (string.IsNullOrWhiteSpace(fragmentPayload))
                    {
                        continue;
                    }

                    Fragments.Add(new FragmentRecord
                    {
                        Marker = GetString(fragment, "Marker", "[ENC-FRAGMENT]"),
                        DocumentName = documentName,
                        Preview = "Зашифрованный фрагмент",
                        EncryptedPayload = fragmentPayload,
                        Length = fragment.TryGetProperty("Length", out var length) ? length.GetInt32() : 0,
                        CreatedAt = fragment.TryGetProperty("CreatedAt", out var createdAt) && createdAt.TryGetDateTimeOffset(out var timestamp)
                            ? timestamp
                            : DateTimeOffset.Now
                    });
                }
            }

            Documents.Insert(0, document);
            SelectedDocument = document;
            StatusMessage = $"Контейнер загружен: {fileName}";
            SaveLocalDocuments();
            RecordAudit("Открытие контейнера", "Успешно", fileName);
        }
        catch (JsonException)
        {
            StatusMessage = "Не удалось прочитать контейнер: поврежденный JSON";
            RecordAudit("Открытие контейнера", "Ошибка: повреждённый JSON", fileName);
        }
        catch (InvalidOperationException)
        {
            StatusMessage = "Не удалось прочитать контейнер: неверная структура";
        }
        catch (KeyNotFoundException)
        {
            StatusMessage = "Не удалось прочитать контейнер: отсутствуют обязательные поля";
        }
        catch (FormatException)
        {
            StatusMessage = "Не удалось прочитать контейнер: неверный формат данных";
        }
    }

    public async Task EncryptFullDocumentAsync(string password)
    {
        if (SelectedDocument is null)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        IsBusy = true;
        StatusMessage = "AES-256-GCM шифрует документ...";
        try
        {
            if (!string.IsNullOrWhiteSpace(SelectedDocument.EncryptedPayload))
                throw new InvalidOperationException("Документ уже зашифрован. Сначала расшифруйте его.");
            if (Fragments.Any(item => item.DocumentName == SelectedDocument.Name))
                throw new InvalidOperationException("В документе уже есть зашифрованные фрагменты. Для полного шифрования сначала расшифруйте документ.");

            await ValidateFragmentPasswordsAsync(SelectedDocument.Name, password);
            SelectedDocument.EncryptedPayload = await _cryptographyService.EncryptAsync(DocumentText, password);
            SelectedDocument.Content = "[ДОКУМЕНТ ЗАШИФРОВАН]\n\nСодержимое скрыто. Для просмотра требуется роль администратора и пароль.";
            SelectedDocument.Status = "Полностью зашифрован";
            SelectedDocument.UpdatedAt = DateTimeOffset.Now;
            DocumentText = SelectedDocument.Content;
            OnPropertyChanged(nameof(DocumentInfo));
            SaveLocalDocuments();
            StatusMessage = "Документ зашифрован с помощью AES-256-GCM";
            RecordAudit("Шифрование документа", "Успешно");
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = exception.Message;
            RecordAudit("Шифрование документа", $"Ошибка: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task EncryptSelectedFragmentAsync(string password)
    {
        if (SelectedDocument is null)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedFragmentText))
        {
            StatusMessage = "Выделите фрагмент текста";
            return;
        }

        if (_selectedFragmentStart < 0 ||
            _selectedFragmentLength <= 0 ||
            _selectedFragmentStart + _selectedFragmentLength > DocumentText.Length ||
            !DocumentText.AsSpan(_selectedFragmentStart, _selectedFragmentLength)
                .SequenceEqual(SelectedFragmentText.AsSpan()))
        {
            ClearSelectedFragment();
            StatusMessage = "Текст изменился после выделения. Выберите фрагмент повторно";
            return;
        }

        IsBusy = true;
        StatusMessage = "AES-256-GCM шифрует выбранный фрагмент...";
        try
        {
            if (!string.IsNullOrWhiteSpace(SelectedDocument.EncryptedPayload))
                throw new InvalidOperationException("Документ уже полностью зашифрован. Сначала расшифруйте его.");

            await ValidateFragmentPasswordsAsync(SelectedDocument.Name, password);
            var marker = $"[ENC-FRAGMENT-{DateTimeOffset.Now:HHmmssfff}]";
        var preview = SelectedFragmentText.Length > 80
            ? SelectedFragmentText[..80] + "..."
            : SelectedFragmentText;

        DocumentText = DocumentText[.._selectedFragmentStart]
            + marker
            + DocumentText[(_selectedFragmentStart + _selectedFragmentLength)..];
        SelectedDocument.Content = DocumentText;
        SelectedDocument.Status = "Частично зашифрован";
        SelectedDocument.UpdatedAt = DateTimeOffset.Now;

        Fragments.Insert(0, new FragmentRecord
        {
            Marker = marker,
            DocumentName = SelectedDocument.Name,
            Preview = preview.ReplaceLineEndings(" "),
            EncryptedPayload = await _cryptographyService.EncryptAsync(SelectedFragmentText, password),
            PlainText = SelectedFragmentText,

            Length = SelectedFragmentText.Length,
            CreatedAt = DateTimeOffset.Now
        });

        SelectedFragmentText = marker;
        _selectedFragmentStart = -1;
        _selectedFragmentLength = 0;
        OnPropertyChanged(nameof(DocumentInfo));
            SaveLocalDocuments();
            StatusMessage = "Фрагмент зашифрован с помощью AES-256-GCM";
            RecordAudit("Шифрование фрагмента", "Успешно");
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = exception.Message;
            RecordAudit("Шифрование фрагмента", $"Ошибка: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DecryptDocumentAsync(string password)
    {
        if (!CanDecrypt)
        {
            StatusMessage = "Расшифрование доступно только администратору";
            return;
        }

        if (SelectedDocument is null)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        IsBusy = true;
        StatusMessage = "AES-256-GCM расшифровывает документ...";
        try
        {
            if (!string.IsNullOrWhiteSpace(SelectedDocument.IntegritySalt) &&
                !string.IsNullOrWhiteSpace(SelectedDocument.IntegrityTag))
            {
                var integrity = new IntegrityInfo(SelectedDocument.IntegritySalt, SelectedDocument.IntegrityTag);
                if (!_integrityService.Verify(GetProtectedValues(SelectedDocument.EncryptedPayload), password, integrity))
                {
                    throw new InvalidOperationException("Контейнер изменён, повреждён или указан неверный пароль");
                }
            }

            if (!string.IsNullOrWhiteSpace(SelectedDocument.EncryptedPayload))
            {
                DocumentText = await _cryptographyService.DecryptAsync(SelectedDocument.EncryptedPayload, password);
            }
            else
            {
                DocumentText = SelectedDocument.Content;
            }

            foreach (var fragment in Fragments.Where(item => item.DocumentName == SelectedDocument.Name))
            {
                var plainText = await _cryptographyService.DecryptAsync(fragment.EncryptedPayload, password);
                DocumentText = DocumentText.Replace(fragment.Marker, plainText, StringComparison.Ordinal);
            }
            foreach (var fragment in Fragments.Where(item => item.DocumentName == SelectedDocument.Name).ToArray())
            {
                Fragments.Remove(fragment);
            }

            SelectedDocument.Content = DocumentText;
            SelectedDocument.EncryptedPayload = null;
            SelectedDocument.IntegritySalt = null;
            SelectedDocument.IntegrityTag = null;
            SelectedDocument.Status = "Расшифрован";
            SelectedDocument.UpdatedAt = DateTimeOffset.Now;
            OnPropertyChanged(nameof(DocumentInfo));
            SaveLocalDocuments();
            StatusMessage = "Документ расшифрован с помощью AES-256-GCM";
            RecordAudit("Расшифрование документа", "Успешно");
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = exception.Message;
            RecordAudit("Расшифрование документа", $"Ошибка: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SaveDraft()
    {
        if (SelectedDocument is null)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        SelectedDocument.Content = DocumentText;
        SelectedDocument.UpdatedAt = DateTimeOffset.Now;
        OnPropertyChanged(nameof(DocumentInfo));
        SaveLocalDocuments();
        StatusMessage = "Черновик сохранен локально";
        RecordAudit("Сохранение черновика", "Успешно");
    }

    public async Task DeleteSelectedDocumentAsync()
    {
        var document = SelectedDocument;
        if (document is null || IsBusy)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        IsBusy = true;
        try
        {
            if (document.IsRemote)
            {
                await _apiClient.DeleteDocumentAsync(document.Id);
            }

            Documents.Remove(document);
            foreach (var fragment in Fragments.Where(item => item.DocumentName == document.Name).ToArray())
            {
                Fragments.Remove(fragment);
            }
            SelectedDocument = Documents.FirstOrDefault();
            SaveLocalDocuments();
            StatusMessage = $"Документ удален: {document.Name}";
            RecordAudit("Удаление документа", "Успешно", document.Name);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            StatusMessage = $"Не удалось удалить документ: {exception.Message}";
            RecordAudit("Удаление документа", $"Ошибка: {exception.Message}", document.Name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<string> BuildEncryptedContainerJsonAsync(string password)
    {
        if (SelectedDocument is null)
        {
            return "{}";
        }

        await ValidateFragmentPasswordsAsync(SelectedDocument.Name, password);
        var encryptedPayload = SelectedDocument.EncryptedPayload;
        if (string.IsNullOrWhiteSpace(encryptedPayload))
        {
            encryptedPayload = await _cryptographyService.EncryptAsync(DocumentText, password);
        }
        else
        {
            await _cryptographyService.DecryptAsync(encryptedPayload, password);
        }

        var integrity = _integrityService.Create(GetProtectedValues(encryptedPayload), password);
        var container = new
        {
            format = "interrogation.enc.v3",
            cryptography = new
            {
                engine = "System.Security.Cryptography",
                cipher = "AES-256-GCM",
                kdf = "PBKDF2-HMAC-SHA256",
                iterations = 200000
            },
            integrity = new
            {
                algorithm = "HMAC-SHA256",
                kdf = "PBKDF2-HMAC-SHA256",
                iterations = 200000,
                salt = integrity.Salt,
                tag = integrity.Tag
            },
            exportedAt = DateTimeOffset.Now,
            role = CurrentRoleText,
            document = new
            {
                SelectedDocument.Id,
                SelectedDocument.Name,
                SelectedDocument.CaseNumber,
                SelectedDocument.Status,
                SelectedDocument.Owner,
                SelectedDocument.UpdatedAt
            },
            encryptedPayload,
            fragments = Fragments
                .Where(fragment => fragment.DocumentName == SelectedDocument.Name)
                .Select(fragment => new
                {
                    fragment.Marker,
                    encryptedPayload = fragment.EncryptedPayload,
                    fragment.Length,
                    fragment.CreatedAt
                })
                .ToArray()
        };

        return JsonSerializer.Serialize(container, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task UploadSelectedDocumentAsync()
    {
        var document = SelectedDocument;
        if (document is null || document.IsRemote || IsBusy)
        {
            StatusMessage = document?.IsRemote == true ? "Документ уже находится на сервере" : "Сначала откройте локальный документ";
            return;
        }

        var mode = SelectedServerMode.Contains("Full", StringComparison.Ordinal) ? 1
            : SelectedServerMode.Contains("Part", StringComparison.Ordinal) ? 2 : 3;
        var localFragments = Fragments.Where(item => item.DocumentName == document.Name).ToArray();
        if (mode == 2 && localFragments.Length == 0)
        {
            StatusMessage = "Для режима Part зашифруйте хотя бы один фрагмент";
            return;
        }

        IsBusy = true;
        ApiSecret? secret = null;
        var documentCreated = false;
        var createdDocumentId = 0;
        try
        {
            StatusMessage = "Подготовка документа к отправке...";
            // Backend currently requires SecretId even for None.
            secret = await _apiClient.CreateSecretAsync();
            byte[] bytes;
            string fileName;
            string contentType;
            if (mode == 1)
            {
                bytes = Encoding.UTF8.GetBytes(await BuildServerFullContainerJsonAsync(document, secret.Value, localFragments));
                fileName = Path.GetFileNameWithoutExtension(document.Name) + ".enc";
                contentType = "application/json";
            }
            else if (mode == 2)
            {
                EnsurePartDocumentContainsOnlyMarkers(localFragments);
                (bytes, fileName, contentType) = BuildPartDocumentUpload(document);
            }
            else
            {
                bytes = document.OriginalFileBytes ?? Encoding.UTF8.GetBytes(DocumentText);
                fileName = document.Name;
                contentType = document.SourceFormat == "docx"
                    ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    : document.SourceFormat == "odt" ? "application/vnd.oasis.opendocument.text" : "text/plain";
            }

            if (bytes.Length > _apiClient.MaxUploadBytes)
                throw new InvalidOperationException($"Размер файла превышает серверный лимит {_apiClient.MaxUploadBytes / 1024 / 1024} МБ");

            var created = await _apiClient.UploadDocumentAsync(new DocumentUpload(
                fileName, contentType, bytes, mode, secret.Id, Path.GetFileNameWithoutExtension(document.Name),
                document.InvestigationActionType, mode == 3 ? null : "AES-256-GCM"));
            documentCreated = true;
            createdDocumentId = created.Id;

            if (mode == 2)
            {
                foreach (var fragment in localFragments)
                {
                    if (fragment.PlainText is null)
                        throw new InvalidOperationException("Импортированный фрагмент нельзя перенести без повторного выделения");
                    var encrypted = await _cryptographyService.EncryptAsync(fragment.PlainText, secret.Value);
                    await _apiClient.CreateFragmentAsync(created.Id, fragment.Marker, encrypted);
                }
            }

            StatusMessage = $"Документ отправлен: {SelectedServerMode}";
            RecordAudit("Отправка документа", "Успешно", document.Name);
            await InitializeAsync();
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            if (documentCreated)
            {
                try { await _apiClient.DeleteDocumentAsync(createdDocumentId); } catch (HttpRequestException) { }
            }
            if (secret is not null)
            {
                try { await _apiClient.DeleteSecretAsync(secret.Id); } catch (HttpRequestException) { }
            }
            StatusMessage = $"Не удалось отправить документ: {exception.Message}";
            RecordAudit("Отправка документа", $"Ошибка: {exception.Message}", document.Name);
        }
        finally { IsBusy = false; }
    }
    private async Task ValidateFragmentPasswordsAsync(string documentName, string password)
    {
        foreach (var fragment in Fragments.Where(item => item.DocumentName == documentName))
        {
            await _cryptographyService.DecryptAsync(fragment.EncryptedPayload, password);
        }
    }

    private void EnsurePartDocumentContainsOnlyMarkers(IReadOnlyCollection<FragmentRecord> localFragments)
    {
        foreach (var fragment in localFragments)
        {
            if (!DocumentText.Contains(fragment.Marker, StringComparison.Ordinal))
                throw new InvalidOperationException($"В документе отсутствует маркер фрагмента {fragment.Marker}. Повторите шифрование фрагмента.");
            if (!string.IsNullOrEmpty(fragment.PlainText) && DocumentText.Contains(fragment.PlainText, StringComparison.Ordinal))
                throw new InvalidOperationException("В документе остался открытый текст зашифрованного фрагмента. Повторите выделение и шифрование.");
        }
    }

    private (byte[] Bytes, string FileName, string ContentType) BuildPartDocumentUpload(DocumentItem document)
    {
        var baseName = Path.GetFileNameWithoutExtension(document.Name);
        return document.SourceFormat switch
        {
            "docx" => (
                _documentExportService.ExportDocx(DocumentText, document),
                baseName + ".docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            "odt" => (
                _documentExportService.ExportOdt(DocumentText, document),
                baseName + ".odt",
                "application/vnd.oasis.opendocument.text"),
            _ => (Encoding.UTF8.GetBytes(DocumentText), baseName + ".txt", "text/plain")
        };
    }

    private async Task<string> BuildServerFullContainerJsonAsync(DocumentItem document, string serverSecret, IReadOnlyCollection<FragmentRecord> localFragments)
    {
        if (!string.IsNullOrWhiteSpace(document.EncryptedPayload))
            throw new InvalidOperationException("Сначала расшифруйте локально зашифрованный документ перед отправкой в режиме Full.");
        if (localFragments.Count > 0)
            throw new InvalidOperationException("Для режима Full нужен цельный документ без локально зашифрованных фрагментов. Используйте Part или расшифруйте фрагменты.");

        var encryptedPayload = await _cryptographyService.EncryptAsync(DocumentText, serverSecret);
        var integrity = _integrityService.Create([encryptedPayload], serverSecret);
        var container = new
        {
            format = "interrogation.enc.v3",
            cryptography = new
            {
                engine = "System.Security.Cryptography",
                cipher = "AES-256-GCM",
                kdf = "PBKDF2-HMAC-SHA256",
                iterations = 200000
            },
            integrity = new
            {
                algorithm = "HMAC-SHA256",
                kdf = "PBKDF2-HMAC-SHA256",
                iterations = 200000,
                salt = integrity.Salt,
                tag = integrity.Tag
            },
            exportedAt = DateTimeOffset.Now,
            role = CurrentRoleText,
            document = new
            {
                document.Id,
                document.Name,
                document.CaseNumber,
                document.Status,
                document.Owner,
                document.UpdatedAt
            },
            encryptedPayload,
            fragments = Array.Empty<object>()
        };

        return JsonSerializer.Serialize(container, new JsonSerializerOptions { WriteIndented = true });
    }

    private IEnumerable<string> GetProtectedValues(string? documentPayload)
    {
        yield return documentPayload ?? string.Empty;
        if (SelectedDocument is null)
        {
            yield break;
        }

        foreach (var fragment in Fragments.Where(item => item.DocumentName == SelectedDocument.Name))
        {
            yield return fragment.Marker;
            yield return fragment.EncryptedPayload;
        }
    }

    public void RecordAudit(string action, string result, string? documentName = null)
    {
        var record = new AuditRecord(
            DateTimeOffset.Now,
            CurrentUserName,
            action,
            documentName ?? SelectedDocument?.Name ?? "Документ не выбран",
            result);
        _auditHistory.Insert(0, record);
        ApplyAuditFilter();
        try
        {
            _auditLogService.Append(record);
        }
        catch (IOException)
        {
            StatusMessage = "Операция выполнена, но журнал не удалось сохранить";
        }
    }

    private void SaveLocalDocuments()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LocalStorePath)!);
            var localDocuments = Documents.Where(document => !document.IsRemote).Select(document => new LocalDocumentState(
                document.Id,
                document.Name,
                document.CaseNumber,
                document.Owner,
                document.InvestigationActionType,
                document.Status,
                document.Content,
                document.UpdatedAt,
                document.EncryptedPayload,
                document.IntegritySalt,
                document.IntegrityTag,
                document.SourceFormat,
                document.FileLocation)).ToArray();
            var fragments = Fragments.Select(fragment => new LocalFragmentState(
                fragment.Marker,
                fragment.DocumentName,
                fragment.Preview,
                fragment.EncryptedPayload,
                fragment.PlainText,
                fragment.Length,
                fragment.CreatedAt)).ToArray();
            File.WriteAllText(LocalStorePath, JsonSerializer.Serialize(new LocalStoreState(localDocuments, fragments), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (IOException)
        {
            StatusMessage = "Локальные документы обновлены, но их не удалось сохранить на диск";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "Нет прав на сохранение локальных документов";
        }
    }

    private void LoadLocalDocuments()
    {
        try
        {
            if (!File.Exists(LocalStorePath)) return;
            var json = File.ReadAllText(LocalStorePath);
            var store = JsonSerializer.Deserialize<LocalStoreState>(json);
            var localDocuments = store?.Documents ?? [];
            foreach (var item in localDocuments.OrderByDescending(item => item.UpdatedAt))
            {
                Documents.Add(new DocumentItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    CaseNumber = item.CaseNumber,
                    Owner = item.Owner,
                    InvestigationActionType = item.InvestigationActionType,
                    Status = item.Status,
                    Content = item.Content,
                    UpdatedAt = item.UpdatedAt,
                    EncryptedPayload = item.EncryptedPayload,
                    IntegritySalt = item.IntegritySalt,
                    IntegrityTag = item.IntegrityTag,
                    SourceFormat = item.SourceFormat,
                    FileLocation = item.FileLocation
                });
                _nextDocumentId = Math.Max(_nextDocumentId, item.Id + 1);
            }
            foreach (var item in store?.Fragments ?? [])
            {
                Fragments.Add(new FragmentRecord
                {
                    Marker = item.Marker,
                    DocumentName = item.DocumentName,
                    Preview = item.Preview,
                    EncryptedPayload = item.EncryptedPayload,
                    PlainText = item.PlainText,
                    Length = item.Length,
                    CreatedAt = item.CreatedAt
                });
            }
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            StatusMessage = "Не удалось восстановить локальные документы";
        }
    }

    private void ApplyAuditFilter()
    {
        var query = _auditHistory.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(AuditSearchText))
        {
            query = query.Where(record =>
                record.Action.Contains(AuditSearchText, StringComparison.OrdinalIgnoreCase) ||
                record.DocumentName.Contains(AuditSearchText, StringComparison.OrdinalIgnoreCase) ||
                record.UserName.Contains(AuditSearchText, StringComparison.OrdinalIgnoreCase));
        }
        query = AuditResultFilter switch
        {
            "Успешно" => query.Where(record => record.Result.StartsWith("Успешно", StringComparison.OrdinalIgnoreCase)),
            "Ошибки" => query.Where(record => record.Result.StartsWith("Ошибка", StringComparison.OrdinalIgnoreCase)),
            _ => query
        };
        AuditLog.Clear();
        foreach (var record in query) AuditLog.Add(record);
    }

    private static string GetString(JsonElement element, string propertyName, string fallback)
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.GetString() ?? fallback
            : fallback;
    }

    private sealed record LocalStoreState(LocalDocumentState[] Documents, LocalFragmentState[] Fragments);

    private sealed record LocalDocumentState(
        int Id,
        string Name,
        string CaseNumber,
        string Owner,
        string InvestigationActionType,
        string Status,
        string Content,
        DateTimeOffset UpdatedAt,
        string? EncryptedPayload,
        string? IntegritySalt,
        string? IntegrityTag,
        string? SourceFormat,
        string? FileLocation);

    private sealed record LocalFragmentState(
        string Marker,
        string DocumentName,
        string Preview,
        string EncryptedPayload,
        string? PlainText,
        int Length,
        DateTimeOffset CreatedAt);
}
