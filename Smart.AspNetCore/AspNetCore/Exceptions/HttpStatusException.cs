namespace Smart.AspNetCore.Exceptions;

using Microsoft.AspNetCore.Http;

#pragma warning disable CA1032
public abstract class HttpStatusException : Exception
{
    public int StatusCode { get; }

    protected HttpStatusException(int statusCode)
    {
        StatusCode = statusCode;
    }
}

public sealed class NotFoundException : HttpStatusException
{
    public NotFoundException()
        : base(StatusCodes.Status404NotFound)
    {
    }
}

public sealed class ForbiddenException : HttpStatusException
{
    public ForbiddenException()
        : base(StatusCodes.Status403Forbidden)
    {
    }
}

public sealed class BadRequestException : HttpStatusException
{
    public object? Value { get; }

    public BadRequestException()
        : base(StatusCodes.Status400BadRequest)
    {
    }

    public BadRequestException(object? value)
        : base(StatusCodes.Status400BadRequest)
    {
        Value = value;
    }
}
#pragma warning restore CA1032
