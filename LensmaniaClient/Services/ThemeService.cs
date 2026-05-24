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
        return await _http.GetFromJsonAsync<List<ThemeResponse>>("api/themes")
               ?? new List<ThemeResponse>();
    }
}
