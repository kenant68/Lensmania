using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace LensmaniaTests.Helpers;

public class FakeWebHostEnvironment : IWebHostEnvironment
{
    public FakeWebHostEnvironment(string webRootPath)
    {
        WebRootPath = webRootPath;
        ContentRootPath = webRootPath;
    }

    public string WebRootPath { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; } = "Test";
    public string ApplicationName { get; set; } = "Test";
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
