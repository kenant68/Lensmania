using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public UploadController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
    {
        var photoUrl = await _fileStorageService.UploadPhotoAsync(photo);
        return Ok(photoUrl);
    }
}
