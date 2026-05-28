// LensmaniaClient/Pages/Events/EventDetailPublicPage.razor.cs
using LensmaniaClient.Services;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaLibrary.DTOs.Posts;
using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace LensmaniaClient.Pages.Events;

public class EventDetailPublicPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    protected string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    [Parameter] public int Id { get; set; }

    protected EventDetailedResponse? Event { get; private set; }
    protected bool IsLoading { get; private set; }
    protected string? ErrorMessage { get; private set; }
    protected bool ShowModal { get; private set; }
    protected bool IsActive { get; private set; }

    private int? _loadedId;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedId == Id) return;

        IsLoading = true;
        ErrorMessage = null;
        Event = null;
        ShowModal = false;

        try
        {
            Event = await EventSvc.GetByIdAsync(Id);
            _loadedId = Id;

            if (Event is null)
            {
                ErrorMessage = "Événement introuvable.";
                return;
            }

            var now = DateTime.UtcNow;
            IsActive = Event.StartDate <= now && now <= Event.EndDate;
        }
        catch
        {
            ErrorMessage = "Impossible de charger l'événement.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void OpenModal() => ShowModal = true;
    protected void CloseModal() => ShowModal = false;

    protected void HandlePostCreated(PostListItemResponse post)
    {
        CloseModal();
    }
}
