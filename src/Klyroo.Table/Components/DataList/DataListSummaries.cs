using System.Globalization;
using Klyroo.Query;

namespace Klyroo.Table;

/// <summary>Built-in column aggregation kinds for the summary footer (<c>&lt;tfoot&gt;</c>) of <see cref="KlyDataListTable{TItem}"/>.</summary>
public enum DataListSummaryKind
{
    /// <summary>No summary value in this column.</summary>
    None,

    /// <summary>Sum of numeric values in the column across the summary scope.</summary>
    Sum,

    /// <summary>Arithmetic mean of numeric values in the column across the summary scope.</summary>
    Average,

    /// <summary>Count of rows with a non-empty value in the column across the summary scope.</summary>
    Count,

    /// <summary>Minimum value in the column across the summary scope.</summary>
    Min,

    /// <summary>Maximum value in the column across the summary scope.</summary>
    Max,
}

/// <summary>Which set of rows a summary footer row aggregates.</summary>
public enum DataListSummaryScope
{
    /// <summary>Every row matching the active search and filters (or the loaded rows on screen for remote sources without <see cref="DataListSummaries{TItem}.ResolveAsync"/>).</summary>
    Filtered,

    /// <summary>The rows currently loaded on screen (<see cref="DataListContext{TItem}.Rows"/>).</summary>
    Page,

    /// <summary>The currently selected rows (<see cref="DataListContext{TItem}.SelectedItems"/>).</summary>
    Selection,
}

/// <summary>
/// The value of a summary footer cell: a number, which the column formats with its <c>SummaryFormat</c> or <c>Format</c> and the culture of the page, or a text shown as it is.
/// A <see cref="decimal"/>, an integer or a <see cref="string"/> converts (use <see cref="FromDouble"/> for a double) to it implicitly, so a resolver can write <c>["amount"] = 1234.5m</c>.
/// </summary>
public readonly record struct DataListSummaryValue
{
    private DataListSummaryValue(decimal? number, string? text)
    {
        Number = number;
        Text = text;
    }

    /// <summary>The value with nothing to show: the footer cell stays empty.</summary>
    public static DataListSummaryValue Empty => default;

    /// <summary>A number.</summary>
    /// <param name="number">The number.</param>
    public static DataListSummaryValue FromNumber(decimal number) => new(number, null);

    /// <summary>A number from a <see cref="double"/>; a value that is not finite or does not fit a decimal is <see cref="Empty"/>.</summary>
    /// <param name="number">The number.</param>
    public static DataListSummaryValue FromDouble(double number) =>
        double.IsFinite(number) && Math.Abs(number) < 7.9e28 ? new((decimal)number, null) : default;

    /// <summary>A text shown as it is; a null or empty text is <see cref="Empty"/>.</summary>
    /// <param name="text">The text.</param>
    public static DataListSummaryValue FromText(string? text) => string.IsNullOrEmpty(text) ? default : new(null, text);

    /// <summary>The number, or <see langword="null"/> when the value is a text or empty.</summary>
    public decimal? Number { get; }

    /// <summary>The text, or <see langword="null"/> when the value is a number or empty.</summary>
    public string? Text { get; }

    /// <summary>Whether there is nothing to show.</summary>
    public bool IsEmpty => Number is null && Text is null;

    /// <summary>The number in the invariant culture, or the text, or an empty string.</summary>
    public override string ToString() => Text ?? Number?.ToString(CultureInfo.InvariantCulture) ?? "";

    /// <summary>Converts a number.</summary>
    public static implicit operator DataListSummaryValue(decimal number) => FromNumber(number);

    /// <summary>Converts a number; a <see langword="null"/> is <see cref="Empty"/>.</summary>
    public static implicit operator DataListSummaryValue(decimal? number) => number is { } n ? FromNumber(n) : default;

    /// <summary>Converts a text.</summary>
    public static implicit operator DataListSummaryValue(string? text) => FromText(text);
}

