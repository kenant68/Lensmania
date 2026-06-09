using LensmaniaLibrary.DTOs.Badges;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Events;

[TestFixture]
public class EventServiceDateValidationTests
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

    private CreateEventRequest Request(DateTime start, DateTime end) => new(
        "Contest", "desc", start, end,
        false, _themeId, _userId,
        new List<CreateBadgeRequest> { new("Champion", "c.svg") });

    [Test]
    public void Create_rejects_start_date_in_the_past()
    {
        var req = Request(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(req));
    }

    [Test]
    public async Task Create_accepts_start_date_in_the_future()
    {
        var req = Request(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));
        var result = await _service.CreateAsync(req);
        Assert.That(result.Id, Is.GreaterThan(0));
    }
}
