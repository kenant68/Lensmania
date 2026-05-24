using System.Net.Http.Json;
using LensmaniaLibrary.DTOs.Themes;
 
namespace LensmaniaClient.Services;

public class ThemeService
{
    private readonly HttpClient _http;
 
    public ThemeService(HttpClient http)
    {
        _http = http;
    }
 
    public async Task<List<ThemeResponse>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<PaginatedThemes>("api/themes?limit=100");
        return result?.Themes ?? new List<ThemeResponse>();
    }
}
