namespace Interrogation.Client.Services;

public interface ICryptographyService
{
    bool IsAvailable { get; }
    string EngineDescription { get; }
    Task<string> EncryptAsync(string plainText, string password);
    Task<string> DecryptAsync(string encryptedText, string password);
}
