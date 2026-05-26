using LensmaniaLibrary.DTOs.Events;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages.Admin;

public class EventsManagementPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;

    protected PaginatedEvents? PagedEvents { get; private set; }
    protected bool IsLoading { get; private set; }
    protected int CurrentOffset { get; private set; }
    protected const int PageSize = 10;
    protected bool HasNextPage => PagedEvents is not null && CurrentOffset + PageSize < PagedEvents.Total;
    protected EventResponse? _selectedToDelete;
    protected bool _showDeleteModal;
    protected string? _errorMessage;

    protected override async Task OnInitializedAsync()
        => await LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;
        _errorMessage = null;
        try
        {
            PagedEvents = await EventSvc.GetAllAsync(CurrentOffset, PageSize);

            if (PagedEvents is not null && PagedEvents.Total > 0 && CurrentOffset >= PagedEvents.Total)
            {
                CurrentOffset = Math.Max(0, ((PagedEvents.Total - 1) / PageSize) * PageSize);
                PagedEvents = await EventSvc.GetAllAsync(CurrentOffset, PageSize);
            }
        }
        catch
        {
            _errorMessage = "Impossible de charger les événements.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task NextPage()
    {
        if (!HasNextPage) return;
        CurrentOffset += PageSize;
        await LoadAsync();
    }

    protected async Task PreviousPage()
    {
        if (CurrentOffset == 0) return;
        CurrentOffset = Math.Max(0, CurrentOffset - PageSize);
        await LoadAsync();
    }

    protected void OpenDeleteModal(EventResponse ev)
    {
        _selectedToDelete = ev;
        _showDeleteModal = true;
    }

    protected void CloseDeleteModal()
    {
        _selectedToDelete = null;
        _showDeleteModal = false;
    }

    protected async Task ConfirmDelete()
    {
        if (_selectedToDelete is null) return;
        try
        {
            await EventSvc.DeleteAsync(_selectedToDelete.Id);
        }
        catch
        {
            _errorMessage = "Erreur lors de la suppression.";
        }
        _selectedToDelete = null;
        _showDeleteModal = false;
        await LoadAsync();
    }
}
