using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace LensmaniaClient.Services.Auth;

public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private readonly ITokenStore _tokenStore;
    private AuthenticationState _currentState = new(Anonymous);
    private bool _isInitialized;

    public CustomAuthenticationStateProvider(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_isInitialized)
        {
            return _currentState;
        }

        await InitializeAsync();
        return _currentState;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _currentState = await BuildStateFromStoredTokenAsync();
        _isInitialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public async Task SetTokenAsync(string token)
    {
        await _tokenStore.SaveTokenAsync(token);
        _currentState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public async Task ClearTokenAsync()
    {
        await _tokenStore.ClearTokenAsync();
        _currentState = new AuthenticationState(Anonymous);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private async Task<AuthenticationState> BuildStateFromStoredTokenAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        var state = BuildAuthenticationState(token);
        if (!state.User.Identity?.IsAuthenticated ?? true)
        {
            await _tokenStore.ClearTokenAsync();
        }

        return state;
    }

    private static AuthenticationState BuildAuthenticationState(string token)
    {
        if (!TryParseClaims(token, out var claims))
        {
            return new AuthenticationState(Anonymous);
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static bool TryParseClaims(string token, out IReadOnlyCollection<Claim> claims)
    {
        claims = Array.Empty<Claim>();
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return false;
        }

        byte[] payloadBytes;
        try
        {
            payloadBytes = ParseBase64WithoutPadding(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadBytes);
            if (payload is null)
            {
                return false;
            }

            if (payload.TryGetValue("exp", out var expirationElement) &&
                TryReadUnixSeconds(expirationElement, out var expiration) &&
                DateTimeOffset.UtcNow >= expiration)
            {
                return false;
            }

            claims = payload
                .Where(entry => entry.Key != "exp" && entry.Key != "nbf" && entry.Key != "iat")
                .SelectMany(ConvertToClaims)
                .ToArray();

            return claims.Count > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static IEnumerable<Claim> ConvertToClaims(KeyValuePair<string, JsonElement> entry)
    {
        if (entry.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var arrayItem in entry.Value.EnumerateArray())
            {
                var value = JsonElementToString(arrayItem);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return new Claim(entry.Key, value);
                }
            }

            yield break;
        }

        var claimValue = JsonElementToString(entry.Value);
        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            yield return new Claim(entry.Key, claimValue);
        }
    }

    private static string? JsonElementToString(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => null
        };

    private static bool TryReadUnixSeconds(JsonElement value, out DateTimeOffset expiration)
    {
        expiration = default;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var numericSeconds))
        {
            expiration = DateTimeOffset.FromUnixTimeSeconds(numericSeconds);
            return true;
        }

        if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var stringSeconds))
        {
            expiration = DateTimeOffset.FromUnixTimeSeconds(stringSeconds);
            return true;
        }

        return false;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        var formatted = base64.Replace('-', '+').Replace('_', '/');
        switch (formatted.Length % 4)
        {
            case 2:
                formatted += "==";
                break;
            case 3:
                formatted += "=";
                break;
        }

        return Convert.FromBase64String(formatted);
    }
}
