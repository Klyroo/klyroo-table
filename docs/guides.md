# Guides

Each guide solves one task and assumes you finished [Getting started](getting-started.md): the services are registered, the imports are in `_Imports.razor` and the stylesheet is set up. The samples use a small `Order` model; the guides that need more say so.

- [Load rows from your own API](#load-rows-from-your-own-api)
- [Query a database with EF Core](#query-a-database-with-ef-core)
- [Filter rows and show counts](#filter-rows-and-show-counts)
- [Keep the state in the URL, or set it from code](#keep-the-state-in-the-url-or-set-it-from-code), including [remembered filters](#remember-a-filter-between-visits)
- [Select rows and run actions](#select-rows-and-run-actions)
- [Group rows, total columns and show a tree](#group-rows-total-columns-and-show-a-tree)
- [Page by cursor and virtualize a long table](#page-by-cursor-and-virtualize-a-long-table)
- [Let users choose their columns](#let-users-choose-their-columns)
- [Change the texts, the look and the delays](#change-the-texts-the-look-and-the-delays)
- [Test your lists](#test-your-lists)

## Load rows from your own API

Goal: a list whose rows come from an HTTP endpoint, with search, sort and paging done on the server.

Give the list a `Provider` instead of `Items`. The list calls it with a `DataListRequest` (the page, sort, filters and search of the current state) and expects a `DataListResult<T>` back.

Start with what the client and the server share: the row type, its schema, and, for the client, a `JsonSerializerContext` that lists `DataListResult<Order>` so that reading the result needs no reflection. Put them in a library project that both reference, so the two sides cannot drift apart: the client needs the schema to know which fields sort and filter, and the server needs it to sanitize requests.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Klyroo.Query;

public sealed record Order(int Id, string Customer, string Country, decimal Total);

public static class OrderSchema
{
    public static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Search(o => o.Customer, o => o.Country)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Country, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DataListResult<Order>))]
public sealed partial class OrderJson : JsonSerializerContext;
```

On the client, post the request and read the result:

```razor
@page "/orders/api"
@inject HttpClient Http

<KlyDataList TItem="Order" Schema="OrderSchema.Schema" Provider="LoadAsync" Label="Orders" />

@code {
    private async ValueTask<DataListResult<Order>> LoadAsync(DataListRequest request, CancellationToken cancellationToken)
    {
        using var response = await Http.PostAsJsonAsync("api/orders/query", request, DataListJsonContext.Default.DataListRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync(OrderJson.Default.DataListResultOrder, cancellationToken);
        return result ?? new DataListResult<Order>([], 0, null, null);
    }
}
```

On the server, a minimal API endpoint runs the request against its data with the same schema. The schema drops what a request must not ask for (unknown fields, operators a filter does not allow, page sizes past the maximum) before it runs it:

```csharp
// Program.cs
using Klyroo.Query;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Order[] orders = [new(1, "Acme", "France", 1200m), new(2, "Globex", "Germany", 800m)];

app.MapPost("/api/orders/query", (DataListRequest request) => OrderSchema.Schema.ExecuteInMemory(orders, request));

app.Run();
```

The token you receive is cancelled when a newer request replaces the one in flight (the user typed another letter), so pass it to the `HttpClient` and the stale request stops. `ExecuteInMemory` is the in-memory executor of Klyroo.Query; replace it with `schema.Apply(query, request)` over an `IQueryable` when the endpoint reads a database.

## Query a database with EF Core

Goal: a list over a `DbSet`, with the counting and loading done asynchronously by EF Core.

Give the list a `Queryable`. It turns each request into queries with the schema and runs them through the `IDataListQueryExecutor` in the service container. The default executor calls `Count()` and `ToList()` and blocks the calling thread while the database answers: in Blazor Server that is the thread of the circuit. For a real database, register an executor that awaits EF Core:

```csharp
using Klyroo.Table;
using Microsoft.EntityFrameworkCore;

public sealed class EfDataListQueryExecutor : IDataListQueryExecutor
{
    public async ValueTask<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        await query.CountAsync(cancellationToken);

    public async ValueTask<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        await query.ToListAsync(cancellationToken);
}
```

The entity and the context are ordinary EF Core code:

```csharp
using Microsoft.EntityFrameworkCore;

public sealed class Order
{
    public int Id { get; set; }
    public string Customer { get; set; } = "";
    public string Country { get; set; } = "";
    public decimal Total { get; set; }
}

public sealed class ShopContext(DbContextOptions<ShopContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
```

Register the executor next to `AddKlyrooTable()`. `AddKlyrooTable()` registers the synchronous executor only when none is registered, so the order of the two calls does not matter:

```csharp
// Program.cs
using Klyroo.Table;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKlyrooTable();
builder.Services.AddSingleton<IDataListQueryExecutor, EfDataListQueryExecutor>();
builder.Services.AddDbContextFactory<ShopContext>(o => o.UseSqlite("Data Source=shop.db"));
```

In the page, create a context for the list and keep the query in a field. A `DbContext` runs one query at a time, so a list needs a context of its own (a page with several lists needs one per list), and the list loads again whenever the `Queryable` reference changes, so an expression like `Db.Orders.AsNoTracking()` in the markup would reload the list on every render of the page:

```razor
@page "/orders/db"
@using Microsoft.EntityFrameworkCore
@implements IAsyncDisposable
@inject IDbContextFactory<ShopContext> Factory

<KlyDataList TItem="Order" Schema="Schema" Queryable="_orders" Label="Orders" />

@code {
    private ShopContext? _db;
    private IQueryable<Order> _orders = default!;

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Search(o => o.Customer, o => o.Country)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Country, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable())
        .DefaultSort(o => o.Id));

    protected override void OnInitialized()
    {
        _db = Factory.CreateDbContext();
        _orders = _db.Orders.AsNoTracking();
    }

    public async ValueTask DisposeAsync()
    {
        if (_db is not null) await _db.DisposeAsync();
    }
}
```

Building the query in `OnInitialized` means it exists before the list first renders. The list rejects an `AsQueryable()` over an in-memory collection, because its expressions would throw on a null along a path where a database returns null: use `Items` for rows that are already in memory. Text filters follow the collation of the database column, while the in-memory executor ignores case and accents.

## Filter rows and show counts

Goal: filters on several kinds of fields, with the number of rows behind each option.

Filters are declared on the schema fields. Five kinds exist: `DataListFilter.Text()`, `Option()`, `Boolean()`, `Number()` and `Date()`. `WithFacets()` on an option or boolean filter makes the list ask for the row count of each option and show it next to the option.

```razor
@page "/orders/filters"

<KlyDataList TItem="Order" Schema="Schema" Items="Orders" Label="Orders">
    <div class="grid gap-4 md:grid-cols-[16rem_1fr]">
        <KlyDataListFilterPanel TItem="Order" Description="Narrow the orders" />
        <div class="flex flex-col gap-3">
            <KlyDataListSearch TItem="Order" />
            <KlyDataListFilterTags TItem="Order" ShowClear="true" />
            <KlyDataListTable TItem="Order">
                <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
                <KlyDataListColumn TItem="Order" Field="country" Title="Country" />
                <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" Align="DataListAlign.End" />
                <KlyDataListColumn TItem="Order" Field="placed" Title="Placed" Format="d" />
            </KlyDataListTable>
            <KlyDataListPagination TItem="Order" />
        </div>
    </div>
</KlyDataList>

@code {
    private sealed record Order(int Id, string Customer, string Country, decimal Total, bool Paid, DateOnly Placed);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Search(o => o.Customer)
        .Field(o => o.Customer, f => f.Sortable().Filter(DataListFilter.Text()))
        .Field(o => o.Country, f => f.Sortable().Filter(DataListFilter.Option().WithFacets().WithOptionLimit(8)))
        .Field(o => o.Total, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(o => o.Paid, f => f.Filter(DataListFilter.Boolean().WithFacets()))
        .Field(o => o.Placed, f => f.Sortable().Filter(DataListFilter.Date()))
        .DefaultSort(o => o.Placed, descending: true));

    private static readonly Order[] Orders =
    [
        new(1, "Acme", "France", 1200m, true, new DateOnly(2026, 11, 2)),
        new(2, "Globex", "Germany", 800m, false, new DateOnly(2026, 11, 9)),
        new(3, "Initech", "France", 450m, true, new DateOnly(2026, 11, 20)),
    ];
}
```

The filter tags show one tag per active filter and open its editor in a popover. The filter panel shows every filter of the schema as a section with the same editors, which suits a sidebar or a slideover on a small screen. A count leaves out the rule of its own filter, so ticking France does not hide the other countries, while every other filter and the search narrow the numbers. A filter with a very long or private list of options loads them page by page with `DataListFilter.WithRemoteOptions`, which is documented with the [Klyroo.Query](https://github.com/klyroo/klyroo-query) types.

Both parts apply a change at once by default. Set `ApplyMode="DataListApplyMode.Submit"` to keep the changes until the user presses Apply. Statically rendered, the panel is one GET form whose Apply button sends every field.

## Keep the state in the URL, or set it from code

Goal: several lists on one page that keep their own state, and a button that changes a list from outside.

By default a list writes its state to the query string: `q` (search), `p` (page), `ps` (page size), `s` (sort, `name|-total` with a leading `-` for descending), `f.<key>` (filters), `c` (cursor) and `l` (layout). Values equal to the defaults of the schema are left out. A page with more than one list needs a distinct `UrlKey` on each, which prefixes the keys (`late.p`, `paid.p`); two lists that claim the same keys on one page throw an `InvalidOperationException`.

Bind `State` to read or set the state from your code. A value you set is written to the URL as a history entry, or loaded directly when `UrlSync` is off:

```razor
@page "/orders/state"

<div class="flex gap-2">
    <KlyButton Label="Biggest first" OnClick="BiggestFirst" />
    <KlyButton Label="Reset" Variant="ButtonVariant.Outline" OnClick="Reset" />
</div>

<KlyDataList TItem="Order" Schema="Schema" Items="Orders" @bind-State="_state" UrlKey="late" Label="Late orders" />
<KlyDataList TItem="Order" Schema="Schema" Items="Orders" UrlKey="paid" Label="Paid orders" />

<p>Page @_state?.Page, sorted by @(_state?.Sort.FirstOrDefault()?.Key ?? "default")</p>

@code {
    private sealed record Order(int Id, string Customer, decimal Total);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));

    private static readonly Order[] Orders = [new(1, "Acme", 1200m), new(2, "Globex", 800m)];

    private DataListState? _state;

    private void BiggestFirst() => _state = DataListState.Initial with { Sort = [new DataListSort("total", Descending: true)] };

    private void Reset() => _state = null;
}
```

With URL sync on, the URL wins at the start: the value you bind only starts the list when the address holds none of its keys. Turn `UrlSync` off for a list in a dialog or a drawer, where the address should not change. To reset from inside the list, `KlyDataList<TItem>.ResetAll()` takes search and filters back to the schema defaults, clears the selection and the tree expansion.

### Remember a filter between visits

Mark a filter with `WithPersist()` and the list keeps its last value in the browser, so a user who always looks at one region finds it selected on the next visit:

```csharp
.Field(o => o.Country, f => f.Filter(DataListFilter.Option().WithFacets().WithPersist()))
```

Each change of such a filter is written to `localStorage` under `klyroo-dl-filter:<full name of the row type>:<PersistKey or UrlKey>:<filter key>`, as the URL keys of its rules (`f.country=France%2CSpain`), and clearing the filter or setting it back to its schema default removes the entry. On its first interactive render, and again whenever a navigation brings the page back to an address that says nothing about such a filter (a menu link to the page, Back), the list reads the entries of the filters the address does not name (with URL sync off, of those the bound `State` leaves at their default), applies them on page 1 and writes them to the address without a history entry. Clearing a filter in the list itself is not such a navigation: the filter stays cleared and its entry is removed. A filter the address does name keeps the address value, and that value is not stored until the user changes it. A stored value is read like an address, so one the schema no longer accepts is dropped.

Storage exists only in the browser: a static render and a prerender neither read nor write it, so a prerendered page first shows the list without the remembered value and the interactive render then loads it with the value. Give lists of the same row type on different pages a `UrlKey` or a `PersistKey` when their filters should not share a value. When the browser refuses storage (private browsing, blocked site data, a full quota) the filter behaves as if it were not marked.

## Select rows and run actions

Goal: a checkbox column, bulk actions on the ticked rows (or on every result), and actions on each row.

Selection needs a `RowKey` in the schema and an interactive render. A `KlyDataListSelectionActions` bar turns it on by itself. `DataListAction<T>` describes a bulk action; `DataListRowAction<T>` describes a row action. An action that throws, or a row action that calls `Fail`, marks the run as failed: the selection stays, the list is not refreshed, `OnError` or `OnRowActionError` is raised and nothing reaches the page.

```razor
@page "/orders/actions"
@rendermode InteractiveServer

<KlyDataList TItem="Order" Schema="Schema" Provider="LoadAsync" Label="Orders">
    <KlyDataListSelectionActions TItem="Order" Actions="_bulk" OnError="OnBulkError" />
    <KlyDataListTable TItem="Order" RowActions="_rowActions" OnRowActionError="OnRowError">
        <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
        <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" Align="DataListAlign.End" />
    </KlyDataListTable>
    <KlyDataListPagination TItem="Order" />
</KlyDataList>

<p>@_message</p>

@code {
    private sealed record Order(int Id, string Customer, decimal Total);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));

    private List<Order> _orders = [new(1, "Acme", 1200m), new(2, "Globex", 800m), new(3, "Initech", 450m)];
    private string _message = "";
    private IReadOnlyList<DataListAction<Order>> _bulk = [];
    private IReadOnlyList<DataListRowAction<Order>> _rowActions = [];

    private ValueTask<DataListResult<Order>> LoadAsync(DataListRequest request, CancellationToken cancellationToken) =>
        ValueTask.FromResult(Schema.ExecuteInMemory(_orders, request));

    protected override void OnInitialized()
    {
        _bulk =
        [
            new DataListAction<Order>
            {
                Key = "delete",
                Label = "Delete",
                Icon = "trash",
                Color = KlyColor.Error,
                Confirm = new DataListActionConfirm<Order>
                {
                    Title = "Delete orders",
                    Description = c => $"{c.TargetCount} orders will be deleted.",
                    ConfirmLabel = "Delete",
                },
                ClearSelection = DataListSelectionClear.Success,
                RefreshOnSuccess = true,
                OnExecute = c =>
                {
                    var keys = c.SelectedKeys.ToHashSet();
                    _orders = _orders.Where(o => !keys.Contains(o.Id.ToString())).ToList();
                    return Task.CompletedTask;
                },
            },
        ];
        _rowActions =
        [
            new DataListRowAction<Order>
            {
                Key = "show",
                Label = "Show",
                Href = c => $"/orders/{c.Item.Id}",
            },
            new DataListRowAction<Order>
            {
                Key = "refund",
                Label = "Refund",
                Icon = "copy",
                Visible = c => c.Item.Total > 500m,
                OnExecute = c =>
                {
                    c.Fail("The payment provider is not reachable.");
                    return Task.CompletedTask;
                },
            },
        ];
    }

    private void OnBulkError(DataListActionFailure<Order> failure) => _message = $"{failure.Action.Label} failed: {failure.Exception.Message}";

    private void OnRowError(DataListRowActionFailure<Order> failure) => _message = failure.Message ?? failure.Exception?.Message ?? "";
}
```

Three details are easy to miss. The list here reads its rows through a provider, so `RefreshOnSuccess` loads them again after the delete and the removed orders disappear. `c.TargetCount` is the number to show in a confirmation, because it is right for ticked rows and for every result, while `SelectedItems.Count` is not before the action runs. And when the source knows the total (`DataListResult.TotalCount`), the bar offers "Select all N results": the action then receives `Scope = DataListActionScope.All` and a `Request` that describes the whole query (search, filters, sort, no paging). A list over a provider or a queryable never holds those rows, so send that request to your server and apply it there with `schema.Apply(query, request)`.

A row action with a `Href` is a link and works in a statically rendered list; every other action needs interactivity. Each row action button group shows up to `RowActionsMaxVisible` buttons (3), puts the rest in a menu, and below the `md` breakpoint collapses into one button that opens a sheet.

## Edit a row in a drawer or a modal

Goal: let users change a row, with Klyroo.Table staying read-only.

The table has no editable cells, and that is on purpose: it shows rows and reports what the user asks for. Editing is a row action that opens a form, waits for the answer, saves it where your data lives, and tells the list what changed. Two ways to ask for the values:

**A Klyroo.Form drawer.** `FormService.CreateFormAsync` opens a `FormSchema` in a drawer (or a modal, or fullscreen) and returns a task, so the row action reads like a function. It needs `<KlyFormProvider>` in the layout and `AddKlyrooForm()` next to `AddKlyrooTable()`.

```razor
@page "/employees"
@rendermode InteractiveServer
@inject FormService Forms
@inject EmployeeStore Store

<KlyDataList TItem="Employee" Schema="Schema" Provider="LoadAsync" Label="Employees">
    <KlyDataListTable TItem="Employee" RowActions="_rowActions">
        <KlyDataListColumn TItem="Employee" Field="name" Title="Name" />
        <KlyDataListColumn TItem="Employee" Field="email" Title="Email" />
    </KlyDataListTable>
    <KlyDataListPagination TItem="Employee" />
</KlyDataList>

@code {
    private sealed record Employee(int Id, string Name, string Email, int Version);

    private static readonly DataListSchema<Employee> Schema = DataListSchema.For<Employee>(s => s
        .RowKey(e => e.Id)
        .RowLabel(e => e.Name)
        .Field(e => e.Name, f => f.Key("name").Sortable())
        .Field(e => e.Email, f => f.Key("email")));

    private static readonly FormSchema EditSchema = new()
    {
        FormKey = "edit-employee",
        Title = "Edit employee",
        Fields =
        [
            new FormFieldSchema { Key = "name", Label = "Name", Required = true },
            new FormFieldSchema { Key = "email", Label = "Email", Required = true },
        ],
    };

    private IReadOnlyList<DataListRowAction<Employee>> _rowActions = [];

    private ValueTask<DataListResult<Employee>> LoadAsync(DataListRequest request, CancellationToken ct) =>
        Store.QueryAsync(request, ct);

    protected override void OnInitialized() =>
        _rowActions =
        [
            new DataListRowAction<Employee>
            {
                Key = "edit",
                Label = "Edit",
                Icon = "pencil",
                OnExecute = async c =>
                {
                    var outcome = await Forms.CreateFormAsync(
                        EditSchema,
                        new Dictionary<string, object?> { ["name"] = c.Item.Name, ["email"] = c.Item.Email },
                        FormShellMode.Drawer);
                    if (!outcome.IsCompleted) return; // the user closed the drawer

                    var saved = await Store.SaveAsync(
                        c.Item with { Name = (string)outcome.FormData["name"]!, Email = (string)outcome.FormData["email"]! },
                        c.CancellationToken);
                    if (saved is null)
                    {
                        c.Fail("Someone else changed this employee. Reload and try again.");
                        return;
                    }
                    c.List.UpdateRow(saved);
                },
            },
        ];
}
```

While the drawer is open the action counts as running: its button shows a spinner and cannot start twice, and a thrown exception or `c.Fail` keeps the list as it is and raises `OnRowActionError` of the table.

**A Klyroo.UI modal.** For a form of your own, keep the open state and the edited copy in the page and bind a `KlyModal` to it. The row action only fills them in (`Employee`, `Schema`, `LoadAsync` and `Store` are the ones of the first page):

```razor
<KlyDataList TItem="Employee" Schema="Schema" Provider="LoadAsync" Label="Employees">
    <KlyDataListTable TItem="Employee" RowActions="_rowActions">
        <KlyDataListColumn TItem="Employee" Field="name" Title="Name" />
    </KlyDataListTable>
    <KlyDataListPagination TItem="Employee" />
</KlyDataList>

<KlyModal @bind-Open="_open" Title="Edit employee">
    <Body>
        <KlyFormField Label="Name">
            <KlyInput @bind-Value="_name" />
        </KlyFormField>
    </Body>
    <Footer>
        <KlyButton Label="Cancel" Variant="ButtonVariant.Ghost" data-kly-close />
        <KlyButton Label="Save" OnClick="SaveAsync" />
    </Footer>
</KlyModal>

@code {
    private DataListContext<Employee>? _list;
    private Employee? _editing;
    private string? _name;
    private bool _open;

    private IReadOnlyList<DataListRowAction<Employee>> _rowActions => [
        new DataListRowAction<Employee>
        {
            Key = "edit",
            Label = "Edit",
            OnExecute = c =>
            {
                _list = c.List;
                _editing = c.Item;
                _name = c.Item.Name;
                _open = true;
                return Task.CompletedTask;
            },
        },
    ];

    private async Task SaveAsync()
    {
        if (_editing is null || _list is null || string.IsNullOrWhiteSpace(_name)) return;
        var saved = await Store.SaveAsync(_editing with { Name = _name.Trim() }, CancellationToken.None);
        if (saved is not null) _list.UpdateRow(saved);
        _open = false;
    }
}
```

In both, `DataListContext<T>.UpdateRow` replaces the row in the loaded result by its row key, without asking the source again, which is right after a save that returns the stored row. Use `await c.List.RefreshAsync()` (or `RefreshOnSuccess = true` on the action) instead when the change can move the row, for example when it is sorted or filtered by the field that was edited, or when other rows depend on it. The schema needs a `RowKey` for `UpdateRow` to find the row. Validation belongs in the form (`FormSchema` rules, or an `EditForm` with Klyroo.UI fields), and a server-side refusal belongs in `c.Fail(...)` so the table reports it.

## Open, style and annotate rows

Goal: make a row lead somewhere, react to a click on it, and style or tag rows and cells from the data.

```razor
<KlyDataListTable TItem="Order"
                  RowHref="@(o => $"/orders/{o.Id}")"
                  RowClass="@(o => o.Status == "Archived" ? "opacity-60" : null)"
                  RowAttributes="@(o => new Dictionary<string, object> { ["data-order"] = o.Id })">
    <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
    <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" Align="DataListAlign.End"
                       CellClass="@(o => o.Total > 1000m ? "font-semibold text-(--klyroo-error-text)" : null)" />
</KlyDataListTable>
```

`RowHref` gives each row with an address an empty link named "Open {row label}" (the `RowLabel` of the schema, else "Open row 3"). The link is stretched over the whole row with CSS, so a click anywhere on the row opens the page, the link is the one Tab stop of the row and Enter follows it, and it is a real `<a>` in a static render. Links, buttons, boxes and fields inside the row (the selection box, the row actions, a link in a cell template) are lifted above it and keep their own clicks. The address is written as given, so check the scheme of anything a user typed. Pinned columns float above the link: a pinned column that sits after it does not take the click, and the link is put in the first column that is not pinned.

`OnRowClick` is for the interactive case. It runs for a click on the row that did not land on a link, button, box or field inside it (the padding of the selection, detail and actions cells counts as inside, and so does anything marked `data-row-ignore`; a click that ends a text selection is ignored too), and for Enter or Space while the row itself has the focus, so the row is reachable with Tab. The browser makes that decision before anything is sent, so a click on a checkbox or a key typed in a field costs no round trip. The argument carries the row, its key, its place on the page and the modifier keys:

```razor
<KlyDataListTable TItem="Order" OnRowClick="Open">...</KlyDataListTable>

@code {
    private void Open(DataListRowClickArgs<Order> click)
    {
        if (click.CtrlKey || click.MetaKey) _selected.Add(click.Item);
        else _current = click.Item;
    }
}
```

A row with an address is not activated through the browser events, because its link already does it; `OnRowClick` is then raised from the click on the link (with its modifier keys) and the navigation still happens. **`RowHref` is the accessible way to open a row**: the focusable row of `OnRowClick` has no role, so a screen reader announces its cells, not an action, and the click should stay a shortcut for the same thing. The price of the link is that the text of a row that has one cannot be selected with the mouse, because the link covers it. A control of your own inside a cell that is not a link, button or field can opt out of the row click with `data-row-ignore`. A static render has no row click; use `RowHref` there.

`RowClass` and `RowAttributes` receive the item and run on every render of the row, so keep them cheap; `class` and `data-slot` in `RowAttributes` are ignored in favor of `RowClass`. `CellClass` on a column styles that cell only (the header keeps `Class`).

## Sort by several columns

Goal: let users sort by a second and a third column, like a spreadsheet or a DataGrid.

The query layer already takes a list of sorts, and the state keeps it in the URL (`s=team|-salary`). In the table a plain click on a sortable header keeps one sort, as before: ascending, then descending, then the default sort; a column that is in the sort but not first becomes the only sort, ascending. **Shift+click** appends instead: a column that is not sorted yet joins the end of the list ascending, a second Shift+click turns it descending in place, and a third takes it out. The other sorts stay.

While more than one sort is active, each sorted header shows its place in the list (1, 2, 3) beside its arrow, and the accessible name of its button adds "sort order 2". `aria-sort` stays on the first sort only, which is the one assistive technology reads as the sort of the table. The sort buttons carry the tooltip "Shift+click to add to the sort" (`SortAppendHint`). A static render has plain links only: they keep one sort, because a link cannot see the Shift key.

`KlyDataListSortMenu` edits the same list. With `Multiple` left at its default (`null`) the menu keeps picking one field and its direction until the list sorts by two or more, then lists the active sorts in order with buttons to reverse, move earlier or later and remove each, and below them the fields that can still be added. `Multiple="true"` shows that editor from the start, `Multiple="false"` never does.

```razor
<KlyDataListSortMenu TItem="Order" Multiple="true" />
```

From code, `DataListContext<T>` has `NextSort(state, key, append)`, `AddSort`, `RemoveSort`, `ReverseSort` and `MoveSort`, each returning the new state on the first page, and `SortIndex(key)` and `SortDescending(key)` read any position of `EffectiveSort`.

## Show a detail row under a row

Goal: a master-detail table, where a button opens a full-width row of extra content under its row.

```razor
<KlyDataList TItem="Order" Schema="Schema" Items="_orders">
    <KlyDataListTable TItem="Order">
        <DetailTemplate Context="order">
            <dl class="grid grid-cols-2 gap-2">
                <dt>Ship to</dt><dd>@order.Address</dd>
                <dt>Lines</dt><dd>@order.LineCount</dd>
            </dl>
        </DetailTemplate>
        <ChildContent>
            <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
            <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" />
        </ChildContent>
    </KlyDataListTable>
</KlyDataList>
```

`DetailTemplate` adds a first column with one button per row. The button has `aria-expanded`, is named "Show details for {row label}" (or "Hide details for ..."), and gets `aria-controls` pointing to the detail row while it is open; ArrowRight opens and ArrowLeft closes from the button, like the tree toggle. The detail is a `<tr>` with one cell that spans every column, drawn right after its row.

Which rows are open is the row expansion state of the list, the one a tree uses, so you can read and drive it from code with `DataListContext<T>`: `IsRowExpanded(key)`, `SetRowExpanded(key, true)`, `ExpandAll()`, `CollapseAll()` and `ExpandedKeys`. The key is the row key of the schema, so give the schema a `RowKey` if rows should stay open when the page reloads or the sort changes; without one (or when its keys repeat) the key is the place of the row on the page, and the list forgets those rows' open state whenever new rows arrive, so that another row never shows a detail the user opened for a different one. `ResetAll` closes every row.

Some combinations are not supported:

- `Virtualize`: virtualized rows need one fixed height and a detail row has none, so the detail template is ignored (no button, no detail) while `Virtualize` is true, in every render mode so that a prerender and the interactive page agree.
- `Tree`: a tree uses the expansion state for its child rows, so the detail template is ignored with a `Tree`.
- The compact summary view (`RowSummary`) draws no detail rows.
- A static render draws the buttons disabled and no detail: opening a row needs interactivity.

## Export rows as CSV

Goal: a button that downloads the list as a CSV file, exactly as the user sees it.

```razor
<KlyDataList TItem="Order" Schema="Schema" Items="_orders" Label="Orders" Selectable="true">
    <KlyDataListExport TItem="Order" FileName="orders" CsvOptions="@(new DataListCsvOptions { Delimiter = ';' })" />
    <KlyDataListTable TItem="Order">
        <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
        <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C2" />
        <KlyDataListColumn TItem="Order" Field="placed" Title="Placed" Format="d" />
    </KlyDataListTable>
</KlyDataList>
```

The file has one column per visible column of the table, in the order the user sees them (pinned columns first and last, hidden ones left out, a column that only has a template skipped because it has no value), with the header of each. Values are formatted like the cells: the `Format` of the column and the culture of the page. A list with no declared columns exports the fields of its schema.

The button offers the rows it can reach, in a menu when there are several: **this page**, the **selection** (offered while a row is ticked, and it includes rows ticked on other pages of a list in memory) and **all results** (every row the search and filters match, the same "all" that bulk actions get). A list over a provider or a queryable never holds those rows, so "all results" is offered only when you give `LoadAll`, a function that receives the request without paging and facets and returns the rows, usually by calling your server:

```razor
<KlyDataListExport TItem="Order" LoadAll="LoadEveryOrderAsync" />

@code {
    private async Task<IReadOnlyList<Order>> LoadEveryOrderAsync(DataListRequest request, CancellationToken ct) =>
        await Http.GetFromJsonAsync<List<Order>>("/api/orders/export?" + ToQuery(request), ct) ?? [];
}
```

What the writer does, which matters when the file reaches Excel or LibreOffice:

- **Delimiter**: `;` when the decimal separator of the culture is a comma (French, German, Spanish, Italian), `,` otherwise, so the file opens in columns in that locale. Set `DataListCsvOptions.Delimiter` to override.
- **Quoting** follows RFC 4180: a field is quoted when it holds the delimiter, a quote (doubled), a line break, or starts or ends with a space. Lines end with CRLF.
- **Formula injection**: a text cell that starts with `=`, `+`, `-`, `@`, a tab or a carriage return gets an apostrophe in front, so a spreadsheet shows it instead of running it. Values of a known type (numbers, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, `Guid`, `bool`) are not touched, so a negative number stays a number; everything else, enums and custom `IFormattable` types included, is treated as text, and a text column holding "-5" is prefixed. `ProtectFormulas = false` turns this off, which is only safe for data you wrote yourself.
- **Byte order mark**: the file starts with U+FEFF by default, which Excel needs to read accents as UTF-8. Set `Utf8Bom = false` for a consumer that does not skip it.

The export holds at most `MaxRows` rows (100,000 by default, whatever the source). A bigger one is refused before the rows are loaded when the source knows the count, with a message under the button and a `DataListExportTooLargeException` to `OnError`, because the file is built in memory on the server. A column that has a template but also a `Property`, or a `Key` that names a schema field, exports that value with the `Format` of the column.

The browser downloads the file by itself, through a small module of the package (`klyroo-table-download.js`), which receives the bytes as a stream (`DotNetStreamReference`), not as one big string in the interop message. To do something else with it, handle `OnExport`: it receives a `DataListExportResult` (file name, text, row count, scope, `ToBytes()`) and the browser download is skipped. That is the way to save the file in a .NET MAUI app, or to upload it. `OnError` receives an exception thrown by `LoadAll` or by your handler. A static render draws no export button.

From code, `DataListContext<T>` has `ExportRows(scope)`, `ExportHeaders()`, `ToCsv(rows, options)` and `ExportCsv(scope, options)`, and the headless `DataListCsv` class (in Klyroo.Table.Core) writes any header and rows with the same rules.

## Group rows, total columns and show a tree

Goal: collapsible groups, footer totals, and hierarchical rows.

Groups are visual: filtering, sorting, paging and actions still work on the flat rows, and the rows on screen are partitioned under collapsible headers. Totals come from the `Summary` of a column, shown in a `tfoot` for the scope you choose:

```razor
@page "/orders/groups"

<KlyDataList TItem="Order" Schema="Schema" Items="Orders" Label="Orders">
    <KlyDataListTable TItem="Order" GroupBy="_groupBy" Summaries="_summaries">
        <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" />
        <KlyDataListColumn TItem="Order" Field="country" Title="Country" />
        <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" Align="DataListAlign.End"
                           Summary="DataListSummaryKind.Sum" />
    </KlyDataListTable>
    <KlyDataListPagination TItem="Order" />
</KlyDataList>

@code {
    private sealed record Order(int Id, string Customer, string Country, decimal Total);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Country, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));

    private static readonly DataListGroupBy<Order> _groupBy = new()
    {
        Key = o => o.Country,
        Order = ["France"],
        Summary = g => $"{g.Items.Count} orders",
    };

    private static readonly DataListSummaries<Order> _summaries = new()
    {
        Scopes = [DataListSummaryScope.Page, DataListSummaryScope.Filtered],
    };

    private static readonly Order[] Orders =
    [
        new(1, "Acme", "France", 1200m),
        new(2, "Globex", "Germany", 800m),
        new(3, "Initech", "France", 450m),
    ];
}
```

`DataListSummaryScope.Page` totals the rows on screen, `Filtered` every row the search and filters match, and `Selection` the ticked rows. For rows in memory the filtered total is computed by the list; for a provider or a queryable, give `DataListSummaries.ResolveAsync` to ask your server, otherwise the filtered scope falls back to the loaded rows. A column can also take a `SummaryValue` function or a `SummaryTemplate`. Without a `Summaries` object, a single filtered row appears as soon as any column declares a summary.

For hierarchical rows, give the table a `DataListTree<T>` and load only the top-level rows: each row carries its children. The table draws a chevron, rail connectors and the keyboard behavior (Enter or Space to toggle, ArrowRight and ArrowLeft to expand and collapse). A tree disables `GroupBy` and `RowSummary` on the table:

```razor
@page "/staff"

<KlyDataList TItem="Employee" Schema="Schema" Items="Staff" PagingMode="DataListPagingMode.None" Label="Staff">
    <KlyDataListTable TItem="Employee" Tree="_tree">
        <KlyDataListColumn TItem="Employee" Field="name" Title="Name" />
        <KlyDataListColumn TItem="Employee" Field="title" Title="Title" />
    </KlyDataListTable>
</KlyDataList>

@code {
    private sealed record Employee(int Id, string Name, string Title, IReadOnlyList<Employee>? Reports = null);

    private static readonly DataListSchema<Employee> Schema = DataListSchema.For<Employee>(s => s
        .RowKey(e => e.Id)
        .Field(e => e.Name, f => f.Sortable())
        .Field(e => e.Title));

    private static readonly DataListTree<Employee> _tree = new()
    {
        Children = e => e.Reports,
        DefaultExpandedDepth = 1,
        Selectable = DataListTreeSelectionMode.Roots,
    };

    private static readonly Employee[] Staff =
    [
        new(1, "Ada", "CTO", [new(2, "Grace", "Engineering manager", [new(3, "Alan", "Engineer")])]),
    ];
}
```

Search, filters, sort and paging apply to the top-level rows the source returns, and children travel with their parent; the list above turns paging off to show every root. `KlyDataList<TItem>.ExpandAll()`, `CollapseAll()` and `ResetExpansion()` control the expansion from code.

## Page by cursor and virtualize a long table

Goal: a feed that loads the next page as the user scrolls, and a table that renders only the rows in view.

Cursor paging asks the source for the page after the last row seen, which stays fast and stable on large tables and when rows are added. It needs a `RowKey` in the schema, because the key breaks ties between equal sort values. Set `PagingMode` to `Cursor` and place a `KlyDataListInfiniteLoader`; `Virtualize` on the table keeps the number of rendered rows small:

```razor
@page "/events"

<KlyDataList TItem="LogEvent" Schema="Schema" Items="Events" PagingMode="DataListPagingMode.Cursor" Label="Events">
    <KlyDataListTable TItem="LogEvent" Virtualize="true">
        <KlyDataListColumn TItem="LogEvent" Field="id" Title="#" Width="90" />
        <KlyDataListColumn TItem="LogEvent" Field="message" Title="Message" />
    </KlyDataListTable>
    <KlyDataListInfiniteLoader TItem="LogEvent" />
</KlyDataList>

@code {
    private sealed record LogEvent(int Id, string Message);

    private static readonly DataListSchema<LogEvent> Schema = DataListSchema.For<LogEvent>(s => s
        .RowKey(e => e.Id)
        .Field(e => e.Id, f => f.Sortable())
        .Field(e => e.Message)
        .DefaultSort(e => e.Id)
        .DefaultPageSize(50));

    private static readonly LogEvent[] Events = [.. Enumerable.Range(1, 5000).Select(i => new LogEvent(i, $"Event {i}"))];
}
```

Virtualization is interactive only (static renders and prerenders draw every loaded row), uses fixed single-line row heights from the density (36, 44 or 60 px, or `ItemSize`), and puts the table in a scroll box of its own (`max-h-[32rem]` by default, change it with `Ui` or the theme). It is skipped while the table shows groups, a tree or the phone summary rows. Statically rendered, the infinite loader is a link to the next cursor, so paging works without JavaScript. Use `DataListPagingMode.None` to load every matching row at once, only for lists you know are small or virtualized.

## Let users choose their columns

Goal: users show, hide, reorder, pin and resize columns, and keep their choice.

Place a `KlyDataListColumnPanel` in the list, turn on `Resizable` on the table, and give the list a `PreferencesKey`. The key names where the choices are saved: a grouped cookie (`klyroo-dl-cols`) in a browser, or `localStorage` in a .NET MAUI Blazor Hybrid web view, which has no server to read a cookie.

```razor
@page "/orders/columns"

<KlyDataList TItem="Order" Schema="Schema" Items="Orders" PreferencesKey="orders" Label="Orders">
    <div class="flex justify-end">
        <KlyDataListColumnPanel TItem="Order" />
    </div>
    <KlyDataListTable TItem="Order" Resizable="true">
        <KlyDataListColumn TItem="Order" Field="id" Title="#" Width="80" Pinned="DataListPin.Start" Required="true" />
        <KlyDataListColumn TItem="Order" Field="customer" Title="Customer" Width="220" />
        <KlyDataListColumn TItem="Order" Field="country" Title="Country" Visible="false" />
        <KlyDataListColumn TItem="Order" Field="total" Title="Total" Format="C0" Align="DataListAlign.End" />
    </KlyDataListTable>
</KlyDataList>

@code {
    private sealed record Order(int Id, string Customer, string Country, decimal Total);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Id, f => f.Sortable())
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Country, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));

    private static readonly Order[] Orders = [new(1, "Acme", "France", 1200m), new(2, "Globex", "Germany", 800m)];
}
```

A pinned column sticks from the `sm` breakpoint up and needs a width (150 px when you give none). `Required` keeps a column from being hidden, moved or pinned from the panel. The cookie is capped at 3,000 characters across up to 32 lists, 600 per list, with the least recently updated list evicted first; two lists cannot share a `PreferencesKey`. A statically rendered list or a prerender reads the cookie from the request only when the server project calls `AddKlyrooServer()` from the `Klyroo.UI.Server` package. Without it, those renders paint the default columns and the interactive render then applies the user's choice, and the list logs one warning in Development.

## Change the texts, the look and the delays

Goal: other wording, other classes, a longer search delay.

The options of the package are set in the delegate of `AddKlyrooTable`, in code shared by the server and the WebAssembly client. `Messages` changes the texts of a locale, `DataList` and `DataListEmpty` are the component themes (their `Slot` method appends classes to a slot), and `SearchDebounce` sets the delay in milliseconds before the search field searches (300 by default; Enter searches at once):

```csharp
// Program.cs
using Klyroo.Table;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKlyrooTable(table =>
{
    table.SearchDebounce = 500;
    table.Messages("en", m => m with { Refresh = "Reload", ClearFilters = "Reset filters" });
    table.DataList.Slot("tableRoot", "shadow-sm");
});
```

Messages and themes become read-only the first time a component resolves them, so change them before the app renders; changing them later throws an `InvalidOperationException`. Twelve locales are built in (`ar`, `de`, `en`, `es`, `fr`, `it`, `ja`, `nl`, `pt`, `pt-BR`, `ru`, `zh-Hans`), and only English and French are reviewed by a native speaker: change the others the same way if their wording is wrong for you. The locale follows the one of Klyroo.UI.

For one list, `Ui` overrides the classes of any slot, merged with the theme classes:

```razor
<KlyDataList TItem="Order" Schema="Schema" Items="Orders" Density="DataListDensity.Compact"
             Ui="@(new DataListUi { Root = "gap-6", ResultCount = "font-medium" })" />

