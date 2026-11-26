using System.Globalization;
using System.Text;

namespace Klyroo.Table;

/// <summary>Which rows an export takes.</summary>
public enum DataListExportScope
{
    /// <summary>The rows on screen: the current page, or the rows loaded so far with cursor paging.</summary>
    Page,

    /// <summary>The ticked rows, including those ticked on other pages of a list of rows in memory.</summary>
    Selection,

    /// <summary>
    /// Every row the search and the filters match, on every page. A list of rows in memory holds them; a list over a provider or a queryable does not, so it needs a
    /// <c>LoadAll</c> function on the export part, which receives the request that stands for "every result".
    /// </summary>
    All,
}

/// <summary>How <see cref="DataListCsv.Write"/> writes a file.</summary>
public sealed record DataListCsvOptions
{
    /// <summary>The field delimiter. <see langword="null"/> (the default) follows <see cref="Culture"/>: <c>;</c> for a culture whose decimal separator is a comma (French, German), else <c>,</c>, so that a spreadsheet of that locale opens the file in columns.</summary>
    public char? Delimiter { get; init; }

    /// <summary>The culture of the numbers and dates, and of the choice of the delimiter. <see langword="null"/> uses the culture of the page, or the current culture where there is no page.</summary>
    public CultureInfo? Culture { get; init; }

    /// <summary>Starts the text with a byte order mark (U+FEFF, written as EF BB BF in UTF-8), which makes Excel read accents correctly. On by default, because Excel is where most exports are opened; turn it off for a consumer that does not skip the mark.</summary>
    public bool Utf8Bom { get; init; } = true;

    /// <summary>Writes the header line (true by default).</summary>
    public bool IncludeHeader { get; init; } = true;

    /// <summary>The end of a line. <c>\r\n</c> by default, as RFC 4180 says.</summary>
    public string NewLine { get; init; } = "\r\n";

    /// <summary>
    /// Keeps a spreadsheet from running a text as a formula (true by default): a text cell that starts with <c>=</c>, <c>+</c>, <c>-</c>, <c>@</c>, a tab or a carriage return
    /// gets an apostrophe in front. Cells that hold a number or a date (see <see cref="DataListCsvCell.Trusted"/>) are left alone, so that a negative number stays a number.
    /// </summary>
    public bool ProtectFormulas { get; init; } = true;
}

/// <summary>A cell of a CSV file: its text, and whether the text comes from a typed value that cannot be a formula.</summary>
/// <param name="Text">The text, already formatted with the culture of the file; <see langword="null"/> is an empty cell.</param>
/// <param name="Trusted">The text is the format of a number, a date or another typed value, not text somebody typed: it is never prefixed against formula injection.</param>
public readonly record struct DataListCsvCell(string? Text, bool Trusted = false)
{
    /// <summary>A text cell, subject to the formula protection.</summary>
    public static implicit operator DataListCsvCell(string? text) => new(text);
}

/// <summary>A CSV file as text, ready to download or hand to the app.</summary>
/// <param name="FileName">The suggested file name, with <c>.csv</c>.</param>
/// <param name="Content">The text, with the byte order mark when it was asked for.</param>
/// <param name="RowCount">How many data rows it holds (not counting the header).</param>
/// <param name="Scope">Which rows were exported.</param>
public sealed record DataListExportResult(string FileName, string Content, int RowCount, DataListExportScope Scope)
{
    /// <summary>The media type of the file.</summary>
    public string MimeType => "text/csv;charset=utf-8";

    /// <summary>The bytes to write to a file: UTF-8 of <see cref="Content"/>, without adding a byte order mark that <see cref="Content"/> does not hold.</summary>
    public byte[] ToBytes() => new UTF8Encoding(false).GetBytes(Content);
}

