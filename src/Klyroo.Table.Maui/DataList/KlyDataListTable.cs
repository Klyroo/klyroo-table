using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Klyroo.Table.Maui;

/// <summary>
/// The table of a data list: a header row (a tap on a sortable header sorts), one row per row of the page, the checkbox column of the selection, the actions of each row, groups, placeholders while the first rows load, and an empty state.
/// Its columns are <see cref="KlyDataListColumn{TItem, TValue}"/>, <see cref="KlyDataListFieldColumn{TItem}"/> and <see cref="KlyDataListTemplateColumn{TItem}"/> items of <see cref="Columns"/>; without any it shows the fields of the schema.
/// A table wider than the screen scrolls sideways. Below <see cref="PhoneWidth"/> points it becomes a list of cards (see <see cref="RowSummary"/>), and a long list is virtualized in a <see cref="CollectionView"/> (see <see cref="Virtualize"/>).
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
[ContentProperty(nameof(Columns))]
public class KlyDataListTable<TItem> : DataListPartBase<TItem>
{
    private const double SelectionWidth = 48;

    /// <summary>Identifies <see cref="Caption"/>.</summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(nameof(Caption), typeof(string), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="RowActions"/>.</summary>
    public static readonly BindableProperty RowActionsProperty = BindableProperty.Create(nameof(RowActions), typeof(IReadOnlyList<DataListRowAction<TItem>>), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="RowActionsMaxVisible"/>.</summary>
    public static readonly BindableProperty RowActionsMaxVisibleProperty = BindableProperty.Create(nameof(RowActionsMaxVisible), typeof(int), typeof(KlyDataListTable<TItem>), 3, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="RowSummary"/>.</summary>
    public static readonly BindableProperty RowSummaryProperty = BindableProperty.Create(nameof(RowSummary), typeof(DataListRowSummary<TItem>), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, v) =>
    {
        var table = (KlyDataListTable<TItem>)b;
        table.Context?.SetRowSummary((DataListRowSummary<TItem>?)v);
        table.Redraw();
    });
    /// <summary>Identifies <see cref="GroupBy"/>.</summary>
    public static readonly BindableProperty GroupByProperty = BindableProperty.Create(nameof(GroupBy), typeof(DataListGroupBy<TItem>), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, v) =>
    {
        var table = (KlyDataListTable<TItem>)b;
        table.Context?.SetTableGroupBy((DataListGroupBy<TItem>?)v);
        table.Redraw();
    });
    /// <summary>Identifies <see cref="EmptyState"/>.</summary>
    public static readonly BindableProperty EmptyStateProperty = BindableProperty.Create(nameof(EmptyState), typeof(DataListEmptyState), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Empty"/>.</summary>
    public static readonly BindableProperty EmptyProperty = BindableProperty.Create(nameof(Empty), typeof(Func<DataListContext<TItem>, View>), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowProgressLine"/>.</summary>
    public static readonly BindableProperty ShowProgressLineProperty = BindableProperty.Create(nameof(ShowProgressLine), typeof(bool), typeof(KlyDataListTable<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).UpdateProgress());
    /// <summary>Identifies <see cref="Virtualize"/>.</summary>
    public static readonly BindableProperty VirtualizeProperty = BindableProperty.Create(nameof(Virtualize), typeof(bool), typeof(KlyDataListTable<TItem>), false, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="VirtualizeThreshold"/>.</summary>
    public static readonly BindableProperty VirtualizeThresholdProperty = BindableProperty.Create(nameof(VirtualizeThreshold), typeof(int), typeof(KlyDataListTable<TItem>), 100, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="VirtualHeight"/>.</summary>
    public static readonly BindableProperty VirtualHeightProperty = BindableProperty.Create(nameof(VirtualHeight), typeof(double), typeof(KlyDataListTable<TItem>), 480d, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="PhoneWidth"/>.</summary>
    public static readonly BindableProperty PhoneWidthProperty = BindableProperty.Create(nameof(PhoneWidth), typeof(double), typeof(KlyDataListTable<TItem>), TableSupport.PhoneWidth, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).MeasureWidth());
    /// <summary>Identifies <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(KlyDataListTable<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListTable<TItem>)b).Redraw());

    private readonly VerticalStackLayout _root = new();
    private readonly BoxView _progress = TableBody.ProgressLine();
    private readonly ContentView _bodyHost = new();
    private readonly Dictionary<string, KlyCheckbox> _boxes = new(StringComparer.Ordinal);
    private readonly List<DataListPartBase<TItem>> _owned = [];
    private List<KlyDataListColumnBase<TItem>>? _implicit;
    private KlyCheckbox? _headerBox;
    private CollectionView? _collection;
    private IReadOnlyList<TableRowEntry<TItem>> _entries = [];
    private object? _signature;
    private DataListContext<TItem>? _subscribed;
    private bool _refreshing;
    private bool _syncing;

    /// <summary>Creates a table: add columns to <see cref="Columns"/>.</summary>
    public KlyDataListTable()
    {
        Columns = [];
        Columns.CollectionChanged += OnColumnsChanged;
        _root.Children.Add(_progress);
        _root.Children.Add(_bodyHost);
        Content = _root;
        SizeChanged += (_, _) => MeasureWidth();
        Refresh();
    }

    /// <summary>Raised when the user taps a row, anywhere that is not a control inside it.</summary>
    public event EventHandler<DataListRowClickArgs<TItem>>? RowClick;

    /// <summary>Raised when a row action of the table threw or reported a failure.</summary>
    public event EventHandler<DataListRowActionFailure<TItem>>? RowActionError;

    /// <summary>The columns: <see cref="KlyDataListColumn{TItem, TValue}"/>, <see cref="KlyDataListFieldColumn{TItem}"/> and <see cref="KlyDataListTemplateColumn{TItem}"/> items, in order.</summary>
    public ObservableCollection<KlyDataListColumnBase<TItem>> Columns { get; }

    /// <summary>Describes the table for a screen reader; without it the name of the list, then the localized "Results".</summary>
    public string? Caption { get => (string?)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }

    /// <summary>The accessible name of the table. Defaults to <see cref="Caption"/>, the label of the list, then the localized "Results".</summary>
    public string? AccessibleName { get => (string?)GetValue(AccessibleNameProperty); set => SetValue(AccessibleNameProperty, value); }

    /// <summary>Actions on each row (<see cref="KlyDataListRowActions{TItem}"/>), drawn in a last column (and in each card).</summary>
    public IReadOnlyList<DataListRowAction<TItem>>? RowActions { get => (IReadOnlyList<DataListRowAction<TItem>>?)GetValue(RowActionsProperty); set => SetValue(RowActionsProperty, value); }

    /// <summary>How many row actions show as buttons on each row (3 by default); the others go into a menu. 0 keeps one menu button per row.</summary>
    public int RowActionsMaxVisible { get => (int)GetValue(RowActionsMaxVisibleProperty); set => SetValue(RowActionsMaxVisibleProperty, value); }

    /// <summary>The card of a row: a title, a status, details. The table draws cards below <see cref="PhoneWidth"/> points (when <see cref="DataListRowSummary{TItem}.AutoPhone"/> is on), when <see cref="DataListRowSummary{TItem}.DefaultActive"/> is on, or when <see cref="KlyDataListViewToggle{TItem}"/> says so. Without a summary the cards use the schema and the columns.</summary>
    public DataListRowSummary<TItem>? RowSummary { get => (DataListRowSummary<TItem>?)GetValue(RowSummaryProperty); set => SetValue(RowSummaryProperty, value); }

    /// <summary>Visual grouping of the rows of the page under collapsible <see cref="KlyDataListGroupHeader{TItem}"/> headers. Grouped rows are not virtualized.</summary>
    public DataListGroupBy<TItem>? GroupBy { get => (DataListGroupBy<TItem>?)GetValue(GroupByProperty); set => SetValue(GroupByProperty, value); }

    /// <summary>The unfiltered empty state: icon, title, description and a first-use action. A search or a filter that matches nothing shows the filtered one instead.</summary>
    public DataListEmptyState? EmptyState { get => (DataListEmptyState?)GetValue(EmptyStateProperty); set => SetValue(EmptyStateProperty, value); }

    /// <summary>Builds the view shown when the list has no rows, instead of <see cref="EmptyState"/> and the default one.</summary>
    public Func<DataListContext<TItem>, View>? Empty { get => (Func<DataListContext<TItem>, View>?)GetValue(EmptyProperty); set => SetValue(EmptyProperty, value); }

    /// <summary>Draws a thin line on top of the rows while they are refreshed or replaced (<see langword="true"/> by default).</summary>
    public bool ShowProgressLine { get => (bool)GetValue(ShowProgressLineProperty); set => SetValue(ShowProgressLineProperty, value); }

    /// <summary>Draws the rows in a <see cref="CollectionView"/> that only builds the rows on screen. Rows are also virtualized once the page holds more than <see cref="VirtualizeThreshold"/> rows. Not used with <see cref="GroupBy"/>.</summary>
    public bool Virtualize { get => (bool)GetValue(VirtualizeProperty); set => SetValue(VirtualizeProperty, value); }

    /// <summary>The number of rows from which the table is virtualized without being asked (100 by default).</summary>
    public int VirtualizeThreshold { get => (int)GetValue(VirtualizeThresholdProperty); set => SetValue(VirtualizeThresholdProperty, value); }

    /// <summary>The height of the virtualized rows, in points (480 by default): a virtualized list needs a height to know which rows are on screen.</summary>
    public double VirtualHeight { get => (double)GetValue(VirtualHeightProperty); set => SetValue(VirtualHeightProperty, value); }

    /// <summary>The width, in points, below which the table is a list of cards (640 by default).</summary>
    public double PhoneWidth { get => (double)GetValue(PhoneWidthProperty); set => SetValue(PhoneWidthProperty, value); }

    internal View? Body => _bodyHost.Content;

    internal BoxView ProgressLine => _progress;

    internal CollectionView? VirtualList => _collection;

    internal bool ShowsCards { get; private set; }

    internal IReadOnlyDictionary<string, KlyCheckbox> RowBoxes => _boxes;

    internal KlyCheckbox? HeaderBox => _headerBox;

    internal IReadOnlyList<TableTap> HeaderTaps { get; private set; } = [];

    internal VerticalStackLayout? RowStack { get; private set; }

    private bool IsPhone => Context?.IsPhone ?? false;

    // ---- columns

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (KlyDataListColumnBase<TItem> column in e.OldItems) column.Changed -= OnColumnChanged;
        }
        if (e.NewItems is not null)
        {
            foreach (KlyDataListColumnBase<TItem> column in e.NewItems) column.Changed += OnColumnChanged;
        }
        _implicit = null;
        Context?.SetColumns(Columns);
        Redraw();
    }

    private void OnColumnChanged(object? sender, EventArgs e)
    {
        if (Context is { } context) context.ColumnPropertyChanged();
        else Redraw();
    }

    // A table with no column shows the fields of the schema, once the list has one.
    private void RegisterColumns(DataListContext<TItem> context)
    {
        if (Columns.Count > 0)
        {
            return;
        }
        if (_implicit is null && context.Schema is { } schema)
        {
            _implicit = [.. schema.FieldInfos.Where(f => !f.IsCollection).Select(f => (KlyDataListColumnBase<TItem>)new KlyDataListFieldColumn<TItem> { Field = f.Key, Sortable = f.Sortable })];
            context.SetColumns(_implicit);
        }
    }

    // ---- life

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        _subscribed = context;
        _implicit = null;
        context.SetColumns(Columns);
        context.SetTableGroupBy(GroupBy);
        context.SetRowSummary(RowSummary);
        context.SelectionChanged += OnSelectionChanged;
        context.AddTable();
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.SelectionChanged -= OnSelectionChanged;
        context.SetTableGroupBy(null);
        context.SetRowSummary(null);
        context.SetColumns([]);
        context.RemoveTable();
        ReleaseOwned();
        _subscribed = null;
        _implicit = null;
        _signature = null;
    }

    private void MeasureWidth()
    {
        if (Width <= 0 || Context is not { } context) return;
        context.SetPhone(Width < PhoneWidth);
    }

    private void UpdateProgress()
    {
        _progress.IsVisible = ShowProgressLine && Context is { IsRefreshing: true } or { IsReplacing: true };
    }

    // ---- drawing

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (_refreshing) return;
        _refreshing = true;
        try
        {
            RefreshCore();
        }
        finally
        {
            _refreshing = false;
        }
    }

