using System.Text;
using LensmaniaServer.Database;
using LensmaniaServer.Services;
using LensmaniaServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new
        {
            code = AuthErrorCodes.ValidationFailed,
            message = "Certaines donnees du formulaire sont invalides.",
            errors
        });
    };
});

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddOptions<PasswordResetOptions>()
    .BindConfiguration("PasswordReset")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<IEmailSender, GmailSmtpEmailSender>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddOptions<EventClosingOptions>()
    .BindConfiguration("EventClosing")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<EventClosureService>();
builder.Services.AddHostedService<EventClosingBackgroundService>();

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userService = context.HttpContext.RequestServices
                    .GetRequiredService<IUserService>();
                var userIdClaim = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim is null)
                {
                    context.Fail("Invalid token");
                    return;
                }

                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    context.Fail("Invalid token");
                    return;
                }
                
                var isActive = await userService.IsActiveAsync(userId);

                if (!isActive)
                {
                    context.Fail("User is blocked");
                }
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("isAdmin", "true"));
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<TokenService>();

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

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeederTheme.Seed(context);
    await SeederPost.Seed(context);
}

app.Run();