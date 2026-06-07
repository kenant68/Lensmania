using LensmaniaLibrary.DTOs.Users;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;
using LensmaniaLibrary.DTOs;
using System.Net.Http.Json;

namespace LensmaniaClient.Pages.Admin;

public class UserManagementPageBase : ComponentBase
{
    [Inject] private UserService UserService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    protected PaginatedUsers? PagedUsers { get; private set; }
    protected bool IsLoading { get; private set; }
    protected int CurrentOffset { get; private set; } = 0;
    protected const int PageSize = 10;
    protected bool HasNextPage => PagedUsers is not null && CurrentOffset + PageSize < PagedUsers.Total;
    protected UserAdminResponse? _selectedUserToDelete;
    protected bool _showDeleteModal;
    protected bool isError { get; private set; }
    protected string? message { get; private set; }

    
    protected override async Task OnInitializedAsync()
        => await LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            PagedUsers = await UserService.GetAllAsync(CurrentOffset, PageSize);

            if (PagedUsers.Total > 0 && CurrentOffset >= PagedUsers.Total)
            {
                CurrentOffset = Math.Max(0, ((PagedUsers.Total - 1) / PageSize) * PageSize);
                PagedUsers = await UserService.GetAllAsync(CurrentOffset, PageSize);
            }
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

    protected async Task OnToggleBlock(UserAdminResponse user)
    {
        await UserService.ToggleUserIsActiveAsync(user.Id, !user.IsActive);
		NotificationService.Info($"Les droits de l'utilisateur ont été modifiés avec succès !");

        await LoadAsync();
    }
    
    // --- Confirmation modal ---
    protected void OpenDeleteModal(UserAdminResponse user)
    {
        _selectedUserToDelete = user;
        message = null;
        isError = false;
        _showDeleteModal = true;
    }

    protected void CloseDeleteModal()
    {
        _selectedUserToDelete = null;
        _showDeleteModal = false;
        message = null;
        isError = false;
    }

    protected async Task ConfirmDeleteUser()
    {
        if (_selectedUserToDelete is null)
            return;

        var response = await UserService.DeleteUserAsync(_selectedUserToDelete.Id);
        NotificationService.Info($"L'utilisateur a été supprimé avec succès !");

        if (response.IsSuccessStatusCode)
        {
            message = "Utilisateur supprimé avec succès.";
            isError = false;
            
            _selectedUserToDelete = null;
            _showDeleteModal = false;

            await LoadAsync();
            return;
        }

        ApiErrorResponse? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        }
        catch
        {
            // fallback to generic message when JSON parse fails
        }

        message = error?.Message ?? "Erreur lors de la suppression.";
        isError = true;
    }
}
