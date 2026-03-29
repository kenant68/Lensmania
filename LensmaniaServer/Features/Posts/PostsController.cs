using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Features.Posts;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
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
}
