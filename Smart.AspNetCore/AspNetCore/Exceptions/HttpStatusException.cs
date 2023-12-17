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
    public BadRequestException()
        : base(StatusCodes.Status400BadRequest)
    {
    }
}
#pragma warning restore CA1032
