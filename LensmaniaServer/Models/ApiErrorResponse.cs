using System.Text.Json.Serialization;

namespace LensmaniaServer.Models;


public record ApiErrorResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);