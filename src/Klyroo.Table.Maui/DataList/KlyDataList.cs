using System.Collections.Specialized;

namespace Klyroo.Table.Maui;

/// <summary>
/// A data list: the root control that owns the state of a list (search, filters, sort, page, layout), loads its rows from one source (<see cref="Items"/>, <see cref="Queryable"/> or <see cref="Provider"/>)
/// with a <see cref="DataListController{TItem}"/>, and hands them to its parts. The parts (<see cref="KlyDataListTable{TItem}"/>, <see cref="KlyDataListGrid{TItem}"/>, <see cref="KlyDataListSearch{TItem}"/>,
/// <see cref="KlyDataListPagination{TItem}"/>...) are views you put in <see cref="ContentView.Content"/>, anywhere below the list: each finds the list above it. Without content the list builds a default layout
/// (search, result count, refresh, filter tags, error state, table, paging) from its <see cref="Schema"/>.
/// </summary>
/// <remarks>
/// The list starts when it loads on a page, or when <see cref="LoadAsync"/> is called. Give it a stable collection: a new reference loads the list again.
/// Parts that open an overlay (the filter popovers, the column panel, the action menus) need a <see cref="KlyOverlayHost"/> above the list.
/// </remarks>
/// <typeparam name="TItem">The row type.</typeparam>
[ContentProperty(nameof(Content))]
public class KlyDataList<TItem> : ContentView
{
    /// <summary>Identifies <see cref="Schema"/>.</summary>
    public static readonly BindableProperty SchemaProperty = BindableProperty.Create(nameof(Schema), typeof(DataListSchema<TItem>), typeof(KlyDataList<TItem>), null);
    /// <summary>Identifies <see cref="Items"/>.</summary>
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IEnumerable<TItem>), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.SourceChanged());
    /// <summary>Identifies <see cref="Queryable"/>.</summary>
    public static readonly BindableProperty QueryableProperty = BindableProperty.Create(nameof(Queryable), typeof(IQueryable<TItem>), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.SourceChanged());
    /// <summary>Identifies <see cref="Provider"/>.</summary>
    public static readonly BindableProperty ProviderProperty = BindableProperty.Create(nameof(Provider), typeof(DataListProvider<TItem>), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.SourceChanged());
    /// <summary>Identifies <see cref="Executor"/>.</summary>
    public static readonly BindableProperty ExecutorProperty = BindableProperty.Create(nameof(Executor), typeof(IDataListQueryExecutor), typeof(KlyDataList<TItem>), null);
    /// <summary>Identifies <see cref="State"/>.</summary>
    public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(DataListState), typeof(KlyDataList<TItem>), null, BindingMode.TwoWay, propertyChanged: (b, _, v) => ((KlyDataList<TItem>)b).OnStateSet((DataListState?)v));
    /// <summary>Identifies <see cref="Density"/>.</summary>
    public static readonly BindableProperty DensityProperty = BindableProperty.Create(nameof(Density), typeof(DataListDensity), typeof(KlyDataList<TItem>), DataListDensity.Default, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.NotifyChanged());
    /// <summary>Identifies <see cref="PagingMode"/>.</summary>
    public static readonly BindableProperty PagingModeProperty = BindableProperty.Create(nameof(PagingMode), typeof(DataListPagingMode), typeof(KlyDataList<TItem>), DataListPagingMode.Offset, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.PagingModeChanged());
    /// <summary>Identifies <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, v) => SemanticProperties.SetDescription((KlyDataList<TItem>)b, (string?)v));
    /// <summary>Identifies <see cref="Messages"/>.</summary>
    public static readonly BindableProperty MessagesProperty = BindableProperty.Create(nameof(Messages), typeof(KlyToolsMessages), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.NotifyChanged());
    /// <summary>Identifies <see cref="Selectable"/>.</summary>
    public static readonly BindableProperty SelectableProperty = BindableProperty.Create(nameof(Selectable), typeof(bool?), typeof(KlyDataList<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataList<TItem>)b).Context.NotifyChanged());
    /// <summary>Identifies <see cref="SelectionScope"/>.</summary>
    public static readonly BindableProperty SelectionScopeProperty = BindableProperty.Create(nameof(SelectionScope), typeof(DataListSelectionScope), typeof(KlyDataList<TItem>), DataListSelectionScope.Auto);
    /// <summary>Identifies <see cref="SelectedKeys"/>.</summary>
    public static readonly BindableProperty SelectedKeysProperty = BindableProperty.Create(nameof(SelectedKeys), typeof(IReadOnlySet<string>), typeof(KlyDataList<TItem>), null, BindingMode.TwoWay, propertyChanged: (b, _, v) => ((KlyDataList<TItem>)b).OnSelectedKeysSet((IReadOnlySet<string>?)v));

    private readonly CancellationTokenSource _lifetime = new();
    private bool _syncingState;
    private bool _syncingKeys;
    private bool _keysHinted;
    private bool _defaultBuilt;

    /// <summary>Creates a data list. Set <see cref="Schema"/> and one source, then put the parts in <see cref="ContentView.Content"/>.</summary>
    public KlyDataList()
    {
        TableIcons.Ensure();
        Context = new DataListContext<TItem>(this);
        DescendantAdded += (_, e) =>
        {
            if (e.Element is IDataListPart<TItem> part) part.Bind(Context);
        };
        DescendantRemoved += (_, e) =>
        {
            if (e.Element is IDataListPart<TItem> part) part.Bind(null);
        };
        Loaded += (_, _) => StartOnPage();
        Unloaded += (_, _) => Context.Stop();
    }

    /// <summary>Raised when the state changes: a search, a filter, a sort, a page, a page size or a layout.</summary>
    public event EventHandler<DataListState>? StateChanged;

    /// <summary>Raised with a new set when the user ticks or unticks rows, or when rows leave the selection because a new result no longer holds them.</summary>
    public event EventHandler<IReadOnlySet<string>>? SelectedKeysChanged;

    /// <summary>What the parts of the list read and change: the state, the rows and the loading flags.</summary>
    public DataListContext<TItem> Context { get; }

    /// <summary>
    /// What a request may ask for: which fields sort, which filter, the default sort and page size. The schema is fixed for the life of the list: set it before the list starts.
    /// Optional for a list of rows in memory whose <see cref="KlyDataListColumnBase{TItem}"/>s name a <c>Property</c>: those columns then describe the schema.
    /// A provider or a queryable needs a schema, because it is the schema that drops what a request must not ask for.
    /// </summary>
    public DataListSchema<TItem>? Schema { get => (DataListSchema<TItem>?)GetValue(SchemaProperty); set => SetValue(SchemaProperty, value); }

    /// <summary>The rows, kept in memory: the list sorts and pages them itself, with the semantics of <see cref="InMemoryDataList"/>. A list takes one source: <see cref="Items"/>, <see cref="Queryable"/> or <see cref="Provider"/>. A list that tells it changed (<see cref="INotifyCollectionChanged"/>) is loaded again.</summary>
    public IEnumerable<TItem>? Items { get => (IEnumerable<TItem>?)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

    /// <summary>A query of a database provider (an EF Core <c>DbSet</c>): the list turns each request into queries with <see cref="QueryableDataList"/> and runs them through <see cref="Executor"/>. Needs a <see cref="Schema"/>.</summary>
    public IQueryable<TItem>? Queryable { get => (IQueryable<TItem>?)GetValue(QueryableProperty); set => SetValue(QueryableProperty, value); }

    /// <summary>Loads a page of rows for a request, from a database, an HTTP API or anything else. It receives a token that is cancelled when a newer request replaces it.</summary>
    public DataListProvider<TItem>? Provider { get => (DataListProvider<TItem>?)GetValue(ProviderProperty); set => SetValue(ProviderProperty, value); }

    /// <summary>Runs the queries of <see cref="Queryable"/>; the synchronous one when <see langword="null"/>, which blocks the UI thread while the database answers: give an asynchronous one for a real database.</summary>
    public IDataListQueryExecutor? Executor { get => (IDataListQueryExecutor?)GetValue(ExecutorProperty); set => SetValue(ExecutorProperty, value); }

    /// <summary>The search, filters, sort, page and page size. Two-way bindable: a value you set (a reset button) loads the list.</summary>
    public DataListState? State { get => (DataListState?)GetValue(StateProperty); set => SetValue(StateProperty, value); }

    /// <summary>How tight the rows are.</summary>
    public DataListDensity Density { get => (DataListDensity)GetValue(DensityProperty); set => SetValue(DensityProperty, value); }

    /// <summary>
    /// How the list pages its rows: numbered pages (<see cref="DataListPagingMode.Offset"/>, the default), keyset cursor paging (<see cref="DataListPagingMode.Cursor"/>, with
    /// <see cref="KlyDataListInfiniteLoader{TItem}"/>) or no paging (<see cref="DataListPagingMode.None"/>, every matching row at once). Changing it starts the list again from its first rows.
    /// </summary>
    public DataListPagingMode PagingMode { get => (DataListPagingMode)GetValue(PagingModeProperty); set => SetValue(PagingModeProperty, value); }

    /// <summary>The name of the list. Its table is named by it (<c>Results</c> when it has none), and its pagination is "Pagination, label".</summary>
    public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>The texts of the parts. Without it the table of the language of the user (<see cref="KlyToolsLocales"/>); build a variant with a <c>with</c> expression.</summary>
    public KlyToolsMessages? Messages { get => (KlyToolsMessages?)GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }

    /// <summary>
    /// Shows a checkbox column (a header box that selects the rows of the page, a box per row). <see langword="null"/>, the default, shows it when the list has a
    /// <see cref="KlyDataListSelectionActions{TItem}"/> bar or <see cref="SelectedKeys"/> was set; <see langword="false"/> turns selection off whatever else is there. Selection needs a <c>RowKey</c> in the schema.
    /// </summary>
    public bool? Selectable { get => (bool?)GetValue(SelectableProperty); set => SetValue(SelectableProperty, value); }

    /// <summary>Whether the ticked rows stay ticked on other pages: see <see cref="DataListSelectionScope"/>.</summary>
    public DataListSelectionScope SelectionScope { get => (DataListSelectionScope)GetValue(SelectionScopeProperty); set => SetValue(SelectionScopeProperty, value); }

    /// <summary>The keys (<c>RowKey</c> of the schema, as text) of the ticked rows. Two-way bindable; give a new set when you change it. Keys the list cannot keep are dropped and reported through <see cref="SelectedKeysChanged"/>.</summary>
    public IReadOnlySet<string>? SelectedKeys { get => (IReadOnlySet<string>?)GetValue(SelectedKeysProperty); set => SetValue(SelectedKeysProperty, value); }

    internal CancellationToken LifetimeToken => _lifetime.Token;

    internal bool SelectedKeyHint => _keysHinted;

    /// <summary>
    /// Starts the list (it also starts when it loads on a page) and completes when the first load is over. Throws when the list has no source, or no schema and no column to describe one.
    /// </summary>
    public Task LoadAsync()
    {
        StartOnPage();
        return Context.WhenLoaded();
    }

    private void StartOnPage()
    {
        AttachParts(this);
        Context.Start();
        if (Content is null && !_defaultBuilt)
        {
            _defaultBuilt = true;
            Content = BuildDefaultContent();
        }
    }

    // The parts below the list that the tree events did not reach (a subtree that was built before it was added) find their list here.
    private void AttachParts(Element root)
    {
        if (root is IVisualTreeElement tree)
        {
            foreach (var child in tree.GetVisualChildren())
            {
                if (child is IDataListPart<TItem> part) part.Bind(Context);
                if (child is Element element) AttachParts(element);
            }
        }
    }

    private View BuildDefaultContent()
    {
        var schema = Context.Schema;
        var root = new VerticalStackLayout { Spacing = 12 };
        var toolbar = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
        if (schema is { HasSearch: true }) toolbar.Children.Add(new KlyDataListSearch<TItem> { Margin = new Thickness(0, 0, 8, 8) });
        toolbar.Children.Add(new KlyDataListResultCount<TItem> { Margin = new Thickness(0, 0, 8, 8) });
        toolbar.Children.Add(new KlyDataListRefresh<TItem> { Margin = new Thickness(0, 0, 8, 8) });
        root.Add(toolbar);
        if (schema is { FilterKeys.Count: > 0 }) root.Add(new KlyDataListFilterTags<TItem> { ShowAdd = true, ShowClear = true });
        root.Add(new KlyDataListErrorState<TItem>());
        root.Add(new KlyDataListTable<TItem>());
        if (PagingMode == DataListPagingMode.Cursor) root.Add(new KlyDataListInfiniteLoader<TItem>());
        else if (PagingMode == DataListPagingMode.Offset) root.Add(new KlyDataListPagination<TItem>());
        return root;
    }

    internal void SyncState(DataListState state)
    {
        if (_syncingState) return;
        _syncingState = true;
        try
        {
            SetValue(StateProperty, state);
            StateChanged?.Invoke(this, state);
        }
        finally
        {
            _syncingState = false;
        }
    }

    private void OnStateSet(DataListState? state)
    {
        if (_syncingState) return;
        Context.ExternalState(state);
    }

    internal void SyncSelectedKeys(IReadOnlySet<string> keys)
    {
        if (_syncingKeys) return;
        _syncingKeys = true;
        try
        {
            SetValue(SelectedKeysProperty, keys);
            SelectedKeysChanged?.Invoke(this, keys);
        }
        finally
        {
            _syncingKeys = false;
        }
    }

    private void OnSelectedKeysSet(IReadOnlySet<string>? keys)
    {
        _keysHinted = _keysHinted || keys is not null;
        if (_syncingKeys || keys is null) return;
        Context.ReplaceSelection(keys);
    }
}
