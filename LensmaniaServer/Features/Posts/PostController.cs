using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.Models;

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
    public async Task<ActionResult<List<Post>>> GetAll()
    {
        var posts = await _postService.GetAllAsync();
        return Ok(posts);
    }
}
