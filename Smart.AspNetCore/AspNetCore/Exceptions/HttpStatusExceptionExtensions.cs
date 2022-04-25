namespace Smart.AspNetCore.Exceptions;

public static class HttpStatusExceptionExtensions
{
    public static T MustExist<T>(this T? value)
    {
        if (value is null)
        {
            throw new NotFoundException();
        }

        return value;
    }

    public static T NotFoundIf<T>(this T value, Func<T, bool> func)
        where T : class
    {
        if (func(value))
        {
            throw new NotFoundException();
        }

        return value;
    }

    public static T ForbidIf<T>(this T value, Func<T, bool> func)
    {
        if (func(value))
        {
            throw new ForbiddenException();
        }

        return value;
    }

    public static T BadRequestIf<T>(this T value, Func<T, bool> func)
    {
        if (func(value))
        {
            throw new BadRequestException();
        }

        return value;
    }
}
