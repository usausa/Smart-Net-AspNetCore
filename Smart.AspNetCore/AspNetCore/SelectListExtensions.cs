namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Mvc.Rendering;

public static class SelectListExtensions
{
    //--------------------------------------------------------------------------------
    // Projection
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<SelectListItem> ToSelectList<T>(
        this IEnumerable<T>? source,
        Func<T, string> valueSelector,
        Func<T, string> textSelector,
        string? selectedValue = null)
    {
        if (source is null)
        {
            return [];
        }

        var capacity = source.TryGetNonEnumeratedCount(out var count) ? count : 0;
        var list = new List<SelectListItem>(capacity);
        foreach (var element in source)
        {
            var value = valueSelector(element);
            list.Add(new SelectListItem
            {
                Value = value,
                Text = textSelector(element),
                Selected = (selectedValue is not null) && String.Equals(value, selectedValue, StringComparison.Ordinal)
            });
        }

        return list;
    }

    //--------------------------------------------------------------------------------
    // String
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<SelectListItem> ToSelectList(this IEnumerable<string>? source, string? selectedValue = null)
    {
        return ToSelectList(source, static x => x, static x => x, selectedValue);
    }

    //--------------------------------------------------------------------------------
    // Empty
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<SelectListItem> WithEmpty(this IReadOnlyList<SelectListItem> items)
    {
        return WithEmpty(items, string.Empty, string.Empty);
    }

    public static IReadOnlyList<SelectListItem> WithEmpty(this IReadOnlyList<SelectListItem> items, string text)
    {
        return WithEmpty(items, text, string.Empty);
    }

    public static IReadOnlyList<SelectListItem> WithEmpty(this IReadOnlyList<SelectListItem> items, string text, string value)
    {
        var list = new List<SelectListItem>(items.Count + 1)
        {
            new() { Value = value, Text = text }
        };
        list.AddRange(items);

        return list;
    }
}
