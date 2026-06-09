using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Users;

[TestFixture]
public class UserServiceBadgesTests
{
    private AppDbContext _db = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new UserService(_db);
    }

    [TearDown]
    public void Teardown() => _db.Dispose();

    [Test]
    public async Task GetByUsername_returns_earned_badges_with_event_and_winning_photo()
    {
        var theme = new Theme { Name = "Nature" };
        var winner = new User { Username = "alice", Email = "alice@x.io" };
        _db.Themes.Add(theme);
        _db.Users.Add(winner);
        _db.SaveChanges();

        var ev = new Event
        {
            Name = "Spring Contest",
            Description = "d",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            ThemeId = theme.Id,
            UserId = winner.Id
        };
        var badge = new Badge { Name = "Champion", ImageUrl = "champ.svg" };
        ev.Badges.Add(badge);
        _db.Events.Add(ev);
        _db.SaveChanges();

        var winningPost = new Post
        {
            PhotoUrl = "winning.jpg",
            EventId = ev.Id,
            UserId = winner.Id,
            LikesCount = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };
        _db.Posts.Add(winningPost);
        _db.SaveChanges();

        ev.WinnerPostId = winningPost.Id;
        _db.Earn.Add(new Earn { UserId = winner.Id, BadgeId = badge.Id, AwardedAt = DateTime.UtcNow });
        _db.SaveChanges();

        var profile = await _service.GetByUsernameAsync("alice");

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.Badges, Has.Count.EqualTo(1));
        var b = profile.Badges[0];
        Assert.That(b.Name, Is.EqualTo("Champion"));
        Assert.That(b.EventName, Is.EqualTo("Spring Contest"));
        Assert.That(b.WinningPhotoUrl, Is.EqualTo("winning.jpg"));
    }

    [Test]
    public async Task GetByUsername_returns_empty_badges_when_none_earned()
    {
        _db.Users.Add(new User { Username = "bob", Email = "bob@x.io" });
        _db.SaveChanges();

        var profile = await _service.GetByUsernameAsync("bob");

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.Badges, Is.Empty);
    }

    [Test]
    public async Task GetByUsername_orders_badges_by_awarded_date_descending()
    {
        var theme = new Theme { Name = "Nature" };
        var user = new User { Username = "carol", Email = "carol@x.io" };
        _db.Themes.Add(theme);
        _db.Users.Add(user);
        _db.SaveChanges();

        var oldEvent = new Event
        {
            Name = "Old Contest",
            Description = "d",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-20),
            ThemeId = theme.Id,
            UserId = user.Id
        };
        var oldBadge = new Badge { Name = "Old", ImageUrl = "old.svg" };
        oldEvent.Badges.Add(oldBadge);

        var recentEvent = new Event
        {
            Name = "Recent Contest",
            Description = "d",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            ThemeId = theme.Id,
            UserId = user.Id
        };
        var recentBadge = new Badge { Name = "Recent", ImageUrl = "recent.svg" };
        recentEvent.Badges.Add(recentBadge);

        _db.Events.AddRange(oldEvent, recentEvent);
        _db.SaveChanges();

        _db.Earn.Add(new Earn { UserId = user.Id, BadgeId = oldBadge.Id, AwardedAt = DateTime.UtcNow.AddDays(-20) });
        _db.Earn.Add(new Earn { UserId = user.Id, BadgeId = recentBadge.Id, AwardedAt = DateTime.UtcNow.AddDays(-1) });
        _db.SaveChanges();

        var profile = await _service.GetByUsernameAsync("carol");

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.Badges, Has.Count.EqualTo(2));
        Assert.That(profile.Badges[0].Name, Is.EqualTo("Recent"));
        Assert.That(profile.Badges[1].Name, Is.EqualTo("Old"));
    }
}
