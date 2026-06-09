using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LensmaniaServer.Services;

public class EventClosureService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EventClosureService> _logger;

    public EventClosureService(AppDbContext db, ILogger<EventClosureService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> CloseDueEventsAsync(DateTime now)
    {
        var dueEvents = await _db.Events
            .Include(e => e.Badges)
            .Where(e => e.EndDate <= now && e.ClosedAt == null)
            .ToListAsync();

        foreach (var ev in dueEvents)
        {
            var badge = ev.Badges.FirstOrDefault();
            if (badge is null)
            {
                _logger.LogWarning("Event {EventId} has no badge; closing without award.", ev.Id);
            }
            else
            {
                var winner = await _db.Posts
                    .Where(p => p.EventId == ev.Id && p.LikesCount >= 1)
                    .OrderByDescending(p => p.LikesCount)
                    .ThenBy(p => p.CreatedAt)
                    .ThenBy(p => p.Id)
                    .FirstOrDefaultAsync();

                if (winner is not null)
                {
                    ev.WinnerPostId = winner.Id;

                    var alreadyEarned = await _db.Earn
                        .AnyAsync(e => e.UserId == winner.UserId && e.BadgeId == badge.Id);
                    if (!alreadyEarned)
                    {
                        _db.Earn.Add(new Earn
                        {
                            UserId = winner.UserId,
                            BadgeId = badge.Id,
                            AwardedAt = now
                        });
                    }
                }
            }

            ev.ClosedAt = now;
        }

        if (dueEvents.Count > 0)
            await _db.SaveChangesAsync();

        return dueEvents.Count;
    }
}
