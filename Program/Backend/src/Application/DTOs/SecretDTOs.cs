using Domain.Entity;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.DTOs;

public static class SecretDTOs
{
    public static class Inner
    {
        public record Create()
        {
            public Secret GetSecret(ISecretManager secretGenerator) => new(secretGenerator);
        }
        public record Delete(
            int Id
        );
        public record Read(
            int Id
        )
        {
            public async Task<Secret> GetSecret(ISecretRepository secretRepository)
            {
                return await secretRepository.GetByIdAsync(Id)
                    ?? throw new Exception($"Секрета с таким Id:{Id} не существует.");
            }
        }
    }
    public static class Request
    {

    }
    public static class Response
    {
        public record Create(
            int Id,
            string Value
        )
        {
            public static Create FromSecret(Secret secret) => new(
                    Id: secret.Id,
                    Value: secret.Value
                );
        }
        public record Read(
            int Id,
            string Value
        )
        {
            public static Read FromSecret(Secret secret, ISecretManager secretManager) => new(
                    Id: secret.Id,
                    Value: secretManager.Decrypt(secret.Value)
                );
        }
    }
}