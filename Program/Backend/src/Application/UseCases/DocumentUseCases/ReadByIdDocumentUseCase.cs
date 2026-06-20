using System.Linq.Expressions;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;

namespace Application.UseCases.DocumentUseCases;

public class ReadByIdDocumentUseCase(
    IDocumentRepository documentRepository,
    IS3DocumentRepository s3DocumentRepository) : IQuestion<DocumentDTOs.Response.ReadById, DocumentDTOs.Inner.ReadById>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IS3DocumentRepository _s3DocumentRepository = s3DocumentRepository;
    public async Task<DocumentDTOs.Response.ReadById> Ask(DocumentDTOs.Inner.ReadById input)
    {
        var document = await _documentRepository.GetByIdAsync(input.Id)
            ?? throw new Exception($"Документа с Id:{input.Id} не существует.");
        return DocumentDTOs.Response.ReadById.FromDocument(document);
    }
}