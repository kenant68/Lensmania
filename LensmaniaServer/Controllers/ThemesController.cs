using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.DTOs.Themes;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThemesController : ControllerBase
{
    private readonly IThemeService _themeService;

    public ThemesController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedThemes>> GetAll(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var result = await _themeService.GetAllAsync(offset, limit);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ThemeResponse>> GetById(int id)
    {
        var theme = await _themeService.GetByIdAsync(id);

        if (theme is null)
            return NotFound();

        return Ok(theme);
    }
}
