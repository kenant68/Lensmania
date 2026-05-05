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

    public async Task<AuthApiResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var result = await SendAsync<RegisterRequestDto, AuthResponseDto>("api/auth/register", request);
        if (result.IsSuccess && result.Data is not null)
        {
            await _authenticationStateProvider.SetTokenAsync(result.Data.Token);
        }
        return result;
    }

    public async Task<AuthApiResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var result = await SendAsync<LoginRequestDto, AuthResponseDto>("api/auth/login", request);
        if (result.IsSuccess && result.Data is not null)
        {
            await _authenticationStateProvider.SetTokenAsync(result.Data.Token);
        }
        return result;
    }

    public Task<AuthApiResult<SimpleMessageResponseDto>> ForgotPasswordAsync(ForgotPasswordRequestDto request) =>
        SendAsync<ForgotPasswordRequestDto, SimpleMessageResponseDto>("api/auth/forgot-password", request);

    public Task<AuthApiResult<SimpleMessageResponseDto>> ResetPasswordAsync(ResetPasswordRequestDto request) =>
        SendAsync<ResetPasswordRequestDto, SimpleMessageResponseDto>("api/auth/reset-password", request);

    private async Task<AuthApiResult<TResponse>> SendAsync<TRequest, TResponse>(string url, TRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<TResponse>();
                if (payload is null)
                {
                    return AuthApiResult<TResponse>.Failure(
                        new AuthApiError(
                            AuthApiErrorType.UnexpectedServerError,
                            "Authentication response body is empty."));
                }

                return AuthApiResult<TResponse>.Success(payload);
            }

            var apiError = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            var message = ExtractUserMessage(apiError);
            var code = apiError?.Code;

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    AuthApiResult<TResponse>.Failure(
                        new AuthApiError(AuthApiErrorType.InvalidCredentials, message, code)),
                HttpStatusCode.Conflict =>
                    AuthApiResult<TResponse>.Failure(
                        new AuthApiError(AuthApiErrorType.DuplicateIdentity, message, code)),
                HttpStatusCode.BadRequest =>
                    AuthApiResult<TResponse>.Failure(
                        new AuthApiError(MapBadRequestErrorType(code), message, code)),
                _ =>
                    AuthApiResult<TResponse>.Failure(
                        new AuthApiError(AuthApiErrorType.UnexpectedServerError, message, code))
            };
        }
        catch (HttpRequestException)
        {
            return AuthApiResult<TResponse>.Failure(
                new AuthApiError(AuthApiErrorType.NetworkFailure, "Impossible de contacter le serveur. Verifiez votre connexion."));
        }
        catch (NotSupportedException)
        {
            return AuthApiResult<TResponse>.Failure(
                new AuthApiError(AuthApiErrorType.UnexpectedServerError, "Reponse serveur non prise en charge."));
        }
        catch (System.Text.Json.JsonException)
        {
            return AuthApiResult<TResponse>.Failure(
                new AuthApiError(AuthApiErrorType.UnexpectedServerError, "Format de reponse serveur invalide."));
        }
    }

    private static AuthApiErrorType MapBadRequestErrorType(string? code) => code switch
    {
        "AUTH_INVALID_OR_EXPIRED_RESET_TOKEN" => AuthApiErrorType.InvalidOrExpiredResetToken,
        _ => AuthApiErrorType.ValidationFailed
    };

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