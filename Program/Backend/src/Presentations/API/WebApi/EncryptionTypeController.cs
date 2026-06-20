using Application.DTOs;
using Application.UseCases.EncryptionTypeUseCases;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.WebApi;

[ApiController]
[Route("")]
[Authorize]
public class EncryptionTypeController(IEncryptionTypeRepository encryptionTypeRepository) : ControllerBase
{
    private readonly IEncryptionTypeRepository _encryptionTypeRepository = encryptionTypeRepository;

    [HttpGet("v1/documents/encryption/types", Name = "EncryptionTypeRead")]
    [Authorize(Roles = "EncryptionTypeRead")]
    public async Task<IActionResult> Read()
    {
        EncryptionTypeDTOs.Inner.ReadAll request = new();
        var useCase = new ReadEncryptionTypeUseCase(_encryptionTypeRepository);
        List<EncryptionTypeDTOs.Response.Read> result = await useCase.Ask(request);
        return Ok(result);
    }
    [HttpGet("v1/documents/encryption/types/{id:int}", Name = "EncryptionTypeReadById")]
    [Authorize(Roles = "EncryptionTypeReadById")]
    public async Task<IActionResult> ReadById([FromRoute] int id)
    {
        EncryptionTypeDTOs.Inner.ReadById request = new(id);
        var useCase = new ReadByIdEncryptionTypeUseCase(_encryptionTypeRepository);
        EncryptionTypeDTOs.Response.Read result = await useCase.Ask(request);
        return Ok(result);
    }
}
