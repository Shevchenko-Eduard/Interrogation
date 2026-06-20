using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces;
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
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            string documentKey = await _documentRepository.GetDocumentKeyByIdAsync(input.Id)
                ?? throw new Exception($"Документа с таким Id:{input.Id} не существует");
            await _documentRepository.DeleteAsync(input.Id);
            await _documentS3Repository.DeleteAsync(documentKey);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}