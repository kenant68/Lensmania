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
            new Post { Title = "Post 12", PhotoUrl = "https://picsum.photos/400/700", Description = "Données de test" },
            new Post { Title = "Post 13", PhotoUrl = "https://picsum.photos/400/600", Description = "Données de test" },
            new Post { Title = "Post 14", PhotoUrl = "https://picsum.photos/400/350", Description = "Données de test" },
            new Post { Title = "Post 15", PhotoUrl = "https://picsum.photos/400/450", Description = "Données de test" },
            new Post { Title = "Post 16", PhotoUrl = "https://picsum.photos/400/280", Description = "Données de test" },
            new Post { Title = "Post 17", PhotoUrl = "https://picsum.photos/400/550", Description = "Données de test" },
            new Post { Title = "Post 18", PhotoUrl = "https://picsum.photos/400/300", Description = "Données de test" },
            new Post { Title = "Post 19", PhotoUrl = "https://picsum.photos/400/500", Description = "Données de test" },
            new Post { Title = "Post 21", PhotoUrl = "https://picsum.photos/400/250", Description = "Données de test" },
            new Post { Title = "Post 22", PhotoUrl = "https://picsum.photos/400/700", Description = "Données de test" },
            new Post { Title = "Post 23", PhotoUrl = "https://picsum.photos/400/300", Description = "Données de test" },
            new Post { Title = "Post 24", PhotoUrl = "https://picsum.photos/400/500", Description = "Données de test" },
            new Post { Title = "Post 25", PhotoUrl = "https://picsum.photos/400/250", Description = "Données de test" },
            new Post { Title = "Post 26", PhotoUrl = "https://picsum.photos/400/600", Description = "Données de test" },
            new Post { Title = "Post 28", PhotoUrl = "https://picsum.photos/400/350", Description = "Données de test" },
            new Post { Title = "Post 29", PhotoUrl = "https://picsum.photos/400/450", Description = "Données de test" },
            new Post { Title = "Post 30", PhotoUrl = "https://picsum.photos/400/280", Description = "Données de test" },
            new Post { Title = "Post 31", PhotoUrl = "https://picsum.photos/400/550", Description = "Données de test" },
            new Post { Title = "Post 32", PhotoUrl = "https://picsum.photos/400/300", Description = "Données de test" },
            new Post { Title = "Post 33", PhotoUrl = "https://picsum.photos/400/500", Description = "Données de test" },
            new Post { Title = "Post 34", PhotoUrl = "https://picsum.photos/400/250", Description = "Données de test" },
            new Post { Title = "Post 35", PhotoUrl = "https://picsum.photos/400/700", Description = "Données de test" },
            new Post { Title = "Post 36", PhotoUrl = "https://picsum.photos/400/600", Description = "Données de test" },
            new Post { Title = "Post 37", PhotoUrl = "https://picsum.photos/400/350", Description = "Données de test" },
            new Post { Title = "Post 38", PhotoUrl = "https://picsum.photos/400/450", Description = "Données de test" },
            new Post { Title = "Post 39", PhotoUrl = "https://picsum.photos/400/280", Description = "Données de test" },
            new Post { Title = "Post 40", PhotoUrl = "https://picsum.photos/400/550", Description = "Données de test" },
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Base de données initialisée avec le seeder");
    }
}