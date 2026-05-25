namespace LensmaniaServer.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        // Validation
        if (file is null || file.Length == 0) 
            throw new ArgumentException("Aucun fichier selectionné ou fichier vide");
        
        // Check allowed extensions
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(ext))
            throw new ArgumentException("Format non autorisé");

        // Check size (maximum 5 Mo)
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Fichier trop volumineux (max 5 Mo)");
        
        // Check real content (magic bytes) -> prevent malicious renaming
        using var reader = new BinaryReader(file.OpenReadStream());
        var magicBytes = reader.ReadBytes(4);
        
        if (!IsValidImage(magicBytes))
            throw new ArgumentException("Contenu du fichier invalide");

        // Upload
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }
    
    public async Task<string> UploadBadgeAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Aucun fichier sélectionné ou fichier vide.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };

        if (!allowedExtensions.Contains(ext))
            throw new ArgumentException("Format non autorisé. Utilisez JPG, PNG, WebP ou SVG.");

        if (file.Length > 2 * 1024 * 1024)
            throw new ArgumentException("Fichier trop volumineux (max 2 Mo).");

        if (ext == ".svg")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            if (!content.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
                && !content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Contenu SVG invalide.");
        }
        else
        {
            using var binaryReader = new BinaryReader(file.OpenReadStream());
            var magicBytes = binaryReader.ReadBytes(12);
            if (!IsValidImage(magicBytes))
                throw new ArgumentException("Contenu du fichier invalide.");
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "badges");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"uploads/badges/{fileName}";
    }

    private bool IsValidImage(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 4)
            return false;
        
        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) 
            return true;
        
        // PNG: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return true;

        // WebP: "RIFF" (0-3) .... "WEBP" (8-11)
        if (bytes.Length >= 12
            && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return true;

        return false;
    }
}
