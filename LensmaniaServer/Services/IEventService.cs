using LensmaniaLibrary.DTOs.Events;

namespace LensmaniaServer.Services;

public interface IEventService
{
    Task<PaginatedEvents> GetAllAsync(int offset, int limit);
    Task<EventDetailledResponse?> GetByIdAsync(int id);
    Task<EventDetailledResponse> CreateAsync(CreateEventRequest request);
    Task<EventDetailledResponse?> UpdateAsync(int id, UpdateEventRequest request);
    Task<bool> DeleteAsync(int id);
}
