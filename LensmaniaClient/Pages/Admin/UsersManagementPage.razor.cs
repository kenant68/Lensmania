using LensmaniaLibrary.DTOs.Users;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;
using LensmaniaLibrary.DTOs;
using System.Net.Http.Json;

namespace LensmaniaClient.Pages.Admin;

public class UserManagementPageBase : ComponentBase
{
    [Inject] private UserService UserService { get; set; } = default!;

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
                CurrentOffset = Math.Max(0, CurrentOffset - PageSize);
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

        await LoadAsync();
    }
    
    // --- Confirmation modal ---
    protected void OpenDeleteModal(UserAdminResponse user)
    {
        _selectedUserToDelete = user;
        _showDeleteModal = true;
    }

    protected void CloseDeleteModal()
    {
        _selectedUserToDelete = null;
        _showDeleteModal = false;
    }

    protected async Task ConfirmDeleteUser()
    {
        if (_selectedUserToDelete is null)
            return;

        var response = await UserService.DeleteUserAsync(_selectedUserToDelete.Id);

        if (response.IsSuccessStatusCode)
        {
            message = "Utilisateur supprimé avec succès.";
            isError = false;
            
            _selectedUserToDelete = null;
            _showDeleteModal = false;

            await LoadAsync();
            return;
        } 
        
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        message = error?.Message ?? "Erreur lors de la suppression.";
        isError = true;
    }
}
