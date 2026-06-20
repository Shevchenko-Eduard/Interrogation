using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.DocumentUseCases;

public class ReadByIdDocumentUseCase(
    IDocumentRepository documentRepository) : IQuestion<DocumentDTOs.Response.Read, DocumentDTOs.Inner.ReadById>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    public async Task<DocumentDTOs.Response.Read> Ask(DocumentDTOs.Inner.ReadById input)
    {
        var document = await _documentRepository.GetByIdAsync(input.Id)
            ?? throw new Exception($"Документа с Id:{input.Id} не существует.");
        return DocumentDTOs.Response.Read.FromDocument(document);
    }
}