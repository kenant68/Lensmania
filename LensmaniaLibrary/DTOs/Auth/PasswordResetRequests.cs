using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Auth;

public record ForgotPasswordRequest(
    [param: Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
    [param: EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    [param: MaxLength(254, ErrorMessage = "L'adresse e-mail ne peut pas depasser 254 caracteres.")]
    string Email
);

public record ResetPasswordRequest(
    [param: Required(ErrorMessage = "Le jeton est obligatoire.")]
    string Token,
    [param: Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [param: MinLength(8, ErrorMessage = "Le mot de passe est trop court (minimum 8 caracteres).")]
    [param: MaxLength(128, ErrorMessage = "Le mot de passe ne peut pas depasser 128 caracteres.")]
    string NewPassword
);

public record SimpleMessageResponse(string Message);