@code {
    private sealed record Order(int Id, string Customer);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Customer, f => f.Sortable()));

    private static readonly Order[] Orders = [new(1, "Acme")];
}
```

## Test your lists

Goal: fast tests for your schema and tree without a browser, and component tests with bUnit.

The tree and the schema are plain objects, so an xUnit test needs nothing else:

```csharp
using Klyroo.Query;
using Klyroo.Table;
using Xunit;

public sealed class TreeTests
{
    private sealed record Employee(int Id, string Name, IReadOnlyList<Employee>? Reports = null);

    [Fact]
    public void Nodes_carry_their_depth_and_skip_duplicate_keys()
    {
        var schema = DataListSchema.For<Employee>(s => s.RowKey(e => e.Id));
        var tree = new DataListTree<Employee> { Children = e => e.Reports };
        var duplicate = new Employee(3, "Duplicate");
        Employee[] roots = [new(1, "Ada", [new(3, "Alan"), duplicate])];

        var (_, nodes) = tree.BuildTree(roots, schema);

        Assert.Equal(["Ada", "Alan"], nodes.Select(n => n.Item.Name));
        Assert.Equal([0, 1], nodes.Select(n => n.Depth));
    }
}
```

`DataListController<T>` is the loading engine behind `KlyDataList`: a plain class that turns a `DataListState` into a request, runs your provider, ignores a result a newer load replaced, and keeps the previous rows while the next ones load. Test your provider and schema with it, no renderer needed:

```csharp
using Klyroo.Query;
using Klyroo.Table;
using Xunit;

