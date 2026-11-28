using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// What the parts of a <see cref="KlyDataList{TItem}"/> (the table, the pagination, the result count...) read and change: the state of the list, its
/// rows and its loading flags. The list cascades one for its whole life (the value is fixed), so a part subscribes to <see cref="Changed"/> once and renders
/// again when it is raised. Write your own part by taking it as a cascading parameter.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed partial class DataListContext<TItem>
{
    private readonly KlyDataList<TItem> _owner;

    internal DataListContext(KlyDataList<TItem> owner) => _owner = owner;

    /// <summary>Raised when the state, the rows or a flag change. It can run on any thread: from a component, go through <c>InvokeAsync</c> before rendering.</summary>
    public event Action? Changed;

    internal void NotifyChanged() => Changed?.Invoke();

    internal Task EnsureLoadedAsync() => _owner.EnsureLoadedAsync();

    internal CancellationToken LifetimeToken => _owner.LifetimeToken;

    internal Microsoft.Extensions.Logging.ILogger? Logger => _owner.Logger;

    private object? _footerOwner;
    private int _tableCount;
    private int _gridCount;

    /// <summary>Whether the list contains a <see cref="KlyDataListTable{TItem}"/>.</summary>
    internal bool HasTable => _tableCount > 0;

    /// <summary>Whether the list contains a <see cref="KlyDataListGrid{TItem}"/>.</summary>
    internal bool HasGrid => _gridCount > 0;

    /// <summary>The layout currently shown: <see cref="DataListState.Layout"/> when both table and grid exist, otherwise the one that is present.</summary>
    public DataListLayout EffectiveLayout =>
        HasGrid && !HasTable ? DataListLayout.Grid
        : HasTable && !HasGrid ? DataListLayout.Table
        : State.Layout;

    /// <summary>Whether the table has a <see cref="DataListRowSummary{TItem}"/> configured.</summary>
    public bool HasRowSummary => _owner.HasRowSummary;

    /// <summary>Whether the table currently renders compact summary rows instead of the multi-column table.</summary>
    public bool SummaryViewActive => EffectiveLayout == DataListLayout.Table && _owner.SummaryViewActive;

    /// <summary>Switches between compact summary rows (<see langword="true"/>) and the full table (<see langword="false"/>), persisting the choice when <c>PreferencesKey</c> is set.</summary>
    public void SetSummaryView(bool active) => _owner.SetSummaryView(active);

    internal void SetRowSummary(bool enabled, bool defaultActive, bool autoPhone) => _owner.SetRowSummary(enabled, defaultActive, autoPhone);

    internal void SetDetectedPhoneSummary(bool isPhone) => _owner.SetDetectedPhoneSummary(isPhone);

    /// <summary>The active grouping configuration for the current layout (<see cref="KlyDataListTable{TItem}.GroupBy"/> or <see cref="KlyDataListGrid{TItem}.GroupBy"/>), or <see langword="null"/>.</summary>
    public DataListGroupBy<TItem>? ActiveGroupBy => _owner.ActiveGroupBy;

    /// <summary>Builds the visual groups of <see cref="Rows"/> for the active layout when <see cref="ActiveGroupBy"/> is configured; empty otherwise.</summary>
    public IReadOnlyList<DataListGroup<TItem>> Groups() => _owner.BuildActiveGroups();

    /// <summary>Whether <paramref name="group"/> is currently collapsed (folded).</summary>
    public bool IsGroupCollapsed(DataListGroup<TItem> group) => _owner.IsGroupCollapsed(group.Key, group.DefaultCollapsed);

    /// <summary>Folds or unfolds <paramref name="group"/>, persisting the toggle when <c>PreferencesKey</c> is set.</summary>
    public void ToggleGroup(DataListGroup<TItem> group) => _owner.ToggleGroup(group.Key);

    /// <summary>Sets whether <paramref name="group"/> is collapsed (folded), persisting the toggle when <c>PreferencesKey</c> is set.</summary>
    public void SetGroupCollapsed(DataListGroup<TItem> group, bool collapsed) => _owner.SetGroupCollapsed(group.Key, group.DefaultCollapsed, collapsed);

    internal void SetTableGroupBy(DataListGroupBy<TItem>? groupBy) => _owner.SetTableGroupBy(groupBy);

    internal void SetGridGroupBy(DataListGroupBy<TItem>? groupBy) => _owner.SetGridGroupBy(groupBy);

    /// <summary>The active hierarchical tree configuration on <see cref="KlyDataListTable{TItem}"/>, or <see langword="null"/>.</summary>
    public DataListTree<TItem>? Tree => _owner.TableTree;

    /// <summary>Whether the table has a <see cref="DataListTree{TItem}"/> configured.</summary>
    public bool HasTree => _owner.HasTree;

    /// <summary>The visible (unfolded) tree nodes in display order when <see cref="Tree"/> is configured; empty otherwise.</summary>
    public IReadOnlyList<DataListTreeNode<TItem>> VisibleTreeNodes() => _owner.VisibleTreeNodes();

    /// <summary>Whether the table shows a detail row under each row (<see cref="KlyDataListTable{TItem}.DetailTemplate"/>). Its rows are opened and closed with <see cref="SetRowExpanded"/>.</summary>
    public bool HasDetail => _owner.HasDetail;

    /// <summary>Whether <paramref name="node"/> is currently expanded.</summary>
    public bool IsNodeExpanded(DataListTreeNode<TItem> node) => _owner.IsTreeNodeExpanded(node.Key, node.Depth);

    /// <summary>
    /// Whether the row with <paramref name="key"/> is currently expanded: for a tree the row at <paramref name="depth"/> shows its children, for a table with a
    /// <see cref="KlyDataListTable{TItem}.DetailTemplate"/> (<paramref name="depth"/> stays 0) it shows its detail row. Always <see langword="false"/> when the table has neither.
    /// </summary>
    public bool IsRowExpanded(string key, int depth = 0) => _owner.IsTreeNodeExpanded(key, depth);

    /// <summary>Expands or collapses <paramref name="node"/>.</summary>
    public void ToggleNodeExpanded(DataListTreeNode<TItem> node) => _owner.ToggleTreeNodeExpanded(node.Key, node.Depth);

    /// <summary>Sets whether <paramref name="node"/> is expanded.</summary>
    public void SetNodeExpanded(DataListTreeNode<TItem> node, bool expanded) => _owner.SetTreeNodeExpanded(node.Key, node.Depth, expanded);

    /// <summary>Sets whether the row with <paramref name="key"/> is expanded: its children in a tree, its detail row in a table with a <see cref="KlyDataListTable{TItem}.DetailTemplate"/>. Nothing happens when the table has neither.</summary>
    public void SetRowExpanded(string key, bool expanded, int depth = 0) => _owner.SetTreeNodeExpanded(key, depth, expanded);

    /// <summary>Expands every branch node in the tree.</summary>
    public void ExpandAll() => _owner.ExpandAllTreeNodes();

    /// <summary>Collapses every branch node in the tree.</summary>
    public void CollapseAll() => _owner.CollapseAllTreeNodes();

    /// <summary>Resets tree expansion back to the <see cref="DataListTree{TItem}"/> defaults.</summary>
    public void ResetExpansion() => _owner.ResetTreeExpansion();

    /// <summary>Keys of all currently expanded branch nodes in the loaded tree; with a detail row, the keys of the rows the user opened.</summary>
    public IReadOnlySet<string> ExpandedKeys => _owner.ExpandedTreeKeys();

    /// <summary>Total number of branch nodes (nodes with children) in the loaded tree.</summary>
    public int TreeBranchCount => _owner.TreeBranchCount();

    /// <summary>Whether every branch node in the loaded tree is currently expanded.</summary>
    public bool AllExpanded => _owner.AllTreeNodesExpanded();

    internal void SetTableTree(DataListTree<TItem>? tree) => _owner.SetTableTree(tree);

    internal void SetTableDetail(bool active) => _owner.SetTableDetail(active);

    internal void AddTable()
    {
        _tableCount++;
        NotifyChanged();
    }

    internal void RemoveTable()
    {
        if (_tableCount > 0) _tableCount--;
        NotifyChanged();
    }

    internal void AddGrid()
    {
        _gridCount++;
        NotifyChanged();
    }

    internal void RemoveGrid()
    {
        if (_gridCount > 0) _gridCount--;
        NotifyChanged();
    }

    /// <summary>Whether the table shows its rows in a scroll box of its own (<c>Virtualize</c> on an interactive table): a loader that watches the scroll position then lives inside that box.</summary>
    internal bool TableVirtualized { get; private set; }

    /// <summary>The CSS selector of the scroll box of a virtualized table, or <see langword="null"/>.</summary>
    internal string? TableScrollSelector { get; private set; }

    /// <summary>What a part asked the table to draw after its rows, inside its scroll box (see <see cref="HostInTable"/>).</summary>
    internal RenderFragment? TableFooter { get; private set; }

    // Set by the table while it renders its rows in a box that scrolls.
    internal void SetTableVirtualized(bool virtualized, string? selector)
    {
        if (TableVirtualized == virtualized && TableScrollSelector == selector) return;
        TableVirtualized = virtualized;
        TableScrollSelector = selector;
        NotifyChanged();
    }

    // A part that cannot sit where the table needs it (the loader of a virtualized table, which has to scroll with the rows) gives the table what to draw there. Only the part that gave it can take it back.
    internal void HostInTable(object owner, RenderFragment? footer)
    {
        if (footer is null)
        {
            if (!ReferenceEquals(_footerOwner, owner)) return;
            _footerOwner = null;
            if (TableFooter is null) return;
            TableFooter = null;
            NotifyChanged();
            return;
        }
        if (ReferenceEquals(_footerOwner, owner) && ReferenceEquals(TableFooter, footer)) return;
        _footerOwner = owner;
        TableFooter = footer;
        NotifyChanged();
    }

    private readonly List<KlyDataListColumnBase<TItem>> _columns = [];
    private int _columnIndex;

    /// <summary>
    /// Every column the list's table declares, visible or not (a columns panel lists them all), by <see cref="KlyDataListColumnBase{TItem}.Order"/> and then in the order they first rendered:
    /// the order of the markup, except for a column that an <c>@if</c> adds later, which comes after the others unless it has an <c>Order</c>. Prefer <see cref="KlyDataListColumnBase{TItem}.Visible"/> to an <c>@if</c>.
    /// </summary>
    public IReadOnlyList<KlyDataListColumnBase<TItem>> Columns => _columns;

    internal string? ColumnsSignature { get; set; }

    internal int NextColumnIndex() => _columnIndex++;

    internal void AddColumn(KlyDataListColumnBase<TItem> column)
    {
        if (!_columns.Contains(column)) _columns.Add(column);
        // Stable: equal orders keep the order of first rendering.
        _columns.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : a.RegistrationIndex.CompareTo(b.RegistrationIndex));
    }

    internal void RemoveColumn(KlyDataListColumnBase<TItem> column) => _columns.Remove(column);

    /// <summary>The name of the list, for the accessible names of its table and its pagination (<c>Label</c> of the list).</summary>
    public string? Label => _owner.Label;

    /// <summary>Counts the results shown so far; it does not change when only a flag does, which lets rows skip a render.</summary>
    public int ResultVersion => _owner.Controller?.ResultVersion ?? 0;

    /// <summary>The schema, once the list has one (an explicit one, or the one its columns describe after they registered).</summary>
    public DataListSchema<TItem>? Schema => _owner.CurrentSchema;

    /// <summary>The state the list shows: search, filters, sort, page, page size. Immutable; change it with <see cref="SetState"/>.</summary>
    public DataListState State => _owner.CurrentState;

    /// <summary>The result on screen: the last one that loaded, kept while the next one loads. <see langword="null"/> before the first load.</summary>
    public DataListResult<TItem>? Result => _owner.Controller?.Result;

    /// <summary>The rows of <see cref="Result"/>; empty before the first load.</summary>
    public IReadOnlyList<TItem> Rows => _owner.Controller?.Rows ?? [];

    /// <summary>Every row matching the active search and filters for an in-memory list (<c>Items</c>), or the loaded <see cref="Rows"/> for a provider or queryable source.</summary>
    public IReadOnlyList<TItem> FilteredRows() => _owner.FilteredRowsForSummary();

    /// <summary>The request for every row the list matches (the loaded request without paging or facets), or <see langword="null"/> before the first load.</summary>
    public DataListRequest? AllResultsRequest => _owner.AllResultsRequest;

    /// <summary>The state <see cref="Result"/> was loaded for: the page and page size of the rows on screen.</summary>
    public DataListState? LoadedState => _owner.Controller?.LoadedState;

    /// <summary>No result yet and nothing failed: show placeholders.</summary>
    public bool IsInitial => _owner.Controller?.IsInitial ?? true;

    /// <summary>A load is running.</summary>
    public bool IsLoading => _owner.Controller?.IsLoading ?? false;

    /// <summary>The rows on screen are being loaded again (refresh or retry).</summary>
    public bool IsRefreshing => _owner.Controller?.IsRefreshing ?? false;

    /// <summary>Other rows are being loaded (a new page, sort or filter) while the previous ones stay on screen.</summary>
    public bool IsReplacing => _owner.Controller?.IsReplacing ?? false;

    /// <summary>The next page of a cursor-paged list is being loaded to append to the rows on screen.</summary>
    public bool IsLoadingMore => _owner.Controller?.IsLoadingMore ?? false;

    /// <summary>Whether the source has another page after the rows loaded so far (cursor paging only).</summary>
    public bool HasMore => _owner.Controller?.HasMore ?? false;

    /// <summary>The opaque cursor for the next page (cursor paging only), or <see langword="null"/> after the last one.</summary>
    public string? NextCursor => _owner.Controller?.NextCursor;

    /// <summary>The newest load-more request failed while the previously loaded rows stayed on screen.</summary>
    public bool HasLoadMoreError => _owner.Controller?.HasLoadMoreError ?? false;

    /// <summary>Why the newest load-more request failed; <see langword="null"/> when it did not.</summary>
    public Exception? LoadMoreError => _owner.Controller?.LoadMoreError;

    /// <summary>How the list pages its rows.</summary>
    public DataListPagingMode PagingMode => _owner.PagingMode;

    /// <summary>The newest load failed. The previous rows, if any, are still in <see cref="Rows"/>.</summary>
    public bool HasError => _owner.Controller?.HasError ?? false;

    /// <summary>Why the newest load failed.</summary>
    public Exception? Error => _owner.Controller?.Error;

    /// <summary>The renderer can run handlers: controls are buttons. Otherwise (static rendering, prerendering) they are links and forms when <see cref="CanLink"/>.</summary>
    public bool IsInteractive => _owner.IsInteractive;

    /// <summary>The state lives in the URL and the renderer is not interactive: controls are links (<see cref="HrefWith"/>) and GET forms.</summary>
    public bool CanLink => _owner.CanLink;

    /// <summary>The rows have a search or filters applied, so an empty result means "nothing matches" rather than "nothing exists".</summary>
    public bool IsFiltered => _owner.IsFiltered;

    /// <summary>Whether <paramref name="state"/> has a search or filters applied: <see cref="IsFiltered"/> for the state the rows on screen were loaded for.</summary>
    public bool IsFilteredFor(DataListState state) => _owner.IsFilteredFor(state);

    /// <summary>The density of the rows.</summary>
    public DataListDensity Density => _owner.Density;

    /// <summary>The key prefix of the URL (<c>UrlKey</c>), or <see langword="null"/>.</summary>
    public string? UrlKey => _owner.NormalizedUrlKey;

    /// <summary>
    /// Changes the state: <paramref name="change"/> receives the current one and returns the new one. With URL sync the change is written to the URL
    /// (<paramref name="history"/> says whether it adds an entry: push for a page or a sort, replace for typing) and the list follows the URL; without it
    /// the list keeps the state itself and reports it through <c>StateChanged</c>. Nothing happens in a renderer that is not interactive: use <see cref="HrefWith"/>.
    /// </summary>
    public void SetState(Func<DataListState, DataListState> change, QueryHistory history = QueryHistory.Push) => _owner.SetState(change, history);

    /// <summary>
    /// Takes the search and the filters off and goes back to the first page (the sort, the page size and the layout stay), with a history entry by default.
    /// Nothing happens in a renderer that is not interactive: link to <see cref="HrefWith"/> with <see cref="ResetState"/>.
    /// </summary>
    public void Reset(QueryHistory history = QueryHistory.Push) => SetState(_owner.ResetState, history);

    /// <summary>
    /// Resets the search and filters to the schema defaults (on page 1), clears the row selection, and resets tree expansion to its defaults.
    /// </summary>
    public void ResetAll(QueryHistory history = QueryHistory.Push) => _owner.ResetAll(history);

    /// <summary>
    /// Replaces a single row in the loaded result in place by its schema <c>RowKey</c>, without querying the source again.
    /// </summary>
    public bool UpdateRow(TItem item) => _owner.UpdateRow(item);

    /// <summary>
    /// Replaces matching rows in the loaded result in place by their schema <c>RowKey</c>, without querying the source again.
    /// </summary>
    public int UpdateRows(IEnumerable<TItem> items) => _owner.UpdateRows(items);

    /// <summary><paramref name="state"/> without its search and its filters (those of the schema's default state), on the first page: what <see cref="Reset"/> sets.</summary>
    public DataListState ResetState(DataListState state) => _owner.ResetState(state);

    /// <summary>Raised when something asks for the focus of the search field of the list (the empty state's "Clear search" action, whose own button is gone once it worked).</summary>
    internal event Action? SearchFocusRequested;

    internal void RequestSearchFocus() => SearchFocusRequested?.Invoke();

    /// <summary>The URL of the current page with the state <paramref name="change"/> returns written in it, for a link of a static render; <see langword="null"/> when the list is not synced with the URL.</summary>
    public string? HrefWith(Func<DataListState, DataListState> change) => _owner.HrefWith(change);

    /// <summary>
    /// The fields of a GET form that submits <paramref name="state"/>: every parameter of the URL that is not the list's own, then the list's own for the state, so submitting
    /// the form does not drop anything. Keys the form supplies itself (<paramref name="exclude"/>, names relative to <see cref="UrlKey"/>: <c>ps</c>, <c>p</c>) are left out.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> FormFields(DataListState state, params string[] exclude) => _owner.FormFields(state, exclude);

    /// <summary>The name of a form field for a key of the list (<c>ps</c> becomes <c>users.ps</c> with <c>UrlKey="users"</c>).</summary>
    public string FieldName(string key) => string.IsNullOrEmpty(UrlKey) ? key : UrlKey + "." + key;

    /// <summary>The path of the current page, the action of a GET form.</summary>
    public string Path => _owner.CurrentPath;

    /// <summary>Loads the rows on screen again, or the failed request again after an error.</summary>
    public Task RefreshAsync() => _owner.RefreshAsync();

    /// <summary>Appends the next page of a cursor-paged list. Does nothing outside cursor mode, while a load is running, when there is no next cursor, or when the renderer is not interactive.</summary>
    public Task LoadMoreAsync() => _owner.LoadMoreAsync();

    /// <summary>The sort that applies: the state's, or the schema's default one when the state has none.</summary>
    public IReadOnlyList<DataListSort> EffectiveSort =>
        State.Sort.Count > 0 ? State.Sort : Schema?.DefaultSort ?? [];

    /// <summary>The sort of a key as it stands: <see langword="null"/> when the list is not sorted by it, else whether it is descending. The key can be anywhere in <see cref="EffectiveSort"/>, not only first.</summary>
    public bool? SortDescending(string key) => SortDirectionIn(EffectiveSort, key);

    /// <summary>The place of a key in <see cref="EffectiveSort"/>, from 0 for the first sort; <see langword="null"/> when the list is not sorted by it.</summary>
    public int? SortIndex(string key)
    {
        var sort = EffectiveSort;
        for (var i = 0; i < sort.Count; i++)
        {
            if (sort[i].Key == key) return i;
        }
        return null;
    }

    private static bool? SortDirectionIn(IReadOnlyList<DataListSort> sort, string key)
    {
        foreach (var entry in sort)
        {
            if (entry.Key == key) return entry.Descending;
        }
        return null;
    }

    private IReadOnlyList<DataListSort> SortOf(DataListState state) => state.Sort.Count > 0 ? state.Sort : Schema?.DefaultSort ?? [];

    /// <summary>
    /// The state after a plain click on the sort control of <paramref name="key"/>, which keeps one sort: ascending, then descending, then back to the default sort. A key that is in the sort but not
    /// first becomes the only sort, ascending. The page goes back to the first.
    /// </summary>
    public DataListState NextSort(DataListState state, string key) => NextSort(state, key, append: false);

    /// <summary>
    /// The state after a click on the sort control of <paramref name="key"/>. Without <paramref name="append"/> it keeps one sort: ascending, then descending, then the default sort.
    /// With <paramref name="append"/> (Shift held) the other sorts stay: a key that is not sorted yet joins the end of the list ascending, then turns descending in place, then leaves the list.
    /// The page goes back to the first.
    /// </summary>
    public DataListState NextSort(DataListState state, string key, bool append)
    {
        var sort = SortOf(state);
        var current = SortDirectionIn(sort, key);
        IReadOnlyList<DataListSort> next;
        if (append)
        {
            next = current switch
            {
                null => [.. sort, new DataListSort(key)],
                false => [.. sort.Select(s => s.Key == key ? s with { Descending = true } : s)],
                true => [.. sort.Where(s => s.Key != key)],
            };
        }
        else
        {
            // A key that is in the sort but not first becomes the only sort, ascending: its next step is never "go back to a sort on neither column".
            var first = sort.Count > 0 && sort[0].Key == key;
            next = !first || current is null
                ? [new DataListSort(key)]
                : current == false ? [new DataListSort(key, true)] : Schema?.DefaultSort ?? [];
        }
        return state with { Sort = next, Page = 1, Cursor = null };
    }

    /// <summary>The state with <paramref name="key"/> added to the end of the sort in the given direction, or turned to that direction in place when it is already sorted. First page.</summary>
    public DataListState AddSort(DataListState state, string key, bool descending = false)
    {
        var sort = SortOf(state);
        IReadOnlyList<DataListSort> next = SortDirectionIn(sort, key) is null
            ? [.. sort, new DataListSort(key, descending)]
            : [.. sort.Select(s => s.Key == key ? s with { Descending = descending } : s)];
        return state with { Sort = next, Page = 1, Cursor = null };
    }

    /// <summary>The state without <paramref name="key"/> in its sort (the default sort when nothing else is left). First page.</summary>
    public DataListState RemoveSort(DataListState state, string key) =>
        state with { Sort = [.. SortOf(state).Where(s => s.Key != key)], Page = 1, Cursor = null };

    /// <summary>The state with the direction of <paramref name="key"/> reversed, in the same place of the sort. First page.</summary>
    public DataListState ReverseSort(DataListState state, string key) =>
        state with { Sort = [.. SortOf(state).Select(s => s.Key == key ? s with { Descending = !s.Descending } : s)], Page = 1, Cursor = null };

    /// <summary>The state with <paramref name="key"/> moved <paramref name="offset"/> places in the sort (negative is earlier), kept inside the list. First page.</summary>
    public DataListState MoveSort(DataListState state, string key, int offset)
    {
        var sort = SortOf(state).ToList();
        var from = sort.FindIndex(s => s.Key == key);
        if (from < 0) return state;
        var to = Math.Clamp(from + offset, 0, sort.Count - 1);
        var entry = sort[from];
        sort.RemoveAt(from);
        sort.Insert(to, entry);
        return state with { Sort = sort, Page = 1, Cursor = null };
    }

    /// <summary>The state with <paramref name="key"/> sorted in the given direction, on the first page.</summary>
    public DataListState WithSort(DataListState state, string key, bool descending = false) =>
        state with { Sort = [new DataListSort(key, descending)], Page = 1, Cursor = null };

    /// <summary>The state with the sort restored to the schema's default sort (or no sort when the schema has none), on the first page.</summary>
    public DataListState ClearSort(DataListState state) =>
        state with { Sort = Schema?.DefaultSort ?? [], Page = 1, Cursor = null };

    /// <summary>Whether the active sort of <paramref name="state"/> equals the schema's default sort.</summary>
    public bool IsDefaultSort(DataListState state)
    {
        var active = state.Sort.Count > 0 ? state.Sort : Schema?.DefaultSort ?? [];
        var defaults = Schema?.DefaultSort ?? [];
        return active.SequenceEqual(defaults);
    }

    /// <summary>The state with <see cref="DataListState.Layout"/> set to <paramref name="layout"/>.</summary>
    public DataListState WithLayout(DataListState state, DataListLayout layout) =>
        state with { Layout = layout };

    /// <summary>Switches the active layout (<see cref="DataListLayout.Table"/> or <see cref="DataListLayout.Grid"/>) as a history entry.</summary>
    public void SetLayout(DataListLayout layout)
    {
        if (State.Layout != layout) SetState(s => WithLayout(s, layout), QueryHistory.Push);
    }

    /// <summary>
    /// The sortable fields of the list for <see cref="KlyDataListSortMenu{TItem}"/>: from <paramref name="explicitOptions"/> when given (filtered to the schema's sortable keys),
    /// otherwise from the sortable <see cref="KlyDataListColumnBase{TItem}"/>s in user order followed by any remaining sortable fields of the schema.
    /// </summary>
    public IReadOnlyList<DataListSortOption> SortOptions(IReadOnlyList<DataListSortOption>? explicitOptions = null)
    {
        var schema = Schema;
        var allowed = schema?.SortKeys;
        var columnsByKey = new Dictionary<string, KlyDataListColumnBase<TItem>>(StringComparer.Ordinal);
        foreach (var col in _owner.OrderedColumns())
        {
            if (col.ResolvedKey is { } k) columnsByKey.TryAdd(k, col);
        }

        var result = new List<DataListSortOption>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (explicitOptions is { Count: > 0 })
        {
            foreach (var opt in explicitOptions)
            {
                if (string.IsNullOrWhiteSpace(opt.Key) || !seen.Add(opt.Key)) continue;
                if (allowed is not null && !allowed.Contains(opt.Key)) continue;
                var label = !string.IsNullOrWhiteSpace(opt.Label)
                    ? opt.Label
                    : columnsByKey.TryGetValue(opt.Key, out var col) ? col.ResolvedTitle : DataListText.Humanize(opt.Key);
                result.Add(new DataListSortOption(opt.Key, label));
            }
            return result;
        }

        foreach (var col in _owner.OrderedColumns())
        {
            if (col.ResolvedKey is not { } key || !seen.Add(key)) continue;
            if (col.Sortable == false) continue;
            if (allowed is not null && !allowed.Contains(key)) continue;
            result.Add(new DataListSortOption(key, col.ResolvedTitle));
        }

        if (allowed is not null)
        {
            foreach (var key in allowed)
            {
                if (!seen.Add(key)) continue;
                if (columnsByKey.TryGetValue(key, out var col) && col.Sortable == false) continue;
                result.Add(new DataListSortOption(key, columnsByKey.TryGetValue(key, out var c) ? c.ResolvedTitle : DataListText.Humanize(key)));
            }
        }
        return result;
    }
}
