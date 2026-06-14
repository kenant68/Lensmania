namespace LensmaniaServer.Exceptions;

public class ApiConflictException : Exception
{
    public ApiConflictException(string message) : base(message) {}
}