    private void RefreshCore()
    {
        if (Context is not { } context)
        {
            _bodyHost.Content = null;
            _signature = null;
            UpdateProgress();
            return;
        }
        RegisterColumns(context);
        UpdateProgress();
        SemanticProperties.SetDescription(_root, AccessibleName ?? Caption ?? context.Label ?? Messages.Results);
        var cards = IsCardMode(context);
        var virtualized = IsVirtualized(context, cards);
        var signature = (context.ResultVersion, context.IsInitial, context.HasError, context.ColumnsVersion, context.GroupsVersion, context.EffectiveLayout, cards, virtualized, context.SelectionEnabled,
            string.Join(';', context.EffectiveSort.Select(s => s.Key + s.Descending)), context.Density, RowActions, RowActionsMaxVisible, context.Rows.Count == 0 && context.Result is not null, context.IsFilteredFor(context.State), System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(context.Messages), context.ActiveGroupBy, context.RowSummary, EmptyState, Empty);
        if (_bodyHost.Content is not null && Equals(signature, _signature)) return;
        _signature = signature;
        ShowsCards = cards;
        BuildBody(context, cards, virtualized);
    }

    private bool IsCardMode(DataListContext<TItem> context) =>
        context.EffectiveLayout == DataListLayout.Table && (context.SummaryViewActive || (context.RowSummary is null && context.IsPhone));

