using LensmaniaServer.Services;

namespace LensmaniaTests.Helpers;

public sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public GoogleUserInfo? Result { get; set; }

    public Task<GoogleUserInfo?> ValidateAsync(string idToken)
        => Task.FromResult(Result);
}
