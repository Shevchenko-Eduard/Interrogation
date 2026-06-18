using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.SecretUseCases;

public sealed class ReadSecretUseCase(
    ISecretRepository secretRepository,
    ISecretManager secretManager) : IQuestion<SecretDTOs.Response.Read, SecretDTOs.Inner.Read>
{
    private readonly ISecretRepository _secretRepository = secretRepository;
    private readonly ISecretManager _secretManager = secretManager;
    public async Task<SecretDTOs.Response.Read> Ask(SecretDTOs.Inner.Read input)
    {
        Secret secret = await input.GetSecret(_secretRepository);
        return SecretDTOs.Response.Read.FromSecret(secret, _secretManager);
    }
}