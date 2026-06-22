using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;

namespace Application.UseCases.DocumentUseCases;

public class CreateDocumentUseCase(
    IDocumentRepository documentRepository,
    IS3DocumentRepository documentS3Repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IDocumentKeyManager documentKeyManager) : IAction<DocumentDTOs.Inner.Create, DocumentDTOs.Response.Create>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IS3DocumentRepository _documentS3Repository = documentS3Repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IClock _clock = clock;
    private readonly IDocumentKeyManager _documentKeyManager = documentKeyManager;
    public async Task<DocumentDTOs.Response.Create> Execute(DocumentDTOs.Inner.Create input)
    {
        ArgumentNullException.ThrowIfNull(input);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            Document document = input.GetDocument(documentKeyGenerator: _documentKeyManager, clock: _clock);
            await _documentRepository.AddAsync(document).ConfigureAwait(false);
            await _documentS3Repository.UploadAsync(input.Stream, document.DocumentKey).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return DocumentDTOs.Response.Create.FromDocument(document);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
            throw;
        }
    }
}