using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Models;

public record RegisterRequest(
    [param: Required(ErrorMessage = "Le nom d'utilisateur est obligatoire.")]
    [param: MinLength(3, ErrorMessage = "Le nom d'utilisateur doit contenir au moins 3 caracteres.")]
    [param: MaxLength(32, ErrorMessage = "Le nom d'utilisateur ne peut pas depasser 32 caracteres.")]
    string Username,
    [param: Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
    [param: EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [param: MaxLength(254, ErrorMessage = "L'adresse e-mail ne peut pas depasser 254 caracteres.")]
    string Email,
    [param: Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [param: MinLength(8, ErrorMessage = "Le mot de passe est trop court (minimum 8 caracteres).")]
    [param: MaxLength(128, ErrorMessage = "Le mot de passe ne peut pas depasser 128 caracteres.")]
    string Password
);

public record LoginRequest(
    [param: Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
    [param: EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [param: MaxLength(254, ErrorMessage = "L'adresse e-mail ne peut pas depasser 254 caracteres.")]
    string Email,
    [param: Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [param: MinLength(8, ErrorMessage = "Le mot de passe est trop court (minimum 8 caracteres).")]
    [param: MaxLength(128, ErrorMessage = "Le mot de passe ne peut pas depasser 128 caracteres.")]
    string Password
);