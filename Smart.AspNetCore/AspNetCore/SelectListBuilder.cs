namespace Smart.AspNetCore;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.Rendering;

public static class SelectListBuilder
{
    //--------------------------------------------------------------------------------
    // Enum
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<SelectListItem> FromEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
        where T : struct, Enum
    {
        return BuildEnum<T>(null, null);
    }

    public static IReadOnlyList<SelectListItem> FromEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(T selected)
        where T : struct, Enum
    {
        return BuildEnum<T>(null, selected);
    }

    public static IReadOnlyList<SelectListItem> FromEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(Func<T, bool> predicate)
        where T : struct, Enum
    {
        return BuildEnum(predicate, null);
    }

    public static IReadOnlyList<SelectListItem> FromEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(Func<T, bool> predicate, T selected)
        where T : struct, Enum
    {
        return BuildEnum(predicate, selected);
    }

    private static List<SelectListItem> BuildEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(Func<T, bool>? predicate, T? selected)
        where T : struct, Enum
    {
        var entries = EnumDisplayCache<T>.Entries;
        var list = new List<SelectListItem>(entries.Length);
        foreach (var entry in entries)
        {
            if ((predicate is not null) && !predicate(entry.Value))
            {
                continue;
            }

            list.Add(new SelectListItem
            {
                Value = entry.Name,
                Text = entry.Text,
                Selected = selected.HasValue && EqualityComparer<T>.Default.Equals(entry.Value, selected.Value)
            });
        }

        return list;
    }

    //--------------------------------------------------------------------------------
    // Range
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<SelectListItem> FromRange(int start, int end)
    {
        return BuildRange(start, end, 1, null, null);
    }

    public static IReadOnlyList<SelectListItem> FromRange(int start, int end, int step)
    {
        return BuildRange(start, end, step, null, null);
    }

    public static IReadOnlyList<SelectListItem> FromRange(int start, int end, int step, string? format)
    {
        return BuildRange(start, end, step, format, null);
    }

    public static IReadOnlyList<SelectListItem> FromRange(int start, int end, int step, string? format, int selected)
    {
        return BuildRange(start, end, step, format, selected);
    }

    private static List<SelectListItem> BuildRange(int start, int end, int step, string? format, int? selected)
    {
        if (start > end)
        {
            return [];
        }

        var count = (int)Math.Min((((long)end - start) / step) + 1, Array.MaxLength);
        var list = new List<SelectListItem>(count);
        var value = (long)start;
        for (var i = 0; i < count; i++)
        {
            var text = ((int)value).ToString(format, CultureInfo.InvariantCulture);
            list.Add(new SelectListItem
            {
                Value = text,
                Text = text,
                Selected = value == selected
            });

            value += step;
        }

        return list;
    }
}
