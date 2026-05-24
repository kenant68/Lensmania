using LensmaniaLibrary.DTOs.Themes;

namespace LensmaniaServer.Services;

public interface IThemeService
{
    Task<PaginatedThemes> GetAllAsync(int offset, int limit);
    Task<ThemeResponse?> GetByIdAsync(int id);
}
