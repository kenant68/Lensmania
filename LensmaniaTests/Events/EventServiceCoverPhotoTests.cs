using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Events;

[TestFixture]
public class EventServiceCoverPhotoTests
{
    private AppDbContext _db = null!;
    private EventService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new EventService(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task<(User user, Theme theme, Event ev, Post post)> SeedAsync()
    {
        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        var theme = new Theme { Name = "Nature", Icon = "🌿" };
        _db.Users.Add(user);
        _db.Themes.Add(theme);
        await _db.SaveChangesAsync();

        var ev = new Event
        {
            Name = "Foodies", Description = "",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ThemeId = _db.Themes.First().Id,
            UserId = _db.Users.First().Id
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var post = new Post
        {
            PhotoUrl = "cover.jpg",
            UserId = _db.Users.First().Id,
            EventId = _db.Events.First().Id
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return (_db.Users.First(), _db.Themes.First(), _db.Events.First(), _db.Posts.First());
    }

    [Test]
    public async Task SetCoverPhotoAsync_ValidPost_SetsCoverPhotoPostId()
    {
        var (_, _, ev, post) = await SeedAsync();

        var result = await _service.SetCoverPhotoAsync(ev.Id, post.Id);

        var updatedEvent = await _db.Events.FindAsync(ev.Id);
        Assert.That(updatedEvent!.CoverPhotoPostId, Is.EqualTo(post.Id));
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SetCoverPhotoAsync_PostNotInEvent_ReturnsNull()
    {
        var (user, theme, ev, _) = await SeedAsync();
        var otherPost = new Post { PhotoUrl = "other.jpg", UserId = user.Id, EventId = null };
        _db.Posts.Add(otherPost);
        await _db.SaveChangesAsync();

        var result = await _service.SetCoverPhotoAsync(ev.Id, otherPost.Id);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetCoverPhotoAsync_EventNotFound_ReturnsNull()
    {
        var result = await _service.SetCoverPhotoAsync(9999, 1);

        Assert.That(result, Is.Null);
    }
}
