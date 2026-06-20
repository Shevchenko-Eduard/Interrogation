// using Application.DTOs;
// using Application.Interfaces;
// using Application.UseCases.DocumentUseCases;
// using Domain.Interfaces;
// using Domain.Interfaces.Repositories;
// using Domain.Interfaces.Repositories.BaseRepository;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace EmployeeWeb.WebApi;

// [ApiController]
// [Route("")]
// [Authorize]
// public class DocumentController(
//     IDocumentRepository documentRepository,
//     IDocumentS3Repository documentS3Repository,
//     IUnitOfWork unitOfWork,
//     IDocumentKeyManager documentKeyManager,
//     IClock clock,
//     ICurrentUser currentUser) : ControllerBase
// {
//     private readonly IDocumentRepository _documentRepository = documentRepository;
//     private readonly IDocumentS3Repository _documentS3Repository = documentS3Repository;
//     private readonly IClock _clock = clock;
//     private readonly IDocumentKeyManager _documentKeyManager = documentKeyManager;
//     private readonly IUnitOfWork _unitOfWork = unitOfWork;
//     private readonly ICurrentUser _currentUser = currentUser;

//     [HttpPost("v1/documents", Name = "DocumentCreate")]
//     [Authorize(Roles = "DocumentCreate")]
//     public async Task<IActionResult> Create(IFormFile file, [FromForm] DocumentDTOs.Request.Create request)
//     {
//         if (file == null || file.Length == 0)
//         {
//             return BadRequest("Файл не выбран или пустой.");
//         }

//         var useCase = new CreateDocumentUseCase(
//             _documentRepository,
//             _documentS3Repository,
//             _unitOfWork,
//             _clock,
//             _documentKeyManager);

//         DocumentDTOs.Inner.Create innerRequest = new(
//             EncryptionTypeId: request.EncryptionTypeId,
//             SecretId: request.SecretId,
//             CreatorId: _currentUser.Id
//                 ?? throw new Exception("Пользователь не зарегистрирован или у него отсутствует Id."),
//             Name: request.Name,
//             Description: request.Description,
//             ContentType: file.ContentType,
//             Extension: Path.GetExtension(file.FileName),
//             Stream: file.OpenReadStream());

//         var result = await useCase.Execute(innerRequest);
//         return Ok(result);
//     }

//     [HttpPut(Name = "HotelImageUpdate")]
//     [Authorize(Roles = "HotelImage-Update")]
//     public async Task<IActionResult> Update(IFormFile file, [FromForm] HotelImageDTOs.Request.Update request)
//     {
//         if (file == null || file.Length == 0)
//         {
//             return BadRequest("Файл не выбран или пустой.");
//         }

//         var useCase = new UpdateImageUseCase(
//             _hotelImageRepository,
//             _s3HotelImageRepository,
//             _unitOfWork);

//         HotelImageDTOs.Inner.Update innerRequest = new(
//             Id: request.Id,
//             Extension: Path.GetExtension(file.FileName),
//             ContentType: file.ContentType,
//             Stream: file.OpenReadStream()
//         );
//         await useCase.Execute(innerRequest);
//         return NoContent();
//     }

//     [HttpDelete(Name = "HotelImageDelete")]
//     [Authorize(Roles = "HotelImage-Delete")]
//     public async Task<IActionResult> Delete(HotelImageDTOs.Request.Delete request)
//     {
//         var useCase = new DeleteImageUseCase(
//             _hotelImageRepository,
//             _unitOfWork,
//             _s3HotelImageRepository);

//         await useCase.Execute(request);
//         return NoContent();
//     }

//     [HttpGet("{id}", Name = "HotelImageRead")]
//     public async Task<IActionResult> Read(int id)
//     {
//         HotelImageDTOs.Request.Read request = new(Id: id);
//         var useCase = new ReadImageUseCase(
//             _hotelImageRepository,
//             _s3HotelImageRepository);

//         var image = await useCase.Ask(request);
//         return File(image.Stream, image.ContentType, image.FileName);
//     }
// }
