using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Users;

public class UpdateUserRequest
{
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Le pseudo doit faire entre 3 et 30 caractères.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Le pseudo ne peut contenir que des lettres, chiffres, _, . et -")]
    public string? Username { get; set; }
    
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [StringLength(254, ErrorMessage = "L'adresse e-mail est trop longue.")]
    public string? Email { get; set; }
}
