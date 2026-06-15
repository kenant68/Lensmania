using System.ComponentModel.DataAnnotations;
using LensmaniaLibrary.DTOs.Badges;

namespace LensmaniaLibrary.DTOs.Events;

public class CreateEventRequest
{
    [Required(ErrorMessage = "Le nom de l'événement est requis")]
    [MaxLength(200, ErrorMessage = "Le nom ne doit pas dépasser 200 caractères")]
    [RegularExpression(@"^[^<>""'%;()&+]*$", ErrorMessage = "Le nom de l'évènement contient des caractères invalides")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(600, ErrorMessage = "La description ne doit pas dépasser 600 caractères")]
    [RegularExpression(@"^[^<>""'%;()&+]*$", ErrorMessage = "La description contient des caractères invalides")]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public bool IsPremium { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ThemeId doit être un identifiant positif")]
    public int ThemeId { get; set; }
    
    [MaxLength(20, ErrorMessage = "L'évènement ne peut pas posséder plus de 20 badges.")]
    public List<CreateBadgeRequest> Badges { get; set; } = [];   
}
