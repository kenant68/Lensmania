using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LensmaniaTests.Events;

[TestFixture]
public class EventClosureServiceTests
{
    private AppDbContext _db = null!;
    private EventClosureService _service = null!;
    private readonly DateTime _now = new(2026, 6, 9, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new EventClosureService(_db, NullLogger<EventClosureService>.Instance);
    }

    [TearDown]
    public void Teardown() => _db.Dispose();

    private User AddUser(string username)
    {
        var user = new User { Username = username, Email = username + "@x.io" };
        _db.Users.Add(user);
        return user;
    }

    private Event AddEndedEventWithBadge(out Badge badge)
    {
        var theme = new Theme { Name = "Nature" };
        var owner = AddUser("owner-" + Guid.NewGuid().ToString("N")[..6]);
        _db.SaveChanges();
        var ev = new Event
        {
            Name = "Contest",
            Description = "d",
            StartDate = _now.AddDays(-10),
            EndDate = _now.AddDays(-1),
            Theme = theme,
            UserId = owner.Id
        };
        _db.Themes.Add(theme);
        badge = new Badge { Name = "Champion", ImageUrl = "b.svg" };
        ev.Badges.Add(badge);
        _db.Events.Add(ev);
        _db.SaveChanges();
        return ev;
    }

    private Post AddPost(int eventId, int userId, int likes, DateTime createdAt)
    {
        var post = new Post
        {
            PhotoUrl = "p.jpg",
            EventId = eventId,
            UserId = userId,
            LikesCount = likes,
            CreatedAt = createdAt
        };
        _db.Posts.Add(post);
        _db.SaveChanges();
        return post;
    }

    [Test]
    public async Task Awards_badge_to_most_liked_post_author()
    {
        var ev = AddEndedEventWithBadge(out var badge);
        var alice = AddUser("alice");
        var bob = AddUser("bob");
        AddPost(ev.Id, alice.Id, likes: 3, createdAt: _now.AddDays(-5));
        var winning = AddPost(ev.Id, bob.Id, likes: 7, createdAt: _now.AddDays(-4));

        var closed = await _service.CloseDueEventsAsync(_now);

        Assert.That(closed, Is.EqualTo(1));
        var reloaded = await _db.Events.FindAsync(ev.Id);
        Assert.That(reloaded!.ClosedAt, Is.EqualTo(_now));
        Assert.That(reloaded.WinnerPostId, Is.EqualTo(winning.Id));
        Assert.That(await _db.Earn.AnyAsync(e => e.UserId == bob.Id && e.BadgeId == badge.Id), Is.True);
    }

    [Test]
    public async Task Tie_breaks_to_oldest_post()
    {
        var ev = AddEndedEventWithBadge(out var badge);
        var alice = AddUser("alice");
        var bob = AddUser("bob");
        var older = AddPost(ev.Id, alice.Id, likes: 5, createdAt: _now.AddDays(-6));
        AddPost(ev.Id, bob.Id, likes: 5, createdAt: _now.AddDays(-4));

        await _service.CloseDueEventsAsync(_now);

        var reloaded = await _db.Events.FindAsync(ev.Id);
        Assert.That(reloaded!.WinnerPostId, Is.EqualTo(older.Id));
        Assert.That(await _db.Earn.AnyAsync(e => e.UserId == alice.Id && e.BadgeId == badge.Id), Is.True);
    }

    [Test]
    public async Task No_winner_when_no_post_has_a_like()
    {
        var ev = AddEndedEventWithBadge(out var badge);
        var alice = AddUser("alice");
        AddPost(ev.Id, alice.Id, likes: 0, createdAt: _now.AddDays(-5));

        await _service.CloseDueEventsAsync(_now);

        var reloaded = await _db.Events.FindAsync(ev.Id);
        Assert.That(reloaded!.ClosedAt, Is.EqualTo(_now));
        Assert.That(reloaded.WinnerPostId, Is.Null);
        Assert.That(await _db.Earn.AnyAsync(e => e.BadgeId == badge.Id), Is.False);
    }

    [Test]
    public async Task Does_not_touch_event_not_yet_ended()
    {
        var ev = AddEndedEventWithBadge(out _);
        ev.EndDate = _now.AddDays(1);
        _db.SaveChanges();
        var alice = AddUser("alice");
        AddPost(ev.Id, alice.Id, likes: 4, createdAt: _now.AddDays(-2));

        var closed = await _service.CloseDueEventsAsync(_now);

        Assert.That(closed, Is.EqualTo(0));
        var reloaded = await _db.Events.FindAsync(ev.Id);
        Assert.That(reloaded!.ClosedAt, Is.Null);
    }

    [Test]
    public async Task Already_closed_event_is_not_reprocessed()
    {
        var ev = AddEndedEventWithBadge(out var badge);
        ev.ClosedAt = _now.AddDays(-1);
        _db.SaveChanges();
        var alice = AddUser("alice");
        AddPost(ev.Id, alice.Id, likes: 9, createdAt: _now.AddDays(-2));

        var closed = await _service.CloseDueEventsAsync(_now);

        Assert.That(closed, Is.EqualTo(0));
        Assert.That(await _db.Earn.AnyAsync(e => e.BadgeId == badge.Id), Is.False);
    }
}
