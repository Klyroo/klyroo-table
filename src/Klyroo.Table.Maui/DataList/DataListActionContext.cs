namespace Klyroo.Table.Maui;

/// <summary>What an action receives when it runs: the rows it targets, the scope and the request that stands for "every result".</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListActionContext<TItem>
{
    internal DataListActionContext(DataListContext<TItem> list, DataListActionScope scope, IReadOnlyList<TItem> items, IReadOnlyList<string> keys, long? matchingCount, long targetCount, DataListRequest? request, CancellationToken cancellationToken)
    {
        TargetCount = targetCount;
        List = list;
        Scope = scope;
        SelectedItems = items;
        SelectedKeys = keys;
        MatchingCount = matchingCount;
        Request = request;
        CancellationToken = cancellationToken;
    }

    /// <summary>The list, to read its state or to load it again (<see cref="DataListContext{TItem}.RefreshAsync"/>).</summary>
    public DataListContext<TItem> List { get; }

    /// <summary>Whether the action targets the ticked rows or every result.</summary>
    public DataListActionScope Scope { get; }

    /// <summary>
    /// The rows the action targets. With <see cref="DataListActionScope.Selection"/>: the ticked rows. With <see cref="DataListActionScope.All"/> on a list of rows in memory:
    /// every row the search and the filters match. With <see cref="DataListActionScope.All"/> over a provider or a queryable: empty, because the list never holds those rows; apply <see cref="Request"/> instead.
    /// </summary>
    public IReadOnlyList<TItem> SelectedItems { get; }

    /// <summary>How many rows the action will act on: <see cref="MatchingCount"/> for <see cref="DataListActionScope.All"/> (when the source knows it), else the number of ticked rows.</summary>
    public long TargetCount { get; }

    /// <summary>The row keys (<c>RowKey</c> of the schema) of <see cref="SelectedItems"/>.</summary>
    public IReadOnlyList<string> SelectedKeys { get; }

    /// <summary>How many rows the search and the filters match, when the source says; <see langword="null"/> when it does not.</summary>
    public long? MatchingCount { get; }

    /// <summary>The request for every row the list matches: its search, filters and sort, with no paging and no facets. <see langword="null"/> before the first load.</summary>
    public DataListRequest? Request { get; }

    /// <summary>Cancelled when the list goes away.</summary>
    public CancellationToken CancellationToken { get; }
}
