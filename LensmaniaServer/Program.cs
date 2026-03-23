using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Database;
using LensmaniaServer.Features.Posts;
using LensmaniaLibrary.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IPostService, PostService>();

// Set up Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:5135")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");
app.MapControllers();

// Template code to delete
/*var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");
*/

// Données de test
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Posts.Any())
    {
        db.Posts.AddRange(
            new Post { Title = "Premier post", PhotoUrl = "https://picsum.photos/400/300", Description = "Test" },
            new Post { Title = "Deuxième post", PhotoUrl = "https://picsum.photos/400/301", Description = "Test 2" }
        );
        db.SaveChanges();
    }
}

app.Run();

/*record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}*/