using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Soenneker.Blazor.Masonry.Registrars;
using LensmaniaClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Todo: for production, use IConfiguration to inject the API URL from appsettings.json
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5078/") });
builder.Services.AddMasonryInteropAsScoped();

await builder.Build().RunAsync();