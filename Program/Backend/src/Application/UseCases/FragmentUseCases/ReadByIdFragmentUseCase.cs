using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.FragmentUseCases;

public class ReadByIdFragmentUseCase(IFragmentRepository fragmentRepository) : IQuestion<FragmentDTOs.Response.Read, FragmentDTOs.Inner.ReadById>
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    public async Task<FragmentDTOs.Response.Read> Ask(FragmentDTOs.Inner.ReadById input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var fragment = await _fragmentRepository.GetByIdAsync(input.Id).ConfigureAwait(false)
            ?? throw new Exception($"Фрагмента с таким Id:{input.Id} не существует.");
        return FragmentDTOs.Response.Read.FromFragment(fragment);
    }
}