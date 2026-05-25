using System.Text;
using Ganss.Xss;

namespace LensmaniaServer.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    private static readonly HtmlSanitizer SvgSanitizer = BuildSvgSanitizer();

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    private static HtmlSanitizer BuildSvgSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "svg", "g", "path", "rect", "circle", "ellipse", "line", "polyline",
            "polygon", "text", "tspan", "defs", "lineargradient", "radialgradient",
            "stop", "clippath", "mask", "pattern", "symbol", "use", "marker",
            "title", "desc"
        })
            sanitizer.AllowedTags.Add(tag);

        sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[]
        {
            "id", "class", "style", "fill", "fill-opacity", "fill-rule", "stroke",
            "stroke-width", "stroke-linecap", "stroke-linejoin", "stroke-dasharray",
            "stroke-dashoffset", "stroke-opacity", "opacity", "d", "cx", "cy", "r",
            "rx", "ry", "x", "y", "x1", "x2", "y1", "y2", "width", "height", "points",
            "transform", "viewbox", "preserveaspectratio", "xmlns", "version",
            "gradientunits", "gradienttransform", "spreadmethod", "fx", "fy", "offset",
            "stop-color", "stop-opacity", "font-family", "font-size", "font-weight",
            "text-anchor", "dominant-baseline", "clip-path", "mask", "patternunits",
            "patterncontentunits", "markerwidth", "markerheight", "refx", "refy", "orient"
        })
            sanitizer.AllowedAttributes.Add(attr);

        // No href/xlink:href and no URI schemes -> blocks external/javascript references.
        sanitizer.AllowedSchemes.Clear();

        return sanitizer;
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

        string? sanitizedSvg = null;

        if (ext == ".svg")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            sanitizedSvg = SvgSanitizer.Sanitize(content);

            if (sanitizedSvg.IndexOf("<svg", StringComparison.OrdinalIgnoreCase) < 0)
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

        if (sanitizedSvg is not null)
        {
            await File.WriteAllTextAsync(fullPath, sanitizedSvg, Encoding.UTF8);
        }
        else
        {
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

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
