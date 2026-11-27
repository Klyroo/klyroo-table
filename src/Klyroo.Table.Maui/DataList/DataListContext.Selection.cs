namespace Klyroo.Table.Maui;

public sealed partial class DataListContext<TItem>
{
    private readonly DataListSelection<TItem> _selection = new();
    private string? _anchorKey;
    private DataListActionScope _bulkScope;
    private int _selectionParts;

    /// <summary>Raised on the UI thread when the ticked rows, or what a bulk action targets, change. Cheaper than <see cref="Changed"/> for a part that only draws a checkbox.</summary>
    public event Action? SelectionChanged;

    /// <summary>The list has a selection: <c>Selectable</c> is on, or it has a <see cref="KlyDataListSelectionActions{TItem}"/> bar, or <c>SelectedKeys</c> was set.</summary>
    public bool SelectionEnabled => _owner.Selectable ?? (_selectionParts > 0 || _owner.SelectedKeyHint);

    /// <summary>Whether the rows of a list in memory stay ticked across pages (see <see cref="DataListSelectionScope"/>).</summary>
    internal bool SelectionAcrossPages => _owner.Items is not null && _owner.SelectionScope != DataListSelectionScope.Page;

    /// <summary>The keys of the ticked rows (<c>RowKey</c> of the schema). A set that later changes do not touch.</summary>
    public IReadOnlySet<string> SelectedKeys => _selection.Snapshot();

    /// <summary>How many rows are ticked.</summary>
    public int SelectedCount => _selection.Count;

    /// <summary>The ticked rows that the list holds: those on screen and, for a list of rows in memory, those ticked on other pages.</summary>
    public IReadOnlyList<TItem> SelectedItems => _selection.Items();

    /// <summary>Whether a row is ticked.</summary>
    /// <param name="item">The row.</param>
    public bool IsSelected(TItem item) => _selection.Contains(RowKey(item));

    /// <summary>Whether a key is ticked.</summary>
    /// <param name="key">The row key.</param>
    public bool IsKeySelected(string key) => _selection.Contains(key);

    /// <summary>The row key of a row, as text. Throws when the schema has no <c>RowKey</c>.</summary>
    /// <param name="item">The row.</param>
    public string RowKey(TItem item)
    {
        if (_schema is not { HasRowKey: true })
        {
            throw new InvalidOperationException($"{nameof(KlyDataList<TItem>)} selection needs a RowKey in the schema (RowKey(e => e.Id)): the keys of the selected rows come from it.");
        }
        return _schema.GetRowKey(item);
    }

    /// <summary>What a row action receives. A list without a row key identifies the row by its place on the page.</summary>
    internal DataListRowActionContext<TItem> CreateRowActionContext(TItem item, int position) =>
        new(this, item, _schema is { HasRowKey: true } ? _schema.GetRowKey(item) : "#" + position, position, LifetimeToken);

    /// <summary>The state of the select-all box for the rows on screen: <see langword="true"/> when all are ticked, <see langword="false"/> when none is (or there are no rows), <see langword="null"/> when only some are.</summary>
    public bool? PageSelection
    {
        get
        {
            var rows = Rows;
            if (rows.Count == 0) return false;
            var ticked = rows.Count(row => _selection.Contains(RowKey(row)));
            return ticked == 0 ? false : ticked == rows.Count ? true : null;
        }
    }

    /// <summary>Ticks or unticks a row. With <paramref name="range"/> every row on screen between the row ticked last and this one gets the same state.</summary>
    /// <param name="item">The row.</param>
    /// <param name="selected">Whether it is ticked.</param>
    /// <param name="range">Apply it to the rows between the last ticked row and this one.</param>
    public void SetSelected(TItem item, bool selected, bool range = false)
    {
        var key = RowKey(item);
        bool changed;
        if (range && _anchorKey is not null && _anchorKey != key && RowSpan(_anchorKey, key) is { Count: > 0 } span) changed = _selection.SetMany(span, selected);
        else changed = _selection.Set(key, item, selected);
        _anchorKey = key;
        AfterUserChange(changed);
    }

    private List<KeyValuePair<string, TItem>>? RowSpan(string from, string to)
    {
        var rows = Rows;
        var a = -1;
        var b = -1;
        for (var i = 0; i < rows.Count; i++)
        {
            var key = RowKey(rows[i]);
            if (key == from) a = i;
            if (key == to) b = i;
        }
        if (a < 0 || b < 0) return null;
        var (lo, hi) = a < b ? (a, b) : (b, a);
        return [.. Enumerable.Range(lo, hi - lo + 1).Select(i => new KeyValuePair<string, TItem>(RowKey(rows[i]), rows[i]))];
    }

