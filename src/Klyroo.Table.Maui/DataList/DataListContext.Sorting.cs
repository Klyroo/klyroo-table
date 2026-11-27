namespace Klyroo.Table.Maui;

public sealed partial class DataListContext<TItem>
{
    /// <summary>The sort that applies: the state's, or the schema's default one when the state has none.</summary>
    public IReadOnlyList<DataListSort> EffectiveSort => SortOf(_state);

    /// <summary>The sort of a key as it stands: <see langword="null"/> when the list is not sorted by it, else whether it is descending. The key can be anywhere in <see cref="EffectiveSort"/>, not only first.</summary>
    /// <param name="key">The key of the sortable field.</param>
    public bool? SortDescending(string key) => SortDirectionIn(EffectiveSort, key);

    /// <summary>The place of a key in <see cref="EffectiveSort"/>, from 0 for the first sort; <see langword="null"/> when the list is not sorted by it.</summary>
    /// <param name="key">The key of the sortable field.</param>
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

    private IReadOnlyList<DataListSort> SortOf(DataListState state) => state.Sort.Count > 0 ? state.Sort : _schema?.DefaultSort ?? [];

    /// <summary>
    /// The state after a plain tap on the sort control of <paramref name="key"/>, which keeps one sort: ascending, then descending, then back to the default sort. A key that is in the sort but not
    /// first becomes the only sort, ascending. The page goes back to the first.
    /// </summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    public DataListState NextSort(DataListState state, string key) => NextSort(state, key, append: false);

    /// <summary>
    /// The state after a tap on the sort control of <paramref name="key"/>. Without <paramref name="append"/> it keeps one sort: ascending, then descending, then the default sort.
    /// With <paramref name="append"/> the other sorts stay: a key that is not sorted yet joins the end of the list ascending, then turns descending in place, then leaves the list.
    /// </summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    /// <param name="append">Keep the other sorts.</param>
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
            var first = sort.Count > 0 && sort[0].Key == key;
            next = !first || current is null
                ? [new DataListSort(key)]
                : current == false ? [new DataListSort(key, true)] : _schema?.DefaultSort ?? [];
        }
        return state with { Sort = next, Page = 1, Cursor = null };
    }

    /// <summary>The state with <paramref name="key"/> added to the end of the sort in the given direction, or turned to that direction in place when it is already sorted. First page.</summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    /// <param name="descending">The direction.</param>
    public DataListState AddSort(DataListState state, string key, bool descending = false)
    {
        var sort = SortOf(state);
        IReadOnlyList<DataListSort> next = SortDirectionIn(sort, key) is null
            ? [.. sort, new DataListSort(key, descending)]
            : [.. sort.Select(s => s.Key == key ? s with { Descending = descending } : s)];
        return state with { Sort = next, Page = 1, Cursor = null };
    }

    /// <summary>The state without <paramref name="key"/> in its sort (the default sort when nothing else is left). First page.</summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    public DataListState RemoveSort(DataListState state, string key) =>
        state with { Sort = [.. SortOf(state).Where(s => s.Key != key)], Page = 1, Cursor = null };

    /// <summary>The state with the direction of <paramref name="key"/> reversed, in the same place of the sort. First page.</summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    public DataListState ReverseSort(DataListState state, string key) =>
        state with { Sort = [.. SortOf(state).Select(s => s.Key == key ? s with { Descending = !s.Descending } : s)], Page = 1, Cursor = null };

    /// <summary>The state with <paramref name="key"/> moved <paramref name="offset"/> places in the sort (negative is earlier), kept inside the list. First page.</summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    /// <param name="offset">How many places to move it.</param>
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
    /// <param name="state">The state to start from.</param>
    /// <param name="key">The key of the sortable field.</param>
    /// <param name="descending">The direction.</param>
    public DataListState WithSort(DataListState state, string key, bool descending = false) =>
        state with { Sort = [new DataListSort(key, descending)], Page = 1, Cursor = null };

    /// <summary>The state with the sort restored to the schema's default sort (or no sort when the schema has none), on the first page.</summary>
    /// <param name="state">The state to start from.</param>
    public DataListState ClearSort(DataListState state) =>
        state with { Sort = _schema?.DefaultSort ?? [], Page = 1, Cursor = null };

    /// <summary>Whether the active sort of <paramref name="state"/> equals the schema's default sort.</summary>
    /// <param name="state">The state to look at.</param>
    public bool IsDefaultSort(DataListState state)
    {
        var defaults = _schema?.DefaultSort ?? [];
        return SortOf(state).SequenceEqual(defaults);
    }

    /// <summary>
    /// The sortable fields of the list for <see cref="KlyDataListSortMenu{TItem}"/>: from <paramref name="explicitOptions"/> when given (filtered to the schema's sortable keys),
    /// otherwise from the sortable columns in user order followed by any remaining sortable fields of the schema.
    /// </summary>
    /// <param name="explicitOptions">The options to offer, or <see langword="null"/> for the columns and fields of the schema.</param>
    public IReadOnlyList<DataListSortOption> SortOptions(IReadOnlyList<DataListSortOption>? explicitOptions = null)
    {
        var allowed = _schema?.SortKeys;
        var columnsByKey = new Dictionary<string, KlyDataListColumnBase<TItem>>(StringComparer.Ordinal);
        foreach (var col in OrderedColumns())
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
                    : columnsByKey.TryGetValue(opt.Key, out var col) ? col.ResolvedTitle : TableValues.Humanize(opt.Key);
                result.Add(new DataListSortOption(opt.Key, label));
            }
            return result;
        }

        foreach (var col in OrderedColumns())
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
                result.Add(new DataListSortOption(key, columnsByKey.TryGetValue(key, out var c) ? c.ResolvedTitle : TableValues.Humanize(key)));
            }
        }
        return result;
    }

    /// <summary>Whether the header of a column sorts: the column allows it and the schema has the field as sortable.</summary>
    internal bool CanSortBy(KlyDataListColumnBase<TItem> column) =>
        column.HeaderTemplate is null && column.Sortable != false && column.ResolvedKey is { } key && _schema?.SortKeys.Contains(key) == true;
}
