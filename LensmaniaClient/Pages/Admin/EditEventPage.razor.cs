using LensmaniaLibrary.DTOs.Events;
using LensmaniaLibrary.DTOs.Badges;
using LensmaniaLibrary.DTOs.Themes;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages.Admin;

public class EditEventPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private ThemeService ThemeSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public int Id { get; set; }

    protected EventFormModel _form = new();
    protected List<CreateBadgeRequest> _badges = new() { new CreateBadgeRequest(string.Empty, string.Empty) };
    protected List<ThemeResponse>? _themes;
    protected bool _isLoading;
    protected bool _isSubmitting;
    protected string? _errorMessage;
    protected Dictionary<string, string> _fieldErrors = new();
    private int? _previousId;

    protected override async Task OnParametersSetAsync()
    {
        if (_previousId == Id) return;
        _previousId = Id;

        _errorMessage = null;
        _isLoading = true;
        try
        {
            _themes = await ThemeSvc.GetAllAsync();
            var ev = await EventSvc.GetByIdAsync(Id);
            if (ev is null)
            {
                _errorMessage = "Événement introuvable.";
                return;
            }
            _form = new EventFormModel
            {
                Name        = ev.Name,
                Description = ev.Description,
                StartDate   = ev.StartDate.Kind == DateTimeKind.Utc ? ev.StartDate.ToLocalTime() : ev.StartDate,
                EndDate     = ev.EndDate.Kind == DateTimeKind.Utc ? ev.EndDate.ToLocalTime() : ev.EndDate,
                IsPremium   = ev.IsPremium,
                ThemeId     = ev.Theme.Id
            };
            var loaded = ev.Badges
                .Select(b => new CreateBadgeRequest(b.Name, b.ImageUrl))
                .ToList();
            _badges = loaded.Count > 0
                ? new() { loaded[0] }
                : new() { new CreateBadgeRequest(string.Empty, string.Empty) };
        }
        catch
        {
            _errorMessage = "Impossible de charger l'événement.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected void UpdateBadge(int index, CreateBadgeRequest updated)
    {
        if (index >= 0 && index < _badges.Count)
            _badges[index] = updated;
    }

    protected async Task SubmitAsync()
    {
        _errorMessage = null;
        _fieldErrors  = new();

        if (!Validate()) return;

        _isSubmitting = true;
        try
        {
            var request = new UpdateEventRequest(
                _form.Name.Trim(),
                _form.Description.Trim(),
                DateTime.SpecifyKind(_form.StartDate, DateTimeKind.Local).ToUniversalTime(),
                DateTime.SpecifyKind(_form.EndDate, DateTimeKind.Local).ToUniversalTime(),
                _form.IsPremium,
                _form.ThemeId,
                _badges
            );

            await EventSvc.UpdateAsync(Id, request);
            Nav.NavigateTo($"/admin/events/{Id}");
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
