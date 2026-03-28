namespace LensmaniaClient.Services.Auth;

public interface ITokenStore
{
    ValueTask SaveTokenAsync(string token);
    ValueTask<string?> GetTokenAsync();
    ValueTask ClearTokenAsync();
}
