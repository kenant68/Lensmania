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
        var result = await _postService.GetAllAsync(cursor, limit);
        return Ok(result);
    }
}
