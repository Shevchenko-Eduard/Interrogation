namespace Domain.Interfaces;

public interface ISecretManager
{
    string New();
    string Encrypt(string value);
    string Decrypt(string value);
}