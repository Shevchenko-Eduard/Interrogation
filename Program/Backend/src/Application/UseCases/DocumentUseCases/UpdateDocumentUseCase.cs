using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.DocumentUseCases;

public class UpdateDocumentUseCase(IUnitOfWork unitOfWork, IDocumentRepository documentRepository) : IAction<DocumentDTOs.Inner.Update>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task Execute(DocumentDTOs.Inner.Update input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var document = await _documentRepository.GetByIdAsync(input.Id).ConfigureAwait(false)
            ?? throw new Exception($"Документа с таким Id:{input.Id} не существует.");
        document = input.GetDocument(document: document);
        await _documentRepository.UpdateAsync(document).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
}