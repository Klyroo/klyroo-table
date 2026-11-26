using System.Globalization;
using Klyroo.Query;

namespace Klyroo.Table;

public sealed partial class DataListContext<TItem>
{
    /// <summary>
    /// Whether the list holds every row its query matches (a list of rows in memory, <c>Items</c>), so that <see cref="ExportRows"/> can return them for <see cref="DataListExportScope.All"/>.
    /// A list over a provider or a queryable does not: export those rows from the request in <see cref="AllResultsRequest"/>.
    /// </summary>
    public bool HoldsAllRows => _owner.HoldsAllRows;

    /// <summary>
    /// The rows an export takes: <see cref="Rows"/> for <see cref="DataListExportScope.Page"/>, the ticked rows for <see cref="DataListExportScope.Selection"/>, and for
    /// <see cref="DataListExportScope.All"/> every row the search and the filters match when <see cref="HoldsAllRows"/> (empty otherwise).
    /// </summary>
    /// <param name="scope">Which rows.</param>
    public IReadOnlyList<TItem> ExportRows(DataListExportScope scope) => scope switch
    {
        DataListExportScope.Selection => [.. SelectedItems],
        DataListExportScope.All => HoldsAllRows ? CreateActionContext(DataListActionScope.All, resolveAll: true, LifetimeToken).SelectedItems : [],
        _ => Rows,
    };

    /// <summary>
    /// The columns an export writes: the visible columns of the table in the order the user sees them (pinned ones first and last, hidden ones left out), without the columns that only have a template
    /// (they have no value to write), or without any declared column the fields of the schema that are not collections. Each has the title of the header.
    /// </summary>
    public IReadOnlyList<string> ExportHeaders() => [.. ExportColumns().Select(c => c.Title)];

    /// <summary>
    /// Writes <paramref name="rows"/> as CSV: one cell per <see cref="ExportHeaders"/> entry, formatted like the table shows it (the <c>Format</c> of the column, the culture of the file). A cell that is
    /// text is protected against formula injection (see <see cref="DataListCsvOptions.ProtectFormulas"/>); numbers and dates are not, so a negative number stays a number.
    /// </summary>
    /// <param name="rows">The rows to write.</param>
    /// <param name="options">How to write the file. Its culture defaults to the current culture; a component passes the culture of the page.</param>
    public string ToCsv(IEnumerable<TItem> rows, DataListCsvOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        options ??= new DataListCsvOptions();
        var culture = options.Culture ?? CultureInfo.CurrentCulture;
        var columns = ExportColumns();
        var lines = rows.Select(item => (IReadOnlyList<DataListCsvCell>)[.. columns.Select(c => c.Cell(item, culture))]);
        return DataListCsv.Write([.. columns.Select(c => c.Title)], lines, options with { Culture = culture });
    }

    /// <summary>
    /// Writes the rows of <paramref name="scope"/> as CSV, see <see cref="ExportRows"/> and <see cref="ToCsv"/>. For <see cref="DataListExportScope.All"/> over a provider or a queryable, load the
    /// rows from <see cref="AllResultsRequest"/> and pass them to <see cref="ToCsv"/>.
    /// </summary>
    /// <param name="scope">Which rows.</param>
    /// <param name="options">How to write the file.</param>
    public string ExportCsv(DataListExportScope scope, DataListCsvOptions? options = null) => ToCsv(ExportRows(scope), options);

    private sealed record ExportColumn(string Title, Func<TItem, CultureInfo, DataListCsvCell> Cell);

    private List<ExportColumn> ExportColumns()
    {
        var schema = Schema;
        if (Columns.Count > 0)
        {
            // A column exports what it reads: its Property or Field, or for a column that only has a template the schema field its key names. The Format of the column applies in every case.
            return [.. _owner.TableColumns()
                .Where(c => c.Property is not null || c.Field is not null || (c.ResolvedKey is { } k && schema?.FieldInfos.Any(f => f.Key == k && !f.IsCollection) == true))
                .Select(c => new ExportColumn(c.ResolvedTitle, (item, culture) =>
                {
                    var value = c.Property is not null || c.Field is not null ? c.ReadValue(item, schema) : schema!.ReadField(item, c.ResolvedKey!);
                    return new DataListCsvCell(c.FormatValue(value, culture), IsTyped(value));
                }))];
        }
        if (schema is null) return [];
        return [.. schema.FieldInfos.Where(f => !f.IsCollection).Select(f => new ExportColumn(DataListText.Humanize(f.Key), (item, culture) =>
        {
            var value = schema.ReadField(item, f.Key);
            return new DataListCsvCell(value is IFormattable formattable ? formattable.ToString(null, culture) : value?.ToString(), IsTyped(value));
        }))];
    }

    // Only the values of these types are exempt from the formula guard: their text cannot be one. Any other type (an enum, a custom IFormattable, a string) is text somebody could have typed.
    private static bool IsTyped(object? value) => value is sbyte or byte or short or ushort or int or uint or long or ulong or nint or nuint or float or double or decimal or Int128 or UInt128
        or DateTime or DateTimeOffset or DateOnly or TimeOnly or TimeSpan or Guid or bool;
}
