using LensmaniaServer.Database;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaTests.Helpers;


public static class TestHelpers
{
    public static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}