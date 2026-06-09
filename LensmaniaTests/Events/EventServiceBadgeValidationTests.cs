using LensmaniaLibrary.DTOs.Badges;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Events;

[TestFixture]
public class EventServiceBadgeValidationTests
{
    private AppDbContext _db = null!;
    private EventService _service = null!;
    private int _themeId;
    private int _userId;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new EventService(_db);

        var theme = new Theme { Name = "Nature" };
        var user = new User { Username = "owner", Email = "owner@x.io" };
        _db.Themes.Add(theme);
        _db.Users.Add(user);
        _db.SaveChanges();
        _themeId = theme.Id;
        _userId = user.Id;
    }

    [TearDown]
    public void Teardown() => _db.Dispose();

    private CreateEventRequest Request(List<CreateBadgeRequest> badges) => new()
    {
        Name = "Contest",
        Description = "desc",
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(2),
        IsPremium = false,
        ThemeId = _themeId,
        Badges = badges
    };

    [Test]
    public void Create_rejects_zero_badges()
    {
        var req = Request(new List<CreateBadgeRequest>());
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(req, _userId));
    }

    [Test]
    public void Create_rejects_two_badges()
    {
        var req = Request(new List<CreateBadgeRequest>
        {
            new() { Name = "A", ImageUrl = "a.svg" },
            new() { Name = "B", ImageUrl = "b.svg" }
        });
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(req, _userId));
    }

    [Test]
    public async Task Create_accepts_exactly_one_badge()
    {
        var req = Request(new List<CreateBadgeRequest> { new() { Name = "Champion", ImageUrl = "c.svg" } });
        var result = await _service.CreateAsync(req, _userId);
        Assert.That(result.Badges, Has.Count.EqualTo(1));
    }
}
