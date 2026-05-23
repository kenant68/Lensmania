using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace LensmaniaClient.Services.Auth;

public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;

    public BearerTokenHandler(
        ITokenStore tokenStore,
        CustomAuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager)
    {
        _tokenStore = tokenStore;
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if ((response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            && !IsAuthEndpoint(request.RequestUri))
        {
            await _authenticationStateProvider.ClearTokenAsync(
                response.StatusCode == HttpStatusCode.Forbidden ? "blocked" : "unauthorized");
            
            _navigationManager.NavigateTo(AuthRoutes.Login);
        }

        return response;
    }

    private static bool IsAuthEndpoint(Uri? requestUri)
    {
        var path = requestUri?.AbsolutePath;
        return path is not null && path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase);
    }
}
