namespace Klyroo.Table.Maui;

/// <summary>
/// The grid layout of a data list: each row is a card (<see cref="ItemTemplate"/>, or a default list of the label and the text of each visible column or field of the schema), with the selection checkbox of the list, the actions of the row,
/// optional groups, placeholders while the first rows load, and an empty state. The cards flow in up to <see cref="Columns"/> columns on wide screens and one on a phone; a long list is virtualized in a <see cref="CollectionView"/>.
/// Next to a <see cref="KlyDataListTable{TItem}"/> only the layout the list shows is drawn (see <see cref="KlyDataListLayoutSwitch{TItem}"/>).
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListGrid<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="ItemTemplate"/>.</summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(Func<TItem, View>), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="SkeletonTemplate"/>.</summary>
    public static readonly BindableProperty SkeletonTemplateProperty = BindableProperty.Create(nameof(SkeletonTemplate), typeof(Func<View>), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Empty"/>.</summary>
    public static readonly BindableProperty EmptyProperty = BindableProperty.Create(nameof(Empty), typeof(Func<DataListContext<TItem>, View>), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="EmptyState"/>.</summary>
    public static readonly BindableProperty EmptyStateProperty = BindableProperty.Create(nameof(EmptyState), typeof(DataListEmptyState), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowProgressLine"/>.</summary>
    public static readonly BindableProperty ShowProgressLineProperty = BindableProperty.Create(nameof(ShowProgressLine), typeof(bool), typeof(KlyDataListGrid<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).UpdateProgress());
    /// <summary>Identifies <see cref="Columns"/>.</summary>
    public static readonly BindableProperty ColumnsProperty = BindableProperty.Create(nameof(Columns), typeof(int), typeof(KlyDataListGrid<TItem>), 3, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowSelectionControl"/>.</summary>
    public static readonly BindableProperty ShowSelectionControlProperty = BindableProperty.Create(nameof(ShowSelectionControl), typeof(bool), typeof(KlyDataListGrid<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="RowActions"/>.</summary>
    public static readonly BindableProperty RowActionsProperty = BindableProperty.Create(nameof(RowActions), typeof(IReadOnlyList<DataListRowAction<TItem>>), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="RowActionsMaxVisible"/>.</summary>
    public static readonly BindableProperty RowActionsMaxVisibleProperty = BindableProperty.Create(nameof(RowActionsMaxVisible), typeof(int), typeof(KlyDataListGrid<TItem>), 0, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="GroupBy"/>.</summary>
    public static readonly BindableProperty GroupByProperty = BindableProperty.Create(nameof(GroupBy), typeof(DataListGroupBy<TItem>), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, v) =>
    {
        var grid = (KlyDataListGrid<TItem>)b;
        grid.Context?.SetGridGroupBy((DataListGroupBy<TItem>?)v);
        grid.Redraw();
    });
    /// <summary>Identifies <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(KlyDataListGrid<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="VirtualizeThreshold"/>.</summary>
    public static readonly BindableProperty VirtualizeThresholdProperty = BindableProperty.Create(nameof(VirtualizeThreshold), typeof(int), typeof(KlyDataListGrid<TItem>), 60, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="VirtualHeight"/>.</summary>
    public static readonly BindableProperty VirtualHeightProperty = BindableProperty.Create(nameof(VirtualHeight), typeof(double), typeof(KlyDataListGrid<TItem>), 520d, propertyChanged: (b, _, _) => ((KlyDataListGrid<TItem>)b).Redraw());

    private readonly VerticalStackLayout _root = new();
    private readonly BoxView _progress = TableBody.ProgressLine();
    private readonly ContentView _bodyHost = new();
    private readonly Dictionary<string, KlyCheckbox> _boxes = new(StringComparer.Ordinal);
    private readonly List<DataListPartBase<TItem>> _owned = [];
    private KlyCheckbox? _headerBox;
    private CollectionView? _collection;
    private IReadOnlyList<TableRowEntry<TItem>> _entries = [];
    private object? _signature;
    private bool _refreshing;
    private bool _syncing;

    /// <summary>Creates a grid.</summary>
    public KlyDataListGrid()
    {
        _root.Children.Add(_progress);
        _root.Children.Add(_bodyHost);
        Content = _root;
        SizeChanged += (_, _) =>
        {
            if (Context is not null && Width > 0) Redraw();
        };
        Refresh();
    }

    /// <summary>Raised when a row action of a card threw or reported a failure.</summary>
    public event EventHandler<DataListRowActionFailure<TItem>>? RowActionError;

    /// <summary>Builds the body of each card. When <see langword="null"/>, the card lists the visible columns or schema fields as label and value pairs.</summary>
    public Func<TItem, View>? ItemTemplate { get => (Func<TItem, View>?)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    /// <summary>Builds each placeholder card shown while the first rows load.</summary>
    public Func<View>? SkeletonTemplate { get => (Func<View>?)GetValue(SkeletonTemplateProperty); set => SetValue(SkeletonTemplateProperty, value); }

    /// <summary>Builds the view shown when the list has no rows, instead of <see cref="EmptyState"/> and the default one.</summary>
    public Func<DataListContext<TItem>, View>? Empty { get => (Func<DataListContext<TItem>, View>?)GetValue(EmptyProperty); set => SetValue(EmptyProperty, value); }

    /// <summary>The unfiltered empty state: icon, title, description and a first-use action. A search or a filter that matches nothing shows the filtered one instead.</summary>
    public DataListEmptyState? EmptyState { get => (DataListEmptyState?)GetValue(EmptyStateProperty); set => SetValue(EmptyStateProperty, value); }

    /// <summary>Draws a thin line on top of the cards while they are refreshed or replaced (<see langword="true"/> by default).</summary>
    public bool ShowProgressLine { get => (bool)GetValue(ShowProgressLineProperty); set => SetValue(ShowProgressLineProperty, value); }

    /// <summary>The most columns of cards on wide screens, from 1 to 4 (3 by default). A phone shows one, a tablet at most two.</summary>
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }

    /// <summary>Draws a checkbox on each card and a select-all above them when the selection is on (<see langword="true"/> by default).</summary>
    public bool ShowSelectionControl { get => (bool)GetValue(ShowSelectionControlProperty); set => SetValue(ShowSelectionControlProperty, value); }

    /// <summary>Actions on each card (<see cref="KlyDataListRowActions{TItem}"/>).</summary>
    public IReadOnlyList<DataListRowAction<TItem>>? RowActions { get => (IReadOnlyList<DataListRowAction<TItem>>?)GetValue(RowActionsProperty); set => SetValue(RowActionsProperty, value); }

    /// <summary>How many row actions show as buttons on each card (0 by default, which keeps one menu button per card).</summary>
    public int RowActionsMaxVisible { get => (int)GetValue(RowActionsMaxVisibleProperty); set => SetValue(RowActionsMaxVisibleProperty, value); }

    /// <summary>Visual grouping of the cards of the page under collapsible <see cref="KlyDataListGroupHeader{TItem}"/> headers. Grouped cards are not virtualized.</summary>
    public DataListGroupBy<TItem>? GroupBy { get => (DataListGroupBy<TItem>?)GetValue(GroupByProperty); set => SetValue(GroupByProperty, value); }

    /// <summary>The accessible name of the grid. Defaults to the label of the list, then the localized "Results".</summary>
    public string? AccessibleName { get => (string?)GetValue(AccessibleNameProperty); set => SetValue(AccessibleNameProperty, value); }

    /// <summary>The number of cards from which the grid is virtualized (60 by default).</summary>
    public int VirtualizeThreshold { get => (int)GetValue(VirtualizeThresholdProperty); set => SetValue(VirtualizeThresholdProperty, value); }

    /// <summary>The height of the virtualized cards, in points (520 by default).</summary>
    public double VirtualHeight { get => (double)GetValue(VirtualHeightProperty); set => SetValue(VirtualHeightProperty, value); }

    internal View? Body => _bodyHost.Content;

    internal BoxView ProgressLine => _progress;

    internal CollectionView? VirtualList => _collection;

    internal IReadOnlyDictionary<string, KlyCheckbox> CardBoxes => _boxes;

    internal KlyCheckbox? HeaderBox => _headerBox;

    internal int SpanFor(double width)
    {
        var columns = Math.Clamp(Columns, 1, 4);
        if (width <= 0) return columns;
        if (width < TableSupport.PhoneWidth) return 1;
        return width < 900 ? Math.Min(2, columns) : columns;
    }

    internal int Span => SpanFor(Width);

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        context.SetGridGroupBy(GroupBy);
        context.SelectionChanged += OnSelectionChanged;
        context.AddGrid();
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.SelectionChanged -= OnSelectionChanged;
        context.SetGridGroupBy(null);
        context.RemoveGrid();
        ReleaseOwned();
        _signature = null;
    }

    private void UpdateProgress()
    {
        _progress.IsVisible = ShowProgressLine && Context is { IsRefreshing: true } or { IsReplacing: true };
    }

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
        UpdateProgress();
        SemanticProperties.SetDescription(_root, AccessibleName ?? context.Label ?? Messages.Results);
        var span = Span;
        var virtualized = context.Rows.Count > VirtualizeThreshold && context.ActiveGroupBy is null && !context.IsInitial;
        var signature = (context.ResultVersion, context.IsInitial, context.HasError, context.ColumnsVersion, context.GroupsVersion, context.EffectiveLayout, span, virtualized, context.SelectionEnabled && ShowSelectionControl,
            context.Density, RowActions, RowActionsMaxVisible, ItemTemplate, context.Rows.Count == 0 && context.Result is not null, context.IsFilteredFor(context.State), System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(context.Messages), context.ActiveGroupBy, EmptyState, Empty, SkeletonTemplate);
        if (_bodyHost.Content is not null && Equals(signature, _signature)) return;
        _signature = signature;
        BuildBody(context, span, virtualized);
    }

    // The parts the cards were built with (group headers, row actions) listen to the list: they stop when the cards are drawn again.
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

    private void BuildBody(DataListContext<TItem> context, int span, bool virtualized)
    {
        ReleaseOwned();
        _boxes.Clear();
        _headerBox = null;
        _collection = null;
        if (context.EffectiveLayout != DataListLayout.Grid)
        {
            _bodyHost.Content = null;
            return;
        }
        if (context.IsInitial)
        {
            _bodyHost.Content = Skeleton(context, span);
            return;
        }
        if (context.Result is not null && context.Rows.Count == 0 && !context.HasError)
        {
            _bodyHost.Content = TableBody.Empty(context, EmptyState, Empty);
            return;
        }
        var selecting = context.SelectionEnabled && ShowSelectionControl;
        var page = context.LoadedState ?? context.State;
        var entries = context.Rows.Select((item, index) => new TableRowEntry<TItem>(item, index + 1 + (context.PagingMode == DataListPagingMode.Offset ? (page.Page - 1) * page.PageSize : 0))).ToList();
        _entries = entries;
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
        View Card(TItem item, int position) => BuildCard(context, selecting, item, position);
        if (virtualized)
        {
            var collection = new CollectionView
            {
                ItemsSource = entries,
                ItemTemplate = new DataTemplate(() => new TableRowHost<TItem>(Card)),
                ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 8, HorizontalItemSpacing = 8 },
                SelectionMode = SelectionMode.None,
                HeightRequest = VirtualHeight,
                RemainingItemsThreshold = 5,
            };
            collection.RemainingItemsThresholdReached += (_, _) =>
            {
                if (context.AutoLoadMore && context.HasMore && !context.IsLoadingMore) _ = context.LoadMoreAsync();
            };
            _collection = collection;
            stack.Children.Add(collection);
        }
        else if (context.ActiveGroupBy is not null)
        {
            foreach (var group in context.Groups())
            {
                var header = Own(new KlyDataListGroupHeader<TItem> { Group = group }, context);
                stack.Children.Add(header);
                if (context.IsGroupCollapsed(group)) continue;
                stack.Children.Add(Flow(group.Items.Select(item => Card(item, entries.First(e => ReferenceEquals(e.Item, item)).Position)), span));
            }
        }
        else
        {
            stack.Children.Add(Flow(entries.Select(e => Card(e.Item, e.Position)), span));
        }
        _bodyHost.Content = stack;
    }

    private static Grid Flow(IEnumerable<View> cards, int span)
    {
        var grid = new Grid { ColumnSpacing = 8, RowSpacing = 8 };
        for (var c = 0; c < span; c++) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        var index = 0;
        foreach (var card in cards)
        {
            var row = index / span;
            if (index % span == 0) grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.Add(card, index % span, row);
            index++;
        }
        return grid;
    }

    private View Skeleton(DataListContext<TItem> context, int span)
    {
        var cards = Enumerable.Range(0, Math.Clamp(context.State.PageSize, 3, 9)).Select(_ =>
        {
            View inner = SkeletonTemplate?.Invoke() ?? new VerticalStackLayout { Spacing = 6, Children = { TableBody.Bar(140, 20), TableBody.Bar(200), TableBody.Bar(160) } };
            var card = new Border { Content = inner, Padding = new Thickness(12) };
            card.Apply(new TableSurface(KlyThemeColor.Transparent, KlyTheme.Border, 1, KlyTheme.Text));
            card.SetRadius(KlyTheme.RadiusOf(1.5));
            return (View)card;
        });
        var grid = Flow(cards, span);
        AutomationProperties.SetIsInAccessibleTree(grid, false);
        return grid;
    }

    private View BuildCard(DataListContext<TItem> context, bool selecting, TItem item, int position)
    {
        var messages = Messages;
        var head = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], ColumnSpacing = 8 };
        if (selecting)
        {
            var key = context.RowKey(item);
            var box = TableBody.Box(TableSupport.Format(messages.SelectRowAt, position), context.IsKeySelected(key), selected =>
            {
                if (_syncing) return;
                context.SetSelected(item, selected);
            });
            _boxes[key] = box;
            head.Add(box, 0);
        }
        if (RowActions is { Count: > 0 })
        {
            var part = new KlyDataListRowActions<TItem> { Item = item, Position = position, Actions = RowActions, MaxVisible = RowActionsMaxVisible, HorizontalOptions = LayoutOptions.End };
            part.Error += (_, e) => RowActionError?.Invoke(this, e);
            head.Add(Own(part, context), 2);
        }
        var stack = new VerticalStackLayout { Spacing = 8 };
        if (selecting || RowActions is { Count: > 0 }) stack.Children.Add(head);
        if (ItemTemplate is { } template)
        {
            stack.Children.Add(template(item));
        }
        else
        {
            var fields = TableBody.Fields(context, item);
            var list = new VerticalStackLayout { Spacing = 4 };
            foreach (var (label, text) in fields)
            {
                var row = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)], ColumnSpacing = 8 };
                var name = TableSupport.NewLabel(12, KlyTheme.TextMuted);
                name.Text = label;
                var value = TableSupport.NewLabel(14, KlyTheme.Text);
                value.Text = text;
                value.HorizontalTextAlignment = TextAlignment.End;
                value.LineBreakMode = LineBreakMode.TailTruncation;
                row.Add(name, 0);
                row.Add(value, 1);
                list.Children.Add(row);
            }
            stack.Children.Add(list);
        }
        var card = new Border { Content = stack, Padding = new Thickness(12) };
        card.Apply(new TableSurface(KlyThemeColor.Transparent, KlyTheme.Border, 1, KlyTheme.Text));
        card.SetRadius(KlyTheme.RadiusOf(1.5));
        return card;
    }

    private void OnSelectionChanged()
    {
        if (Context is not { } context) return;
        _syncing = true;
        try
        {
            foreach (var (key, box) in _boxes) box.IsChecked = context.IsKeySelected(key);
            if (_headerBox is { } header) header.IsChecked = context.PageSelection == true;
            if (_collection is { } collection) collection.ItemsSource = _entries.ToList();
        }
        finally
        {
            _syncing = false;
        }
    }
}
