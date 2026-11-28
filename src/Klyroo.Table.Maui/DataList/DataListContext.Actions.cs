using System.Globalization;

namespace Klyroo.Table.Maui;

public sealed partial class DataListContext<TItem>
{
    /// <summary>
    /// Whether the list holds every row its query matches (a list of rows in memory, <c>Items</c>), so that <see cref="ExportRowsAsync"/> can return them for <see cref="DataListExportScope.All"/> (<see cref="ExportRowsAsync"/>).
    /// A list over a provider or a queryable does not: export those rows from the request in <see cref="AllResultsRequest"/>.
    /// </summary>
    public bool HoldsAllRows => _owner.Items is not null;

    /// <summary>The request for every row the list matches (the loaded request without paging or facets), or <see langword="null"/> before the first load.</summary>
    public DataListRequest? AllResultsRequest =>
        Controller?.LoadedRequest is { } request ? request with { Paging = DataListPaging.None.Instance, Facets = [] } : null;

    /// <summary>Every row the search and the filters match for a list of rows in memory (<c>Items</c>), or the loaded <see cref="Rows"/> for a provider or queryable source.</summary>
    public async Task<IReadOnlyList<TItem>> MatchingRowsAsync(CancellationToken cancellationToken = default)
    {
        if (!HoldsAllRows || _schema is null || AllResultsRequest is not { } request) return Rows;
        var provider = _schema.InMemoryProvider(_owner.Items!);
        var result = await provider(request, cancellationToken).ConfigureAwait(true);
        return result.Items;
    }

    /// <summary>What a bulk action receives: the ticked rows, or for the scope <see cref="DataListActionScope.All"/> every matching row of a list in memory.</summary>
    /// <param name="scope">The scope of the action.</param>
    /// <param name="resolveAll">Load every matching row of a list in memory (done when the action runs, not to describe it).</param>
    /// <param name="cancellationToken">Cancelled when the list goes away.</param>
    internal async Task<DataListActionContext<TItem>> CreateActionContextAsync(DataListActionScope scope, bool resolveAll, CancellationToken cancellationToken)
    {
        var matching = MatchingCount;
        if (scope == DataListActionScope.All)
        {
            IReadOnlyList<TItem> items = resolveAll && HoldsAllRows ? await MatchingRowsAsync(cancellationToken).ConfigureAwait(true) : [];
            IReadOnlyList<string> keys = _schema is { HasRowKey: true } ? [.. items.Select(_schema.GetRowKey)] : [];
            return new DataListActionContext<TItem>(this, scope, items, keys, matching, matching ?? items.Count, AllResultsRequest, cancellationToken);
        }
        var selected = SelectedItems;
        return new DataListActionContext<TItem>(this, scope, selected, [.. SelectedKeys], matching, _selection.Count, AllResultsRequest, cancellationToken);
    }

    /// <summary>The same as <see cref="CreateActionContextAsync"/> without loading anything: what <c>Visible</c> and <c>Disabled</c> of an action read.</summary>
    internal DataListActionContext<TItem> CreateActionContext(DataListActionScope scope)
    {
        var matching = MatchingCount;
        var selected = SelectedItems;
        return new DataListActionContext<TItem>(this, scope, scope == DataListActionScope.All ? [] : selected, [.. SelectedKeys], matching,
            scope == DataListActionScope.All ? matching ?? _selection.Count : _selection.Count, AllResultsRequest, LifetimeToken);
    }

    /// <summary>
    /// The rows an export takes: <see cref="Rows"/> for <see cref="DataListExportScope.Page"/>, the ticked rows for <see cref="DataListExportScope.Selection"/>, and for
    /// <see cref="DataListExportScope.All"/> every row the search and the filters match when <see cref="HoldsAllRows"/> (empty otherwise).
    /// </summary>
    /// <param name="scope">Which rows.</param>
    /// <param name="cancellationToken">Cancels loading the rows.</param>
    public async Task<IReadOnlyList<TItem>> ExportRowsAsync(DataListExportScope scope, CancellationToken cancellationToken = default) => scope switch
    {
        DataListExportScope.Selection => [.. SelectedItems],
        DataListExportScope.All => HoldsAllRows ? await MatchingRowsAsync(cancellationToken).ConfigureAwait(true) : [],
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
    /// <param name="options">How to write the file. Its culture defaults to the current culture.</param>
    public string ToCsv(IEnumerable<TItem> rows, DataListCsvOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        options ??= new DataListCsvOptions();
        var culture = options.Culture ?? CultureInfo.CurrentCulture;
        var columns = ExportColumns();
        var lines = rows.Select(item => (IReadOnlyList<DataListCsvCell>)[.. columns.Select(c => c.Cell(item, culture))]);
        return DataListCsv.Write([.. columns.Select(c => c.Title)], lines, options with { Culture = culture });
    }

    private sealed record ExportColumn(string Title, Func<TItem, CultureInfo, DataListCsvCell> Cell);

    private List<ExportColumn> ExportColumns()
    {
        var schema = _schema;
        if (_columns.Count > 0)
        {
            return [.. TableColumns().Where(c => c.CanExport(schema)).Select(c => new ExportColumn(c.ResolvedTitle, (item, culture) => c.ReadCell(item, schema, culture)))];
        }
        if (schema is null) return [];
        return [.. schema.FieldInfos.Where(f => !f.IsCollection).Select(f => new ExportColumn(TableValues.Humanize(f.Key), (item, culture) =>
            new DataListCsvCell(schema.FormatField(item, f.Key, null, culture), schema.HasStructuredValueFor(item, f.Key))))];
    }
}
