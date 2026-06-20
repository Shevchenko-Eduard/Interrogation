using System.Security.Cryptography;
using Domain.Interfaces;

namespace Infrastructure;

public class SecretManager : ISecretManager
{
    public string New(int numberOfBytes)
    {
        byte[] key = new byte[numberOfBytes];
        RandomNumberGenerator.Fill(key);
        return Convert.ToHexString(key);
    }
}