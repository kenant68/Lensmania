using LensmaniaLibrary.DTOs.Posts;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace LensmaniaClient.Services.Posts;

public class PostService
{
    private readonly HttpClient _http;

    public PostService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PostDto?> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/posts", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PostDto>();
    }
    
    public async Task<string> UploadImageAsync(IBrowserFile file)
    {
        var content = new MultipartFormDataContent();

        var stream = file.OpenReadStream(maxAllowedSize: 5_000_000);
        content.Add(new StreamContent(stream), "photo", file.Name);

        var response = await _http.PostAsync("api/uploads/photo", content);

        return await response.Content.ReadAsStringAsync();
    }
}
