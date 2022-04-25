namespace Smart.AspNetCore.Routing;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ConvertAttribute : Attribute
{
    public abstract object? Convert(object? source);
}