/// <summary>Context passed to <see cref="KlyDataListColumnBase{TItem}.SummaryTemplate"/> when rendering a summary footer cell.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="List">The list the cell belongs to.</param>
/// <param name="ColumnKey">The key of the column of the cell.</param>
/// <param name="Scope">The set of rows the value aggregates.</param>
/// <param name="Rows">The rows of that scope.</param>
/// <param name="Value">The aggregated value: a number (formatted with the format of the column) or a text that is already formatted.</param>
/// <param name="FormattedValue">The value as text, with the format of the column and the culture of the page.</param>
/// <param name="IsLoading">Whether an asynchronous <see cref="DataListSummaries{TItem}.ResolveAsync"/> is still running for this scope.</param>
public sealed record DataListSummaryCellContext<TItem>(
    DataListContext<TItem> List,
    string? ColumnKey,
    DataListSummaryScope Scope,
    IReadOnlyList<TItem> Rows,
    DataListSummaryValue Value,
    string? FormattedValue,
    bool IsLoading);

/// <summary>Context passed to <see cref="DataListSummaries{TItem}.ResolveAsync"/> when resolving summary values asynchronously.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="List">The list the summary belongs to.</param>
/// <param name="Scope">The set of rows to aggregate.</param>
/// <param name="Rows">The rows of that scope that the list holds.</param>
/// <param name="Request">The request for every row the list matches, to send to a server that aggregates; <see langword="null"/> before the first load.</param>
/// <param name="CancellationToken">Cancelled when the list goes away or a newer resolution replaces this one.</param>
public sealed record DataListSummaryResolveContext<TItem>(
    DataListContext<TItem> List,
    DataListSummaryScope Scope,
    IReadOnlyList<TItem> Rows,
    DataListRequest? Request,
    CancellationToken CancellationToken);

/// <summary>
/// Footer summary configuration (<c>&lt;tfoot&gt;</c>) for <see cref="KlyDataListTable{TItem}"/>.
/// When omitted on <see cref="KlyDataListTable{TItem}"/>, a single summary row with <see cref="DataListSummaryScope.Filtered"/>
/// is still rendered automatically whenever at least one <see cref="KlyDataListColumnBase{TItem}"/> sets <see cref="KlyDataListColumnBase{TItem}.Summary"/>,
/// <see cref="KlyDataListColumn{TItem, TValue}.SummaryValue"/> or <see cref="KlyDataListColumnBase{TItem}.SummaryTemplate"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListSummaries<TItem>
{
    /// <summary>Creates a summary configuration with one row for the <see cref="DataListSummaryScope.Filtered"/> scope.</summary>
    public DataListSummaries() { }

    /// <summary>The default scope aggregated when <see cref="Scopes"/> is not set (<see cref="DataListSummaryScope.Filtered"/> by default).</summary>
    public DataListSummaryScope Scope { get; init; } = DataListSummaryScope.Filtered;

    /// <summary>
    /// Optional list of scopes to render as separate summary rows in <c>&lt;tfoot&gt;</c> (for example <c>[DataListSummaryScope.Page, DataListSummaryScope.Filtered]</c>).
    /// When <see langword="null"/> or empty, renders one row for <see cref="Scope"/>.
    /// </summary>
    public IReadOnlyList<DataListSummaryScope>? Scopes { get; init; }

    /// <summary>
    /// Label of the summary row (defaults to the localized <see cref="KlyToolsMessages.SummaryTotal"/> "Total" when a single scope is shown,
    /// or the localized scope name when multiple <see cref="Scopes"/> are shown).
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Optional asynchronous resolver returning a map of column key to aggregated value (<see cref="DataListSummaryValue"/>, converted implicitly from a number or a text) for a scope (for remote sources or custom async totals).
    /// Columns not present in the returned dictionary fall back to their declared <see cref="KlyDataListColumnBase{TItem}.Summary"/> or <see cref="KlyDataListColumn{TItem, TValue}.SummaryValue"/>.
    /// </summary>
    public Func<DataListSummaryResolveContext<TItem>, ValueTask<IReadOnlyDictionary<string, DataListSummaryValue>>>? ResolveAsync { get; init; }

    internal IReadOnlyList<DataListSummaryScope> EffectiveScopes =>
        Scopes is { Count: > 0 } ? Scopes.Distinct().ToArray() : [Scope];
}

