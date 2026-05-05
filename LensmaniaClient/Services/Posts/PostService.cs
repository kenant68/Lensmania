using LensmaniaLibrary.DTOs.Posts;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Net;

namespace LensmaniaClient.Services.Posts;

public class PostService
{
    private readonly HttpClient _http;

    public PostService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PostListItemResponse?> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/posts", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PostListItemResponse>();
    }
    
    public async Task<string?> UploadImageAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(maxAllowedSize: 5_000_000);

        content.Add(new StreamContent(stream), "photo", file.Name);

        var response = await _http.PostAsync("api/uploads/photo", content);

		if (!response.IsSuccessStatusCode)
			return null;

        return await response.Content.ReadAsStringAsync();
    }

	public async Task<PaginatedPosts?> GetPostsByUsernameAsync(
        string username, int? cursor, int limit)
	{
        var url = $"api/posts/{Uri.EscapeDataString(username)}?limit={limit}";

        if (cursor.HasValue)
            url += $"&cursor={cursor.Value}";

        return await _http.GetFromJsonAsync<PaginatedPosts?>(url);
	}
    
    public async Task<DeletePostResult> DeletePostAsync(int postId)
    {
        var response = await _http.DeleteAsync($"api/posts/{postId}");
        
        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => DeletePostResult.Success,
            HttpStatusCode.NotFound => DeletePostResult.NotFound,
            HttpStatusCode.Forbidden => DeletePostResult.Forbidden,
			      HttpStatusCode.Unauthorized => DeletePostResult.Unauthorized,
            _ => DeletePostResult.Error
        };
    }
}
