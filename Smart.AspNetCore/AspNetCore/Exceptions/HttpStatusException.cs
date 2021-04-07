namespace Smart.AspNetCore.Exceptions
{
    using System;

    using Microsoft.AspNetCore.Http;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Ignore")]
    public class HttpStatusException : Exception
    {
        public int StatusCode { get; }

        public HttpStatusException(int statusCode)
        {
            StatusCode = statusCode;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Ignore")]
    public sealed class NotFoundException : HttpStatusException
    {
        public NotFoundException()
            : base(StatusCodes.Status404NotFound)
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Ignore")]
    public sealed class ForbiddenException : HttpStatusException
    {
        public ForbiddenException()
            : base(StatusCodes.Status403Forbidden)
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Ignore")]
    public sealed class BadRequestException : HttpStatusException
    {
        public BadRequestException()
            : base(StatusCodes.Status400BadRequest)
        {
        }
    }
}
