using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.FragmentUseCases;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.WebApi;

[ApiController]
[Route("")]
[Authorize]
#pragma warning disable CA1812 // Избегайте внутренних классов, не имеющих экземпляры
public sealed class FragmentController(IUnitOfWork unitOfWork, IFragmentRepository fragmentRepository) : ControllerBase
{
    private readonly IFragmentRepository _fragmentRepository = fragmentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [HttpPost("v1/documents/fragment", Name = "FragmentCreate")]
    [Authorize(Roles = "FragmentCreate")]
    public async Task<IActionResult> Create([FromBody] FragmentDTOs.Inner.Create create)
    {
        CreateFragmentUseCase useCase = new(fragmentRepository: _fragmentRepository, unitOfWork: _unitOfWork);
        var result = await useCase.Execute(create).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpDelete("v1/documents/fragment/{id:int}", Name = "FragmentDelete")]
    [Authorize(Roles = "FragmentDelete")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        DeleteFragmentUseCase useCase = new(fragmentRepository: _fragmentRepository, unitOfWork: _unitOfWork);
        FragmentDTOs.Inner.Delete delete = new(Id: id);
        await useCase.Execute(delete).ConfigureAwait(false);
        return Ok($"Фрагмент {id} документа удален.");
    }
    [HttpGet("v1/documents/fragment/{id:int}", Name = "FragmentReadById")]
    [Authorize(Roles = "FragmentReadById")]
    public async Task<IActionResult> ReadById([FromRoute] int id)
    {
        ReadByIdFragmentUseCase useCase = new(fragmentRepository: _fragmentRepository);
        FragmentDTOs.Inner.ReadById readById = new(Id: id);
        FragmentDTOs.Response.Read read = await useCase.Ask(readById).ConfigureAwait(false);
        return Ok(read);
    }
    [HttpGet("v1/documents/{id:int}/fragment", Name = "FragmentReadByDocumentId")]
    [Authorize(Roles = "FragmentReadByDocumentId")]
    public async Task<IActionResult> ReadByDocumentId([FromRoute] int id)
    {
        ReadByDocumentIdFragmentUseCase useCase = new(fragmentRepository: _fragmentRepository);
        FragmentDTOs.Inner.ReadByDocumentId readById = new(DocumentId: id);
        List<FragmentDTOs.Response.Read> read = await useCase.Ask(readById).ConfigureAwait(false);
        return Ok(read);
    }
}
