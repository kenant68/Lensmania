using LensmaniaServer.Models;

namespace LensmaniaServer.Database;

public class SeederTheme
{
    public static async Task Seed(AppDbContext context)
    {
        if (context.Themes.Any()) return;

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
