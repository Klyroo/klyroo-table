# Getting started

This tutorial installs Klyroo.Table, builds a tree without any UI, then builds a customer list in a Blazor page with search, filters, sorting, paging and custom columns. It takes about fifteen minutes and assumes a Blazor Web App on .NET 10 that already uses [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

Check the [prerequisites](../README.md#prerequisites) first: the .NET 10 SDK, and for the components a Blazor app with Klyroo.UI set up.

## 1. Install

```bash
dotnet add package Klyroo.Table --prerelease
```

`Klyroo.Table` brings `Klyroo.Table.Core`, `Klyroo.Query` and `Klyroo.UI` with it. A console app, an API or a background worker that only needs the tree model installs the core package instead:

```bash
dotnet add package Klyroo.Table.Core --prerelease
```

## 2. Register the services

Call `AddKlyrooTable()` in `Program.cs`. In a Blazor Web App with WebAssembly or Auto render modes, call it in the server project and in the client project, because each side builds its own service container. In a .NET MAUI Blazor Hybrid app, call it on `builder.Services` in `MauiProgram.cs`.

```csharp
using Klyroo.Table;

builder.Services.AddKlyrooTable();
```

The call also registers Klyroo.UI and Klyroo.Query, so you do not call `AddKlyrooUi()` or `AddKlyrooQuery()` yourself. It accepts a delegate to change the options, shown in [Change the texts, the look and the delays](guides.md#change-the-texts-the-look-and-the-delays).

## 3. Add the imports

In `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Table
@using Klyroo.Query
```

## 4. Style the list

Klyroo.Table has no stylesheet that you can load next to the Klyroo.UI one. A second precompiled Tailwind file loaded after `klyroo.css` would repeat the base utilities in the same layer and override the responsive and dark variants of Klyroo.UI. The app compiles Tailwind itself, with the classes of the table components included.

With the build-time Tailwind of Klyroo.UI, turn it on in the project file of the web app and import the Klyroo theme in your stylesheet, as the [Klyroo.UI installation](https://github.com/klyroo/klyroo-ui#installation) describes:

```xml
<PropertyGroup>
  <KlyrooTailwind>true</KlyrooTailwind>
</PropertyGroup>
```

```css
@import "tailwindcss";
@import "../obj/klyroo/klyroo.theme.css";
```

Nothing else is needed for the table: the package copies the list of classes its components use to `obj/klyroo/klyroo-tools.theme.css` on every build, and the Tailwind entry that Klyroo.UI generates imports it, so Tailwind generates them. Load the compiled file in the `<head>` of `App.razor` (`<link rel="stylesheet" href="@Assets["app.css"]" />`).

If your app compiles Tailwind with Node or another pipeline, import the table classes yourself after the Klyroo theme, and set `<KlyrooToolsOwnTailwind>true</KlyrooToolsOwnTailwind>` so the build does not warn (`KLYTOOLS001`):

```css
@import "tailwindcss";
@import "../obj/klyroo/klyroo.theme.css";
@import "../obj/klyroo/klyroo-tools.theme.css";
```

Load only your compiled stylesheet, not `klyroo.css`.

## 5. Build a tree without UI

The headless part lives in `Klyroo.Table.Core` and `Klyroo.Query.Core`. `DataListTree<TItem>` turns rows that carry their children into a flat list of nodes with the depth and the rail information a tree view needs, and `DataListSchema<TItem>` from Klyroo.Query describes the rows.

```csharp
using Klyroo.Query;
using Klyroo.Table;

Employee[] staff =
[
    new(1, "Ada", [new(2, "Grace", [new(3, "Alan")]), new(4, "Linus")]),
];

var schema = DataListSchema.For<Employee>(s => s.RowKey(e => e.Id));
var tree = new DataListTree<Employee> { Children = e => e.Reports, DefaultExpandedDepth = 2 };

var (roots, nodes) = tree.BuildTree(staff, schema);
foreach (var node in nodes)
{
    Console.WriteLine($"{new string(' ', node.Depth * 2)}{node.Item.Name}");
}

public sealed record Employee(int Id, string Name, IReadOnlyList<Employee>? Reports = null);
```

It prints the four employees in depth-first order, indented by depth:

```text
Ada
  Grace
    Alan
  Linus
```

Each node also carries its parent, whether it is the last of its siblings, and, for every ancestor level, whether that ancestor has later siblings (`AncestorContinuations`), which is what draws the vertical connector lines. `BuildTree` keys nodes by the row key of the schema, and skips a row whose key was already seen.

The same schema also runs a request against rows in memory, which is what the list does for `Items` and what your API can do for a client-side list:

```csharp
using Klyroo.Query;

Customer[] customers =
[
    new(1, "Acme", "France", 1200m),
    new(2, "Globex", "Germany", 800m),
    new(3, "Initech", "France", 450m),
];

var schema = DataListSchema.For<Customer>(s => s
    .RowKey(c => c.Id)
    .Search(c => c.Name, c => c.Country)
    .Field(c => c.Mrr, f => f.Sortable()));

var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 2),
    [new DataListSort("mrr", Descending: true)],
    DataListFilterGroup.Empty,
    new DataListSearch("France"));

var result = schema.ExecuteInMemory(customers, request);
Console.WriteLine($"{result.TotalCount} match, first: {result.Items[0].Name}");

public sealed record Customer(int Id, string Name, string Country, decimal Mrr);
```

It prints `2 match, first: Acme`: two customers match "France", and the sort puts the larger `Mrr` first.

## 6. Show a list in a page

Add a page that gives a `KlyDataList` a schema and rows. The list is a component of the render mode of its page, and it also works in static server rendering, where search, sort and paging are links and GET forms.

```razor
@page "/customers"

<KlyDataList TItem="Customer" Schema="Schema" Items="Customers" Label="Customers" />

@code {
    private sealed record Customer(int Id, string Name, string Country, decimal Mrr);

    private static readonly DataListSchema<Customer> Schema = DataListSchema.For<Customer>(s => s
        .RowKey(c => c.Id)
        .Search(c => c.Name, c => c.Country)
        .Field(c => c.Name, f => f.Sortable())
        .Field(c => c.Country, f => f.Sortable())
        .Field(c => c.Mrr, f => f.Sortable()));

    private static readonly Customer[] Customers =
    [
        new(1, "Acme", "France", 1200m),
        new(2, "Globex", "Germany", 800m),
        new(3, "Initech", "France", 450m),
    ];
}
```

Run the app and open `/customers`. You get a search field, the result count, a refresh button, a table with one column per field of the schema (click a header to sort), and numbered pagination. Search and sort are written to the address (`?q=acme`, `?s=-mrr` for a descending sort on `mrr`), so reloading the page or sharing the link restores them.

The schema is a `static readonly` field because the list reads it once: a new schema instance given after the first render is ignored, so a schema built inline in the markup would be rebuilt on every render for nothing.

## 7. Choose the columns and the parts

Child content replaces the default layout with the parts you place. Add filters to the schema, then place the search field, filter tags, a table with your own columns, and pagination:

```razor
@page "/customers/columns"

<KlyDataList TItem="Customer" Schema="Schema" Items="Customers" Label="Customers">
    <div class="flex flex-wrap items-center gap-2">
        <KlyDataListSearch TItem="Customer" />
        <KlyDataListRefresh TItem="Customer" />
    </div>
    <KlyDataListFilterTags TItem="Customer" ShowAdd="true" ShowClear="true" />
    <KlyDataListTable TItem="Customer" HeaderMenu="true">
        <KlyDataListFieldColumn TItem="Customer" Field="name" Title="Customer" />
        <KlyDataListFieldColumn TItem="Customer" Field="country" Title="Country" />
        <KlyDataListFieldColumn TItem="Customer" Field="mrr" Title="MRR" Format="C0" Align="DataListAlign.End" />
        <KlyDataListTemplateColumn TItem="Customer" Title="Plan" Key="plan">
            <Template Context="customer">
                <KlyBadge Label="@(customer.Mrr >= 1000m ? "Pro" : "Basic")" />
            </Template>
        </KlyDataListTemplateColumn>
    </KlyDataListTable>
    <KlyDataListPagination TItem="Customer" />
</KlyDataList>

@code {
    private sealed record Customer(int Id, string Name, string Country, decimal Mrr);

    private static readonly DataListSchema<Customer> Schema = DataListSchema.For<Customer>(s => s
        .RowKey(c => c.Id)
        .Search(c => c.Name, c => c.Country)
        .Field(c => c.Name, f => f.Sortable())
        .Field(c => c.Country, f => f.Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(c => c.Mrr, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
        .DefaultSort(c => c.Name)
        .DefaultPageSize(10));

    private static readonly Customer[] Customers =
    [
        new(1, "Acme", "France", 1200m),
        new(2, "Globex", "Germany", 800m),
        new(3, "Initech", "France", 450m),
    ];
}
```

A column reads its value from a field of the schema (`Field`), from an expression (`Property`) or from a `Template`. `Format` applies a .NET format string with the culture of the page. `HeaderMenu` adds a menu to each header to sort, pin or hide the column. The filter tags show one tag per active filter, and the add button lists the filters of the schema that are not on yet: the country filter lists its options with the number of rows behind each one (`WithFacets`).

## 8. Make it interactive

Add a render mode to the page to update the list without a page load. The app must have the matching interactive render mode enabled (`AddInteractiveServerComponents()` and `AddInteractiveServerRenderMode()` for Interactive Server).

```razor
@page "/customers/interactive"
@rendermode InteractiveServer

<KlyDataList TItem="Customer" Schema="Schema" Items="Customers" Label="Customers" />

@code {
    private sealed record Customer(int Id, string Name);

    private static readonly DataListSchema<Customer> Schema = DataListSchema.For<Customer>(s => s
        .RowKey(c => c.Id)
        .Field(c => c.Name, f => f.Sortable()));

    private static readonly Customer[] Customers = [new(1, "Acme"), new(2, "Globex")];
}
```

The state is still written to the address, with a history entry for a page or a sort and a replacement for typing, so Back restores the previous view. Selection, bulk actions and row actions need an interactive render: a statically rendered list has no checkbox column and shows only the row actions that are links.

## Next steps

- [Guides](guides.md): load rows from an API or a database, add filters, select rows and run actions, group and total rows, page by cursor, and test your lists.
- [Concepts](concepts.md): how the list, its parts, the state and the URL fit together.
- [Platforms](platforms.md): render modes, .NET MAUI and NativeAOT.
- [API reference](api-reference.md): every public type and member.
