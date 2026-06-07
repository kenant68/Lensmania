using LensmaniaLibrary.DTOs.Users;
using LensmaniaLibrary.DTOs.Posts;
using System.Security.Claims;
using LensmaniaClient.Services.Auth;
using LensmaniaClient.Services;
using LensmaniaClient.Components;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Pages;

public class ProfilePageBase : ComponentBase
{
    [Inject] private UserService UserService { get; set; } = default!;
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    
    [Parameter] public required string Username { get; set; }

    protected Posts? _postsComponent;
    protected bool _isOwnProfile;
    protected string? _username;
    protected bool _showDeleteModal;
    protected bool _showEditModal;
    protected bool _showActionsMenu;
    protected bool _isLoading = true;
    protected bool _userNotFound = false;
    protected bool _hasError = false;
    protected string? _currentUsername;
    protected string? _currentEmail;
    protected UpdateUserRequest _editModel = new();

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _userNotFound = false;
        
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        var usernameClaim = user.FindFirst(ClaimTypes.Name)?.Value;
        
        _isOwnProfile = string.Equals(usernameClaim, Username, StringComparison.OrdinalIgnoreCase);
        _username = _isOwnProfile ? usernameClaim : Username;

        _currentEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        _currentUsername = usernameClaim;
        
        try {
            var userSearched = await UserService.GetByUsernameAsync(Username);

            if (userSearched is null)
            {
                _userNotFound = true;
                return;
            }
        }
        catch {
            _hasError = true;
        }
        finally {
            _isLoading = false;
        }
    }
    
    protected void ToggleActionsMenu()
    {
        _showActionsMenu = !_showActionsMenu;
    }

    // --- Confirmation modal ---
    protected void OpenDeleteModal()
    {
        _showActionsMenu = false;
        _showDeleteModal = true;
    }

    protected void CloseDeleteModal()
    {
        _showDeleteModal = false;
    }

    protected async Task ConfirmDeleteMe()
    {
        await UserService.DeleteMeAsync();
        _showDeleteModal = false;

        await AuthStateProvider.ClearTokenAsync();
        Navigation.NavigateTo(string.Empty, replace: true);
    }
    
    // --- Edit modal ---
    protected void OpenEditModal()
    {
        _showActionsMenu = false;
        
        _editModel = new UpdateUserRequest
        {
            Username = _currentUsername ?? string.Empty,
            Email = _currentEmail ?? string.Empty
        };
        
        _showEditModal = true;
    }

    protected void CloseEditModal()
    {
        _showEditModal = false;
    }
    
    protected async Task ConfirmEditProfile()
    {
        var updatedUser = await UserService.UpdateMeAsync(_editModel);
        NotificationService.Success($"Les informations de votre profil ont été modifiées ! Elles seront appliquées après reconnexion.");

        if (updatedUser is null)
            return;

        _currentUsername = updatedUser.Username;
        _currentEmail = updatedUser.Email;
        _username = updatedUser.Username;
        _showEditModal = false;
    }
}
