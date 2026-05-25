using LensmaniaLibrary.DTOs.Events;

namespace LensmaniaServer.Services;

public interface IEventService
{
    Task<PaginatedEvents> GetAllAsync(int offset, int limit);
    Task<EventDetailedResponse?> GetByIdAsync(int id);
    Task<EventDetailedResponse> CreateAsync(CreateEventRequest request);
    Task<EventDetailedResponse?> UpdateAsync(int id, UpdateEventRequest request);
    Task<bool> DeleteAsync(int id);
}
