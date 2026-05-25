using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class SeederTheme
{
    public static async Task Seed(AppDbContext context)
    {
        if (await context.Themes.AnyAsync()) return;

        var themes = new List<Theme>
        {
            new Theme { Name = "Noël",             Icon = "🎄" },
            new Theme { Name = "Été",              Icon = "☀️" },
            new Theme { Name = "Nouvel An Chinois", Icon = "🧧" },
        };

        context.Themes.AddRange(themes);
        await context.SaveChangesAsync();

        Console.WriteLine("Thèmes initiaux insérés.");
    }
}
