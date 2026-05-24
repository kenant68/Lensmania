using System.Net.Http;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages.Admin;

public class EventDetailPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    protected string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    [Parameter] public int Id { get; set; }

    protected EventDetailledResponse? Event { get; private set; }
    protected bool IsLoading { get; private set; }
    protected string? ErrorMessage { get; private set; }
    protected bool ShowDeleteModal { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            Event = await EventSvc.GetByIdAsync(Id);
            if (Event is null) ErrorMessage = "Événement introuvable.";
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

    protected void OpenDeleteModal() => ShowDeleteModal = true;
    protected void CloseDeleteModal() => ShowDeleteModal = false;

    protected async Task ConfirmDelete()
    {
        try
        {
            await EventSvc.DeleteAsync(Id);
            Nav.NavigateTo("/admin/events");
        }
        catch
        {
            ErrorMessage = "Erreur lors de la suppression.";
            ShowDeleteModal = false;
        }
    }
}
