namespace LensmaniaClient.Services.Posts;

public enum DeletePostResult
{
    Success,
    NotFound,
    Forbidden,
    Unauthorized,
    Error  
}