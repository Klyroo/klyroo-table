namespace Klyroo.Table.Maui;

public sealed partial class DataListContext<TItem>
{
    private readonly List<KlyDataListColumnBase<TItem>> _columns = [];
    private readonly Dictionary<string, bool> _visibleOverrides = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DataListPin> _pinOverrides = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double?> _widthOverrides = new(StringComparer.Ordinal);
    private List<string>? _order;
    private int _columnsVersion;

    /// <summary>Raised on the UI thread when a column is shown, hidden, moved, pinned or resized.</summary>
    public event Action? ColumnsChanged;

    /// <summary>Every column the list's table declares, visible or not (a columns panel lists them all), by <see cref="KlyDataListColumnBase{TItem}.Order"/> and then in the order they were declared, with the order the user chose applied to the keyed ones.</summary>
    public IReadOnlyList<KlyDataListColumnBase<TItem>> Columns => OrderedColumns();

    /// <summary>Counts column changes so cached rows draw again when columns reorder, hide, pin or resize.</summary>
    internal int ColumnsVersion => _columnsVersion;

    /// <summary>The effective state of every keyed column of the table in its active order.</summary>
    public IReadOnlyList<DataListColumnState> ColumnStates =>
    [
        .. OrderedColumns().Where(c => c.ResolvedKey is not null).Select(c => new DataListColumnState(c.ResolvedKey!, c.ResolvedTitle, IsVisible(c), c.Required, EffectivePin(c), EffectiveWidth(c))),
    ];

    /// <summary>Whether any column visibility, order, pinning or width currently differs from the columns' defaults.</summary>
    public bool HasCustomColumnPreferences => _visibleOverrides.Count > 0 || _pinOverrides.Count > 0 || _widthOverrides.Count > 0 || _order is not null;

    internal void SetColumns(IEnumerable<KlyDataListColumnBase<TItem>> columns)
    {
        _columns.Clear();
        _columns.AddRange(columns);
        foreach (var column in _columns) column.Validate();
        BumpColumns();
    }

    internal List<KlyDataListColumnBase<TItem>> OrderedColumns()
    {
        var baseOrder = _columns.OrderBy(c => c.Order).ThenBy(c => c.RegistrationIndex).ToList();
        if (_order is null) return baseOrder;
        var keyed = baseOrder.Where(c => c.ResolvedKey is not null).OrderBy(c => { var i = _order.IndexOf(c.ResolvedKey!); return i < 0 ? int.MaxValue : i; }).ToList();
        var next = 0;
        for (var i = 0; i < baseOrder.Count; i++)
        {
            if (baseOrder[i].ResolvedKey is not null) baseOrder[i] = keyed[next++];
        }
        return baseOrder;
    }

    /// <summary>The columns the table shows, in the order it shows them: the pinned ones at the start, the others, the pinned ones at the end.</summary>
    internal List<KlyDataListColumnBase<TItem>> TableColumns()
    {
        var visible = OrderedColumns().Where(IsVisible).ToList();
        return
        [
            .. visible.Where(c => EffectivePin(c) == DataListPin.Start),
            .. visible.Where(c => EffectivePin(c) == DataListPin.None),
            .. visible.Where(c => EffectivePin(c) == DataListPin.End),
        ];
    }

    internal bool IsVisible(KlyDataListColumnBase<TItem> column) =>
        column.ResolvedKey is { } key && _visibleOverrides.TryGetValue(key, out var shown) ? shown || column.Required : column.Visible;

    internal DataListPin EffectivePin(KlyDataListColumnBase<TItem> column) =>
        column.ResolvedKey is { } key && _pinOverrides.TryGetValue(key, out var pin) ? pin : column.Pinned;

    internal double EffectiveWidth(KlyDataListColumnBase<TItem> column, bool forLayout) =>
        (column.ResolvedKey is { } key && _widthOverrides.TryGetValue(key, out var width) ? width : column.Width) ?? KlyDataListColumnBase<TItem>.DefaultWidth;

    private double? EffectiveWidth(KlyDataListColumnBase<TItem> column) =>
        column.ResolvedKey is { } key && _widthOverrides.TryGetValue(key, out var width) ? width : column.Width;

    /// <summary>Whether the column may be hidden now: it is not <see cref="KlyDataListColumnBase{TItem}.Required"/> and another column stays visible.</summary>
    internal bool CanHideColumn(KlyDataListColumnBase<TItem> column) =>
        !column.Required && column.ResolvedKey is not null && _columns.Count(c => c != column && IsVisible(c)) > 0;