public sealed class ControllerTests
{
    private sealed record Order(int Id, string Customer, decimal Total);

    [Fact]
    public async Task Loads_the_first_page_sorted_by_the_default_sort()
    {
        var schema = DataListSchema.For<Order>(s => s
            .RowKey(o => o.Id)
            .Field(o => o.Total, f => f.Sortable())
            .DefaultSort(o => o.Total, descending: true)
            .DefaultPageSize(2));
        Order[] orders = [new(1, "Acme", 100m), new(2, "Globex", 300m), new(3, "Initech", 200m)];
        using var controller = new DataListController<Order>(schema, schema.InMemoryProvider(orders));

        await controller.LoadAsync(schema.DefaultState);

        Assert.Equal([2, 3], controller.Rows.Select(o => o.Id));
        Assert.Equal(3, controller.Result?.TotalCount);
    }
}
```

For components, use [bUnit](https://bunit.dev/). Register the services before you render, set an interactive renderer, and turn `UrlSync` off, because a test host has no address to write to. `JSInterop.Mode = JSRuntimeMode.Loose` lets the components call their browser helpers without a setup for each call:

```csharp
using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class OrderListTests : BunitContext
{
    private sealed record Order(int Id, string Customer);

    private static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Field(o => o.Customer, f => f.Sortable()));

    [Fact]
    public void Renders_one_row_per_order()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));
        Order[] orders = [new(1, "Acme"), new(2, "Globex")];

        var cut = Render<KlyDataList<Order>>(p => p
            .Add(x => x.Schema, Schema)
            .Add(x => x.Items, orders)
            .Add(x => x.UrlSync, false));

        Assert.Equal(2, cut.FindAll("tbody tr").Count);
        Assert.Contains("Acme", cut.Markup);
    }
}
```

bUnit locks `Services` at the first render, so register everything before it. The test project is a Razor SDK project (`Microsoft.NET.Sdk.Razor`) when you write component tests with Razor syntax; plain C# tests like the one above also work in a standard test project that references `Klyroo.Table`.
