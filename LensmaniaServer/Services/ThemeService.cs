using LensmaniaLibrary.DTOs.Themes;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Services;

public class ThemeService : IThemeService
{
    private readonly AppDbContext _db;

    public ThemeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedThemes> GetAllAsync(int offset, int limit)
    {
        var total = await _db.Themes.CountAsync();

        var themes = await _db.Themes
            .OrderByDescending(t => t.Id)
            .Skip(offset)
            .Take(limit)
            .Select(t => new ThemeResponse(
                t.Id,
                t.Name,
                t.Icon
            ))
            .ToListAsync();

        return new PaginatedThemes
        {
            Themes = themes,
            Total = total,
            Offset = offset,
            Limit = limit
        };
    }

    public async Task<ThemeResponse?> GetByIdAsync(int id)
    {
        return await _db.Themes
            .Where(t => t.Id == id)
            .Select(t => new ThemeResponse(
                t.Id,
                t.Name,
                t.Icon
            ))
            .FirstOrDefaultAsync();
    }
}
