using System.Security.Cryptography;
using System.Text;

namespace Interrogation.Client.Services;

// Name retained to avoid breaking existing containers and dependency registration.
public sealed class OpenSslCryptographyService : ICryptographyService
{
    private const int Iterations = 200_000;
    private const int KeySize = 32;

    public bool IsAvailable => AesGcm.IsSupported;
    public string EngineDescription => "AES-256-GCM / System.Security.Cryptography";

    public Task<string> EncryptAsync(string plainText, string password) => Task.Run(() =>
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        var plaintext = Encoding.UTF8.GetBytes(plainText);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        CryptographicOperations.ZeroMemory(key);
        return $"aesgcm:v1:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(nonce)}:{Convert.ToBase64String(tag)}:{Convert.ToBase64String(ciphertext)}";
    });

    public Task<string> DecryptAsync(string encryptedText, string password) => Task.Run(() =>
    {
        var parts = encryptedText.Split(':');
        if (parts.Length != 6 || parts[0] != "aesgcm" || parts[1] != "v1")
            throw new InvalidOperationException("Неподдерживаемый формат шифротекста");
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var nonce = Convert.FromBase64String(parts[3]);
            var tag = Convert.FromBase64String(parts[4]);
            var ciphertext = Convert.FromBase64String(parts[5]);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            var plaintext = new byte[ciphertext.Length];
            using var aes = new AesGcm(key, tag.Length);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            CryptographicOperations.ZeroMemory(key);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException)
        {
            throw new InvalidOperationException("Неверный ключ или повреждённые данные", exception);
        }
    });
}