    private bool IsVirtualized(DataListContext<TItem> context, bool cards) =>
        (Virtualize || context.Rows.Count > VirtualizeThreshold) && context.ActiveGroupBy is null && context.Rows.Count > 0 && !context.IsInitial;

    // The parts the rows were built with (group headers, row actions) listen to the list: they stop when the rows are drawn again.
    private void ReleaseOwned()
    {
        foreach (var part in _owned) part.Release();
        _owned.Clear();
    }

    private T Own<T>(T part, DataListContext<TItem> context) where T : DataListPartBase<TItem>
    {
        part.BindExplicit(context);
        _owned.Add(part);
        return part;
    }

    private void BuildBody(DataListContext<TItem> context, bool cards, bool virtualized)
    {
        ReleaseOwned();
        _boxes.Clear();
        _headerBox = null;
        _collection = null;
        RowStack = null;
        HeaderTaps = [];
        if (context.EffectiveLayout != DataListLayout.Table)
        {
            _bodyHost.Content = null;
            return;
        }
        if (context.IsInitial)
        {
            _bodyHost.Content = Skeleton(context, cards);
            return;
        }
        if (context.Result is not null && context.Rows.Count == 0 && !context.HasError)
        {
            _bodyHost.Content = TableBody.Empty(context, EmptyState, Empty);
            return;
        }
        _bodyHost.Content = cards ? BuildCards(context, virtualized) : BuildTable(context, virtualized);
    }

