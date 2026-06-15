using LensmaniaLibrary.Enums;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Posts;

[TestFixture]
public class PostServiceSortTests
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

    private async Task<User> SeedUserAsync()
    {
        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Test]
    public async Task GetAllAsync_DefaultSort_ReturnsNewestFirst()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10);

        Assert.That(result.Posts[0].Id, Is.GreaterThan(result.Posts[1].Id));
        Assert.That(result.Posts[1].Id, Is.GreaterThan(result.Posts[2].Id));
    }

    [Test]
    public async Task GetAllAsync_DateAsc_ReturnsOldestFirst()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, sortOrder: PostSortOrder.DateAsc);

        Assert.That(result.Posts[0].Id, Is.LessThan(result.Posts[1].Id));
        Assert.That(result.Posts[1].Id, Is.LessThan(result.Posts[2].Id));
    }

    [Test]
    public async Task GetAllAsync_DateAsc_WithCursor_ReturnsPostsAfterCursor()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var page1 = await _service.GetAllAsync(null, null, 1, sortOrder: PostSortOrder.DateAsc);
        var cursor = page1.NextCursor;

        var page2 = await _service.GetAllAsync(null, cursor, 1, sortOrder: PostSortOrder.DateAsc);

        Assert.That(cursor, Is.Not.Null);
        Assert.That(page2.Posts[0].Id, Is.GreaterThan(cursor));
    }

    [Test]
    public async Task GetAllAsync_DateDesc_WithCursor_ReturnsPostsBeforeCursor()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var page1 = await _service.GetAllAsync(null, null, 1);
        var cursor = page1.NextCursor;

        var page2 = await _service.GetAllAsync(null, cursor, 1);

        Assert.That(cursor, Is.Not.Null);
        Assert.That(page2.Posts[0].Id, Is.LessThan(cursor));
    }
}
