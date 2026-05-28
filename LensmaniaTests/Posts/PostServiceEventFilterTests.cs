using LensmaniaLibrary.Enums;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Posts;

[TestFixture]
public class PostServiceEventFilterTests
{
    private AppDbContext _db = null!;
    private PostService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new PostService(_db, null!);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task<(User user, Event ev)> SeedAsync()
    {
        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        _db.Users.Add(user);
        var ev = new Event
        {
            Name = "Foodies", Description = "",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ThemeId = 1, UserId = 1
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        user = _db.Users.First();
        ev = _db.Events.First();
        return (user, ev);
    }

    [Test]
    public async Task GetAllAsync_WithEventId_ReturnsOnlyPostsForThatEvent()
    {
        var (user, ev) = await SeedAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, EventId = ev.Id },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, EventId = ev.Id },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, EventId = null }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, eventId: ev.Id);

        Assert.That(result.Posts.Count, Is.EqualTo(2));
        Assert.That(result.Posts.All(p => p.PhotoUrl == "a.jpg" || p.PhotoUrl == "b.jpg"));
    }

    [Test]
    public async Task GetAllAsync_WithEventId_SortedByLikesDesc()
    {
        var (user, ev) = await SeedAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, EventId = ev.Id, LikesCount = 1 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, EventId = ev.Id, LikesCount = 10 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, EventId = ev.Id, LikesCount = 5 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, sortOrder: PostSortOrder.LikesDesc, eventId: ev.Id);

        Assert.That(result.Posts[0].LikesCount, Is.EqualTo(10));
        Assert.That(result.Posts[1].LikesCount, Is.EqualTo(5));
        Assert.That(result.Posts[2].LikesCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CreatePostAsync_WithEventId_SetsEventIdOnPost()
    {
        var (user, ev) = await SeedAsync();
        var request = new LensmaniaLibrary.DTOs.Posts.CreatePostRequest
        {
            PhotoUrl = "test.jpg",
            EventId = ev.Id
        };

        var tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var uploadsPath = Path.Combine(tempRoot, "uploads", "photos");
        Directory.CreateDirectory(uploadsPath);
        var filePath = Path.Combine(uploadsPath, "test.jpg");
        try
        {
            await File.WriteAllTextAsync(filePath, "fake");
            var env = new FakeWebHostEnvironment(tempRoot);
            var service = new PostService(_db, env);

            var result = await service.CreatePostAsync(request, user.Id);
            var savedPost = _db.Posts.First(p => p.Id == result.Id);

            Assert.That(savedPost.EventId, Is.EqualTo(ev.Id));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task CreatePostAsync_WithInvalidEventId_ThrowsArgumentException()
    {
        var user = new User { Username = "bob", Email = "bob@test.com", PasswordHash = "x" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var uploadsPath = Path.Combine(tempRoot, "uploads", "photos");
        Directory.CreateDirectory(uploadsPath);
        var filePath = Path.Combine(uploadsPath, "test2.jpg");
        try
        {
            await File.WriteAllTextAsync(filePath, "fake");
            var env = new FakeWebHostEnvironment(tempRoot);
            var service = new PostService(_db, env);

            var request = new LensmaniaLibrary.DTOs.Posts.CreatePostRequest
            {
                PhotoUrl = "test2.jpg",
                EventId = 9999
            };

            Assert.ThrowsAsync<ArgumentException>(() => service.CreatePostAsync(request, user.Id));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
