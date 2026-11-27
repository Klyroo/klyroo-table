using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// What the parts of a <see cref="KlyDataList{TItem}"/> (the table, the pagination, the result count...) read and change: the state of the list, its rows and its loading flags.
/// The list owns one for its whole life and hands it to its parts, so a part subscribes to <see cref="Changed"/> once and draws again when it is raised.
/// Write your own part by deriving from <see cref="DataListPartBase{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed partial class DataListContext<TItem>
{
    private readonly KlyDataList<TItem> _owner;
    private DataListSchema<TItem>? _schema;
    private DataListState _state = DataListState.Initial;
    private Task _loading = Task.CompletedTask;
    private bool _started;
    private IDisposable? _collectionWatch;
    private int _tableCount;
    private bool _autoLoadMore;
    private int _gridCount;

    internal DataListContext(KlyDataList<TItem> owner) => _owner = owner;

    /// <summary>Raised on the UI thread when the state, the rows or a flag change.</summary>
    public event Action? Changed;

    internal DataListController<TItem>? Controller { get; private set; }

    internal KlyDataList<TItem> Owner => _owner;

    internal bool IsStarted => _started;

    internal CancellationToken LifetimeToken => _owner.LifetimeToken;

    /// <summary>The name of the list, for the accessible names of its table and its pagination (<c>Label</c> of the list).</summary>
    public string? Label => _owner.Label;

    /// <summary>The texts of the parts, in the language of the user (or the <c>Messages</c> of the list).</summary>
    public KlyToolsMessages Messages => _owner.Messages ?? TableMessages.For(CultureInfo.CurrentUICulture);

    /// <summary>The culture numbers and dates are written in.</summary>
    public CultureInfo Culture => CultureInfo.CurrentCulture;

    /// <summary>The schema, once the list has one (an explicit one, or the one its columns describe once the list started).</summary>
    public DataListSchema<TItem>? Schema => _schema;

    /// <summary>The state the list shows: search, filters, sort, page, page size. Immutable; change it with <see cref="SetState"/>.</summary>
    public DataListState State => _state;

    /// <summary>The state <see cref="Result"/> was loaded for: the page and page size of the rows on screen.</summary>
    public DataListState? LoadedState => Controller?.LoadedState;

    /// <summary>The result on screen: the last one that loaded, kept while the next one loads. <see langword="null"/> before the first load.</summary>
    public DataListResult<TItem>? Result => Controller?.Result;

    /// <summary>The rows of <see cref="Result"/>; empty before the first load.</summary>
    public IReadOnlyList<TItem> Rows => Controller?.Rows ?? [];

    /// <summary>Counts the results shown so far; it does not change when only a flag does, which lets rows skip drawing.</summary>
    public int ResultVersion => Controller?.ResultVersion ?? 0;

    /// <summary>No result yet and nothing failed: show placeholders.</summary>
    public bool IsInitial => Controller?.IsInitial ?? true;

    /// <summary>A load is running.</summary>
    public bool IsLoading => Controller?.IsLoading ?? false;

    /// <summary>The rows on screen are being loaded again (refresh or retry).</summary>
    public bool IsRefreshing => Controller?.IsRefreshing ?? false;

    /// <summary>Other rows are being loaded (a new page, sort or filter) while the previous ones stay on screen.</summary>
    public bool IsReplacing => Controller?.IsReplacing ?? false;

    /// <summary>The next page of a cursor-paged list is being loaded to append to the rows on screen.</summary>
    public bool IsLoadingMore => Controller?.IsLoadingMore ?? false;

    /// <summary>Whether the source has another page after the rows loaded so far (cursor paging only).</summary>
    public bool HasMore => Controller?.HasMore ?? false;

    /// <summary>The opaque cursor for the next page (cursor paging only), or <see langword="null"/> after the last one.</summary>
    public string? NextCursor => Controller?.NextCursor;

    /// <summary>The newest load-more request failed while the previously loaded rows stayed on screen.</summary>
    public bool HasLoadMoreError => Controller?.HasLoadMoreError ?? false;

    /// <summary>Why the newest load-more request failed; <see langword="null"/> when it did not.</summary>
    public Exception? LoadMoreError => Controller?.LoadMoreError;

    /// <summary>The newest load failed. The previous rows, if any, are still in <see cref="Rows"/>.</summary>
    public bool HasError => Controller?.HasError ?? false;

    /// <summary>Why the newest load failed.</summary>
    public Exception? Error => Controller?.Error;

    /// <summary>How the list pages its rows.</summary>
    public DataListPagingMode PagingMode => _owner.PagingMode;

    /// <summary>The density of the rows.</summary>
    public DataListDensity Density => _owner.Density;

    /// <summary>The rows have a search or filters applied, so an empty result means "nothing matches" rather than "nothing exists".</summary>
    public bool IsFiltered => IsFilteredFor(_state);

    /// <summary>Whether <paramref name="state"/> has a search or filters applied.</summary>
    public bool IsFilteredFor(DataListState state) =>
        !string.IsNullOrWhiteSpace(state.Search) || (_schema is not null && !state.Filters.SequenceEqual(_schema.DefaultState.Filters));

    /// <summary>
    /// Changes the state: <paramref name="change"/> receives the current one and returns the new one. The list loads the rows of the new state, unless only the layout changed.
    /// The new state is reported through <see cref="KlyDataList{TItem}.StateChanged"/>.
    /// </summary>
    /// <param name="change">Builds the new state from the current one.</param>
    public void SetState(Func<DataListState, DataListState> change)
    {
        ArgumentNullException.ThrowIfNull(change);
        var proposed = change(_state);
        var next = Controller is { } controller ? controller.Normalize(proposed) : proposed;
        if (next == _state) return;
        _state = next;
        _owner.SyncState(next);
        if (Controller is null)
        {
            NotifyChanged();
            return;
        }
        if (Controller.TryAdoptLayout(next)) return;
        _loading = ReloadAsync();
    }

    /// <summary>Takes the search and the filters off and goes back to the first page (the sort, the page size and the layout stay).</summary>
    public void Reset() => SetState(ResetState);

    /// <summary>Resets the search and filters to the schema defaults (on page 1) and clears the row selection.</summary>
    public void ResetAll()
    {
        ClearSelection();
        SetState(ResetState);
    }

    /// <summary><paramref name="state"/> without its search and its filters (those of the schema's default state), on the first page: what <see cref="Reset"/> sets.</summary>
    public DataListState ResetState(DataListState state) =>
        state with { Search = "", Filters = _schema?.DefaultState.Filters ?? [], Page = 1, Cursor = null };

    /// <summary>Replaces a single row in the loaded result in place by its schema <c>RowKey</c>, without querying the source again.</summary>
    public bool UpdateRow(TItem item) => item is not null && UpdateRows([item]) > 0;

    /// <summary>Replaces matching rows in the loaded result in place by their schema <c>RowKey</c>, without querying the source again.</summary>
    public int UpdateRows(IEnumerable<TItem> items)
    {
        if (Controller is null) return 0;
        var updated = Controller.UpdateRows(items);
        if (updated > 0) ReconcileSelection();
        return updated;
    }

    /// <summary>Loads the rows on screen again, or the failed request again after an error.</summary>
    public async Task RefreshAsync()
    {
        if (Controller is null) return;
        await Controller.RefreshAsync();
        AfterLoad();
    }

    /// <summary>Appends the next page of a cursor-paged list. Does nothing outside cursor mode, while a load is running or when there is no next cursor.</summary>
    public Task LoadMoreAsync() => Controller?.LoadMoreAsync() ?? Task.CompletedTask;

    /// <summary>Completes when the load in progress is over.</summary>
    internal Task WhenLoaded() => _loading;

    /// <summary>A loader asked for the next page to be fetched when the rows run out: a virtualized table calls <see cref="LoadMoreAsync"/> when it scrolls near its end.</summary>
    internal bool AutoLoadMore
    {
        get => _autoLoadMore;
        set => _autoLoadMore = value;
    }

    internal void NotifyChanged() => TableSupport.Post(_owner, () => Changed?.Invoke());

    // ---- life of the list

    internal void Start()
    {
        if (_started) return;
        _started = true;
        _schema = ResolveSchema();
        var provider = DataListController<TItem>.CreateProvider(_schema, _owner.Items, _owner.Provider, _owner.Queryable, _owner.Executor);
        Controller = new DataListController<TItem>(_schema, provider)
        {
            PagingMode = _owner.PagingMode,
            TimeZone = TimeZoneInfo.Local.Id,
        };
        Controller.Changed += NotifyChanged;
        _state = Controller.Normalize(_owner.State ?? _schema.DefaultState);
        WatchItems(_owner.Items);
        _loading = ReloadAsync();
    }

    internal void Stop()
    {
        if (!_started) return;
        Controller?.Cancel();
    }

    /// <summary>The source of the list changed after the start: build the provider again and load from the first rows.</summary>
    internal void SourceChanged()
    {
        if (Controller is null) return;
        Controller.Provider = DataListController<TItem>.CreateProvider(_schema!, _owner.Items, _owner.Provider, _owner.Queryable, _owner.Executor);
        Controller.DropFacets();
        WatchItems(_owner.Items);
        _loading = ReloadAsync();
    }

    /// <summary>The paging mode changed after the start: start again from the first rows.</summary>
    internal void PagingModeChanged()
    {
        if (Controller is null) return;
        Controller.PagingMode = _owner.PagingMode;
        _state = Controller.Normalize(_state with { Page = 1, Cursor = null });
        _owner.SyncState(_state);
        _loading = ReloadAsync();
    }

    /// <summary>The state was set from outside (a binding): follow it.</summary>
    internal void ExternalState(DataListState? state)
    {
        if (Controller is null || state is null) return;
        SetState(_ => state);
    }

    private DataListSchema<TItem> ResolveSchema()
    {
        var sources = (_owner.Items is not null ? 1 : 0) + (_owner.Queryable is not null ? 1 : 0) + (_owner.Provider is not null ? 1 : 0);
        if (sources > 1) throw new InvalidOperationException($"{nameof(KlyDataList<TItem>)} takes one source: give it Items, a Queryable or a Provider, not several.");
        if (sources == 0) throw new InvalidOperationException($"{nameof(KlyDataList<TItem>)} needs a source: give it Items, a Queryable or a Provider.");
        if (_owner.Schema is { } explicitSchema) return explicitSchema;
        if (_owner.Provider is not null || _owner.Queryable is not null)
        {
            throw new InvalidOperationException($"{nameof(KlyDataList<TItem>)} with a Provider or a Queryable needs a Schema: it is the schema that drops what a request must not ask for.");
        }
        var describing = _columns.Where(c => c.DescribesField).ToList();
        if (describing.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(KlyDataList<TItem>)} needs a Schema, or columns that name a Property: the schema says which fields sort and filter.");
        }
        return DataListSchema.For<TItem>(schema =>
        {
            foreach (var column in describing) column.AddImplicitField(schema);
        });
    }

    private void WatchItems(IEnumerable<TItem>? items)
    {
        _collectionWatch?.Dispose();
        _collectionWatch = null;
        if (items is System.Collections.Specialized.INotifyCollectionChanged observable)
        {
            void Handler(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => TableSupport.Post(_owner, () =>
            {
                Controller?.DropFacets();
                _loading = ReloadAsync();
            });
            observable.CollectionChanged += Handler;
            _collectionWatch = new Unsubscribe(() => observable.CollectionChanged -= Handler);
        }
    }

    private sealed class Unsubscribe(Action action) : IDisposable
    {
        public void Dispose() => action();
    }

    internal async Task ReloadAsync()
    {
        var controller = Controller;
        if (controller is null) return;
        await controller.LoadAsync(_state);
        AfterLoad();
    }

    private void AfterLoad()
    {
        if (Controller is { LoadedState: { } loaded } && loaded.Page != _state.Page && loaded.PageSize == _state.PageSize)
        {
            _state = _state with { Page = loaded.Page };
            _owner.SyncState(_state);
        }
        ReconcileSelection();
        NotifyChanged();
    }

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

    /// <summary>Whether the list contains a <see cref="KlyDataListTable{TItem}"/>.</summary>
    internal bool HasTable => _tableCount > 0;

    /// <summary>Whether the list contains a <see cref="KlyDataListGrid{TItem}"/>.</summary>
    internal bool HasGrid => _gridCount > 0;

    /// <summary>The layout currently shown: <see cref="DataListState.Layout"/> when both table and grid exist, otherwise the one that is present.</summary>
    public DataListLayout EffectiveLayout =>
        HasGrid && !HasTable ? DataListLayout.Grid
        : HasTable && !HasGrid ? DataListLayout.Table
        : _state.Layout;

    /// <summary>Switches the active layout (<see cref="DataListLayout.Table"/> or <see cref="DataListLayout.Grid"/>).</summary>
    /// <param name="layout">The layout to show.</param>
    public void SetLayout(DataListLayout layout)
    {
        if (_state.Layout != layout) SetState(s => s with { Layout = layout });
    }

    /// <summary>The state with <see cref="DataListState.Layout"/> set to <paramref name="layout"/>.</summary>
    public DataListState WithLayout(DataListState state, DataListLayout layout) => state with { Layout = layout };
}
