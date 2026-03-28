using Microsoft.JSInterop;

namespace LensmaniaClient.Services.Auth;

public sealed class LocalStorageTokenStore : ITokenStore
{
    private const string TokenStorageKey = "lensmania.auth.token";
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageTokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask SaveTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, token);
    }

    public async ValueTask<string?> GetTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async ValueTask ClearTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
        }
        catch (JSException)
        {
        }
    }
}
