// LensmaniaClient/Pages/Events/EventsPage.razor.cs
using LensmaniaClient.Services;
using LensmaniaLibrary.DTOs.Events;
using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace LensmaniaClient.Pages.Events;

public class EventsPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    protected string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    protected List<EventResponse> ActiveEvents { get; private set; } = [];
    protected List<EventResponse> PastEvents { get; private set; } = [];
    protected bool IsLoading { get; private set; }
    protected string? ErrorMessage { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            var active = await EventSvc.GetActiveAsync();
            var past = await EventSvc.GetPastAsync();
            ActiveEvents = active?.Events ?? [];
            PastEvents = past?.Events ?? [];
        }
        catch
        {
            ErrorMessage = "Impossible de charger les événements.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
