namespace Infrastructure.Interfaces;

public interface ICryptograph
{
    string Encrypt(string value);
    string Decrypt(string value);
}