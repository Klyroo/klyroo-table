# Klyroo.Table

Data lists for Blazor and .NET MAUI Blazor Hybrid: a table and a card grid with search, filters, sorting, paging, selection and bulk actions, for .NET developers who build list pages.

[![CI](https://github.com/klyroo/klyroo-table/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-table/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/Klyroo.Table.svg)](https://www.nuget.org/packages/Klyroo.Table)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/klyroo/klyroo-table/blob/main/LICENSE)

A list page needs search, filters, sort, paging and URL state wired to a data source, and it needs them again on the next page. `KlyDataList` holds that state, loads rows from memory, an `IQueryable` or any asynchronous provider, and shares it with parts you compose: table, grid, search field, filter tags, pagination. The state lives in the query string, so the same markup works in static server rendering (links and forms) and in the interactive render modes. The request and filter model comes from [Klyroo.Query](https://github.com/klyroo/klyroo-query) and the controls are [Klyroo.UI](https://github.com/klyroo/klyroo-ui) components.

## Features

- **Three row sources**: pass in-memory `Items`, a database `Queryable` run by a replaceable `IDataListQueryExecutor`, or a `Provider` delegate that loads a page from an API.
- **One schema for search, filters and sort**: a `DataListSchema<T>` declares which fields sort, filter and search, and requests are sanitized against it before they reach the data.
- **State in the URL**: search, filters, sort, page, page size and layout are written to the query string by default, so Back, shared links and static server rendering work; `@bind-State` reads and sets it from your code.
- **Table, card grid and phone summary rows**: the same list renders as a table, as cards, or as compact rows below the `md` breakpoint.
- **Three paging modes**: numbered pages, keyset cursor paging with an infinite loader, or no paging; the table can virtualize rows in the last two.
- **Selection and actions**: checkbox column, a bulk action bar that can target every result of a query, per-row actions with confirmation dialogs, and a toolbar actions menu.
- **Grouping, totals and trees**: collapsible row groups, footer aggregates (sum, average, count, min, max) and a headless `DataListTree<T>` for hierarchical rows.
- **Multi-column sort**: Shift+click appends a column to the sort, headers number the sorts, and the sort menu edits the list.
- **CSV export**: `KlyDataListExport` downloads the visible columns as the user sees them (this page, the selection or every result), with formula-injection protection, a culture-aware delimiter and an `OnExport` hook for MAUI.
- **Master-detail**: `DetailTemplate` draws an expandable full-width row under each row, driven by the row expansion state of the list.
- **Row hooks**: `RowHref` stretches an accessible link over each row (static renders too), `OnRowClick` reports clicks and Enter on a row with the modifier keys, and `RowClass`, `RowAttributes` and `CellClass` style rows and cells from the data.
- **Column control**: show, hide, reorder, pin and resize columns, with preferences kept in a cookie (or `localStorage` in a MAUI web view).

## Packages

| Package | Description |
| :--- | :--- |
| [`Klyroo.Table.Core`](https://www.nuget.org/packages/Klyroo.Table.Core) | UI-free tree model (`DataListTree<T>`) built on Klyroo.Query.Core. NativeAOT compatible and trimmable. |
| [`Klyroo.Table`](https://www.nuget.org/packages/Klyroo.Table) | The Blazor components, `AddKlyrooTable()`, themes and 12 message tables. Depends on Klyroo.Table.Core, Klyroo.Query and Klyroo.UI. |

## Prerequisites

To use the library in an app:

- **The .NET 10 SDK**, and an app that targets `net10.0`. The packages are prereleases, so every `dotnet add package` command needs `--prerelease`.
- **A Blazor app** for `Klyroo.Table`: a Blazor Web App or a .NET MAUI Blazor Hybrid app. Selection, the selection bar and actions need an interactive render mode (Interactive Server, WebAssembly or Auto); in static server rendering, sorting, filtering and paging work through links and GET forms. In a WebAssembly or Auto app, register the services in the server project and in the `.Client` project.
- **Klyroo.UI set up in the app.** `Klyroo.Table` brings the Klyroo.UI package, and `AddKlyrooTable()` registers its services, but the layout still needs the Klyroo.UI stylesheets in `<head>` and `<KlyApp>` around the body, as described in the [Klyroo.UI installation](https://github.com/klyroo/klyroo-ui#installation).
- **Klyroo.Query** for the list state; `Klyroo.Table` brings it. To run lists against a database through EF Core, add `Klyroo.Query.EntityFrameworkCore` (EF Core 10.0.15 or later). To cut days in the user's time zone in static server rendering, add `Klyroo.UI.Server` and call `AddKlyrooServer()`.
- **A browser with the native popover API**, which Klyroo.UI uses: Chrome and Edge 114, Safari 17 or Firefox 125, or later. In a .NET MAUI app, that means iOS or Mac Catalyst 17, or an Android System WebView 114 or later.
- **Network access on the first build** only if your app self-hosts web fonts (`KlyrooFont`), uses icons outside the built-in set, or compiles Tailwind (`KlyrooTailwind`): the build downloads them once and caches them under `obj/klyroo`. Node.js is not needed.
- **`Klyroo.Table.Core`** alone (the tree model) runs in any .NET 10 app.

To build this repository you need the .NET 10 SDK (a newer SDK works too, see `global.json`) and clones of `klyroo-query` and `klyroo-ui` in the same parent folder: some projects reference them by relative path (`../klyroo-query/...`). See [Contributing](https://github.com/klyroo/klyroo-table/blob/main/CONTRIBUTING.md).

## Installation

```bash
dotnet add package Klyroo.Table --prerelease
```

Register the services in `Program.cs`, in the server project and in the WebAssembly client project if you have one, or in `MauiProgram.cs`:

```csharp
using Klyroo.Table;

builder.Services.AddKlyrooTable();
```

Add the imports to `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Table
@using Klyroo.Query
```

Klyroo.Table ships no stylesheet that is safe to load next to the Klyroo.UI one, so the app compiles Tailwind CSS itself. See [Getting started](https://github.com/klyroo/klyroo-table/blob/main/docs/getting-started.md#4-style-the-list) for the stylesheet setup.

For code that needs no UI, install only the core package:

```bash
dotnet add package Klyroo.Table.Core --prerelease
```

## Quick start

Headless, with `Klyroo.Table.Core`: build the nodes of a tree of rows.

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

In a Blazor page, a searchable, sortable, paged table of in-memory rows:

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

Without child content the list renders a default layout: a search field (when the schema has search fields), the result count, a refresh button, filter tags (when the schema has filters), a table with one column per schema field, and numbered pagination. Child content replaces that layout with the parts you place.

## Documentation

| | |
| :--- | :--- |
| [Getting started](https://github.com/klyroo/klyroo-table/blob/main/docs/getting-started.md) | Step-by-step first integration |
| [Guides](https://github.com/klyroo/klyroo-table/blob/main/docs/guides.md) | Task-oriented recipes |
| [Concepts](https://github.com/klyroo/klyroo-table/blob/main/docs/concepts.md) | How the library works |
| [Platforms](https://github.com/klyroo/klyroo-table/blob/main/docs/platforms.md) | Render modes, .NET MAUI, NativeAOT |
| [API reference](https://github.com/klyroo/klyroo-table/blob/main/docs/api-reference.md) | Every public type and member |
| [FAQ](https://github.com/klyroo/klyroo-table/blob/main/docs/faq.md) | Troubleshooting and limitations |

## Compatibility

- .NET 10 SDK.
- Blazor Web App in static server rendering, Interactive Server, WebAssembly or Auto. In static rendering the controls are links and GET forms; selection and actions need an interactive render.
- .NET MAUI Blazor Hybrid: the package declares `browser`, `ios`, `android`, `maccatalyst` and `windows` as supported platforms. This repository has no MAUI app or test that runs it.
- NativeAOT and trimming: `Klyroo.Table.Core` sets `IsAotCompatible` and `IsTrimmable`, and CI builds it with the AOT and trimming analyzers. `Klyroo.Table` sets `IsTrimmable` only.
- Tailwind CSS v4 compiled by your app, as described in [Getting started](https://github.com/klyroo/klyroo-table/blob/main/docs/getting-started.md#4-style-the-list).

## Klyroo ecosystem

Klyroo is a family of independent .NET libraries that compose. Each one has its own repository, versions and releases.

| Library | Description |
| :--- | :--- |
| [Klyroo.UI](https://github.com/klyroo/klyroo-ui) | Accessible Blazor components for the web and .NET MAUI Blazor Hybrid, styled with Tailwind CSS v4 |
| [Klyroo.Query](https://github.com/klyroo/klyroo-query) | Typed query model, URL state sync, and IQueryable / EF Core execution |
| **[Klyroo.Table](https://github.com/klyroo/klyroo-table)** | Data lists with table and card-grid layouts, filters, selection and actions |
| [Klyroo.Form](https://github.com/klyroo/klyroo-form) | Schema-driven forms, validation and wizards |
| [Klyroo.Dashboard](https://github.com/klyroo/klyroo-dashboard) | Dashboard controller, filters and Blazor blocks: stats, gauges, charts, tables and feeds |
| [Klyroo.Display](https://github.com/klyroo/klyroo-display) | Read-only display blocks, media player controls and file preview |
| [Klyroo.Spreadsheet](https://github.com/klyroo/klyroo-spreadsheet) | Import wizard for CSV, TSV and pasted data, with column and value mapping |
| [Klyroo.Agent](https://github.com/klyroo/klyroo-agent) | Bridge that lets AI agents read and act on Klyroo forms, dashboards and imports |
| [Klyroo.Stack](https://github.com/klyroo/klyroo-stack) | Meta-package, MCP documentation server and `dotnet new` templates |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](https://github.com/klyroo/klyroo-table/blob/main/CONTRIBUTING.md) before opening a pull request, and follow the
[code of conduct](https://github.com/klyroo/klyroo-table/blob/main/CODE_OF_CONDUCT.md). Report security issues privately as described in [SECURITY.md](https://github.com/klyroo/klyroo-table/blob/main/SECURITY.md).

## License

[MIT](https://github.com/klyroo/klyroo-table/blob/main/LICENSE)
