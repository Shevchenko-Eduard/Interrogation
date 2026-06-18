using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.FragmentUseCases;

public class CreateFragmentUseCase(
    IFragmentRepository fragmentRepository,
    IUnitOfWork unitOfWork) : IAction<FragmentDTOs.Inner.Create, FragmentDTOs.Response.Create>
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<FragmentDTOs.Response.Create> Execute(FragmentDTOs.Inner.Create input)
    {
        Fragment fragment = input.GetFragment();
        await _fragmentRepository.AddAsync(fragment);
        await _unitOfWork.SaveChangesAsync();
        return FragmentDTOs.Response.Create.FromFragment(fragment);
    }
}