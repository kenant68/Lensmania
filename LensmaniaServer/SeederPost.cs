using LensmaniaServer.Models;

namespace LensmaniaServer.Database;

public class SeederPost
{
    public static async Task Seed(AppDbContext context)
    {
        // Seed database only if it's empty
        if (context.Posts.Any()) return;
                
        // Ensure a seed user exists
        var seedUser = context.Users.FirstOrDefault(u => u.Username == "seed")
            ?? context.Users.Add(new User
            {
               Username = "seed",
               Email = "seed@example.local",
               PasswordHash = "password",
            }).Entity;
        await context.SaveChangesAsync();
        var userId = seedUser.Id;

        var posts = new List<Post>
        {
            new Post { Title = "Post 1", PhotoUrl = "3c9b3922-2dc8-43f9-8624-ecf033479039.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 2", PhotoUrl = "50bf864a-e821-46a1-993c-30bcd12ca4a9.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 3", PhotoUrl = "85efea09-1488-49a0-b82d-f2f7ffbe6d17.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 4", PhotoUrl = "7292f3ea-9342-4f9b-9265-0943c219aa16.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 5", PhotoUrl = "84232484-bb00-461d-93e8-3ab151736145.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 6", PhotoUrl = "a1bf6eec-72d9-4b7b-b4ef-cc0bde2f154e.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 7", PhotoUrl = "aeefa464-64e4-4a01-814d-6d48507c8541.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 8", PhotoUrl = "b6fb7de3-6194-488a-a1d4-82812e247a27.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 9", PhotoUrl = "b2210896-95de-4312-9ed9-b988a88303ab.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 10", PhotoUrl = "50bf864a-e821-46a1-993c-30bcd12ca4a9.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 11", PhotoUrl = "cebedb79-9893-4a7c-a057-2556c86ee4f9.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 12", PhotoUrl = "efa9fa16-252b-4109-b9d3-de5a0d222aa0.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 13", PhotoUrl = "f35dc072-1681-44c9-a867-c76dc7d71e83.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 14", PhotoUrl = "f50b807d-142d-4276-810e-55c235aa43b8.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 15", PhotoUrl = "85efea09-1488-49a0-b82d-f2f7ffbe6d17.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 16", PhotoUrl = "f35dc072-1681-44c9-a867-c76dc7d71e83.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 17", PhotoUrl = "b6fb7de3-6194-488a-a1d4-82812e247a27.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 18", PhotoUrl = "a1bf6eec-72d9-4b7b-b4ef-cc0bde2f154e.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 19", PhotoUrl = "3c9b3922-2dc8-43f9-8624-ecf033479039.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 20", PhotoUrl = "b6fb7de3-6194-488a-a1d4-82812e247a27.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 21", PhotoUrl = "b2210896-95de-4312-9ed9-b988a88303ab.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 22", PhotoUrl = "f50b807d-142d-4276-810e-55c235aa43b8.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 23", PhotoUrl = "84232484-bb00-461d-93e8-3ab151736145.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 24", PhotoUrl = "7292f3ea-9342-4f9b-9265-0943c219aa16.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 25", PhotoUrl = "f35dc072-1681-44c9-a867-c76dc7d71e83.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 26", PhotoUrl = "7292f3ea-9342-4f9b-9265-0943c219aa16.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 27", PhotoUrl = "f35dc072-1681-44c9-a867-c76dc7d71e83.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 28", PhotoUrl = "b2210896-95de-4312-9ed9-b988a88303ab.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 29", PhotoUrl = "50bf864a-e821-46a1-993c-30bcd12ca4a9.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 30", PhotoUrl = "f50b807d-142d-4276-810e-55c235aa43b8.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 31", PhotoUrl = "a1bf6eec-72d9-4b7b-b4ef-cc0bde2f154e.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 32", PhotoUrl = "efa9fa16-252b-4109-b9d3-de5a0d222aa0.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 33", PhotoUrl = "cebedb79-9893-4a7c-a057-2556c86ee4f9.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 34", PhotoUrl = "84232484-bb00-461d-93e8-3ab151736145.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 35", PhotoUrl = "b6fb7de3-6194-488a-a1d4-82812e247a27.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 36", PhotoUrl = "7292f3ea-9342-4f9b-9265-0943c219aa16.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 37", PhotoUrl = "85efea09-1488-49a0-b82d-f2f7ffbe6d17.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 38", PhotoUrl = "aeefa464-64e4-4a01-814d-6d48507c8541.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 39", PhotoUrl = "3c9b3922-2dc8-43f9-8624-ecf033479039.jpg", Description = "Données de test", UserId = userId },
            new Post { Title = "Post 40", PhotoUrl = "84232484-bb00-461d-93e8-3ab151736145.jpg", Description = "Données de test", UserId = userId },
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Base de données initialisée avec le seeder");
    }
}
