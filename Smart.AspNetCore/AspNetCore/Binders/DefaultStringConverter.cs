namespace Smart.AspNetCore.Binders;

using System;

public static class DefaultStringConverter
{
    public static bool ToBoolean(ReadOnlySpan<char> value) =>
        Boolean.TryParse(value, out var result) ? result : default;

    public static byte ToByte(ReadOnlySpan<char> value) =>
        Byte.TryParse(value, out var result) ? result : default;

    public static sbyte ToSByte(ReadOnlySpan<char> value) =>
        SByte.TryParse(value, out var result) ? result : default;

    public static short ToInt16(ReadOnlySpan<char> value) =>
        Int16.TryParse(value, out var result) ? result : default;

    public static ushort ToUInt16(ReadOnlySpan<char> value) =>
        UInt16.TryParse(value, out var result) ? result : default;

    public static int ToInt32(ReadOnlySpan<char> value) =>
        Int32.TryParse(value, out var result) ? result : default;

    public static uint ToUInt32(ReadOnlySpan<char> value) =>
        UInt32.TryParse(value, out var result) ? result : default;

    public static long ToInt64(ReadOnlySpan<char> value) =>
        Int64.TryParse(value, out var result) ? result : default;

    public static ulong ToUInt64(ReadOnlySpan<char> value) =>
        UInt64.TryParse(value, out var result) ? result : default;

    public static float ToSingle(ReadOnlySpan<char> value) =>
        Single.TryParse(value, out var result) ? result : default;

    public static double ToDouble(ReadOnlySpan<char> value) =>
        Double.TryParse(value, out var result) ? result : default;

    public static decimal ToDecimal(ReadOnlySpan<char> value) =>
        Decimal.TryParse(value, out var result) ? result : default;

    public static char ToChar(ReadOnlySpan<char> value) =>
        value.Length == 1 ? value[0] : default;

    public static DateTime ToDateTime(ReadOnlySpan<char> value) =>
        DateTime.TryParse(value, out var result) ? result : default;

    public static DateTimeOffset ToDateTimeOffset(ReadOnlySpan<char> value) =>
        DateTimeOffset.TryParse(value, out var result) ? result : default;

    public static DateOnly ToDateOnly(ReadOnlySpan<char> value) =>
        DateOnly.TryParse(value, out var result) ? result : default;

    public static TimeOnly ToTimeOnly(ReadOnlySpan<char> value) =>
        TimeOnly.TryParse(value, out var result) ? result : default;

    public static TimeSpan ToTimeSpan(ReadOnlySpan<char> value) =>
        TimeSpan.TryParse(value, out var result) ? result : default;

    public static Guid ToGuid(ReadOnlySpan<char> value) =>
        Guid.TryParse(value, out var result) ? result : default;

    public static T ToEnum<T>(ReadOnlySpan<char> value)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : default;
}