    /// <summary>Shows or hides a column by key. Does nothing for a <see cref="KlyDataListColumnBase{TItem}.Required"/> column, nor for the last visible one.</summary>
    /// <param name="key">The key of the column.</param>
    /// <param name="visible">Whether to show it.</param>
    public void SetColumnVisible(string key, bool visible)
    {
        var column = _columns.FirstOrDefault(c => c.ResolvedKey == key);
        if (column is null || column.Required) return;
        if (!visible && !CanHideColumn(column)) return;
        if (IsVisible(column) == visible) return;
        _visibleOverrides[key] = visible;
        if (column.Visible == visible) _visibleOverrides.Remove(key);
        BumpColumns();
    }

    /// <summary>Shows every column (<paramref name="visible"/> true) or hides every column that is not <see cref="KlyDataListColumnBase{TItem}.Required"/>; when none is, the first one stays so the table is never empty.</summary>
    /// <param name="visible">Whether to show them.</param>
    public void SetAllColumnsVisible(bool visible)
    {
        var keyed = OrderedColumns().Where(c => c.ResolvedKey is not null).ToList();
        foreach (var column in keyed)
        {
            var wanted = visible || column.Required;
            if (column.Visible == wanted) _visibleOverrides.Remove(column.ResolvedKey!);
            else _visibleOverrides[column.ResolvedKey!] = wanted;
        }
        if (!visible && !_columns.Any(IsVisible) && keyed.Count > 0)
        {
            var first = keyed[0];
            if (first.Visible) _visibleOverrides.Remove(first.ResolvedKey!);
            else _visibleOverrides[first.ResolvedKey!] = true;
        }
        BumpColumns();
    }

    /// <summary>Pins a column to the start or end edge, or unpins it.</summary>
    /// <param name="key">The key of the column.</param>
    /// <param name="pin">The edge to pin it to, or <see cref="DataListPin.None"/>.</param>
    public void SetColumnPinned(string key, DataListPin pin)
    {
        var column = _columns.FirstOrDefault(c => c.ResolvedKey == key);
        if (column is null || column.Required) return;
        if (column.Pinned == pin) _pinOverrides.Remove(key);
        else _pinOverrides[key] = pin;
        BumpColumns();
    }

    /// <summary>Moves a column earlier (<c>-1</c>) or later (<c>+1</c>) in the column order.</summary>
    /// <param name="key">The key of the column.</param>
    /// <param name="delta">How many places to move it.</param>
    public void MoveColumn(string key, int delta)
    {
        var keys = OrderedColumns().Where(c => c.ResolvedKey is not null).Select(c => c.ResolvedKey!).ToList();
        var from = keys.IndexOf(key);
        if (from < 0) return;
        var to = Math.Clamp(from + delta, 0, keys.Count - 1);
        if (to == from) return;
        keys.RemoveAt(from);
        keys.Insert(to, key);
        _order = keys;
        BumpColumns();
    }

    /// <summary>Puts a column right before or right after another one in the column order.</summary>
    /// <param name="key">The key of the column to move.</param>
    /// <param name="targetKey">The key of the column it goes next to.</param>
    /// <param name="after">After the target (<see langword="true"/>) or before it.</param>
    public void MoveColumnNextTo(string key, string targetKey, bool after)
    {
        if (key == targetKey) return;
        var keys = OrderedColumns().Where(c => c.ResolvedKey is not null).Select(c => c.ResolvedKey!).ToList();
        if (!keys.Remove(key)) return;
        var target = keys.IndexOf(targetKey);
        if (target < 0) return;
        keys.Insert(after ? target + 1 : target, key);
        _order = keys;
        BumpColumns();
    }

    /// <summary>Sets a custom width in points for a column, or clears it when <see langword="null"/>.</summary>
    /// <param name="key">The key of the column.</param>
    /// <param name="width">The width, or <see langword="null"/> for the default.</param>
    public void SetColumnWidth(string key, double? width)
    {
        if (_columns.All(c => c.ResolvedKey != key)) return;
        if (width is null) _widthOverrides.Remove(key);
        else _widthOverrides[key] = Math.Clamp(width.Value, 64, 900);
        BumpColumns();
    }

    /// <summary>Restores every column's visibility, order, pinning and width to its declared defaults.</summary>
    public void ResetColumns()
    {
        _visibleOverrides.Clear();
        _pinOverrides.Clear();
        _widthOverrides.Clear();
        _order = null;
        BumpColumns();
    }

    /// <summary>A column changed one of its own properties (its visibility, its title, its width): the table draws again.</summary>
    internal void ColumnPropertyChanged() => BumpColumns();

    private void BumpColumns()
    {
        _columnsVersion++;
        TableSupport.Post(_owner, () => ColumnsChanged?.Invoke());
        NotifyChanged();
    }
}
