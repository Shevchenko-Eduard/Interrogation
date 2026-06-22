using System.Diagnostics;
using System.Text;

namespace Interrogation.Client.Services;

public sealed class OpenSslCryptographyService : ICryptographyService
{
    private const int Iterations = 200_000;
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private readonly string? _executablePath;

    public OpenSslCryptographyService()
    {
        _executablePath = FindModernOpenSsl();
        EngineDescription = _executablePath is null
            ? "OpenSSL 1.1.1+ не найден"
            : ReadVersion(_executablePath);
    }

    public bool IsAvailable => _executablePath is not null;

    public string EngineDescription { get; }

    public Task<string> EncryptAsync(string plainText, string password) =>
        ExecuteAsync(plainText, password, decrypt: false);

    public Task<string> DecryptAsync(string encryptedText, string password) =>
        ExecuteAsync(encryptedText, password, decrypt: true);

    private async Task<string> ExecuteAsync(string input, string password, bool decrypt)
    {
        if (_executablePath is null)
        {
            throw new InvalidOperationException("Современный OpenSSL не найден");
        }

        var arguments = decrypt
            ? $"enc -d -aes-256-cbc -pbkdf2 -iter {Iterations} -md sha256 -a -A -pass env:INTERROGATION_PASSWORD"
            : $"enc -aes-256-cbc -salt -pbkdf2 -iter {Iterations} -md sha256 -a -A -pass env:INTERROGATION_PASSWORD";

        var startInfo = new ProcessStartInfo(_executablePath, arguments)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Utf8WithoutBom,
            StandardOutputEncoding = Utf8WithoutBom,
            StandardErrorEncoding = Utf8WithoutBom,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["INTERROGATION_PASSWORD"] = password;

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Не удалось запустить OpenSSL");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteAsync(input);
        process.StandardInput.Close();
        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(decrypt
                ? "Неверный пароль или повреждённые данные"
                : $"OpenSSL завершился с ошибкой: {error.Trim()}");
        }

        return output.Trim();
    }

    private static string? FindModernOpenSsl()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "openssl", "openssl.exe"),
            @"C:\Program Files\Git\usr\bin\openssl.exe",
            @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe",
            @"C:\Program Files\OpenSSL\bin\openssl.exe"
        };

        return candidates.FirstOrDefault(path => File.Exists(path) && IsModernVersion(ReadVersion(path)));
    }

    private static string ReadVersion(string executablePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo(executablePath, "version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            var version = process?.StandardOutput.ReadToEnd().Trim();
            process?.WaitForExit();
            return string.IsNullOrWhiteSpace(version) ? "OpenSSL" : version;
        }
        catch
        {
            return "OpenSSL";
        }
    }

    private static bool IsModernVersion(string version) =>
        version.StartsWith("OpenSSL 3.", StringComparison.Ordinal) ||
        version.StartsWith("OpenSSL 1.1.1", StringComparison.Ordinal);
}
