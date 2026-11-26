# Concepts

This page explains how Klyroo.Table is put together: which package owns what, how a click on a header ends up as new rows, what runs where in each render mode, and the decisions behind the design. For task-oriented steps, see the [guides](guides.md).

## The packages and what they own

Klyroo.Table is the top of a small stack. Each layer knows only the ones below it:

```text
Klyroo.Table            KlyDataList and its parts, DataListController, themes, 12 message tables
   |                    depends on the three below
   +-- Klyroo.UI        buttons, popovers, checkboxes, dialogs; theming; locale
   +-- Klyroo.Query     QueryState: reads and writes the query string of the page
   +-- Klyroo.Table.Core   DataListTree<T>, DataListTreeNode<T>
          |
          +-- Klyroo.Query.Core   DataListSchema<T>, DataListState, DataListRequest,
                                  DataListResult<T>, filters, in-memory and IQueryable execution
```

The split matters because the heavy logic is not in the Blazor package. What a list may ask for (fields, filters, operators, page sizes) is the `DataListSchema<T>` of Klyroo.Query.Core. How a request runs against rows in memory or against an `IQueryable` is Klyroo.Query.Core too, so an API server can answer the same `DataListRequest` the component sends without referencing Blazor. `Klyroo.Table.Core` holds the one UI-free piece that belongs to the table itself: `DataListTree<T>`, which turns rows that carry their children into flat nodes with depth and rail information. Everything with a render tree is in `Klyroo.Table`.

## The main types

- **`DataListSchema<T>`** declares the fields of a row: which ones sort, which ones filter and with what kind of filter, which ones the search box reads, the row key, the default sort and page size. It is the gatekeeper of every request.
- **`DataListState`** is what the user left on screen: search text, filters, sort, page, page size, layout and, for cursor paging, the cursor. It is immutable and converts to and from the query string.
- **`DataListRequest` and `DataListResult<T>`** are what a source receives and returns: a request is a state turned into paging, sort, filter tree, search and facet requests; a result is a page of rows, the total count when known, the next cursor and facet counts.
- **`KlyDataList<TItem>`** is the component you place. It owns one `DataListController<TItem>` and one `DataListContext<TItem>`, reads the state from the URL or from your `@bind-State`, and renders its child content.
- **`DataListController<TItem>`** loads: it turns a state into a request, calls the source, cancels the load a newer one replaces, and keeps the previous rows while the next ones load. It is a plain class with no dependency on a renderer.
- **`DataListContext<TItem>`** is what the parts read and change: the state, the rows, the loading flags, the selection, the columns, the expansion of tree nodes. The list cascades one for its whole life (the value is fixed), and a part subscribes to its `Changed` event once.
- **The parts** (`KlyDataListTable`, `KlyDataListGrid`, `KlyDataListSearch`, `KlyDataListFilterTags`, `KlyDataListPagination` and the rest) are components that derive from `DataListPartBase<TItem>`: they take the context as a cascading parameter and render again when it changes. You can write your own part the same way.
- **`KlyDataListColumn<TItem>`** renders nothing. It registers with the context, and the table reads the registered columns when it renders. That is why a columns panel can list the columns of a table that renders later.

## From a click to new rows

A part never loads rows itself. It asks the context to change the state, and the state travels the same path whatever started it:

```text
header click ──> context.SetState(change)
                      │
          UrlSync on? │ yes: write the keys to the URL (QueryState)
                      │        └─> the list hears the URL change ──┐
                      │ no:  keep the new state in the list ───────┤
                                                                    v
                                         controller.LoadAsync(state)
                                           ├─ builds the DataListRequest (sanitized by the schema)
                                           ├─ calls the provider with a cancellation token
                                           ├─ a newer load cancels this one and wins
                                           v
                         result replaces the rows ──> context.Changed ──> parts render again
```

With URL sync on, which is the default, the URL is the single source of the state. A write to it notifies the list, and the list loads from what it reads back, so a typed address, a shared link, Back and Forward and a click all go through the same code and cannot disagree. A page or a sort adds a history entry; typing in the search field replaces the current one. Without URL sync, the list keeps the state itself, which suits a list in a dialog. `@bind-State` reads the state out, and sets it from your code: the value you set is written to the URL, or loaded directly when URL sync is off.

The keys in the query string are `q` (search), `p` (page), `ps` (page size), `s` (sort), `f.<key>` (filters), `c` (cursor) and `l` (layout), under the prefix of `UrlKey` when you give one. Values equal to the defaults of the schema are left out, and bad input in an address is dropped rather than thrown. A filter marked `WithPersist()` is also kept in the browser's `localStorage` and fills an address that names no value for it, on the first interactive render and on later navigations to the page (see [Remember a filter between visits](guides.md#remember-a-filter-between-visits)).

While a load runs, the rows of the previous result stay on screen and `IsReplacing` is true, so the table dims instead of flashing empty; the first load shows placeholders (`IsInitial`). A failed load keeps the previous rows, sets `HasError`, and shows the error state with a Retry button. A page past the last one is replaced by the last page, and the URL is corrected without a history entry.

## Three sources, one request shape

`Items`, `Queryable` and `Provider` all end up as a `DataListProvider<T>`: a function from a `DataListRequest` and a cancellation token to a `DataListResult<T>`.

