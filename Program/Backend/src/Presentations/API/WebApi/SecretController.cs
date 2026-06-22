using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.SecretUseCases;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.WebApi;

[ApiController]
[Route("")]
[Authorize]
#pragma warning disable CA1812 // Избегайте внутренних классов, не имеющих экземпляры
public sealed class SecretController(
    IUnitOfWork unitOfWork,
    ISecretRepository secretRepository,
    ISecretManager secretManager) : ControllerBase
{
    private readonly ISecretRepository _secretRepository = secretRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISecretManager _secretManager = secretManager;

    [HttpPost("v1/documents/encryption/secrets", Name = "SecretCreate")]
    [Authorize(Roles = "SecretCreate")]
    public async Task<IActionResult> Create([FromBody] SecretDTOs.Request.Create input)
    {
        SecretDTOs.Inner.Create create = new(NumberOfBytes: input.NumberOfBytes);
        CreateSecretUseCase useCase = new(
            secretRepository: _secretRepository,
            unitOfWork: _unitOfWork,
            secretManager: _secretManager);
        var result = await useCase.Execute(create).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpDelete("v1/documents/encryption/secrets/{id:int}", Name = "SecretDelete")]
    [Authorize(Roles = "SecretDelete")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        DeleteSecretUseCase useCase = new(secretRepository: _secretRepository, unitOfWork: _unitOfWork);
        SecretDTOs.Inner.Delete delete = new(Id: id);
        await useCase.Execute(delete).ConfigureAwait(false);
        return Ok($"Секрет {id} удален.");
    }
    [HttpGet("v1/documents/encryption/secrets/{id:int}", Name = "SecretReadById")]
    [Authorize(Roles = "SecretReadById")]
    public async Task<IActionResult> ReadById([FromRoute] int id)
    {
        ReadByIdSecretUseCase useCase = new(secretRepository: _secretRepository);
        SecretDTOs.Inner.ReadById readById = new(Id: id);
        SecretDTOs.Response.Read read = await useCase.Ask(readById).ConfigureAwait(false);
        return Ok(read);
    }
}
