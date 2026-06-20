using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.SecretUseCases;

public sealed class ReadByIdSecretUseCase(
    ISecretRepository secretRepository) : IQuestion<SecretDTOs.Response.Read, SecretDTOs.Inner.ReadById>
{
    private readonly ISecretRepository _secretRepository = secretRepository;
    public async Task<SecretDTOs.Response.Read> Ask(SecretDTOs.Inner.ReadById input)
    {
        Secret secret = await input.GetSecret(_secretRepository);
        return SecretDTOs.Response.Read.FromSecret(secret);
    }
}