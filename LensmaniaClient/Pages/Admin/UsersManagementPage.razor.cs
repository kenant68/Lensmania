using LensmaniaLibrary.DTOs.Users;
using LensmaniaClient.Services;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages.Admin;

public class UserManagementPageBase : ComponentBase
{
    [Inject] private UserService UserService { get; set; } = default!;

    protected PaginatedUsers? PagedUsers { get; private set; }
    protected bool IsLoading { get; private set; }
    protected string SearchQuery { get; set; } = string.Empty;
    protected int CurrentOffset { get; private set; } = 0;
    protected const int PageSize = 10;
    protected bool HasNextPage => PagedUsers is not null && CurrentOffset + PageSize < PagedUsers.Total;
    protected UserAdminResponse? _selectedUserToDelete;
    protected bool _showDeleteModal;
    
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

    protected async Task OnToggleBan(UserAdminResponse user)
    {
        await UserService.ToggleUserIsActiveAsync(user.Id, !user.IsActive);

        await LoadAsync();
    }

    protected async Task OnDelete(UserAdminResponse user)
    {
        await UserService.DeleteUserAsync(user.Id);
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

        await UserService.DeleteUserAsync(_selectedUserToDelete.Id);

        _selectedUserToDelete = null;
        _showDeleteModal = false;

        await LoadAsync();
    }
}
