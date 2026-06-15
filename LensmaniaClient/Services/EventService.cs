using System.Net.Http.Json;
using LensmaniaClient.Models.Auth;
using LensmaniaLibrary.DTOs.Events;

namespace LensmaniaClient.Services;

public class EventService
{
    private readonly HttpClient _http;

    public EventService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedEvents?> GetAllAsync(int offset = 0, int limit = 10)
    {
        return await _http.GetFromJsonAsync<PaginatedEvents>(
            $"api/events?offset={offset}&limit={limit}");
    }

    public async Task<EventDetailedResponse?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<EventDetailedResponse>($"api/events/{id}");
    }

    public async Task<EventDetailedResponse> CreateAsync(CreateEventRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/events", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            throw new Exception(error?.Message ?? "Une erreur est survenue.");
        }

        return (await response.Content.ReadFromJsonAsync<EventDetailedResponse>())!;
    }

    public async Task<EventDetailedResponse> UpdateAsync(int id, UpdateEventRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/events/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            throw new Exception(error?.Message ?? "Une erreur est survenue.");
        }

        return (await response.Content.ReadFromJsonAsync<EventDetailedResponse>())!;
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/events/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<PaginatedEvents?> GetActiveAsync()
    {
        return await _http.GetFromJsonAsync<PaginatedEvents>("api/events?status=active&limit=100");
    }

    public async Task<PaginatedEvents?> GetPastAsync(int limit = 3)
    {
        return await _http.GetFromJsonAsync<PaginatedEvents>($"api/events?status=past&limit={limit}");
    }

    public async Task<EventDetailedResponse> SetCoverPhotoAsync(int eventId, int postId)
    {
        var response = await _http.PutAsJsonAsync($"api/events/{eventId}/cover", new SetCoverPhotoRequest(postId));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            throw new Exception(error?.Message ?? "Une erreur est survenue.");
        }

        return (await response.Content.ReadFromJsonAsync<EventDetailedResponse>())!;
    }
}
