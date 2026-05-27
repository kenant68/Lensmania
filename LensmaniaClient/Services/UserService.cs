using LensmaniaLibrary.DTOs.Users;
using System.Net.Http.Json;
using System.Net;

namespace LensmaniaClient.Services;

public class UserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http)
    {
        _http = http;
    }

	public async Task<PublicUserProfileResponse?> GetByUsernameAsync(string username)
    {
        var url = $"api/user/{username}";

        return await _http.GetFromJsonAsync<PublicUserProfileResponse?>(url);
    }

    public async Task<PaginatedUsers> GetAllAsync(int offset = 0, int limit = 10)
    {
        var url = $"api/user?offset={offset}&limit={limit}";

        return await _http.GetFromJsonAsync<PaginatedUsers>(url)
			?? new PaginatedUsers();
    }
    
    public async Task ToggleUserIsActiveAsync(int userId, bool isActive)
	{        
 		var response = await _http.PatchAsJsonAsync($"api/user/{userId}/active", isActive);
		response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(int userId)
	{
    	var response = await _http.DeleteAsync($"api/user/{userId}");
		response.EnsureSuccessStatusCode();
	}

    public async Task DeleteMeAsync()
	{
    	var response = await _http.DeleteAsync($"api/user/me");
		response.EnsureSuccessStatusCode();
	}

    public async Task<UserResponse?> UpdateMeAsync(UpdateUserRequest request)
    {
        var response = await _http.PutAsJsonAsync("api/user/me", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserResponse>();
    }
}