/// <summary>Reads the typed value of a column from a row; <see langword="false"/> when there is none (a null along the path).</summary>
internal delegate bool ValueReader<in TItem, TValue>(TItem item, out TValue value);

internal static class DataListSummaryCalculator
{
    /// <summary>The summary of a column that reads a typed value from each row. A row the reader cannot read (a null along the path) does not count.</summary>
    public static DataListSummaryValue Compute<TItem, TValue>(
        DataListSummaryKind kind,
        IReadOnlyList<TItem> rows,
        ValueReader<TItem, TValue> read,
        string? format,
        CultureInfo culture)
    {
        switch (kind)
        {
            case DataListSummaryKind.None:
                return default;
            case DataListSummaryKind.Count:
            {
                var count = 0;
                foreach (var row in rows)
                {
                    if (read(row, out var value) && !IsBlank(value)) count++;
                }
                return count;
            }
            case DataListSummaryKind.Sum or DataListSummaryKind.Average:
            {
                var sum = 0m;
                var count = 0;
                foreach (var row in rows)
                {
                    if (read(row, out var value) && DataListNumbers.TryToDecimal(value, out var d))
                    {
                        sum += d;
                        count++;
                    }
                }
                if (count == 0) return kind == DataListSummaryKind.Sum ? DataListSummaryValue.FromNumber(0m) : default;
                return kind == DataListSummaryKind.Sum ? sum : sum / count;
            }
        }

        // Min and Max.
        var max = kind == DataListSummaryKind.Max;
        if (DataListNumbers.IsNumeric<TValue>())
        {
            decimal? best = null;
            foreach (var row in rows)
            {
                if (!read(row, out var value) || !DataListNumbers.TryToDecimal(value, out var d)) continue;
                if (best is null || (max ? d > best : d < best)) best = d;
            }
            return best;
        }

        var comparer = Comparer<TValue>.Default;
        var comparable = DataListNumbers.IsComparable<TValue>();
        var found = false;
        TValue winner = default!;
        foreach (var row in rows)
        {
            if (!read(row, out var value) || IsBlank(value)) continue;
            if (!found)
            {
                winner = value;
                found = true;
            }
            else if (comparable)
            {
                var order = comparer.Compare(value, winner);
                if (max ? order > 0 : order < 0) winner = value;
            }
        }
        return found ? DataListSummaryValue.FromText(FormatText(winner, format, culture)) : default;
    }

    /// <summary>The summary of a column that reads a field of the schema by its key.</summary>
    public static DataListSummaryValue ComputeField<TItem>(
        DataListSummaryKind kind,
        IReadOnlyList<TItem> rows,
        DataListSchema<TItem> schema,
        string key,
        string? format,
        CultureInfo culture)
    {
        switch (kind)
        {
            case DataListSummaryKind.None:
                return default;
            case DataListSummaryKind.Count:
            {
                var count = 0;
                foreach (var row in rows)
                {
                    if (!string.IsNullOrWhiteSpace(schema.FormatField(row, key, null, CultureInfo.InvariantCulture))) count++;
                }
                return count;
            }
            case DataListSummaryKind.Sum or DataListSummaryKind.Average:
            {
                var sum = 0m;
                var count = 0;
                foreach (var row in rows)
                {
                    if (schema.TryReadNumber(row, key, out var d))
                    {
                        sum += d;
                        count++;
                    }
                }
                if (count == 0) return kind == DataListSummaryKind.Sum ? DataListSummaryValue.FromNumber(0m) : default;
                return kind == DataListSummaryKind.Sum ? sum : sum / count;
            }
        }

        var max = kind == DataListSummaryKind.Max;
        var numeric = false;
        decimal? bestNumber = null;
        TItem? bestRow = default;
        var found = false;
        foreach (var row in rows)
        {
            if (schema.TryReadNumber(row, key, out var d))
            {
                numeric = true;
                if (bestNumber is null || (max ? d > bestNumber : d < bestNumber)) bestNumber = d;
                continue;
            }
            if (string.IsNullOrWhiteSpace(schema.FormatField(row, key, null, CultureInfo.InvariantCulture))) continue;
            if (!found)
            {
                bestRow = row;
                found = true;
                continue;
            }
            var order = schema.CompareField(row, bestRow!, key);
            if (max ? order > 0 : order < 0) bestRow = row;
        }
        if (numeric) return bestNumber;
        return found ? DataListSummaryValue.FromText(schema.FormatField(bestRow!, key, format, culture)) : default;
    }