- `Items` is run by the in-memory executor of Klyroo.Query. The list compares the collection by reference: a new reference loads the list again, so give it a field and not a new list on every render.
- `Queryable` is a database query. The list applies the request to it with the schema (`schema.Apply`) and runs the count and the page through the `IDataListQueryExecutor` of the container. The default executor is synchronous and blocks the calling thread; register an executor that awaits your database client.
- `Provider` is yours. It receives the token that is cancelled when a newer request replaces this one, so pass it to the HTTP call or the query.

A list takes exactly one source, and a provider or a queryable also needs a schema, because it is the schema that drops what a request must not ask for. Both mistakes throw an `InvalidOperationException` when the component initializes.

## Render modes and what runs where

A list works in static server rendering, in Interactive Server, WebAssembly and Auto, and in a .NET MAUI Blazor Hybrid web view. What changes is whether .NET can run a handler:

- **Interactive.** Controls are buttons. A change goes through `SetState` and, with URL sync, `QueryState` navigates without a page load.
- **Static server rendering and prerendering.** There is no handler to run, so controls that change the state are links that carry the new state in the address, and the search field, the filter panel and the page size picker are GET forms. The server loads the list to answer each request. Selection, bulk actions and row actions that are not links need an interactive render.
- **A prerender followed by an interactive render** loads the list twice unless you pass `ResultJson`: the prerender then hands its result to the interactive render, which shows it at once. See [Platforms](platforms.md#prerendering-and-the-auto-render-mode).

Dates add one more input. A day filter means a day in the time zone of the user, so the request carries a time zone id. An interactive list asks the browser for it. A static render or a prerender has no browser, so it reads the `klyroo-timezone` cookie that the browser script sets, through `IKlyRequestCookies`, which the `Klyroo.UI.Server` package implements when the server calls `AddKlyrooServer()`. Without that service the time zone is UTC until the page is interactive.

## Threading

Parts render on the renderer's synchronization context. Notifications from the controller and the list arrive from wherever a load completes, so `DataListPartBase<TItem>` marshals them with `InvokeAsync` and folds several notifications that arrive before the render into one render. Your provider runs inside a load and should not block: awaiting a database or an HTTP call is the expectation, and the default synchronous executor is the exception that blocks, which is why it is documented for tests and small local databases only. Resizing and dragging a column run in the browser script of the package, which reports the final width or position to .NET once the gesture ends.

## Error handling

Configuration mistakes throw at initialization with a message that names the fix: no source, two sources, a missing schema, two lists claiming the same `UrlKey`, `PersistKey` or `PreferencesKey` on one page. Runtime failures stay in the list:

- A failed load shows the error state, keeps the previous rows and offers Retry.
- An action that throws, or a row action that calls `Fail`, keeps the selection, does not refresh the list, raises `OnError` or `OnRowActionError`, and rethrows nothing to the page. Without a handler an exception is logged.
- A failed "load more" in a cursor-paged list keeps the rows loaded so far and shows an inline retry.
- A resolver for remote option labels never throws into the render: values it cannot label are shown as they are.

## Design decisions and their trade-offs

**The schema decides what a request may ask for.** A request from the address bar or from a client is untrusted input. The schema drops unknown fields, operators a filter does not allow, repeated sort keys and page sizes past the maximum before anything runs. The cost is that you declare every sortable and filterable field, once, which is also what lets the list build the filter editors and the sort menu without more configuration.

**The URL owns the state.** It gives deep links, Back and static rendering for free. The cost is that the state is limited to what fits in an address, that a page with several lists needs a `UrlKey` for each, and that a list in a dialog must turn `UrlSync` off.

**One context for the whole list, with parts as components.** Parts read a single fixed context instead of receiving parameters, so you can rearrange them, drop the ones you do not need, or write your own without wiring. The cost is that a part cannot be used outside a `KlyDataList`, and that every part renders when the context changes, which is why the rows of the table are a separate component that renders again only when its row or the result changes.

**Groups are visual, trees are loaded rows.** Grouping partitions the loaded rows under headers, while filtering, sorting, paging and actions keep working on the flat rows, so grouping never changes what a query returns. A tree is built over the loaded top-level rows and the children travel with their parent. Both are therefore cheap to add and neither is a server-side hierarchy: a tree over a very large hierarchy needs its own loading.

**Selection survives pages only where the rows are in memory.** A list over `Items` can keep ticked rows across pages while the search and the filters still match them. A list over a provider or a queryable keeps the rows of the page on screen, because it never holds the others. "Select all N results" covers the gap: the action receives the request that describes the whole query, and your server applies it.

**Prerender results are small by design.** Handing the prerendered result to the interactive render means the browser sends it back to the server when a Server circuit starts, and SignalR refuses messages past 32 KB. Sizes are counted as sent, with the escaping and the two base64 encodings Blazor adds (the JSON is about a third of it). A list persists at most 24 KB by default (`PersistMaxBytes`), within a budget of 28 KB per page that every Klyroo library of the page shares, Klyroo.Dashboard resources included (`KlyrooOptions.PersistedStateMaxBytes`); past that the interactive render loads the list again and a warning names the list and the size.

**No stylesheet is shipped for you to load.** The components use Tailwind utility classes through Klyroo.UI themes, and a second precompiled sheet would repeat base utilities and override the responsive variants of Klyroo.UI. The app compiles Tailwind once, with the classes of both libraries.