    private View Skeleton(DataListContext<TItem> context, bool cards)
    {
        var stack = new VerticalStackLayout { Spacing = 8 };
        AutomationProperties.SetIsInAccessibleTree(stack, false);
        SemanticProperties.SetDescription(stack, Messages.Loading);
        var rows = Math.Clamp(context.State.PageSize, 3, 9);
        var columns = cards ? [] : context.TableColumns();
        for (var i = 0; i < rows; i++)
        {
            if (cards || columns.Count == 0)
            {
                stack.Children.Add(new VerticalStackLayout { Spacing = 6, Padding = new Thickness(12), Children = { TableBody.Bar(160, 16), TableBody.Bar(240), TableBody.Bar(200) } });
            }
            else
            {
                var row = new HorizontalStackLayout { Spacing = 0, Padding = new Thickness(0, TableBody.RowPadding(context.Density)) };
                foreach (var column in columns) row.Children.Add(new ContentView { WidthRequest = context.EffectiveWidth(column, true), Padding = new Thickness(12, 0), Content = TableBody.Bar(Math.Max(24, context.EffectiveWidth(column, true) * 0.6)) });
                stack.Children.Add(row);
            }
        }
        return stack;
    }

    // ---- the table

    private double ActionsWidth => RowActions is { Count: > 0 } ? (RowActionsMaxVisible > 0 ? 96 * Math.Min(RowActionsMaxVisible, RowActions.Count) + 56 : 64) : 0;

