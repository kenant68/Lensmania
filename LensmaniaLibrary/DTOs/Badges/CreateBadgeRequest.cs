using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Badges;

public class CreateBadgeRequest
{
    [Required(ErrorMessage = "Le nom du badge est requis")]
    [MaxLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères")]
    [RegularExpression(@"^[^<>""'%;()&+]*$", ErrorMessage = "Le nom du badge contient des caractères invalides")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Une image est requise")]
    [MaxLength(400, ErrorMessage = "L'url de l'image est limité à 400 caractères")]
    public string ImageUrl { get; set; } = string.Empty;
}
