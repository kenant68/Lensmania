using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Blazor.Masonry.Registrars;
using LensmaniaClient;
using LensmaniaClient.Services.Auth;
using LensmaniaClient.Services.Posts;
using LensmaniaClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'Api:BaseUrl' is missing.");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient("LensmaniaApi", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("LensmaniaApi"));
builder.Services.AddMasonryInteropAsScoped();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<NotificationService>();

var host = builder.Build();
await host.Services.GetRequiredService<CustomAuthenticationStateProvider>().InitializeAsync();
await host.RunAsync();