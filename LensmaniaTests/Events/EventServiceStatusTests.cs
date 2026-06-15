using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;

namespace LensmaniaTests.Events;

[TestFixture]
public class EventServiceStatusTests
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

    private async Task<(User user, Theme theme)> SeedPrerequisitesAsync()
    {
        var user = new User { Username = "admin", Email = "admin@test.com", PasswordHash = "x" };
        var theme = new Theme { Name = "Nature", Icon = "🌿" };
        _db.Users.Add(user);
        _db.Themes.Add(theme);
        await _db.SaveChangesAsync();
        return (_db.Users.First(), _db.Themes.First());
    }

    [Test]
    public async Task GetAllAsync_StatusActive_ReturnsOnlyActiveEvents()
    {
        var (user, theme) = await SeedPrerequisitesAsync();
        _db.Events.AddRange(
            new Event { Name = "Active", Description = "", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), ThemeId = theme.Id, UserId = user.Id },
            new Event { Name = "Past", Description = "", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-2), ThemeId = theme.Id, UserId = user.Id },
            new Event { Name = "Future", Description = "", StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(10), ThemeId = theme.Id, UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(0, 10, "active");

        Assert.That(result.Events.Count, Is.EqualTo(1));
        Assert.That(result.Events[0].Name, Is.EqualTo("Active"));
    }

    [Test]
    public async Task GetAllAsync_StatusPast_ReturnsOnlyPastEventsOrderedByEndDateDesc()
    {
        var (user, theme) = await SeedPrerequisitesAsync();
        _db.Events.AddRange(
            new Event { Name = "Active", Description = "", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), ThemeId = theme.Id, UserId = user.Id },
            new Event { Name = "Past1", Description = "", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-2), ThemeId = theme.Id, UserId = user.Id },
            new Event { Name = "Past2", Description = "", StartDate = DateTime.UtcNow.AddDays(-20), EndDate = DateTime.UtcNow.AddDays(-5), ThemeId = theme.Id, UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(0, 3, "past");

        Assert.That(result.Events.Count, Is.EqualTo(2));
        Assert.That(result.Events[0].Name, Is.EqualTo("Past1"));
        Assert.That(result.Events[1].Name, Is.EqualTo("Past2"));
    }

    [Test]
    public async Task GetAllAsync_StatusNull_ReturnsAllEventsOrdered()
    {
        var (user, theme) = await SeedPrerequisitesAsync();
        _db.Events.AddRange(
            new Event { Name = "A", Description = "", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), ThemeId = theme.Id, UserId = user.Id },
            new Event { Name = "B", Description = "", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-2), ThemeId = theme.Id, UserId = user.Id }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(0, 10);

        Assert.That(result.Events.Count, Is.EqualTo(2));
        Assert.That(result.Events[0].Name, Is.EqualTo("A"));
        Assert.That(result.Events[1].Name, Is.EqualTo("B"));
    }
}