/// <summary>An export refused because it holds more rows than the limit.</summary>
public sealed class DataListExportTooLargeException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    /// <param name="count">How many rows the export would hold.</param>
    /// <param name="limit">The limit.</param>
    public DataListExportTooLargeException(long count, int limit)
        : base($"The export holds {count} rows, more than the limit of {limit}.")
    {
        Count = count;
        Limit = limit;
    }

    /// <summary>How many rows the export would hold.</summary>
    public long Count { get; }

    /// <summary>The limit that applied.</summary>
    public int Limit { get; }
}

/// <summary>A CSV writer: RFC 4180 quoting, a delimiter that fits the culture, and protection against formula injection.</summary>
public static class DataListCsv
{
    /// <summary>The delimiter a culture needs: <c>;</c> when its decimal separator is a comma, else <c>,</c>.</summary>
    /// <param name="culture">The culture.</param>
    public static char DelimiterFor(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return culture.NumberFormat.NumberDecimalSeparator == "," ? ';' : ',';
    }

    /// <summary>Writes a header line and rows as CSV text.</summary>
    /// <param name="header">The header cells (they are text, so they are protected like any other).</param>
    /// <param name="rows">The rows; each one should have as many cells as the header.</param>
    /// <param name="options">How to write it; <see langword="null"/> for the defaults.</param>
    public static string Write(IReadOnlyList<string> header, IEnumerable<IReadOnlyList<DataListCsvCell>> rows, DataListCsvOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(rows);
        options ??= new DataListCsvOptions();
        var delimiter = options.Delimiter ?? DelimiterFor(options.Culture ?? CultureInfo.CurrentCulture);
        var builder = new StringBuilder();
        if (options.Utf8Bom) builder.Append('﻿');
        if (options.IncludeHeader)
        {
            AppendLine(builder, header.Select(h => new DataListCsvCell(h)), delimiter, options);
        }
        foreach (var row in rows) AppendLine(builder, row, delimiter, options);
        return builder.ToString();
    }

    /// <summary>One field as it is written in the file: protected against formula injection when it is text and the protection is on, and quoted when it needs quotes.</summary>
    /// <param name="cell">The cell.</param>
    /// <param name="delimiter">The delimiter of the file.</param>
    /// <param name="protectFormulas">Whether to protect a text cell.</param>
    public static string Field(DataListCsvCell cell, char delimiter, bool protectFormulas = true)
    {
        var text = cell.Text ?? "";
        if (text.Length == 0) return "";
        if (protectFormulas && !cell.Trusted && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r') text = "'" + text;
        var quote = text.AsSpan().IndexOfAny(delimiter, '"') >= 0 || text.AsSpan().IndexOfAny('\r', '\n') >= 0 || char.IsWhiteSpace(text[0]) || char.IsWhiteSpace(text[^1]);
        return quote ? "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"" : text;
    }

    /// <summary>A file name for an export: <paramref name="name"/> without the characters Windows file names refuse, and <c>.csv</c>. <c>export.csv</c> when nothing is left.</summary>
    /// <param name="name">The name, with or without <c>.csv</c>.</param>
    public static string FileName(string? name)
    {
        // The characters Windows refuses too: a file saved on Linux or in a browser is often opened on Windows.
        const string invalid = "\\/:*?\"<>|";
        var cleaned = new string([.. (name ?? "").Where(c => !invalid.Contains(c, StringComparison.Ordinal) && !char.IsControl(c))]).Trim().Trim('.');
        if (cleaned.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) cleaned = cleaned[..^4].Trim();
        return (cleaned.Length == 0 ? "export" : cleaned) + ".csv";
    }

    private static void AppendLine(StringBuilder builder, IEnumerable<DataListCsvCell> cells, char delimiter, DataListCsvOptions options)
    {
        var first = true;
        foreach (var cell in cells)
        {
            if (!first) builder.Append(delimiter);
            first = false;
            builder.Append(Field(cell, delimiter, options.ProtectFormulas));
        }
        builder.Append(options.NewLine);
    }
}
