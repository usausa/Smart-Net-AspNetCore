namespace Smart.AspNetCore;

using System.Diagnostics.CodeAnalysis;

public static class EnumDisplayExtensions
{
    public static string GetDisplayName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(this T value)
        where T : struct, Enum
    {
        return EnumDisplayCache<T>.GetText(value);
    }
}
