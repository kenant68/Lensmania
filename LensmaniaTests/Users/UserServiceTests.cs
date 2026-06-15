using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaServer.Exceptions;
using LensmaniaTests.Helpers;
using LensmaniaLibrary.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaTests.Users;

[TestFixture]
public class UserServiceTests
{
    private AppDbContext _db = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _service = new UserService(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task<(User user, User admin)> SeedUsersAsync()
    {
        var user = new User
        {
            Id = 1, 
            Username = "alice", 
            Email = "alice@example.com", 
            PasswordHash = "x", 
            IsAdmin = false, 
            IsActive = true
        };
        
        var admin = new User
        {
            Id = 2, 
            Username = "admin", 
            Email = "admin@example.com", 
            PasswordHash = "x", 
            IsAdmin = true, 
            IsActive = true
        };

        _db.Users.AddRange(user, admin);
        await _db.SaveChangesAsync();
        
        return (user, admin);
    }
    
    #region GetAllAsync

    [Test]
    public async Task GetAllAsync_ShouldReturnPaginatedUsers()
    {
        await SeedUsersAsync();

        var result = await _service.GetAllAsync(0, 10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Users.Count, Is.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Offset, Is.EqualTo(0));
        Assert.That(result.Limit, Is.EqualTo(10));
    }

    [Test]
    public async Task GetAllAsync_ShouldRespectPagination()
    {
        await SeedUsersAsync();

        var result = await _service.GetAllAsync(0, 1);

        Assert.That(result.Users.Count, Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(2));
    }
    
    #endregion
    
    #region GetByUsernameAsync

    [Test]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameIsEmpty()
    {
        var result = await _service.GetByUsernameAsync("");
        
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        var (user, _) = await SeedUsersAsync();
        
        var result = await _service.GetByUsernameAsync("alice");
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(user.Id));
        Assert.That(result.Username, Is.EqualTo(user.Username));
    }

    #endregion

    #region UpdateAsync
    
    [Test]
    public async Task UpdateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var request = new UpdateUserRequest
        {
            Username = "newname"
        };

        var result = await _service.UpdateAsync(999, request);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldThrowException_WhenUsernameAlreadyTaken()
    {
        var (user, admin) = await SeedUsersAsync();

        var request = new UpdateUserRequest
        {
            Username = admin.Username
        };

        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(user.Id, request)
        );

        Assert.That(
            exception!.Message,
            Is.EqualTo("Ce nom d'utilisateur est déjà pris."));
    }
    
    [Test]
    public async Task UpdateAsync_ShouldUpdateUser_WhenRequestIsValid()
    {
        var (user, _) = await SeedUsersAsync();

        var request = new UpdateUserRequest
        {
            Username = "newalice",
            Email = "newalice@example.com"
        };

        var result = await _service.UpdateAsync(user.Id, request);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("newalice"));
        Assert.That(result.Email, Is.EqualTo("newalice@example.com"));

        var updatedUser = await _db.Users.FindAsync(user.Id);

        Assert.That(updatedUser!.Username, Is.EqualTo("newalice"));
        Assert.That(updatedUser.Email, Is.EqualTo("newalice@example.com"));
    }
    
    #endregion
    
    #region DeleteByAdminAsync

    [Test]
    public async Task DeleteByAdminAsync_ShouldThrowException_WhenUserIsAdmin()
    {
        var (_, admin) = await SeedUsersAsync();
        
        var exception = Assert.ThrowsAsync<BusinessException>(
            () => _service.DeleteByAdminAsync(admin.Id)
        );

        Assert.That(
            exception!.Message,
            Is.EqualTo("Vous n'êtes pas autorisé à supprimer un administrateur."));
    }
    
    [Test]
    public async Task DeleteByAdminAsync_ShouldDeleteUser_WhenUserIsNotAdmin()
    {
        var (user, _) = await SeedUsersAsync();
        
        var result = await _service.DeleteByAdminAsync(user.Id);
        
        Assert.That(result, Is.True);
        var deletedUser = await _db.Users.FindAsync(user.Id);
        Assert.That(deletedUser, Is.Null);
    }
    
    #endregion
    
    #region DeleteMeAsync

    [Test]
    public async Task DeleteMeAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var result = await _service.DeleteMeAsync(999);

        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task DeleteMeAsync_ShouldDeleteUser_WhenUserExists()
    {
        var (user, _) = await SeedUsersAsync();
        
        var result = await _service.DeleteMeAsync(user.Id);
        
        Assert.That(result, Is.True);
        var deletedUser = await _db.Users.FindAsync(user.Id);
        Assert.That(deletedUser, Is.Null);
    }
    
    #endregion
    
    #region ToggleIsActiveAsync

    [Test]
    public async Task ToggleIsActiveAsync_ShouldDeactivateUser_WhenIsActiveIsFalse()
    {
        var (user, _) = await SeedUsersAsync();

        var result = await _service.ToggleIsActiveAsync(user.Id, false);

        Assert.That(result, Is.True);
        
        var updatedUser = await _db.Users.FindAsync(user.Id);
        Assert.That(updatedUser!.IsActive, Is.False);
    }

    [Test]
    public async Task ToggleIsActiveAsync_ShouldActivateUser_WhenIsActiveIsTrue()
    {
        var (user, _) = await SeedUsersAsync();

        var result = await _service.ToggleIsActiveAsync(user.Id, true);

        Assert.That(result, Is.True);
        
        var updatedUser = await _db.Users.FindAsync(user.Id);
        Assert.That(updatedUser!.IsActive, Is.True);
    }

    [Test]
    public async Task ToggleIsActiveAsync_UnknownUser_ReturnsFalse()
    {
        var result = await _service.ToggleIsActiveAsync(999, false);

        Assert.That(result, Is.False); 
    }

    #endregion
}
