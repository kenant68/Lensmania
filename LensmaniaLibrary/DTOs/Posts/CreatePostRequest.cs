using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Posts;

public class CreatePostRequest
{
    [MaxLength(150, ErrorMessage = "Le titre ne doit pas dépasser 150 caractères")]
    [RegularExpression(@"^[^<>""'%;()&+]*$", ErrorMessage = "Le titre contient des caractères invalides")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Une photo est requise")]
    [MaxLength(400, ErrorMessage = "L'url photo est limité à 400 caractères")]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "La description ne doit pas dépasser 300 caractères")]
    [RegularExpression(@"^[^<>""'%;()&+]*$", ErrorMessage = "La description contient des caractères invalides")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "EventId doit être un identifiant positif")]
    public int? EventId { get; set; }
}
