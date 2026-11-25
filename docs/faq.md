# FAQ

Troubleshooting first (symptom, cause, fix), then known limitations, then frequent questions. Every entry comes from the source of this repository.

## Troubleshooting

### The list renders but looks unstyled

**Cause.** The classes of the table components are not in your compiled Tailwind stylesheet. Klyroo.Table ships no stylesheet to load next to the Klyroo.UI one, so your Tailwind build must generate the classes.

**Fix.** Compile Tailwind in your app and import `klyroo-table.theme.css` after the Klyroo theme, as in [Getting started](getting-started.md#4-style-the-list). Rebuild after you copy the file.

### `InvalidOperationException: KlyDataList takes one source` or `needs a source`

**Cause.** A list takes exactly one of `Items`, `Queryable` and `Provider`.

**Fix.** Pass one. Do not keep an old `Items` next to a new `Provider`.

### `KlyDataList with a Provider or a Queryable needs a Schema`

**Cause.** Without a schema nothing can drop what a request must not ask for, so only a list of in-memory `Items` may omit it, and then only if its columns name a `Property`.

**Fix.** Build a `DataListSchema<TItem>` once, in a `static readonly` field or a cached service, and pass it as `Schema`.

### `KlyDataList has no Schema and its table has no column with a Property`

**Cause.** An in-memory list without a schema derives one from its columns, and none of them has a `Property` expression.

**Fix.** Give the list a `Schema`, or give the columns a `Property` (`Property="@(c => c.Name)"`).

### `the prefix ... of the query string is already claimed by ...`

**Cause.** Two lists on one page write the same query keys, because both use URL sync and neither has a `UrlKey`.

**Fix.** Give each list a distinct `UrlKey`, or turn `UrlSync` off for one of them (a list in a dialog should). The same applies to `PersistKey` (two lists of the same row type that hand over a prerendered result need distinct keys) and to `PreferencesKey` (each list needs its own).

### `selection needs a RowKey in the schema`

**Cause.** The keys of the selected rows come from the schema's row key. A `KlyDataListSelectionActions` bar, `Selectable` or a bound `SelectedKeys` turns selection on.

**Fix.** Add `.RowKey(x => x.Id)` to the schema. The same applies to cursor paging, which needs the key to order rows with equal sort values.

### There is no checkbox column

**Cause.** Selection is interactive only. A statically rendered list has no checkbox column, and a prerender of an interactive page shows the boxes disabled until the page is interactive. `Selectable="false"` turns it off whatever else is there.

**Fix.** Give the page an interactive render mode, and make sure there is a selection bar, `Selectable="true"` or a bound `SelectedKeys`.

### The list loads again on every render, or flickers

**Cause.** `Items` and `Queryable` are compared by reference, and a new reference loads the list again. An expression such as `Items="Orders.ToList()"` or `Queryable="Db.Orders.AsNoTracking()"` in the markup builds a new one on each render.

**Fix.** Keep the collection or the query in a field and assign it once, in `OnInitialized` or when the data really changes. A `Provider` is the exception: a lambda or a method group may be new on each render, because it replaces the previous provider without loading again.

### A new schema or a changed `Schema` has no effect

**Cause.** The schema is fixed for the life of the list: an instance passed after the first render is ignored.

**Fix.** Build it once in a static field. To use another schema, re-key the list with `@key`, which creates a new list.

### Blazor Server becomes slow or unresponsive with a database

**Cause.** The default `IDataListQueryExecutor` calls `Count()` and `ToList()` on the calling thread, which in Blazor Server is the thread of the circuit.

**Fix.** Register an executor that awaits the database client, as in [Query a database with EF Core](guides.md#query-a-database-with-ef-core).

### `An AsQueryable() over a list is rejected`

**Cause.** A `Queryable` source is for a database provider. An in-memory `IQueryable` would throw on a null along a path where a database returns null.

**Fix.** Use `Items` for rows that are already in memory.

### Date filters are off by some hours, or a warning mentions `AddKlyrooServer`

**Cause.** A day filter means a day in the user's time zone. A static render or a prerender has no browser, so it reads the `klyroo-timezone` cookie, which needs `IKlyRequestCookies` from the `Klyroo.UI.Server` package. Without it the zone is UTC until the page is interactive, and one warning is logged in Development.

**Fix.** Call `services.AddKlyrooServer()` in the server project. See [Platforms](platforms.md#time-zone-and-cookies-on-the-server).

### The interactive render loads the list a second time, or logs `did not hand its prerendered result`

**Cause.** Without `ResultJson` nothing is persisted from the prerender. With it, a result over 16 KB for one list, or 8 KB for all the lists of a page, is not persisted either.

**Fix.** Pass `ResultJson` (see [Platforms](platforms.md#prerendering-and-the-auto-render-mode)), use a smaller first page, or raise `PersistMaxBytes` and `PersistPageMaxBytes` while keeping SignalR's 32 KB message limit in mind.

### `Virtualize` has no effect

**Cause.** Virtualization is interactive only, and the table skips it while it shows groups (`GroupBy`), a tree (`Tree`) or the phone summary rows, and when it has no rows.

**Fix.** Drop `GroupBy` and `Tree` on the virtualized table, or accept regular rows. Pair `Virtualize` with `DataListPagingMode.Cursor` or `None`.

### A pinned column does not stick

**Cause.** Pinned columns stick from the `sm` breakpoint up; below it they scroll with the table but keep their place in the order. A pinned column also needs a width and gets 150 px without one.

**Fix.** Give the column a `Width`, and test on a screen wider than `sm`.

### The columns the user chose are missing on the first paint

**Cause.** A static render or a prerender can read the preferences cookie only through `AddKlyrooServer()`, and only for a list with a `PreferencesKey`. Without them the default columns are painted and the interactive render then applies the saved ones.

**Fix.** Set a `PreferencesKey` on the list and call `AddKlyrooServer()` in the server project.

### Some texts are in English or read badly in another language

**Cause.** Twelve locales are built in, but only English and French are reviewed by a native speaker. The other tables are drafts.

**Fix.** Change a text with `KlyrooTableOptions.Messages` in `AddKlyrooTable`, as in [Change the texts, the look and the delays](guides.md#change-the-texts-the-look-and-the-delays).

### `Messages cannot change once a component has resolved a text`

**Cause.** The messages and the themes freeze when the first component reads them.

**Fix.** Make every change inside the delegate of `AddKlyrooTable`, before the app renders.

## Known limitations

- **Selection, bulk actions and most row actions need an interactive render.** A statically rendered list has no checkbox column, no selection bar, no toolbar actions menu, and only the row actions that are links.
- **Groups are visual.** They partition the rows on screen. They do not change a query, a count or a page, and a table with a tree ignores `GroupBy`.
- **A tree works on loaded top-level rows.** Children come with their parent item. Search, filters, sort and paging act on the rows the source returns, not inside the hierarchy, and nothing loads children on demand.
- **Selection across pages needs rows in memory.** Over a provider or a queryable the selection holds the rows of the page on screen; "Select all N results" hands your server the request instead of the rows.
- **The filtered summary needs a resolver over a provider.** For a provider or a queryable, `DataListSummaries.ResolveAsync` must give the filtered totals, otherwise the list totals the rows it loaded.
- **State is limited to the address.** Search, filters, sort, page and layout travel in the query string, so a very long filter is a very long URL.
- **A column compiled from `Property` and the in-memory executor use expression trees.** `Klyroo.Table` does not declare `IsAotCompatible`; see [Platforms](platforms.md#nativeaot-and-trimming).
- **Ten of the twelve message tables are unreviewed drafts.**
- **No MAUI app or test in this repository runs the list.** The platforms are declared, not exercised here.

## Frequent questions

**Does it work in static server rendering?** Yes: sort, paging, search, filters and the layout switch are links and GET forms, and the server loads the list for each request. Selection and actions need an interactive render, and the popovers, such as the filter editors, use the script of Klyroo.UI.

**Can I use my own table markup?** The parts are components over one context, so you can write a part that derives from `DataListPartBase<TItem>` and read the rows and flags from `Context`. The table itself, with sorting, selection, pinning and virtualization, is `KlyDataListTable`.

**Where do filters come from?** From the schema. A field with a `Filter(...)` gets an editor of the kind you chose in the filter tags, the add filter menu and the filter panel; there is nothing else to declare.

**How do I load the children of a tree row on demand?** You do not: `DataListTree<T>` takes the children from the rows you loaded.

**Can the list sync with something other than the URL?** Turn `UrlSync` off and bind `State`: the list keeps the state itself and reports every change through `StateChanged`.

**Is `Klyroo.Table.Core` useful without Blazor?** It has one job: `DataListTree<T>`. For the request, filter and in-memory execution model, which is UI-free too, use `Klyroo.Query.Core`.
