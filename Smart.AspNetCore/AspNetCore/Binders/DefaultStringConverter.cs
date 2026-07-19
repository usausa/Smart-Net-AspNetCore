namespace Smart.AspNetCore.Binders;

using System;
using System.Globalization;

public static class DefaultStringConverter
{
    // Strict

    public static bool ToBoolean(ReadOnlySpan<char> value) =>
        Boolean.TryParse(value, out var result) && result;

    public static byte ToByte(ReadOnlySpan<char> value) =>
        Byte.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static sbyte ToSByte(ReadOnlySpan<char> value) =>
        SByte.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static short ToInt16(ReadOnlySpan<char> value) =>
        Int16.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static ushort ToUInt16(ReadOnlySpan<char> value) =>
        UInt16.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static int ToInt32(ReadOnlySpan<char> value) =>
        Int32.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static uint ToUInt32(ReadOnlySpan<char> value) =>
        UInt32.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static long ToInt64(ReadOnlySpan<char> value) =>
        Int64.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static ulong ToUInt64(ReadOnlySpan<char> value) =>
        UInt64.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static float ToSingle(ReadOnlySpan<char> value) =>
        Single.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static double ToDouble(ReadOnlySpan<char> value) =>
        Double.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static decimal ToDecimal(ReadOnlySpan<char> value) =>
        Decimal.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static char ToChar(ReadOnlySpan<char> value) =>
        value.Length == 1 ? value[0] : default;

    public static DateTime ToDateTime(ReadOnlySpan<char> value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static DateTimeOffset ToDateTimeOffset(ReadOnlySpan<char> value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static DateOnly ToDateOnly(ReadOnlySpan<char> value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static TimeOnly ToTimeOnly(ReadOnlySpan<char> value) =>
        TimeOnly.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static TimeSpan ToTimeSpan(ReadOnlySpan<char> value) =>
        TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : default;

    public static Guid ToGuid(ReadOnlySpan<char> value) =>
        Guid.TryParse(value, out var result) ? result : default;

    public static T ToEnum<T>(ReadOnlySpan<char> value)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : default;

    // Try

    public static bool TryToBoolean(ReadOnlySpan<char> value, out bool result) =>
        Boolean.TryParse(value, out result);

    public static bool TryToByte(ReadOnlySpan<char> value, out byte result) =>
        Byte.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToSByte(ReadOnlySpan<char> value, out sbyte result) =>
        SByte.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToInt16(ReadOnlySpan<char> value, out short result) =>
        Int16.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToUInt16(ReadOnlySpan<char> value, out ushort result) =>
        UInt16.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToInt32(ReadOnlySpan<char> value, out int result) =>
        Int32.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToUInt32(ReadOnlySpan<char> value, out uint result) =>
        UInt32.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToInt64(ReadOnlySpan<char> value, out long result) =>
        Int64.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToUInt64(ReadOnlySpan<char> value, out ulong result) =>
        UInt64.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToSingle(ReadOnlySpan<char> value, out float result) =>
        Single.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToDouble(ReadOnlySpan<char> value, out double result) =>
        Double.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToDecimal(ReadOnlySpan<char> value, out decimal result) =>
        Decimal.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToChar(ReadOnlySpan<char> value, out char result)
    {
        if (value.Length == 1)
        {
            result = value[0];
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryToDateTime(ReadOnlySpan<char> value, out DateTime result) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToDateTimeOffset(ReadOnlySpan<char> value, out DateTimeOffset result) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToDateOnly(ReadOnlySpan<char> value, out DateOnly result) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToTimeOnly(ReadOnlySpan<char> value, out TimeOnly result) =>
        TimeOnly.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToTimeSpan(ReadOnlySpan<char> value, out TimeSpan result) =>
        TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result);

    public static bool TryToGuid(ReadOnlySpan<char> value, out Guid result) =>
        Guid.TryParse(value, out result);

    public static bool TryToEnum<T>(ReadOnlySpan<char> value, out T result)
        where T : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out result);
}
