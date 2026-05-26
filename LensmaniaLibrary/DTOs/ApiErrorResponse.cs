using System.Text.Json.Serialization;

namespace LensmaniaLibrary.DTOs;

public record ApiErrorResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);
    