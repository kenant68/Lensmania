using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaTests.Posts;

[TestFixture]
public class PostServiceLikeTests
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

    private async Task<(User user, Post post)> SeedAsync()
    {
        var user = new User { Username = "alice", Email = "alice@example.com", PasswordHash = "x" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var post = new Post { PhotoUrl = "photo.jpg", UserId = user.Id };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        return (user, post);
    }

    [Test]
    public async Task ToggleLikeAsync_NewLike_InsertsPostLikeAndIncrementsCount()
    {
        var (user, post) = await SeedAsync();

        var result = await _service.ToggleLikeAsync(post.Id, user.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.PostLikes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ToggleLikeAsync_ExistingLike_RemovesPostLikeAndDecrementsCount()
    {
        var (user, post) = await SeedAsync();
        await _service.ToggleLikeAsync(post.Id, user.Id);

        var result = await _service.ToggleLikeAsync(post.Id, user.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.PostLikes.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ToggleLikeAsync_UnknownPost_ReturnsFalse()
    {
        var result = await _service.ToggleLikeAsync(999, 1);

        Assert.That(result, Is.False);
        Assert.That(await _db.PostLikes.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllAsync_ReturnsLikesCount()
    {
        var (_, post) = await SeedAsync();
        var p = await _db.Posts.FindAsync(post.Id);
        p!.LikesCount = 3;
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10);

        Assert.That(result.Posts[0].LikesCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAllAsync_WithCurrentUserId_WhenLiked_ReturnsIsLikedTrue()
    {
        var (user, post) = await SeedAsync();
        _db.PostLikes.Add(new PostLike { UserId = user.Id, PostId = post.Id });
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10, user.Id);

        Assert.That(result.Posts[0].IsLikedByCurrentUser, Is.True);
    }

    [Test]
    public async Task GetAllAsync_WithoutCurrentUser_IsLikedByCurrentUserIsFalse()
    {
        var (user, post) = await SeedAsync();
        _db.PostLikes.Add(new PostLike { UserId = user.Id, PostId = post.Id });
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, 10);

        Assert.That(result.Posts[0].IsLikedByCurrentUser, Is.False);
    }
}
