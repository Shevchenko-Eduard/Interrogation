using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;

namespace Application.UseCases.DocumentUseCases;

public class DeleteDocumentUseCase(
    IDocumentRepository documentRepository,
    IS3DocumentRepository documentS3Repository,
    IUnitOfWork unitOfWork) : IAction<DocumentDTOs.Inner.Delete>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IS3DocumentRepository _documentS3Repository = documentS3Repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task Execute(DocumentDTOs.Inner.Delete input)
    {
        ArgumentNullException.ThrowIfNull(input);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            string documentKey = await _documentRepository.GetDocumentKeyByIdAsync(input.Id).ConfigureAwait(false)
                ?? throw new Exception($"Документа с таким Id:{input.Id} не существует");
            await _documentRepository.DeleteAsync(input.Id).ConfigureAwait(false);
            await _documentS3Repository.DeleteAsync(documentKey).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
            throw;
        }
    }
}