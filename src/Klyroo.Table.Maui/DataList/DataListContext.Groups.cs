namespace Klyroo.Table.Maui;

public sealed partial class DataListContext<TItem>
{
    private readonly HashSet<string> _toggledGroups = new(StringComparer.Ordinal);
    private DataListGroupBy<TItem>? _tableGroupBy;
    private DataListGroupBy<TItem>? _gridGroupBy;
    private DataListRowSummary<TItem>? _rowSummary;
    private bool? _summaryViewUser;
    private bool _phone;
    private int _groupsVersion;

    /// <summary>Counts the folds and unfolds of groups, so a table draws its rows again when one happens.</summary>
    internal int GroupsVersion => _groupsVersion;

    /// <summary>Whether the table is narrower than a phone breakpoint, as the table reported it.</summary>
    internal bool IsPhone => _phone;

    /// <summary>The active grouping configuration for the current layout (<see cref="KlyDataListTable{TItem}.GroupBy"/> or <see cref="KlyDataListGrid{TItem}.GroupBy"/>), or <see langword="null"/>.</summary>
    public DataListGroupBy<TItem>? ActiveGroupBy => EffectiveLayout == DataListLayout.Grid ? _gridGroupBy : _tableGroupBy;

    /// <summary>Builds the visual groups of <see cref="Rows"/> for the active layout when <see cref="ActiveGroupBy"/> is configured; empty otherwise.</summary>
    public IReadOnlyList<DataListGroup<TItem>> Groups() => ActiveGroupBy?.BuildGroups(Rows, _schema, Messages, Culture) ?? [];

    /// <summary>Whether <paramref name="group"/> is currently collapsed (folded).</summary>
    /// <param name="group">The group.</param>
    public bool IsGroupCollapsed(DataListGroup<TItem> group) => group.DefaultCollapsed ^ _toggledGroups.Contains(group.Key);

    /// <summary>Folds or unfolds <paramref name="group"/>.</summary>
    /// <param name="group">The group.</param>
    public void ToggleGroup(DataListGroup<TItem> group) => SetGroupCollapsed(group, !IsGroupCollapsed(group));

    /// <summary>Sets whether <paramref name="group"/> is collapsed (folded).</summary>
    /// <param name="group">The group.</param>
    /// <param name="collapsed">Whether it is folded.</param>
    public void SetGroupCollapsed(DataListGroup<TItem> group, bool collapsed)
    {
        if (IsGroupCollapsed(group) == collapsed) return;
        if (!_toggledGroups.Remove(group.Key)) _toggledGroups.Add(group.Key);
        _groupsVersion++;
        NotifyChanged();
    }

    internal void SetTableGroupBy(DataListGroupBy<TItem>? groupBy) => _tableGroupBy = groupBy;

    internal void SetGridGroupBy(DataListGroupBy<TItem>? groupBy) => _gridGroupBy = groupBy;

    /// <summary>Whether the table has a <see cref="DataListRowSummary{TItem}"/> configured.</summary>
    public bool HasRowSummary => _rowSummary is not null;

    internal DataListRowSummary<TItem>? RowSummary => _rowSummary;

    /// <summary>Whether the table currently draws compact summary cards instead of the multi-column table.</summary>
    public bool SummaryViewActive =>
        EffectiveLayout == DataListLayout.Table && _rowSummary is { } summary && (_summaryViewUser ?? (summary.DefaultActive || (summary.AutoPhone && _phone)));

    /// <summary>Switches between compact summary cards (<see langword="true"/>) and the full table (<see langword="false"/>).</summary>
    /// <param name="active">Whether the cards show.</param>
    public void SetSummaryView(bool active)
    {
        if (_summaryViewUser == active) return;
        _summaryViewUser = active;
        NotifyChanged();
    }

    internal void SetRowSummary(DataListRowSummary<TItem>? summary)
    {
        _rowSummary = summary;
        NotifyChanged();
    }

    internal void SetPhone(bool phone)
    {
        if (_phone == phone) return;
        _phone = phone;
        NotifyChanged();
    }
}
