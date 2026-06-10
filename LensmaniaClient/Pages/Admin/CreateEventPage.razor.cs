using LensmaniaLibrary.DTOs.Events;
using LensmaniaLibrary.DTOs.Badges;
using LensmaniaLibrary.DTOs.Themes;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LensmaniaClient.Pages.Admin;

public class CreateEventPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private ThemeService ThemeSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    protected EventFormModel _form = new();
    protected List<CreateBadgeRequest> _badges = new() { new CreateBadgeRequest { Name = string.Empty, ImageUrl = string.Empty } };
    protected List<ThemeResponse>? _themes;
    protected bool _isSubmitting;
    protected string? _errorMessage;
    protected EventDetailedResponse? _createdEvent;
    protected Dictionary<string, string> _fieldErrors = new();
    private int _currentUserId;

    protected override async Task OnInitializedAsync()
    {
        _themes = await ThemeSvc.GetAllAsync();
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        var claim = state.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                 ?? state.User.FindFirst("sub");
        if (claim is null || !int.TryParse(claim.Value, out var uid))
        {
            _errorMessage = "Session expirée, veuillez vous reconnecter.";
            return;
        }
        _currentUserId = uid;
    }

    protected void UpdateBadge(int index, CreateBadgeRequest updated)
    {
        if (index >= 0 && index < _badges.Count)
            _badges[index] = updated;
    }

    protected async Task SubmitAsync()
    {
        _errorMessage = null;
        _createdEvent = null;
        _fieldErrors  = new();

        if (_currentUserId == 0)
        {
            _errorMessage = "Session expirée, veuillez vous reconnecter.";
            return;
        }

        if (!Validate()) return;

        _isSubmitting = true;

        try
        {
            var request = new CreateEventRequest
            {
                Name = _form.Name.Trim(),
                Description = _form.Description.Trim(),
                StartDate = DateTime.SpecifyKind(_form.StartDate, DateTimeKind.Local).ToUniversalTime(),
                EndDate = DateTime.SpecifyKind(_form.EndDate, DateTimeKind.Local).ToUniversalTime(),
                IsPremium = _form.IsPremium,
                ThemeId = _form.ThemeId,
                Badges = _badges
            };

            _createdEvent = await EventSvc.CreateAsync(request);
            NotificationService.Success($"L'événement a été créé avec succès !");
            Nav.NavigateTo($"/admin/events/{_createdEvent.Id}");
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(_form.Name))
            _fieldErrors["Name"] = "Obligatoire";

        if (_form.StartDate == default)
            _fieldErrors["StartDate"] = "Obligatoire";
        else if (_form.StartDate <= DateTime.Now)
            _fieldErrors["StartDate"] = "La date de début doit être dans le futur.";

        if (_form.EndDate == default)
            _fieldErrors["EndDate"] = "Obligatoire";
        else if (_form.EndDate <= _form.StartDate)
            _fieldErrors["EndDate"] = "La date de fin doit être postérieure à la date de début.";

        if (_form.ThemeId == 0)
            _fieldErrors["ThemeId"] = "Obligatoire";

        return _fieldErrors.Count == 0;
    }

    protected bool ShowError(string field) => _fieldErrors.ContainsKey(field);

    protected class EventFormModel
    {
        public string   Name        { get; set; } = string.Empty;
        public string   Description { get; set; } = string.Empty;
        public DateTime StartDate   { get; set; }
        public DateTime EndDate     { get; set; }
        public bool     IsPremium   { get; set; }
        public int      ThemeId     { get; set; }
    }
}
