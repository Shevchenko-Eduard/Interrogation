using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.DocumentUseCases;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.WebApi;

[ApiController]
[Route("")]
[Authorize]
public class DocumentController(
    IDocumentRepository documentRepository,
    IClock clock,
    IDocumentKeyManager documentKeyManager,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IS3DocumentRepository s3DocumentRepository) : ControllerBase
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IS3DocumentRepository _s3DocumentRepository = s3DocumentRepository;
    private readonly IClock _clock = clock;
    private readonly IDocumentKeyManager _documentKeyManager = documentKeyManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUser _currentUser = currentUser;

    [HttpPost("v1/documents", Name = "DocumentCreate")]
    [Authorize(Roles = "DocumentCreate")]
    public async Task<IActionResult> Create(IFormFile file, [FromForm] DocumentDTOs.Request.Create request)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не выбран или пустой.");
        }

        var useCase = new CreateDocumentUseCase(
            _documentRepository,
            _s3DocumentRepository,
            _unitOfWork,
            _clock,
            _documentKeyManager);

        DocumentDTOs.Inner.Create innerRequest = new(
            EncryptionTypeId: request.EncryptionTypeId,
            SecretId: request.SecretId,
            CreatorId: _currentUser.Id
                ?? throw new Exception("Пользователь не зарегистрирован или у него отсутствует Id."),
            Name: request.Name,
            Description: request.Description,
            ContentType: file.ContentType,
            Extension: Path.GetExtension(file.FileName),
            EncryptionAlgorithm: request.EncryptionAlgorithm,
            Stream: file.OpenReadStream());

        var result = await useCase.Execute(innerRequest);
        return Ok(result);
    }

    [HttpPut("v1/documents/{id:int}", Name = "DocumentUpdate")]
    [Authorize(Roles = "DocumentUpdate")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DocumentDTOs.Request.Update request)
    {
        UpdateDocumentUseCase useCase = new(
            unitOfWork: _unitOfWork,
            documentRepository: _documentRepository);

        DocumentDTOs.Inner.Update innerRequest = new(
            Id: id,
            Name: request.Name,
            Description: request.Description
        );

        await useCase.Execute(innerRequest);
        return NoContent();
    }

    [HttpDelete("v1/documents/{id:int}", Name = "DocumentDelete")]
    [Authorize(Roles = "DocumentDelete")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var useCase = new DeleteDocumentUseCase(
            documentRepository: _documentRepository,
            documentS3Repository: _s3DocumentRepository,
            unitOfWork: _unitOfWork);
        DocumentDTOs.Inner.Delete delete = new(Id: id);
        await useCase.Execute(delete);
        return NoContent();
    }

[HttpGet("v1/documents", Name = "DocumentsRead")]
    [Authorize(Roles = "DocumentsRead")]
    public async Task<IActionResult> Read()
    {
        DocumentDTOs.Inner.Read request = new();
        var useCase = new ReadDocumentUseCase(documentRepository: _documentRepository);
        IEnumerable<DocumentDTOs.Response.Read> result = await useCase.Ask(request);
        return Ok(result);
    }

    [HttpGet("v1/documents/{id:int}", Name = "DocumentReadById")]
    [Authorize(Roles = "DocumentReadById")]
    public async Task<IActionResult> ReadById([FromRoute] int id)
    {
        DocumentDTOs.Inner.ReadById request = new(Id: id);
        var useCase = new ReadByIdDocumentUseCase(
            documentRepository: _documentRepository
        );
        DocumentDTOs.Response.Read result = await useCase.Ask(request);
        return Ok(result);
    }
    
    [HttpGet("v1/documents/{id:int}/download", Name = "DocumentDownloadById")]
    [Authorize(Roles = "DocumentDownloadById")]
    public async Task<IActionResult> DownloadById([FromRoute] int id)
    {
        DocumentDTOs.Inner.DownloadById request = new(Id: id);
        var useCase = new DownloadByIdDocumentUseCase(
            documentRepository: _documentRepository,
            s3DocumentRepository: _s3DocumentRepository
        );
        DocumentDTOs.Response.DownloadById result = await useCase.Ask(request);
        return File(result.Stream, result.ContentType, result.Name);
    }
}
