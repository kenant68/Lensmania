using LensmaniaLibrary.DTOs.Events;
using LensmaniaLibrary.DTOs.Badges;
using LensmaniaLibrary.DTOs.Themes;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _db;

    public EventService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedEvents> GetAllAsync(int offset, int limit)
    {
        var total = await _db.Events.CountAsync();

        var events = await _db.Events
            .Include(e => e.Theme)
            .Include(e => e.Badges)
            .OrderByDescending(e => e.StartDate)
            .Skip(offset)
            .Take(limit)
            .Select(e => new EventResponse(
                e.Id,
                e.Name,
                e.StartDate,
                e.EndDate,
                e.IsPremium,
                e.Theme.Name,
                e.Theme.Icon,
                e.Badges.Count
            ))
            .ToListAsync();

        return new PaginatedEvents
        {
            Events = events,
            Total  = total,
            Offset = offset,
            Limit  = limit
        };
    }

    public async Task<EventDetailedResponse?> GetByIdAsync(int id)
    {
        return await _db.Events
            .Include(e => e.Theme)
            .Include(e => e.Badges)
            .Where(e => e.Id == id)
            .Select(e => new EventDetailedResponse(
                e.Id,
                e.Name,
                e.Description,
                e.StartDate,
                e.EndDate,
                e.IsPremium,
                new ThemeResponse(e.Theme.Id, e.Theme.Name, e.Theme.Icon),
                e.Badges.Select(b => new BadgeResponse(b.Id, b.Name, b.ImageUrl)).ToList()
            ))
            .FirstOrDefaultAsync();
    }
    
    public async Task<EventDetailedResponse> CreateAsync(CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Le nom de l'événement est obligatoire.");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("La date de fin doit être postérieure à la date de début.");

        var themeExists = await _db.Themes.AnyAsync(t => t.Id == request.ThemeId);
        if (!themeExists)
            throw new ArgumentException($"Le thème #{request.ThemeId} n'existe pas.");

        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
            throw new ArgumentException($"L'utilisateur #{request.UserId} n'existe pas.");
        
        var ev = new Event
        {
            Name        = request.Name.Trim(),
            Description = request.Description.Trim(),
            StartDate   = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate     = DateTime.SpecifyKind(request.EndDate,   DateTimeKind.Utc),
            IsPremium   = request.IsPremium,
            ThemeId     = request.ThemeId,
            UserId      = request.UserId,
        };

        // Associated badges
        foreach (var b in request.Badges)
        {
            if (string.IsNullOrWhiteSpace(b.Name))
                throw new ArgumentException("Chaque badge doit avoir un nom.");

            ev.Badges.Add(new Badge
            {
                Name     = b.Name.Trim(),
                ImageUrl = b.ImageUrl.Trim()
            });
        }

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        await _db.Entry(ev).Reference(e => e.Theme).LoadAsync();

        return new EventDetailedResponse(
            ev.Id,
            ev.Name,
            ev.Description,
            ev.StartDate,
            ev.EndDate,
            ev.IsPremium,
            new ThemeResponse(ev.Theme.Id, ev.Theme.Name, ev.Theme.Icon),
            ev.Badges.Select(b => new BadgeResponse(b.Id, b.Name, b.ImageUrl)).ToList()
        );
    }
    
    public async Task<EventDetailedResponse?> UpdateAsync(int id, UpdateEventRequest request)
    {
        var ev = await _db.Events
            .Include(e => e.Badges)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Le nom de l'événement est obligatoire.");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("La date de fin doit être postérieure à la date de début.");

        var themeExists = await _db.Themes.AnyAsync(t => t.Id == request.ThemeId);
        if (!themeExists)
            throw new ArgumentException($"Le thème #{request.ThemeId} n'existe pas.");

        ev.Name        = request.Name.Trim();
        ev.Description = request.Description.Trim();
        ev.StartDate   = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        ev.EndDate     = DateTime.SpecifyKind(request.EndDate,   DateTimeKind.Utc);
        ev.IsPremium   = request.IsPremium;
        ev.ThemeId     = request.ThemeId;

        _db.Badges.RemoveRange(ev.Badges);
        ev.Badges.Clear();

        foreach (var b in request.Badges)
        {
            if (string.IsNullOrWhiteSpace(b.Name))
                throw new ArgumentException("Chaque badge doit avoir un nom.");

            ev.Badges.Add(new Badge
            {
                Name     = b.Name.Trim(),
                ImageUrl = b.ImageUrl.Trim()
            });
        }

        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Theme).LoadAsync();

        return new EventDetailedResponse(
            ev.Id,
            ev.Name,
            ev.Description,
            ev.StartDate,
            ev.EndDate,
            ev.IsPremium,
            new ThemeResponse(ev.Theme.Id, ev.Theme.Name, ev.Theme.Icon),
            ev.Badges.Select(b => new BadgeResponse(b.Id, b.Name, b.ImageUrl)).ToList()
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return false;

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return true;
    }
}
