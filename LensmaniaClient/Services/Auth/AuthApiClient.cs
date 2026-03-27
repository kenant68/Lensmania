using System.Net;
using System.Net.Http.Json;
using LensmaniaClient.Models.Auth;

namespace LensmaniaClient.Services.Auth;

public sealed class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    public AuthApiClient(HttpClient httpClient, CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public Task<AuthApiResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request) =>
        SendAsync("api/auth/register", request);

    public Task<AuthApiResult<AuthResponseDto>> LoginAsync(LoginRequestDto request) =>
        SendAsync("api/auth/login", request);

    private async Task<AuthApiResult<AuthResponseDto>> SendAsync<TRequest>(string url, TRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (payload is null)
                {
                    return AuthApiResult<AuthResponseDto>.Failure(
                        new AuthApiError(
                            AuthApiErrorType.UnexpectedServerError,
                            "Authentication response body is empty."));
                }

                await _authenticationStateProvider.SetTokenAsync(payload.Token);
                return AuthApiResult<AuthResponseDto>.Success(payload);
            }

            var apiError = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            var message = ExtractUserMessage(apiError);
            var code = apiError?.Code;

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    AuthApiResult<AuthResponseDto>.Failure(
                        new AuthApiError(AuthApiErrorType.InvalidCredentials, message, code)),
                HttpStatusCode.Conflict =>
                    AuthApiResult<AuthResponseDto>.Failure(
                        new AuthApiError(AuthApiErrorType.DuplicateIdentity, message, code)),
                HttpStatusCode.BadRequest =>
                    AuthApiResult<AuthResponseDto>.Failure(
                        new AuthApiError(AuthApiErrorType.ValidationFailed, message, code)),
                _ =>
                    AuthApiResult<AuthResponseDto>.Failure(
                        new AuthApiError(AuthApiErrorType.UnexpectedServerError, message, code))
            };
        }
        catch (HttpRequestException)
        {
            return AuthApiResult<AuthResponseDto>.Failure(
                new AuthApiError(AuthApiErrorType.NetworkFailure, "Impossible de contacter le serveur. Verifiez votre connexion."));
        }
        catch (NotSupportedException)
        {
            return AuthApiResult<AuthResponseDto>.Failure(
                new AuthApiError(AuthApiErrorType.UnexpectedServerError, "Reponse serveur non prise en charge."));
        }
    }

    private static string ExtractUserMessage(ApiErrorDto? apiError)
    {
        if (apiError?.Errors is { Count: > 0 })
        {
            var firstMessages = apiError.Errors.Values.FirstOrDefault(messages => messages.Length > 0);
            if (firstMessages is not null)
            {
                return firstMessages[0];
            }
        }

        return string.IsNullOrWhiteSpace(apiError?.Message)
            ? "La requete d'authentification a echoue."
            : apiError.Message;
    }
}
