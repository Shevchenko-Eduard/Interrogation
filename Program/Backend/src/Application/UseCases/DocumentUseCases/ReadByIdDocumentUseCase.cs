using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.DocumentUseCases;

public class ReadByIdDocumentUseCase(
    IDocumentRepository documentRepository) : IQuestion<DocumentDTOs.Response.ReadById, DocumentDTOs.Inner.ReadById>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    public async Task<DocumentDTOs.Response.ReadById> Ask(DocumentDTOs.Inner.ReadById input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var document = await _documentRepository.GetByIdAsync(input.Id).ConfigureAwait(false)
            ?? throw new Exception($"Документа с Id:{input.Id} не существует.");
        return DocumentDTOs.Response.ReadById.FromDocument(document);
    }
}