    /// <summary>A custom aggregation result as a summary value: a number stays a number, anything else becomes text with the format.</summary>
    public static DataListSummaryValue FromTyped<TValue>(TValue value, string? format, CultureInfo culture)
    {
        if (DataListNumbers.TryToDecimal(value, out var number)) return number;
        return IsBlank(value) ? default : DataListSummaryValue.FromText(FormatText(value, format, culture));
    }

    private static bool IsBlank<TValue>(TValue value) => value is null or string { Length: 0 } || (value is string s && string.IsNullOrWhiteSpace(s));

    private static string? FormatText<TValue>(TValue value, string? format, CultureInfo culture) => value switch
    {
        null => null,
        IFormattable formattable => formattable.ToString(string.IsNullOrEmpty(format) ? null : format, culture),
        _ => value.ToString(),
    };
}

/// <summary>Reads a number of any numeric type as a decimal, without asking the caller to box it into an <see cref="object"/>.</summary>
internal static class DataListNumbers
{
    private static class Kind<T>
    {
        public static readonly bool Numeric = IsNumericType(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    private static bool IsNumericType(Type type) =>
        type == typeof(decimal) || type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(sbyte)
        || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(double) || type == typeof(float);

    public static bool IsNumeric<T>() => Kind<T>.Numeric;

    /// <summary>Whether values of type <typeparamref name="T"/> (or the type under a nullable) have a natural order: they implement <see cref="IComparable"/>, as the numbers, dates, strings and enums do.</summary>
    public static bool IsComparable<T>() => Orderable<T>.Value;

    private static class Orderable<T>
    {
        public static readonly bool Value = Check(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));

        private static bool Check(Type type) => typeof(IComparable).IsAssignableFrom(type);
    }

    /// <summary>Whether the values of type <typeparamref name="T"/> are numbers, dates, times, GUIDs or booleans: the type writes their text, a person does not.</summary>
    public static bool IsStructured<T>() => Structured<T>.Value;

    private static class Structured<T>
    {
        public static readonly bool Value = Check(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));

        private static bool Check(Type type) =>
            type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint)
            || type == typeof(long) || type == typeof(ulong) || type == typeof(nint) || type == typeof(nuint) || type == typeof(float) || type == typeof(double)
            || type == typeof(decimal) || type == typeof(Int128) || type == typeof(UInt128) || type == typeof(DateTime) || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan) || type == typeof(Guid) || type == typeof(bool);
    }

    /// <summary>Whether <paramref name="value"/> is a number: a double that is not finite or does not fit a decimal is not.</summary>
    public static bool TryToDecimal<T>(T value, out decimal result)
    {
        result = 0m;
        if (!Kind<T>.Numeric || value is null) return false;
        switch (value)
        {
            case decimal d: result = d; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case short s: result = s; return true;
            case byte b: result = b; return true;
            case sbyte sb: result = sb; return true;
            case uint ui: result = ui; return true;
            case ulong ul: result = ul; return true;
            case ushort us: result = us; return true;
            case double db when double.IsFinite(db) && Math.Abs(db) < 7.9e28: result = (decimal)db; return true;
            case float fl when float.IsFinite(fl) && Math.Abs(fl) < 7.9e28f: result = (decimal)fl; return true;
            default: return false;
        }
    }
}
