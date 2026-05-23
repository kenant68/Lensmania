using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.DTOs.Users;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase {
    
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public IActionResult Me() {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var isAdmin = User.FindFirstValue("isAdmin");
        var isPremium = User.FindFirstValue("isPremium");
        return Ok(new { email, isAdmin, isPremium });
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<PublicUserProfileResponse>> GetByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);
        if (user is null) 
            return NotFound();
        
        return Ok(user);
    }
    
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var updated = await _userService.UpdateAsync(userId, request);
            if (updated is null) 
                return NotFound();
            
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var deleted = await _userService.DeleteMeAsync(userId);
        if (!deleted) 
            return NotFound();

        return NoContent();
    }
    
    // Admin endpoints
    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<ActionResult<PaginatedUsers>> GetAll(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 20)
    {
        if (offset < 0)
            return BadRequest("`offset` must be >= 0.");
        if (limit < 1 || limit > 100)
            return BadRequest("`limit` must be between 1 and 100.");

        var result = await _userService.GetAllAsync(offset, limit);
        
        return Ok(result);
    }
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> ToggleIsActive(int id, [FromBody] bool isActive)
    {
        var found = await _userService.ToggleIsActiveAsync(id, isActive);

        if (!found)
            return NotFound();

        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> AdminDeleteUser(int id)
    {
		try
		{
        	var deleted = await _userService.DeleteByAdminAsync(id);
        	if (!deleted) 
            	return NotFound();
        
        	return NoContent();
		}
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = e.Message });
        }
    }
}
