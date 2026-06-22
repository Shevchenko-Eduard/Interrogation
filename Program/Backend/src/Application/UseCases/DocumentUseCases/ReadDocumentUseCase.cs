using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.DocumentUseCases;

public class ReadDocumentUseCase(IDocumentRepository documentRepository) : IQuestion<IEnumerable<DocumentDTOs.Response.Read>, DocumentDTOs.Inner.Read>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    public async Task<IEnumerable<DocumentDTOs.Response.Read>> Ask(DocumentDTOs.Inner.Read input)
    {
        var documents = await _documentRepository.GetAsync(expression: _ => true).ConfigureAwait(false);
        return [.. documents.Select(DocumentDTOs.Response.Read.FromDocument)];
    }
}