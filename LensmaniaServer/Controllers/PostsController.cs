using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IFileStorageService _fileStorageService;

    public PostsController(IPostService postService, IFileStorageService fileStorageService)
    {
        _postService = postService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedPosts>> GetAll(
        [FromQuery] int? cursor = null,
        [FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 50)
            return BadRequest("`limit` must be between 1 and 50.");
        
        if (cursor.HasValue && cursor.Value <= 0)
            return BadRequest("`cursor` must be a positive integer.");
        
        var result = await _postService.GetAllAsync(cursor, limit);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetById(int id)
    {       
        var result = await _postService.GetByIdAsync(id);
        return Ok(result);
    }
}
