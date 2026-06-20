using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.FragmentUseCases;

public class ReadByDocumentIdFragmentUseCase(IFragmentRepository fragmentRepository) : IQuestion<List<FragmentDTOs.Response.Read>, FragmentDTOs.Inner.ReadByDocumentId>
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    public async Task<List<FragmentDTOs.Response.Read>> Ask(FragmentDTOs.Inner.ReadByDocumentId input)
    {
        var fragments = await _fragmentRepository.GetAsync(d => d.DocumentId == input.DocumentId)
            ?? throw new Exception($"Документов с таким Id:{input.DocumentId} не существует.");
        return [.. fragments.Select(FragmentDTOs.Response.Read.FromFragment)];
    }
}