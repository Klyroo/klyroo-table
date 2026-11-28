using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>Small value helpers of the data list.</summary>
internal static class TableValues
{
    /// <summary>A key written as words: <c>firstName</c> becomes "First name", <c>department.name</c> becomes "Name" (the last segment).</summary>
    public static string Humanize(string key)
    {
        if (key.Length == 0) return "";
        var segment = key[(key.LastIndexOf('.') + 1)..];
        var words = new System.Text.StringBuilder(segment.Length + 4);
        for (var i = 0; i < segment.Length; i++)
        {
            var c = segment[i];
            if (c is '_' or '-')
            {
                words.Append(' ');
                continue;
            }
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(segment[i - 1]) && segment[i - 1] is not ('_' or '-')) words.Append(' ');
            words.Append(i == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
        }
        return words.ToString();
    }

    /// <summary>Whether the type writes its own text, so that a CSV export does not guard it against a formula: numbers, booleans, dates, times and GUIDs.</summary>
    public static bool IsStructured(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsEnum || type == typeof(bool) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan)
            || Type.GetTypeCode(type) is >= TypeCode.SByte and <= TypeCode.Decimal;
    }

    /// <summary>Whether a value writes its own text: the type decides, and for <see cref="object"/> the runtime value.</summary>
    public static bool IsStructured(object? value, Type declared) =>
        declared == typeof(object) ? value is not null && IsStructured(value.GetType()) : IsStructured(declared);

    /// <summary>The text of a value with the format of a column and the culture of the user.</summary>
    public static string? FormatValue(object? value, string? format, CultureInfo culture) => value switch
    {
        null => null,
        IFormattable formattable => formattable.ToString(string.IsNullOrEmpty(format) ? null : format, culture),
        _ => value.ToString(),
    };
}
