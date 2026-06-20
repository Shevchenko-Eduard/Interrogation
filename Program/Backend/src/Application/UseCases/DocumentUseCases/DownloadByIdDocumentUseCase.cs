using System.Linq.Expressions;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;

namespace Application.UseCases.DocumentUseCases;

public class DownloadByIdDocumentUseCase(
    IDocumentRepository documentRepository,
    IS3DocumentRepository s3DocumentRepository) : IQuestion<DocumentDTOs.Response.DownloadById, DocumentDTOs.Inner.DownloadById>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IS3DocumentRepository _s3DocumentRepository = s3DocumentRepository;
    public async Task<DocumentDTOs.Response.DownloadById> Ask(DocumentDTOs.Inner.DownloadById input)
    {
        var document = await _documentRepository.GetByIdAsync(input.Id)
            ?? throw new Exception($"Документа с Id:{input.Id} не существует.");
        Stream stream = await _s3DocumentRepository.DownloadAsync(document.DocumentKey);
        return DocumentDTOs.Response.DownloadById.FromDocument(document, stream);
    }
}