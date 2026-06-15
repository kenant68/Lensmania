using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public UploadController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("badge")]
    public async Task<IActionResult> UploadBadge([FromForm] IFormFile badge)
    {
        if (badge is null) return BadRequest(new { message = "Aucun fichier fourni." });

        try
        {
            var url = await _fileStorageService.UploadBadgeAsync(badge);
            return Ok(url);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
    {
        if (photo is null) return BadRequest(new { message = "Aucun fichier fourni." });

        try
        {
            var photoUrl = await _fileStorageService.UploadPhotoAsync(photo);
            return Ok(photoUrl);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
