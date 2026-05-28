using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaLibrary.Enums;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserService _userService;

    public PostsController(IPostService postService, IFileStorageService fileStorageService, IUserService userService)
    {
        _postService = postService;
        _fileStorageService = fileStorageService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedPosts>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] int? cursor = null,
        [FromQuery] int limit = 10,
        [FromQuery] string sort = "date_desc",
        [FromQuery] int? offset = null,
        [FromQuery] int? eventId = null)
    {
        if (userId.HasValue && userId.Value <= 0)
            return BadRequest("`userId` must be a positive integer.");

        if (limit < 1 || limit > 50)
            return BadRequest("`limit` must be between 1 and 50.");

        if (cursor.HasValue && cursor.Value <= 0)
            return BadRequest("`cursor` must be a positive integer.");

        if (offset.HasValue && offset.Value < 0)
            return BadRequest("`offset` must be a non-negative integer.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? currentUserId = int.TryParse(userIdClaim, out var id) ? id : null;

        var sortOrder = sort switch
        {
            "date_asc"   => PostSortOrder.DateAsc,
            "likes_desc" => PostSortOrder.LikesDesc,
            "likes_asc"  => PostSortOrder.LikesAsc,
            _            => PostSortOrder.DateDesc,
        };

        var result = await _postService.GetAllAsync(userId, cursor, limit, currentUserId, sortOrder, offset, eventId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponse>> GetById(int id)
    {
        var result = await _postService.GetByIdAsync(id);

		if (result is null)
			return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:int}/likes")]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var found = await _postService.ToggleLikeAsync(id, userId);
        if (!found) return NotFound();

        return NoContent();
    }

	[Authorize]
    [HttpPost]
    public async Task<ActionResult<PostResponse>> CreatePost([FromBody] CreatePostRequest request)
    {
        try
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
			if (!int.TryParse(userIdClaim, out var userId))
            	return Unauthorized();
        
			var post = await _postService.CreatePostAsync(request, userId);
        	return Ok(post);
		}
		// Todo: add ValidationException + middleware
    	catch (ArgumentException ex)
    	{
        	return BadRequest(new { message = ex.Message });
    	}
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<PaginatedPosts>> GetPostsByUsername(
        string username,
        [FromQuery] int? cursor,
        [FromQuery] int limit = 10,
        [FromQuery] string sort = "date_desc",
        [FromQuery] int? offset = null)
    {
        if (limit < 1 || limit > 50)
            return BadRequest("`limit` must be between 1 and 50.");

        if (cursor.HasValue && cursor.Value <= 0)
            return BadRequest("`cursor` must be a positive integer.");

        if (offset.HasValue && offset.Value < 0)
            return BadRequest("`offset` must be a non-negative integer.");

        var user = await _userService.GetByUsernameAsync(username);

        if (user == null)
            return NotFound();

        var sortOrder = sort switch
        {
            "date_asc"   => PostSortOrder.DateAsc,
            "likes_desc" => PostSortOrder.LikesDesc,
            "likes_asc"  => PostSortOrder.LikesAsc,
            _            => PostSortOrder.DateDesc,
        };

        var posts = await _postService.GetAllAsync(user.Id, cursor, limit, sortOrder: sortOrder, offset: offset);

        return Ok(posts);
    }
    
	[Authorize]
	[HttpDelete("{id:int}")]
	public async Task<IActionResult> DeletePost(int id)
	{
		var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
		if (!int.TryParse(userIdClaim, out var userId))
			return Unauthorized();
		
		try
		{
			var postToDelete = await _postService.DeletePostAsync(id, userId);

			if (!postToDelete)
				return NotFound();
			
			return NoContent();
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
	}
}
