using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.SecretUseCases;

public sealed class DeleteSecretUseCase(
    ISecretRepository secretRepository,
    IUnitOfWork unitOfWork) : IAction<SecretDTOs.Inner.Delete>
{
    private readonly ISecretRepository _secretRepository = secretRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task Execute(SecretDTOs.Inner.Delete input)
    {
        await _secretRepository.DeleteAsync(input.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}