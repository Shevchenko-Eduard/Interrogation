namespace Domain.Interfaces;

public interface ISecretManager
{
    string Create(int numberOfBytes);
}