using LensmaniaLibrary.Enums;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Posts;

[TestFixture]
public class PostServiceLikesSortTests
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
    public async Task GetAllAsync_LikesDesc_ReturnsHighestLikesFirst()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, LikesCount = 1 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, LikesCount = 10 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, LikesCount = 5 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, sortOrder: PostSortOrder.LikesDesc);

        Assert.That(result.Posts[0].LikesCount, Is.EqualTo(10));
        Assert.That(result.Posts[1].LikesCount, Is.EqualTo(5));
        Assert.That(result.Posts[2].LikesCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_LikesAsc_ReturnsLowestLikesFirst()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, LikesCount = 1 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, LikesCount = 10 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, LikesCount = 5 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, sortOrder: PostSortOrder.LikesAsc);

        Assert.That(result.Posts[0].LikesCount, Is.EqualTo(1));
        Assert.That(result.Posts[1].LikesCount, Is.EqualTo(5));
        Assert.That(result.Posts[2].LikesCount, Is.EqualTo(10));
    }

    [Test]
    public async Task GetAllAsync_LikesDesc_HasMore_ReturnsNextOffset()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, LikesCount = 3 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, LikesCount = 2 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, LikesCount = 1 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 2, sortOrder: PostSortOrder.LikesDesc);

        Assert.That(result.HasMore, Is.True);
        Assert.That(result.NextOffset, Is.EqualTo(2));
        Assert.That(result.NextCursor, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_LikesDesc_WithOffset_SkipsFirstItems()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, LikesCount = 3 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, LikesCount = 2 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, LikesCount = 1 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 2, sortOrder: PostSortOrder.LikesDesc, offset: 2);

        Assert.That(result.Posts.Count, Is.EqualTo(1));
        Assert.That(result.Posts[0].LikesCount, Is.EqualTo(1));
        Assert.That(result.HasMore, Is.False);
        Assert.That(result.NextOffset, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_LikesDesc_NextCursorIsNull()
    {
        var user = await SeedUserAsync();
        _db.Posts.AddRange(
            new Post { PhotoUrl = "a.jpg", UserId = user.Id, LikesCount = 3 },
            new Post { PhotoUrl = "b.jpg", UserId = user.Id, LikesCount = 2 },
            new Post { PhotoUrl = "c.jpg", UserId = user.Id, LikesCount = 1 }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 2, sortOrder: PostSortOrder.LikesDesc);

        Assert.That(result.NextCursor, Is.Null);
        Assert.That(result.NextOffset, Is.Not.Null);
    }
}
