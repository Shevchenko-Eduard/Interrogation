using System.Security.Cryptography;
using System.Text;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class ContainerIntegrityService : IContainerIntegrityService
{
    private const int Iterations = 200_000;

    public IntegrityInfo Create(IEnumerable<string> protectedValues, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var tag = ComputeTag(protectedValues, password, salt);
        return new IntegrityInfo(Convert.ToBase64String(salt), Convert.ToBase64String(tag));
    }

    public bool Verify(IEnumerable<string> protectedValues, string password, IntegrityInfo integrity)
    {
        try
        {
            var salt = Convert.FromBase64String(integrity.Salt);
            var expectedTag = Convert.FromBase64String(integrity.Tag);
            var actualTag = ComputeTag(protectedValues, password, salt);
            return CryptographicOperations.FixedTimeEquals(actualTag, expectedTag);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] ComputeTag(IEnumerable<string> protectedValues, string password, byte[] salt)
    {
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        using var hmac = new HMACSHA256(key);
        var canonicalValue = string.Concat(protectedValues.Select(value => $"{value.Length}:{value}"));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(canonicalValue));
    }
}
