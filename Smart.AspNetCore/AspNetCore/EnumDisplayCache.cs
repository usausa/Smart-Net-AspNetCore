namespace Smart.AspNetCore;

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

internal readonly record struct EnumDisplayEntry<T>(T Value, string Name, string Text)
    where T : struct, Enum;

internal static class EnumDisplayCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>
    where T : struct, Enum
{
    public static EnumDisplayEntry<T>[] Entries { get; } = CreateEntries();

    private static readonly FrozenDictionary<T, string> TextMap = CreateTextMap(Entries);

    public static string GetText(T value)
    {
        return TextMap.TryGetValue(value, out var text) ? text : value.ToString();
    }

    private static EnumDisplayEntry<T>[] CreateEntries()
    {
        var values = Enum.GetValues<T>();
        var names = Enum.GetNames<T>();

        var entries = new EnumDisplayEntry<T>[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            entries[i] = new EnumDisplayEntry<T>(values[i], names[i], ResolveText(names[i]));
        }

        return entries;
    }

    private static FrozenDictionary<T, string> CreateTextMap(EnumDisplayEntry<T>[] entries)
    {
        var map = new Dictionary<T, string>(entries.Length);
        foreach (var entry in entries)
        {
            map.TryAdd(entry.Value, entry.Text);
        }

        return map.ToFrozenDictionary();
    }

    private static string ResolveText(string name)
    {
        var field = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? name;
    }
}
