using LensmaniaLibrary.Models;

namespace LensmaniaServer.Database;

public class SeederPost
{
    public static async Task Seed(AppDbContext context)
    {
        // Seed database only if it's empty
        if (context.Posts.Any()) return;

        var posts = new List<Post>
        {
            new Post { Title = "Post 1", PhotoUrl = "https://picsum.photos/400/300", Description = "Données de test" },
            new Post { Title = "Post 2", PhotoUrl = "https://picsum.photos/400/500", Description = "Données de test" },
            new Post { Title = "Post 3", PhotoUrl = "https://picsum.photos/400/250", Description = "Données de test" },
            new Post { Title = "Post 4", PhotoUrl = "https://picsum.photos/400/600", Description = "Données de test" },
            new Post { Title = "Post 5", PhotoUrl = "https://picsum.photos/400/350", Description = "Données de test" },
            new Post { Title = "Post 6", PhotoUrl = "https://picsum.photos/400/450", Description = "Données de test" },
            new Post { Title = "Post 7", PhotoUrl = "https://picsum.photos/400/280", Description = "Données de test" },
            new Post { Title = "Post 8", PhotoUrl = "https://picsum.photos/400/550", Description = "Données de test" },
            new Post { Title = "Post 9", PhotoUrl = "https://picsum.photos/400/300", Description = "Données de test" },
            new Post { Title = "Post 10", PhotoUrl = "https://picsum.photos/400/500", Description = "Données de test" },
            new Post { Title = "Post 11", PhotoUrl = "https://picsum.photos/400/250", Description = "Données de test" },
            new Post { Title = "Post 12", PhotoUrl = "https://picsum.photos/400/700", Description = "Données de test" }
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Base de données intialisée avec le seeder");
    }
}