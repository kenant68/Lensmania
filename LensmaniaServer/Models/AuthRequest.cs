using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire.")]
    [MinLength(3, ErrorMessage = "Le nom d'utilisateur doit contenir au moins 3 caractères.")]
    [MaxLength(32, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 32 caractères.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Le pseudo ne peut contenir que des lettres, chiffres, _, . et -")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [MaxLength(254, ErrorMessage = "L'adresse e-mail ne peut pas dépasser 254 caractères.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [MinLength(8, ErrorMessage = "Le mot de passe est trop court (minimum 8 caractères).")]
    [MaxLength(128, ErrorMessage = "Le mot de passe ne peut pas dépasser 128 caractères.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [MaxLength(254, ErrorMessage = "L'adresse e-mail ne peut pas dépasser 254 caractères.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [MinLength(8, ErrorMessage = "Le mot de passe est trop court (minimum 8 caractères).")]
    [MaxLength(128, ErrorMessage = "Le mot de passe ne peut pas dépasser 128 caractères.")]
    public string Password { get; set; } = string.Empty;
}

public class GoogleSignInRequest
{
    [Required(ErrorMessage = "Le jeton Google est obligatoire.")]
    public string IdToken { get; set; } = string.Empty;
}