    /// <summary>Ticks or unticks the rows on screen.</summary>
    /// <param name="selected">Whether they are ticked.</param>
    public void SetPageSelected(bool selected)
    {
        var changed = _selection.SetMany(Rows.Select(row => new KeyValuePair<string, TItem>(RowKey(row), row)), selected);
        _anchorKey = null;
        AfterUserChange(changed);
    }

    /// <summary>The selection state of a visual row group: <see langword="true"/> when all rows are ticked, <see langword="false"/> when none is, <see langword="null"/> when only some are.</summary>
    /// <param name="group">The group.</param>
    public bool? GetGroupSelection(DataListGroup<TItem> group)
    {
        if (group.Items.Count == 0) return false;
        var ticked = group.Items.Count(row => _selection.Contains(RowKey(row)));
        return ticked == 0 ? false : ticked == group.Items.Count ? true : null;
    }

    /// <summary>Ticks or unticks every row of a group.</summary>
    /// <param name="group">The group.</param>
    /// <param name="selected">Whether they are ticked.</param>
    public void SetGroupSelected(DataListGroup<TItem> group, bool selected)
    {
        var changed = _selection.SetMany(group.Items.Select(row => new KeyValuePair<string, TItem>(RowKey(row), row)), selected);
        _anchorKey = null;
        AfterUserChange(changed);
    }

    /// <summary>Unticks every row, and goes back to acting on the selection when the bulk scope was every result.</summary>
    public void ClearSelection()
    {
        var changed = _selection.Clear();
        _anchorKey = null;
        if (_bulkScope != DataListActionScope.Selection)
        {
            _bulkScope = DataListActionScope.Selection;
            changed = true;
        }
        AfterUserChange(changed);
    }

    /// <summary>Replaces the selection by these keys: rows already known stay known, the others wait for a load to find them.</summary>
    /// <param name="keys">The row keys.</param>
    internal void ReplaceSelection(IEnumerable<string> keys)
    {
        if (!_selection.Replace(keys)) return;
        ReconcileSelection();
        RaiseSelectionChanged();
    }

    internal void ClearSelection(IEnumerable<string> keys)
    {
        var changed = false;
        foreach (var key in keys.ToList()) changed |= _selection.Remove(key);
        AfterUserChange(changed);
    }

    /// <summary>What a bulk action targets: the ticked rows, or every result of the query on screen.</summary>
    public DataListActionScope BulkScope => _bulkScope;

    /// <summary>How many rows the search and the filters match, when the source says; <see langword="null"/> when it does not.</summary>
    public long? MatchingCount => Controller?.Result?.TotalCount;

    /// <summary>Whether the bulk scope can switch to every result: the source knows how many results there are and there are more than the ticked rows.</summary>
    public bool CanSelectAllResults => MatchingCount is { } total && total > _selection.Count;

    /// <summary>Sets what a bulk action targets. <see cref="DataListActionScope.All"/> needs <see cref="CanSelectAllResults"/>; it is dropped when the rows or the query change.</summary>
    /// <param name="scope">The scope.</param>
    public void SetBulkScope(DataListActionScope scope)
    {
        if (scope == DataListActionScope.All && !CanSelectAllResults) return;
        if (_bulkScope == scope) return;
        _bulkScope = scope;
        RaiseSelectionChanged();
    }

    internal void AddSelectionPart()
    {
        _selectionParts++;
        NotifyChanged();
    }

    internal void RemoveSelectionPart() => _selectionParts--;

    private void AfterUserChange(bool changed)
    {
        if (!changed) return;
        if (_bulkScope == DataListActionScope.All && !CanSelectAllResults) _bulkScope = DataListActionScope.Selection;
        RaiseSelectionChanged();
    }

    private void RaiseSelectionChanged()
    {
        _owner.SyncSelectedKeys(_selection.Snapshot());
        TableSupport.Post(_owner, () => SelectionChanged?.Invoke());
    }

    // A new result: rows that are no longer on screen leave the selection of a list that keeps one page, and the rows on screen refresh what a bulk action sees.
    private void ReconcileSelection()
    {
        if (_selection.Count == 0 || _schema is not { HasRowKey: true }) return;
        var onScreen = new Dictionary<string, TItem>(StringComparer.Ordinal);
        foreach (var row in Rows) onScreen[_schema.GetRowKey(row)] = row;
        bool changed;
        if (SelectionAcrossPages)
        {
            foreach (var key in _selection.Keys.ToList())
            {
                if (onScreen.TryGetValue(key, out var item)) _selection.Set(key, item, true);
            }
            changed = false;
        }
        else
        {
            changed = _selection.Retain(onScreen);
        }
        if (_bulkScope == DataListActionScope.All && !CanSelectAllResults)
        {
            _bulkScope = DataListActionScope.Selection;
            changed = true;
        }
        if (changed) RaiseSelectionChanged();
    }
}
