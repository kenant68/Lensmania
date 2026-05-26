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

//Todo: for production, use IConfiguration to inject the API URL from appsettings.json
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient("LensmaniaApi", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5078/");
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

var host = builder.Build();
await host.Services.GetRequiredService<CustomAuthenticationStateProvider>().InitializeAsync();
await host.RunAsync();