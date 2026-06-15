using System.Net.Http;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaClient.Services;
using LensmaniaClient.Services.Posts;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages.Admin;

public class EventDetailPageBase : ComponentBase
{
    [Inject] private EventService EventSvc { get; set; } = default!;
    [Inject] private PostService PostSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    protected string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    [Parameter] public int Id { get; set; }

    protected EventDetailedResponse? Event { get; private set; }
    protected List<PostListItemResponse> EventPosts { get; private set; } = [];
    protected bool IsLoading { get; private set; }
    protected string? ErrorMessage { get; private set; }
    protected bool ShowDeleteModal { get; private set; }
    protected bool IsSettingCover { get; private set; }

    private int? _loadedId;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedId == Id) return;

        IsLoading = true;
        ErrorMessage = null;
        Event = null;
        EventPosts = [];

        try
        {
            Event = await EventSvc.GetByIdAsync(Id);

            if (Event is null)
            {
                ErrorMessage = "Événement introuvable.";
                return;
            }

            var posts = await PostSvc.GetByEventAsync(Id, limit: 50);
            EventPosts = posts?.Posts ?? [];
            _loadedId = Id;
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

    protected async Task SetCoverPhoto(int postId)
    {
        IsSettingCover = true;
        ErrorMessage = null;
        try
        {
            var updated = await EventSvc.SetCoverPhotoAsync(Id, postId);
            if (updated is not null)
                Event = updated;
        }
        catch
        {
            ErrorMessage = "Erreur lors de la mise à jour de la photo de couverture.";
        }
        finally
        {
            IsSettingCover = false;
        }
    }
}
