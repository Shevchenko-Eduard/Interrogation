using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.FragmentUseCases;

public class DeleteFragmentUseCase(IFragmentRepository fragmentRepository, IUnitOfWork unitOfWork) : IAction<FragmentDTOs.Inner.Delete>
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task Execute(FragmentDTOs.Inner.Delete input)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _fragmentRepository.DeleteAsync(input.Id).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
}