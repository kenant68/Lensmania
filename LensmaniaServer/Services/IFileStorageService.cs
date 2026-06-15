namespace LensmaniaServer.Services;

public interface IFileStorageService
{
    Task<string> UploadPhotoAsync(IFormFile file);
    Task<string> UploadBadgeAsync(IFormFile file);
}
