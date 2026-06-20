using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.SecretUseCases;

public sealed class CreateSecretUseCase(
    ISecretRepository secretRepository,
    IUnitOfWork unitOfWork,
    ISecretManager secretManager) : IAction<SecretDTOs.Inner.Create, SecretDTOs.Response.Create>
{
    private readonly ISecretRepository _secretRepository = secretRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISecretManager _secretGenerator = secretManager;
    public async Task<SecretDTOs.Response.Create> Execute(SecretDTOs.Inner.Create input)
    {
        var secret = input.GetSecret(_secretGenerator);
        await _secretRepository.AddAsync(secret);
        await _unitOfWork.SaveChangesAsync();
        return SecretDTOs.Response.Create.FromSecret(secret);
    }
}