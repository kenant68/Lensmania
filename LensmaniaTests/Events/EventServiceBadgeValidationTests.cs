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

    private CreateEventRequest Request(List<CreateBadgeRequest> badges) => new(
        "Contest", "desc",
        new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc),
        false, _themeId, _userId, badges);

    [Test]
    public void Create_rejects_zero_badges()
    {
        var req = Request(new List<CreateBadgeRequest>());
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(req));
    }

    [Test]
    public void Create_rejects_two_badges()
    {
        var req = Request(new List<CreateBadgeRequest>
        {
            new("A", "a.svg"),
            new("B", "b.svg")
        });
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(req));
    }

    [Test]
    public async Task Create_accepts_exactly_one_badge()
    {
        var req = Request(new List<CreateBadgeRequest> { new("Champion", "c.svg") });
        var result = await _service.CreateAsync(req);
        Assert.That(result.Badges, Has.Count.EqualTo(1));
    }
}