    private View BuildTable(DataListContext<TItem> context, bool virtualized)
    {
        var columns = context.TableColumns();
        var selecting = context.SelectionEnabled;
        var actions = ActionsWidth;
        var total = (selecting ? SelectionWidth : 0) + columns.Sum(c => context.EffectiveWidth(c, true)) + actions;
        var stack = new VerticalStackLayout { Spacing = 0, WidthRequest = total };
        stack.Children.Add(BuildHeader(context, columns, selecting, actions));
        stack.Children.Add(TableBody.Separator());
        var entries = context.Rows.Select((item, index) => new TableRowEntry<TItem>(item, index + 1 + (context.PagingMode == DataListPagingMode.Offset ? ((context.LoadedState ?? context.State).Page - 1) * (context.LoadedState ?? context.State).PageSize : 0))).ToList();
        _entries = entries;
        View Row(TItem item, int position) => BuildTableRow(context, columns, selecting, actions, item, position);
        if (virtualized)
        {
            stack.Children.Add(BuildCollection(context, entries, Row, total));
        }
        else
        {
            var rows = new VerticalStackLayout { Spacing = 0 };
            RowStack = rows;
            if (context.ActiveGroupBy is not null)
            {
                foreach (var group in context.Groups())
                {
                    var header = Own(new KlyDataListGroupHeader<TItem> { Group = group, Margin = new Thickness(0, 4) }, context);
                    rows.Children.Add(header);
                    if (context.IsGroupCollapsed(group)) continue;
                    foreach (var item in group.Items) rows.Children.Add(Row(item, entries.First(e => ReferenceEquals(e.Item, item)).Position));
                }
            }
            else
            {
                foreach (var entry in entries) rows.Children.Add(Row(entry.Item, entry.Position));
            }
            stack.Children.Add(rows);
        }
        var scroll = new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = stack };
        SemanticProperties.SetDescription(scroll, AccessibleName ?? Caption ?? context.Label ?? Messages.Results);
        return scroll;
    }

    private View BuildHeader(DataListContext<TItem> context, List<KlyDataListColumnBase<TItem>> columns, bool selecting, double actions)
    {
        var messages = Messages;
        var grid = new Grid { MinimumHeightRequest = 40, ColumnSpacing = 0 };
        var taps = new List<TableTap>();
        var index = 0;
        if (selecting)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(SelectionWidth));
            var all = context.PageSelection == true;
            var box = TableBody.Box(messages.SelectAllRows, all, selected =>
            {
                if (_syncing) return;
                context.SetPageSelected(selected);
            });
            box.HorizontalOptions = LayoutOptions.Center;
            box.VerticalOptions = LayoutOptions.Center;
            _headerBox = box;
            grid.Add(box, index++);
        }
        foreach (var column in columns)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(context.EffectiveWidth(column, true)));
            var key = column.ResolvedKey;
            View header;
            if (column.HeaderTemplate is { } template)
            {
                header = template;
            }
            else
            {
                var label = TableSupport.NewLabel(12, KlyTheme.TextMuted, FontAttributes.Bold);
                label.Text = column.ResolvedTitle;
                label.HorizontalTextAlignment = KlyDataListColumnBase<TItem>.TextAlignmentOf(column.Align);
                label.HorizontalOptions = LayoutOptions.Fill;
                label.LineBreakMode = LineBreakMode.TailTruncation;
                var cell = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], ColumnSpacing = 4 };
                cell.Add(label, 0);
                if (context.CanSortBy(column) && key is not null)
                {
                    var descending = context.SortDescending(key);
                    cell.Add(new KlyIcon { Name = descending is null ? TableIcons.ArrowUpDown : descending == true ? KlyIconNames.ArrowDown : KlyIconNames.ArrowUp, Size = KlySize.Xs, IconColor = descending is null ? KlyTheme.TextDimmed : KlyTheme.TextHighlighted, VerticalOptions = LayoutOptions.Center }, 1);
                    var tap = new TableTap(cell) { MinimumHeightRequest = 40 };
                    tap.AccessibleName = column.ResolvedTitle;
                    tap.AccessibleHint = descending is null ? TableSupport.Format(messages.SortAscending, column.ResolvedTitle) : descending == false ? TableSupport.Format(messages.SortDescending, column.ResolvedTitle) : TableSupport.Format(messages.SortClear, column.ResolvedTitle);
                    var captured = key;
                    tap.Tapped += (_, _) => context.SetState(state => context.NextSort(state, captured));
                    taps.Add(tap);
                    header = tap;
                }
                else
                {
                    header = cell;
                }
            }
            grid.Add(new ContentView { Content = header, Padding = new Thickness(12, 0), VerticalOptions = LayoutOptions.Center }, index++);
        }
        if (actions > 0) grid.ColumnDefinitions.Add(new ColumnDefinition(actions));
        HeaderTaps = taps;
        SemanticProperties.SetHeadingLevel(grid, SemanticHeadingLevel.None);
        return grid;
    }

    private View BuildTableRow(DataListContext<TItem> context, List<KlyDataListColumnBase<TItem>> columns, bool selecting, double actions, TItem item, int position)
    {
        var messages = Messages;
        var vertical = TableBody.RowPadding(context.Density);
        var grid = new Grid { ColumnSpacing = 0, Padding = new Thickness(0, vertical) };
        var index = 0;
        if (selecting)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(SelectionWidth));
            var key = context.RowKey(item);
            var box = TableBody.Box(TableSupport.Format(messages.SelectRowAt, position), context.IsKeySelected(key), selected =>
            {
                if (_syncing) return;
                context.SetSelected(item, selected);
            });
            box.HorizontalOptions = LayoutOptions.Center;
            box.VerticalOptions = LayoutOptions.Center;
            _boxes[key] = box;
            grid.Add(box, index++);
        }
        foreach (var column in columns)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(context.EffectiveWidth(column, true)));
            var cell = column.CreateCell(item, context.Schema, context.Culture);
            cell.VerticalOptions = LayoutOptions.Center;
            grid.Add(new ContentView { Content = cell, Padding = new Thickness(12, 0), VerticalOptions = LayoutOptions.Center }, index++);
        }
        if (actions > 0)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(actions));
            grid.Add(NewRowActions(context, item, position), index);
        }
        var row = new TableTap(new VerticalStackLayout { Spacing = 0, Children = { grid, TableBody.Separator() } });
        row.Tapped += (_, _) => RaiseRowClick(context, item, position);
        return row;
    }

    private View NewRowActions(DataListContext<TItem> context, TItem item, int position)
    {
        var part = new KlyDataListRowActions<TItem> { Item = item, Position = position, Actions = RowActions!, MaxVisible = RowActionsMaxVisible, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
        part.Error += (_, e) => RowActionError?.Invoke(this, e);
        return Own(part, context);
    }

    /// <summary>Does what a tap on a row does.</summary>
    /// <param name="item">The row.</param>
    /// <param name="position">Its place on the page, from 1.</param>
    internal void RaiseRowClick(TItem item, int position)
    {
        if (Context is { } context) RaiseRowClick(context, item, position);
    }

    private void RaiseRowClick(DataListContext<TItem> context, TItem item, int position)
    {
        var key = context.Schema is { HasRowKey: true } ? context.RowKey(item) : "#" + position;
        RowClick?.Invoke(this, new DataListRowClickArgs<TItem>(item, key, position));
    }

    // ---- the virtualized list

    private View BuildCollection(DataListContext<TItem> context, IReadOnlyList<TableRowEntry<TItem>> entries, Func<TItem, int, View> row, double width)
    {
        var collection = new CollectionView
        {
            ItemsSource = entries,
            ItemTemplate = new DataTemplate(() => new TableRowHost<TItem>(row)),
            SelectionMode = SelectionMode.None,
            HeightRequest = VirtualHeight,
            RemainingItemsThreshold = 5,
        };
        if (width > 0) collection.WidthRequest = width;
        collection.RemainingItemsThresholdReached += (_, _) =>
        {
            if (context.AutoLoadMore && context.HasMore && !context.IsLoadingMore) _ = context.LoadMoreAsync();
        };
        _collection = collection;
        return collection;
    }

    // ---- the cards

    private View BuildCards(DataListContext<TItem> context, bool virtualized)
    {
        var summary = context.RowSummary ?? new DataListRowSummary<TItem>();
        var selecting = context.SelectionEnabled;
        var page = context.LoadedState ?? context.State;
        var entries = context.Rows.Select((item, index) => new TableRowEntry<TItem>(item, index + 1 + (context.PagingMode == DataListPagingMode.Offset ? (page.Page - 1) * page.PageSize : 0))).ToList();
        _entries = entries;
        View Card(TItem item, int position) => BuildCard(context, summary, selecting, item, position);
        var stack = new VerticalStackLayout { Spacing = 8 };
        if (selecting)
        {
            var all = TableBody.Box(Messages.SelectAllRows, context.PageSelection == true, selected =>
            {
                if (_syncing) return;
                context.SetPageSelected(selected);
            });
            all.Text = Messages.SelectAllRows;
            _headerBox = all;
            stack.Children.Add(all);
        }
        if (virtualized)
        {
            stack.Children.Add(BuildCollection(context, entries, Card, 0));
            return stack;
        }
        var rows = new VerticalStackLayout { Spacing = 8 };
        RowStack = rows;
        if (context.ActiveGroupBy is not null)
        {
            foreach (var group in context.Groups())
            {
                var header = Own(new KlyDataListGroupHeader<TItem> { Group = group }, context);
                rows.Children.Add(header);
                if (context.IsGroupCollapsed(group)) continue;
                foreach (var item in group.Items) rows.Children.Add(Card(item, entries.First(e => ReferenceEquals(e.Item, item)).Position));
            }
        }
        else
        {
            foreach (var entry in entries) rows.Children.Add(Card(entry.Item, entry.Position));
        }
        stack.Children.Add(rows);
        return stack;
    }

    private View BuildCard(DataListContext<TItem> context, DataListRowSummary<TItem> summary, bool selecting, TItem item, int position)
    {
        var messages = Messages;
        var grid = new Grid { ColumnSpacing = 12, ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] };
        if (selecting)
        {
            var key = context.RowKey(item);
            var box = TableBody.Box(TableSupport.Format(messages.SelectRowAt, position), context.IsKeySelected(key), selected =>
            {
                if (_syncing) return;
                context.SetSelected(item, selected);
            });
            box.VerticalOptions = LayoutOptions.Start;
            _boxes[key] = box;
            grid.Add(box, 0);
        }
        View body;
        if (summary.Template is { } template)
        {
            body = template(item);
        }
        else
        {
            var text = new VerticalStackLayout { Spacing = 4 };
            var title = TableSupport.NewLabel(16, KlyTheme.TextHighlighted, FontAttributes.Bold);
            title.Text = summary.Title?.Invoke(item) ?? context.Schema?.GetRowLabel(item) ?? FirstText(context, item);
            title.LineBreakMode = LineBreakMode.TailTruncation;
            SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level3);
            var head = new HorizontalStackLayout { Spacing = 8 };
            if (summary.Leading is { } leading) head.Children.Add(leading(item));
            head.Children.Add(title);
            text.Children.Add(head);
            var details = summary.Details is { } custom ? custom(item) : DefaultDetails(context, item);
            var joined = string.Join(" · ", details.Where(d => !string.IsNullOrWhiteSpace(d)));
            if (summary.Status is not null || joined.Length > 0)
            {
                var line = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
                if (summary.Status is { } status)
                {
                    var view = status(item);
                    view.Margin = new Thickness(0, 0, 8, 0);
                    line.Children.Add(view);
                }
                if (joined.Length > 0)
                {
                    var detail = TableSupport.NewLabel(14, KlyTheme.TextMuted);
                    detail.Text = joined;
                    line.Children.Add(detail);
                }
                text.Children.Add(line);
            }
            body = text;
        }
        grid.Add(body, 1);
        if (RowActions is { Count: > 0 }) grid.Add(NewRowActions(context, item, position), 2);
        var card = new Border { Content = grid, Padding = new Thickness(12, TableBody.RowPadding(context.Density) + 4) };
        card.Apply(new TableSurface(KlyThemeColor.Transparent, KlyTheme.Border, 1, KlyTheme.Text));
        card.SetRadius(KlyTheme.RadiusOf(1.5));
        var tap = new TableTap(card);
        tap.Tapped += (_, _) =>
        {
            summary.OnSelect?.Invoke(item);
            RaiseRowClick(context, item, position);
        };
        return tap;
    }

    private static string FirstText(DataListContext<TItem> context, TItem item)
    {
        var column = context.TableColumns().FirstOrDefault(c => c.ReadText(item, context.Schema, context.Culture) is { Length: > 0 });
        return column?.ReadText(item, context.Schema, context.Culture) ?? item?.ToString() ?? "";
    }

    private static IEnumerable<string?> DefaultDetails(DataListContext<TItem> context, TItem item)
    {
        var columns = context.TableColumns();
        var first = columns.FirstOrDefault(c => c.ReadText(item, context.Schema, context.Culture) is { Length: > 0 });
        return columns.Where(c => c != first).Select(c => c.ReadText(item, context.Schema, context.Culture));
    }

    // ---- the selection

    private void OnSelectionChanged()
    {
        if (Context is not { } context) return;
        _syncing = true;
        try
        {
            foreach (var (key, box) in _boxes) box.IsChecked = context.IsKeySelected(key);
            if (_headerBox is { } header) header.IsChecked = context.PageSelection == true;
            // The rows a virtualized list recycles are given the new state when it draws them again.
            if (_collection is { } collection) collection.ItemsSource = _entries.ToList();
        }
        finally
        {
            _syncing = false;
        }
    }
}
