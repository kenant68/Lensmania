using Microsoft.AspNetCore.Components.Forms;

namespace LensmaniaClient.Services;

public class UploadService
{
    private readonly HttpClient _http;

    public UploadService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> UploadBadgeAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(maxAllowedSize: 2_000_000);

        content.Add(new StreamContent(stream), "badge", file.Name);

        var response = await _http.PostAsync("api/uploads/badge", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
        }

        return await response.Content.ReadAsStringAsync();
    }
}
