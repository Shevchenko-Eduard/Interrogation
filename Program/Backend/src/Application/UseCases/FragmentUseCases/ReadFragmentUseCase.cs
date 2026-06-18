using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.FragmentUseCases;

public class ReadFragmentUseCase(IFragmentRepository fragmentRepository) : IQuestion<FragmentDTOs.Inner.ReadById, FragmentDTOs.Response.Read>
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    public Task<FragmentDTOs.Inner.ReadById> Ask(FragmentDTOs.Response.Read input)
    {
        var fragment = _fragmentRepository.GetByIdAsync(input.Id);
        return Task.FromResult(new FragmentDTOs.Inner.ReadById(fragment.Id));
    }
}