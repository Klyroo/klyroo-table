# Platforms

This page lists where Klyroo.Table runs, what each render mode changes, and what is known about trimming and NativeAOT. Every statement comes from the project files and the source of this repository; where something is not verified here, the page says so.

## What the packages declare

| Package | Target | `IsTrimmable` | `IsAotCompatible` | Supported platforms |
| :--- | :--- | :--- | :--- | :--- |
| `Klyroo.Table.Core` | `net10.0` | `true` | `true` | any platform .NET 10 runs on |
| `Klyroo.Table` | `net10.0` | `true` | not set | `browser`, `ios`, `android`, `maccatalyst`, `windows` |

CI builds `Klyroo.Table.Core` with the AOT and trimming analyzers on every change. It does not publish or run anything with NativeAOT, and it does not run `Klyroo.Table` in a MAUI app.

## Render modes

| Render mode | What the list does | Limits |
| :--- | :--- | :--- |
| Static server rendering | The server loads the list for each request. Sort, page, layout and filter controls are links that carry the new state in the address, and search, the filter panel and the page size picker are GET forms. | No checkbox column, no selection bar, no actions menu, and only the row actions that are links. No virtualization. Controls that need script (popovers) use the script of Klyroo.UI. |
| Interactive Server | Everything. State changes go through `QueryState` without a page load. | A `Queryable` source blocks the circuit thread unless you register an asynchronous `IDataListQueryExecutor`. |
| Interactive WebAssembly | Everything. | The list runs in the browser, so it cannot reach a database: use `Items` or a `Provider` that calls your API. |
| Interactive Auto | Everything. The first load runs in whichever render mode the page starts in. | `AddKlyrooTable()` must be called in the server project and in the client project, and a `Provider` has to work in both places. |
| .NET MAUI Blazor Hybrid | See [below](#net-maui-blazor-hybrid). | Not run by any test in this repository. |

A page that is prerendered and then becomes interactive loads its list twice unless you hand the prerendered result over, as described next.

## Prerendering and the Auto render mode

A component with an interactive render mode runs once on the server for the prerender and again in the interactive runtime. For a data list that means two loads. `ResultJson` removes the second one: the result of the prerender is serialized into the persistent component state, and the interactive list adopts it when the state it starts from is the one the prerender loaded.

The result is written with the `JsonTypeInfo` you give, not by reflection, so it works with trimming. Create it from a `JsonSerializerContext` that lists `DataListResult<T>` for your row type:

```csharp
using System.Text.Json.Serialization;
using Klyroo.Query;

public sealed record Order(int Id, string Customer, string Country, decimal Total);

public static class OrderSchema
{
    public static readonly DataListSchema<Order> Schema = DataListSchema.For<Order>(s => s
        .RowKey(o => o.Id)
        .Search(o => o.Customer, o => o.Country)
        .Field(o => o.Customer, f => f.Sortable())
        .Field(o => o.Total, f => f.Sortable()));
}

[JsonSerializable(typeof(DataListResult<Order>))]
public sealed partial class OrderJson : JsonSerializerContext;
```

```razor
@page "/orders/auto"
@rendermode InteractiveAuto
@inject HttpClient Http

<KlyDataList TItem="Order" Schema="OrderSchema.Schema" Provider="LoadAsync"
             ResultJson="OrderJson.Default.DataListResultOrder" UrlKey="orders" Label="Orders" />

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

Three rules apply:

- Lists that persist need distinct `UrlKey` or `PersistKey` values. Two lists of the same row type under the same key throw when the second one initializes.
- The persisted result is small on purpose. The browser sends everything persisted back to the server when a Server circuit starts, and SignalR refuses messages past 32 KB. Sizes are counted as sent, with the escaping and the two base64 encodings Blazor adds, so the JSON itself is about a third of them. One list persists at most 24 KB (`PersistMaxBytes`), and all the Klyroo libraries of the page share 28 KB (`KlyrooOptions.PersistedStateMaxBytes`, which Klyroo.Dashboard resources also draw from). A list over its limit, or one that no longer fits, is not persisted: the interactive render loads it again once and a warning names the list and its size. Use a smaller first page, or raise the limits together with the `MaximumReceiveMessageSize` of the hub.
- The provider runs on the server for the prerender and in the browser afterwards, so it must work in both. An HTTP call to your own API does, with a base address on the server `HttpClient` too. A call to a service that exists only on the server does not.

## Time zone and cookies on the server

A day filter means a day in the time zone of the user, and the request carries that zone. An interactive list asks the browser. A static render or a prerender has no browser, so it reads the `klyroo-timezone` cookie that the browser script sets on the first visit. The cookie is reachable through `IKlyRequestCookies`, which the `Klyroo.UI.Server` package implements:

```csharp
using Klyroo.UI.Server;

builder.Services.AddKlyrooServer();
```

Without it, a server-rendered list cuts days in UTC until the page is interactive, and logs one warning in Development. The same service lets a static render or a prerender read the column preferences cookie, so a list with a `PreferencesKey` paints the user's columns on the first paint. In WebAssembly and in a MAUI web view nothing implements `IKlyRequestCookies`, and the zone comes from the browser engine once the list is interactive.

## .NET MAUI Blazor Hybrid

`Klyroo.Table` declares `ios`, `android`, `maccatalyst` and `windows` next to `browser` as supported platforms, so a MAUI project that references it gets no platform compatibility warning from the package. Register it with `builder.Services.AddKlyrooTable()` in `MauiProgram.cs`.

What the source says about the web view:

- Column preferences (`PreferencesKey`) are stored in `localStorage` of the web view, because there is no server to read a cookie.
- Remembered filters (`DataListFilter.WithPersist()`) are stored in `localStorage` too, in the web view as in a browser, through the `klyroo-table-storage.js` module of the package.
- The CSV export (`KlyDataListExport`) downloads through a browser module (`klyroo-table-download.js`), which a web view may not turn into a saved file. In a MAUI app handle `OnExport`: it receives the finished file (`DataListExportResult` with `Content` and `ToBytes()`; the byte order mark is part of both unless you turned it off), and you save it with the platform's file picker or share sheet.
- Nothing implements `IKlyRequestCookies` there, so the time zone for date filters comes from the browser engine once the list is interactive.

The stylesheet setup of [Getting started](getting-started.md#4-style-the-list) applies to the project that holds the Razor pages; the build-time Tailwind of Klyroo.UI serves its output to any host that references that project, a MAUI Blazor Hybrid app included.

What this repository does not contain: a MAUI app that uses the list, or a test that runs it in a web view. If you hit a platform-specific problem, please report it with the platform and the version.

For a MAUI app that does not use a `BlazorWebView`, install `Klyroo.Table.Core` alone: it has no ASP.NET Core or Razor dependency, so a view model can use `DataListTree<T>`.

## NativeAOT and trimming

**`Klyroo.Table.Core`** sets `IsAotCompatible` and `IsTrimmable`, builds with warnings as errors, and CI runs it through the AOT and trimming analyzers. Its only dependency is `Klyroo.Query.Core`, which the Klyroo.Query repository declares the same way. A NativeAOT publish of an app that uses it is not part of CI.

**`Klyroo.Table`** sets `IsTrimmable` but not `IsAotCompatible`. What the source shows:

- A `KlyDataListColumn` (the one with a `Property` expression) is compiled to a delegate with `Expression.Compile()` at render time, and the in-memory executor of Klyroo.Query builds and compiles its predicates from expressions. Check an AOT or WebAssembly AOT publish before relying on either.
- The prerendered result is serialized with the `JsonTypeInfo` you pass in `ResultJson` and with the source-generated `DataListJsonContext` of Klyroo.Query, so that part needs no reflection. Two internal call sites suppress the trimming warning `IL2026` for the string-only `PersistentComponentState` calls, with a justification in the code.
- Components reach the browser through a JavaScript initializer (`Klyroo.Table.lib.module.js`) that Blazor loads on its own; nothing to register.

If you need NativeAOT for the UI, test your publish: nothing in this repository claims it for `Klyroo.Table`.
