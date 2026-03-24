using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase {

    [HttpGet("me")]
    public IActionResult Me() {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var isAdmin = User.FindFirstValue("isAdmin");
        var isPremium = User.FindFirstValue("isPremium");
        return Ok(new { email, isAdmin, isPremium });
    }
}