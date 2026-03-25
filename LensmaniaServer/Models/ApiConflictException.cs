namespace LensmaniaServer.Models;

public class ApiConflictException : Exception
{
    public ApiConflictException(string message) : base(message) {}
}

