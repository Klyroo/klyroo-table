# Klyroo.Table API reference

> Generated from the compiled assemblies and their XML documentation. Do not edit by hand.

## Klyroo.Table.Core

Static classes and extensions (1) · Classes (3) · Records (2) · Structs (1) · Enums (2)

### Static classes and extensions

#### `DataListCsv`

`Klyroo.Table` · service

A CSV writer: RFC 4180 quoting, a delimiter that fits the culture, and protection against formula injection.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static DelimiterFor(CultureInfo culture)` | `char` | The delimiter a culture needs: `;` when its decimal separator is a comma, else `,`. |
| `static Field(DataListCsvCell cell, char delimiter, bool protectFormulas = true)` | `string` | One field as it is written in the file: protected against formula injection when it is text and the protection is on, and quoted when it needs quotes. |
| `static FileName(string? name)` | `string` | A file name for an export: `name` without the characters Windows file names refuse, and `.csv`. `export.csv` when nothing is left. |
| `static Write(IReadOnlyList<string> header, IEnumerable<IReadOnlyList<DataListCsvCell>> rows, DataListCsvOptions? options = null)` | `string` | Writes a header line and rows as CSV text. |

### Classes

#### `DataListExportTooLargeException`

`Klyroo.Table` · class · inherits `InvalidOperationException`

An export refused because it holds more rows than the limit.

| Constructor | Description |
| :--- | :--- |
| `DataListExportTooLargeException(long count, int limit)` | Creates the exception. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Count` | `long` | How many rows the export would hold. |
| `Limit` | `int` | The limit that applied. |

#### `DataListTree<TItem>`

`Klyroo.Table` · class

Hierarchical tree configuration for `KlyDataListTable{TItem}`. Child rows travel with their parent item (`Children`), expand and collapse with a chevron button (mouse, Enter/Space, or ArrowRight/ArrowLeft) and visual rail connectors, and participate in row selection according to `Selectable`. When a tree is configured on `KlyDataListTable{TItem}`, summary rows and visual groups are disabled on the table.

| Constructor | Description |
| :--- | :--- |
| `DataListTree<TItem>()` | Creates a tree configuration. `DataListTree.Children` is required. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Children` (init) | `Func<TItem, IEnumerable<TItem>?>` | Extracts the child items of a row. Null items and nodes whose key duplicates an earlier node in the tree are skipped. |
| `DefaultExpanded` (init) | `bool` | Whether all branch nodes start expanded by default (`false` by default). When `DefaultExpandedDepth` is set, it takes precedence over this flag. |
| `DefaultExpandedDepth` (init) | `int?` | Optional number of nesting levels expanded by default (`0` = none, `1` = root branches expanded, `2` = roots and their children expanded, etc.). Overrides `DefaultExpanded` when not `null`. |
| `Selectable` (init) | `DataListTreeSelectionMode` | Which rows in the tree display a selection checkbox (`DataListTreeSelectionMode.All` by default). |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `BuildTree(IReadOnlyList<TItem> rootItems, DataListSchema<TItem>? schema = null)` | `(IReadOnlyList<DataListTreeNode<TItem>>, IReadOnlyList<DataListTreeNode<TItem>>)` | Builds the tree of nodes for the given top-level rows, reading each row's children with `DataListTree.Children`. Each node is keyed by the row key of `schema` when it has one (and the key is not empty), else by its position in the walk; a row whose key was already seen is skipped, with its children. |
| `IsDefaultExpanded(int depth)` | `bool` | Whether a branch node at `depth` starts expanded according to the baseline settings. |

#### `DataListTreeNode<TItem>`

`Klyroo.Table` · class

One node in the flattened hierarchy produced by `DataListTree{TItem}` over the loaded top-level rows of a data list.

| Property | Type | Description |
| :--- | :--- | :--- |
| `AncestorContinuations` | `IReadOnlyList<bool>` | For each ancestor depth from `1` to `Depth - 1`, whether that ancestor has further siblings below it (used to draw vertical continuation rails). |
| `Children` | `IReadOnlyList<DataListTreeNode<TItem>>` | Child tree nodes. |
| `Depth` | `int` | Nesting depth: `0` for top-level root rows, `1` for immediate children, etc. |
| `HasChildren` | `bool` | Whether this node has at least one child node. |
| `IsLast` | `bool` | Whether this node is the last sibling of its parent. |
| `Item` | `TItem` | The row data item. |
| `Key` | `string` | Unique key of the row across the tree. |
| `Parent` | `DataListTreeNode<TItem>?` | Parent tree node, or `null` for top-level root rows. |
| `RootIndex` | `int` | Zero-based index of the top-level root ancestor. |

### Records

#### `DataListCsvOptions`

`Klyroo.Table` · record

How `DataListCsv.Write` writes a file.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Culture` (init) | `CultureInfo?` | The culture of the numbers and dates, and of the choice of the delimiter. `null` uses the culture of the page, or the current culture where there is no page. |
| `Delimiter` (init) | `char?` | The field delimiter. `null` (the default) follows `DataListCsvOptions.Culture`: `;` for a culture whose decimal separator is a comma (French, German), else `,`, so that a spreadsheet of that locale opens the file in columns. |
| `IncludeHeader` (init) | `bool` | Writes the header line (true by default). |
| `NewLine` (init) | `string` | The end of a line. `\r\n` by default, as RFC 4180 says. |
| `ProtectFormulas` (init) | `bool` | Keeps a spreadsheet from running a text as a formula (true by default): a text cell that starts with `=`, `+`, `-`, `@`, a tab or a carriage return gets an apostrophe in front. Cells that hold a number or a date (see `DataListCsvCell.Trusted`) are left alone, so that a negative number stays a number. |
| `Utf8Bom` (init) | `bool` | Starts the text with a byte order mark (U+FEFF, written as EF BB BF in UTF-8), which makes Excel read accents correctly. On by default, because Excel is where most exports are opened; turn it off for a consumer that does not skip the mark. |

#### `DataListExportResult`

`Klyroo.Table` · record

A CSV file as text, ready to download or hand to the app.

| Constructor | Description |
| :--- | :--- |
| `DataListExportResult(string FileName, string Content, int RowCount, DataListExportScope Scope)` | A CSV file as text, ready to download or hand to the app. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Content` (init) | `string` | The text, with the byte order mark when it was asked for. |
| `FileName` (init) | `string` | The suggested file name, with `.csv`. |
| `MimeType` | `string` | The media type of the file. |
| `RowCount` (init) | `int` | How many data rows it holds (not counting the header). |
| `Scope` (init) | `DataListExportScope` | Which rows were exported. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `ToBytes()` | `byte[]` | The bytes to write to a file: UTF-8 of `DataListExportResult.Content`, without adding a byte order mark that `DataListExportResult.Content` does not hold. |

### Structs

#### `DataListCsvCell`

`Klyroo.Table` · struct

A cell of a CSV file: its text, and whether the text comes from a typed value that cannot be a formula.

| Constructor | Description |
| :--- | :--- |
| `DataListCsvCell(string? Text, bool Trusted = false)` | A cell of a CSV file: its text, and whether the text comes from a typed value that cannot be a formula. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Text` (init) | `string?` | The text, already formatted with the culture of the file; `null` is an empty cell. |
| `Trusted` (init) | `bool` | The text is the format of a number, a date or another typed value, not text somebody typed: it is never prefixed against formula injection. |

### Enums

#### `DataListExportScope`

`Klyroo.Table` · enum

Which rows an export takes.

| Value | Description |
| :--- | :--- |
| `Page` | The rows on screen: the current page, or the rows loaded so far with cursor paging. |
| `Selection` | The ticked rows, including those ticked on other pages of a list of rows in memory. |
| `All` | Every row the search and the filters match, on every page. A list of rows in memory holds them; a list over a provider or a queryable does not, so it needs a `LoadAll` function on the export part, which receives the request that stands for "every result". |

#### `DataListTreeSelectionMode`

`Klyroo.Table` · enum

Which rows of a hierarchical tree table can be selected with checkboxes.

| Value | Description |
| :--- | :--- |
| `All` | Every row in the tree (roots and descendants) has its own independent selection checkbox. |
| `Roots` | Only top-level root rows (depth 0) have a selection checkbox; descendant rows reserve the selection column width. |

## Klyroo.Table

Components (36) · Static classes and extensions (3) · Classes (18) · Records (11) · Structs (1) · Interfaces (1) · Enums (12)

### Components

#### `DataListActionDialog`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

The confirmation dialog of an action: a title, an optional description, a Cancel button and a confirm button. `KlyDataListSelectionActions` and `KlyDataListActionsDropdown` open it for an action that has a `DataListAction.Confirm`.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Color` | `KlyColor` | The color of the confirm button; `KlyColor.Error` for a destructive action. |
| `ConfirmLabel` | `string` | The label of the confirm button. |
| `Description` | `string?` | The text under the title. Nothing is shown without it. |
| `OnConfirm` | `EventCallback` | Raised when the confirm button is clicked, after which the dialog closes. |
| `Open` | `bool` | Whether the dialog is open. Bind it with `@bind-Open`. |
| `OpenChanged` | `EventCallback<bool>` | Raised when the dialog opens or closes. |
| `Title` | `string` | The title of the dialog. |

Also accepts the parameters of `KlyToolsComponentBase`.

#### `DataListActionPartBase<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

What the parts that show actions share (`KlyDataListSelectionActions`, `KlyDataListActionsDropdown`): which actions are visible, disabled or running, the confirmation dialog and the run itself. An action never starts twice at once, and a failure keeps the selection.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Actions` | `IReadOnlyList<DataListAction<TItem>>` | The actions, in the order they are shown. |
| `OnError` | `EventCallback<DataListActionFailure<TItem>>` | Raised when an action throws; the part keeps the selection, does not refresh the list and does not rethrow. Without a handler the failure is logged. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `DataListLoadTrigger<TItem>`

`Klyroo.Table` · component · inherits `ComponentBase`

Starts the first load of a list that has no schema yet. The table places it after its columns: they register while they render, so by the time it initializes they can describe the schema. For a list with a schema it only waits for the load the list already started.

#### `DataListPartBase<TItem>`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

Base of the parts of a `KlyDataList`: takes the cascaded `DataListContext` and renders again when it changes. Derive from it to write a part of your own.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Ui` | `DataListUi?` | Per-instance classes for each slot. |

Also accepts the parameters of `KlyToolsComponentBase`.

#### `DataListRowSelect<TItem>`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

The selection checkbox of one row. It listens to the selection only, so ticking a box does not render the other rows.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Item` | `TItem` | The row. |
| `Position` | `int` | The place of the row on the page, from 1: the name of its box when the schema gives no `RowLabel`. |

Also accepts the parameters of `KlyToolsComponentBase`.

#### `DataListRowView<TItem>`

`Klyroo.Table` · component · inherits `ComponentBase`

A row of the table. It renders its cells again only when its row, or the `Revision` the table gives it, changes: the table renders for every flag of the list (loading, busy, error) and the rows should not rebuild each time. The revision changes when the table's parent renders (so a template that reads the page's state stays current) and when a new result arrives.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Attributes` | `IReadOnlyDictionary<string, object>?` | Attributes of the row from `KlyDataListTable.RowAttributes`. `class` and `data-slot` are ignored: the row sets them itself. |
| `ChildContent` | `RenderFragment?` | The cells. |
| `Class` | `string?` | The classes of the row. |
| `Clickable` | `bool` | Makes the row reachable with the keyboard and listen to activations: a click on it that is not on a control inside it, and Enter or Space on the row itself. |
| `Item` | `TItem` | The row. |
| `OnActivate` | `EventCallback<KlyRowEventArgs>` | Raised for an activation of the row, as the browser side decided it. |
| `Revision` | `int` | Changes when the cells must be built again. |
| `RowIndex` | `int?` | 1-based row index in a virtualized table (2 for the first data row, after the header); `null` otherwise. |

#### `DataListSelectAll<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The select-all checkbox in the header of the table. It ticks the rows on screen and is mixed while only some of them are ticked.

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataList<TItem>`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

A data list: it loads rows from one source (`KlyDataList.Items`, `KlyDataList.Queryable` or `KlyDataList.Provider`) for a `DataListState` (search, filters, sort, page), keeps that state in the URL, and shares it with its parts (`KlyDataListTable`, `KlyDataListPagination`, `KlyDataListSearch`...) through a `DataListContext`. Without child content it renders a default layout: search, result count, refresh, filter tags, error state, table and pagination.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ChildContent` | `RenderFragment?` | The parts of the list: toolbar items, the table with its columns, the pagination. Without it the list renders a default layout: result count, refresh, error state, table and pagination. |
| `Density` | `DataListDensity` | How tight the rows are. |
| `Items` | `IEnumerable<TItem>?` | The rows, kept in memory: the list sorts and pages them itself, with the semantics of `InMemoryDataList`. Give a stable collection (a field, not a new list on every render): a new reference loads the list again. A list takes one source: `KlyDataList.Items`, `KlyDataList.Queryable` or `KlyDataList.Provider`. |
| `Label` | `string?` | The name of the list. Its table is named by it (`Results` when it has none), and its pagination landmark is "Pagination, <label>": give the lists of a page distinct labels, because two landmarks with the same name cannot be told apart. |
| `PagingMode` | `DataListPagingMode` | How the list pages its rows: numbered pages (`DataListPagingMode.Offset`, the default), keyset cursor paging (`DataListPagingMode.Cursor`, with `KlyDataListInfiniteLoader`) or no paging (`DataListPagingMode.None`, every matching row at once, for virtualized tables). Changing it after the first render starts the list again from its first rows (page 1, no cursor) and loads it in the new mode; a cursor in the address of a list that does not page by cursor is dropped. |
| `PersistKey` | `string?` | The key the prerendered result is persisted under, when the list has no `KlyDataList.UrlKey` or two lists share it. The row type is part of the key. It also scopes the `localStorage` entries of the filters marked `WithPersist()` (before `KlyDataList.UrlKey`): lists of one row type whose remembered filters must not share a value need distinct keys. |
| `PreferencesKey` | `string?` | Name under which column visibility, order, pinning and widths are saved in the grouped cookie `klyroo-dl-cols` (scoped to the path base of the app like the other Klyroo cookies, SameSite=Lax, a year, capped at 3,000 characters across up to 32 lists with least-recently-updated eviction, and 600 characters per list), or a localStorage entry of the same name in a Blazor Hybrid web view (MAUI), where there is no server to read a cookie. Statically rendered and prerendered lists read it from the request when the server calls `AddKlyrooServer()` (package `Klyroo.UI.Server`), so the first paint already has the user's columns; an interactive list takes it from the prerender or from the browser. Only letters, digits, `_` and `-` are kept (64 characters at most). Without a key the preferences stay in memory for the life of the component. Never part of the URL. |
| `Provider` | `DataListProvider<TItem>?` | Loads a page of rows for a request, from a database, an HTTP API or anything else. It receives a token that is cancelled when a newer request replaces it. |
| `Queryable` | `IQueryable<TItem>?` | A query of a database provider (an EF Core `DbSet`, with the filters that are not the user's already applied): the list turns each request into queries with `QueryableDataList` and runs them through the `IDataListQueryExecutor` of the container. Needs a `KlyDataList.Schema`. The default executor blocks the thread while the database answers: register an asynchronous `IDataListQueryExecutor` (for EF Core, one that calls `CountAsync` and `ToListAsync` with the token). An `AsQueryable()` over a list is rejected: use `KlyDataList.Items`. A DbContext allows one query at a time: give the list a context of its own (a factory) when the page has several. |
| `ResultJson` | `JsonTypeInfo<DataListResult<TItem>>?` | Metadata to serialize the result of the prerender (`MyJson.Default.DataListResultMyRow`, from a `JsonSerializerContext` of the app with `[JsonSerializable(typeof(DataListResult<MyRow>))]`). The result of a prerendered list is then handed to the interactive render (WebAssembly, Server or Auto), which shows it at once and does not load it again. Without it nothing is persisted and the interactive render loads the list a second time. Lists that persist need distinct `KlyDataList.UrlKey` or `KlyDataList.PersistKey` values (two of the same row type under the same key throw). |
| `Schema` | `DataListSchema<TItem>?` | What a request may ask for: which fields sort, which filter, the default sort and page size. The schema is fixed for the life of the list: a new instance given after the first render is ignored (build it once, in a static field or a cached service; an inline `DataListSchema.For<T>(...)` in markup would create a new one on every render), and giving another schema means re-keying the list with `@key`. Optional for a list of rows in memory whose `KlyDataListColumnBase`s name a `Property`: those columns then describe the schema (every one sortable unless it says otherwise). A provider or a queryable needs a schema, because it is the schema that drops what a request must not ask for. |
| `Selectable` | `bool?` | Shows a checkbox column (a header box that selects the rows of the page, a box per row). `null`, the default, shows it when the list has a `KlyDataListSelectionActions` bar or `KlyDataList.SelectedKeys` is bound; `false` turns selection off whatever else is there. Selection needs a `RowKey` in the schema and is interactive only: a statically rendered list has no checkbox column (a prerender of an interactive page shows the boxes disabled until the page is interactive). |
| `SelectedKeys` | `IReadOnlySet<string>?` | The keys (`RowKey` of the schema, as text) of the ticked rows. Bind it with `@bind-SelectedKeys` to read or set the selection; give a new set when you change it (the list compares references). Keys the list cannot keep (rows that are not on the page of a list over a provider, rows the search no longer matches) are dropped and reported through `KlyDataList.SelectedKeysChanged`. |
| `SelectedKeysChanged` | `EventCallback<IReadOnlySet<string>>` | Raised with a new set when the user ticks or unticks rows, or when rows leave the selection because a new result no longer holds them. |
| `SelectionScope` | `DataListSelectionScope` | Whether the ticked rows stay ticked on other pages: see `DataListSelectionScope`. |
| `State` | `DataListState?` | The search, filters, sort, page and page size. Bind it with `@bind-State` to read or set it from outside: a value you set (a reset button) is written to the URL (a history entry) with `KlyDataList.UrlSync`, or loaded directly without it. With `KlyDataList.UrlSync` the URL wins at the start: this value only starts the list when the URL holds none of its keys. |
| `StateChanged` | `EventCallback<DataListState>` | Raised when the state changes, from the list or, with URL sync, from the URL (a link, Back and Forward, a typed address). |
| `Ui` | `DataListUi?` | Per-instance classes for each slot. |
| `UrlKey` | `string?` | A prefix for the URL keys of this list (`users` gives `users.s`, `users.p`): needed when a page has several lists that sync with the URL. Without it the keys are `q`, `p`, `ps`, `s`, `f.<key>`, `c` and `l`. |
| `UrlSync` | `bool` | Keeps the state in the URL (true by default): a page or a sort is pushed as a history entry, so Back restores it, and a typed or shared address loads its state. In static server rendering the controls are links and GET forms. Turn it off for a list in a dialog. |

Also accepts the parameters of `KlyToolsComponentBase`.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `CollapseAll()` | `void` | Collapses every branch node in the table's hierarchical tree (`KlyDataListTable.Tree`). |
| `ExpandAll()` | `void` | Expands every branch node in the table's hierarchical tree (`KlyDataListTable.Tree`). |
| `ResetAll(QueryHistory history = Push)` | `void` | Resets the search and filters to the schema defaults (on page 1), clears the row selection, and resets tree expansion to its defaults. |
| `ResetExpansion()` | `void` | Resets tree expansion back to the defaults declared on `KlyDataListTable.Tree`. |
| `UpdateRow(TItem item)` | `bool` | Replaces a row in the loaded result in place by its schema `RowKey`, refreshing any matching entry in the selection without re-querying the source. |
| `UpdateRows(IEnumerable<TItem> items)` | `int` | Replaces matching rows in the loaded result in place by their schema `RowKey`, refreshing any matching entries in the selection without re-querying the source. |

#### `KlyDataListActionsDropdown<TItem>`

`Klyroo.Table` · component · inherits `DataListActionPartBase<TItem>`

The toolbar actions of a list (export, import, a new row) as a menu behind one button. They do not need a selection; an action that wants one reads it from its context.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Icon` | `string` | The name of the icon of the button (`ellipsis` by default). |
| `Label` | `string?` | The label of the button that opens the menu. Without it the localized "Actions". |

Also accepts the parameters of `DataListActionPartBase<TItem>`.

#### `KlyDataListAddFilter<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The add filter button: a menu that lists the filters of the schema that are not on, and opens the editor of the one you pick. With many filters the menu gets a search field (`KlyDataListAddFilter.SearchFrom`).

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether the editors apply each change at once (`DataListApplyMode.Live`) or wait for an Apply button (`DataListApplyMode.Submit`). |
| `SearchFrom` | `int` | The number of filters from which the menu has a search field. Defaults to 8. |
| `Size` | `KlySize?` | The size of the button. Defaults to small. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListClearFilters<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A button that takes every filter off and goes back to the first page; the search, the sort and the page size stay. It shows only while a filter is on.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | The text of the button. Without it, the localized "Clear filters". |
| `Size` | `KlySize?` | The size of the button. Defaults to small. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListColumn<TItem, TValue>`

`Klyroo.Table` · component · inherits `KlyDataListColumnBase<TItem>`

A column that shows a typed value of the row, `<KlyDataListColumn Property="@(o => o.Total)" Format="C2" />`. The row type comes from the list and the value type from the expression, so neither is written. Without a schema on the list the column describes the schema: its key is the member path in camel case (`name`, `department.name`) unless `KlyDataListColumnBase.Key` says otherwise, and it sorts (give `Sortable="false"` to turn that off). With a schema the key must be one of its fields to sort.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Property` | `Expression<Func<TItem, TValue>>` | The value of a cell as an expression, `@(e => e.Name)`: the cell shows it, formatted with `KlyDataListColumnBase.Format`. |
| `SummaryValue` | `Func<IReadOnlyList<TItem>, TValue>?` | Custom synchronous aggregation calculator for this column in the table's summary footer (`<tfoot>`), receiving the rows of the active `DataListSummaryScope`. A numeric result stays a number (formatted with `KlyDataListColumnBase.SummaryFormat`), anything else is shown as text. |
| `Template` | `RenderFragment<TItem>?` | The content of a cell, instead of the formatted value (which still sorts, summarizes and exports). |

Also accepts the parameters of `KlyDataListColumnBase<TItem>`.

#### `KlyDataListColumnBase<TItem>`

`Klyroo.Table` · component · inherits `ComponentBase`

The parameters and behavior every column of `KlyDataListTable` shares. A column renders nothing itself: it registers with its list (`DataListContext.Columns`). What a cell shows comes from the kind of column: `KlyDataListColumn` reads a typed value of the row (`Property`), `KlyDataListFieldColumn` reads a field of the schema by its key and `KlyDataListTemplateColumn` renders a template.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Align` | `DataListAlign` | Where the text of the column sits. Numbers read best at the end. |
| `CellClass` | `Func<TItem, string?>?` | Classes added to the cell of this column for a row (a color for an overdue date, a muted style for an archived row): it receives the row. The header is not affected; use `KlyDataListColumnBase.Class` for both. It runs on every render of the row: keep it cheap. |
| `Class` | `string?` | Classes added to the header and the cells of this column. |
| `Format` | `string?` | A format string for the value of a cell (`"C2"`, `"d"`), used with the culture of the page. Ignored by a `KlyDataListTemplateColumn` and by a template. |
| `HeaderMenu` | `bool?` | Overrides `KlyDataListTable.HeaderMenu` for this column: `true` shows the header actions menu (sort, pin, hide), `false` omits it. |
| `HeaderTemplate` | `RenderFragment?` | The content of the header cell, instead of `KlyDataListColumnBase.Title` and the sort control: a select-all checkbox, or an accessible name that is not shown (an actions column). It renders inside the list, so a part that derives from `DataListPartBase` works in it. A column that sorts but has this template has no sort control. |
| `Key` | `string?` | The key of the column, and of the schema field its header sorts by. A `KlyDataListColumn` takes the member path of its `Property` in camel case when it has none (`name`, `department.name`), a `KlyDataListFieldColumn` its field, and a `KlyDataListTemplateColumn` needs one to sort. |
| `MaxWidth` | `int` | Maximum width in pixels when resized (900 by default). |
| `MinWidth` | `int` | Minimum width in pixels when resized (64 by default). |
| `Order` | `int` | Where the column sits among the others: columns are sorted by this number, then by the order in which they first rendered (the order of the markup for columns that exist from the start). A column that an `@if` adds later renders after the others unless it has an `Order`; `KlyDataListColumnBase.Visible` avoids the question. |
| `Pinned` | `DataListPin` | Default horizontal pin edge (`DataListPin.Start` or `DataListPin.End`). Users can override it through the column panel. A pinned column sticks from the `sm` breakpoint up (below it pinned columns scroll with the table but keep their place in the order), after the selection column and before the row actions column, and needs a width: without `KlyDataListColumnBase.Width` it gets 150 px. |
| `Required` | `bool` | When `true`, the column cannot be hidden: `KlyDataListColumnPanel` does not list it and `DataListContext.SetColumnVisible` ignores it. It cannot be reordered or pinned from the panel either. |
| `Resizable` | `bool` | Whether the user can resize the column by dragging its header handle or using the keyboard (true by default). Only when `KlyDataListTable.Resizable` is on. |
| `Skeleton` | `DataListSkeletonKind` | Placeholder shape rendered inside this column's cells while the initial rows load (`DataListSkeletonKind.Text` by default). |
| `Sortable` | `bool?` | Whether the header sorts. By default the schema decides (a field marked sortable); `false` keeps a sortable field from sorting here. A column cannot sort by what the schema does not allow. |
| `Summary` | `DataListSummaryKind` | Built-in aggregation shown for this column in the table's summary footer (`<tfoot>`): `DataListSummaryKind.Sum`, `DataListSummaryKind.Average`, `DataListSummaryKind.Count`, `DataListSummaryKind.Min` or `DataListSummaryKind.Max`. |
| `SummaryFormat` | `string?` | Format string applied to the summary footer value of this column. When `null`, falls back to `KlyDataListColumnBase.Format` (or the localized row count for `DataListSummaryKind.Count`). |
| `SummaryTemplate` | `RenderFragment<DataListSummaryCellContext<TItem>>?` | Custom template for rendering the summary footer cell (`<tfoot>`) of this column. |
| `Title` | `string?` | The header. Without it, the key written as words (`firstName` becomes "First name"). |
| `Visible` | `bool` | Whether the table shows the column (true by default). A hidden column stays in `DataListContext.Columns` and keeps its place in the order, which is why this is the way to show and hide columns; an `@if` around a column works too, but the column then registers at its place in the markup only while it exists. |
| `Width` | `int?` | Default width in pixels, clamped to [`KlyDataListColumnBase.MinWidth`, `KlyDataListColumnBase.MaxWidth`]. `null` lets the browser size the column to its content. A column with a width keeps it whatever its content (a longer text is cut with an ellipsis); when every column has one the table is as wide as their sum, otherwise the columns without a width share the room that is left. Rendered on the `<col>` and on every cell, so a static render has the width at the first paint. The user's width (the column resize, kept in the preferences) wins. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `EffectiveKey` | `string?` | The key of the column: `KlyDataListColumnBase.Key`, or what its kind derives it from. |
| `EffectiveTitle` | `string` | The header of the column: `KlyDataListColumnBase.Title`, or its key written as words. |

#### `KlyDataListColumnPanel<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A button that opens a popover to show and hide columns, reorder them, pin them to either edge and restore the defaults. Changes are kept in the column preferences of the list (`KlyDataList.PreferencesKey`).

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AllowPinning` | `bool` | Shows Pin to start / Pin to end controls (true by default). |
| `AllowReorder` | `bool` | Shows the drag grip and the Move up / Move down buttons to reorder columns (true by default). |
| `Label` | `string?` | Label of the trigger button and heading of the popover. Defaults to the localized "Columns". |
| `SearchThreshold` | `int` | Shows a search box inside the panel when at least this many configurable columns exist (30 by default; 0 always shows it; a negative number never). |
| `Size` | `KlySize` | Size of the trigger button (small by default). |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListEmpty`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

The empty state of a data list: an icon, a title, a description and optional actions. The table and the grid draw it when there are no rows.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Actions` | `RenderFragment?` | Buttons or links shown under the text, for example one that clears the filters. |
| `ChildContent` | `RenderFragment?` | Extra content between the description and the actions. |
| `Description` | `string?` | The text under the title. Without it, the localized hint to change the search or the filters; an empty string shows none. |
| `Icon` | `string?` | The icon above the title, by name (any icon name Klyroo.UI accepts). Defaults to `search-x`; an empty string shows none. |
| `Title` | `string?` | The title. Without it, the localized "No results". |
| `Ui` | `DataListEmptyUi?` | Per-instance classes for each slot. |

Also accepts the parameters of `KlyToolsComponentBase`.

#### `KlyDataListErrorState<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The error state, shown when the newest load failed. The rows of the previous result, if any, stay in the table under it, and a Retry button loads the request again.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Description` | `string?` | The text under the title. Without it, the localized hint to try again. |
| `Title` | `string?` | The title. Without it, the localized "Could not load the data". |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListExport<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A button that exports the list as a CSV file: the visible columns in the order the user sees them, formatted like the table, with the culture of the page and a delimiter that fits it. With several sources of rows (this page, the selection, every result) it opens a menu. The file is downloaded by the browser, or handed to `KlyDataListExport.OnExport` when the app saves it itself.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `CsvOptions` | `DataListCsvOptions?` | How the file is written: delimiter, byte order mark, header, protection against formulas. Its culture defaults to the culture of the page, and the delimiter follows it (`;` where the decimal separator is a comma). |
| `FileName` | `string?` | The name of the file, with or without `.csv`. Without it the label of the list, else `export`. |
| `Icon` | `string` | The name of the icon of the button (`download` by default). |
| `Label` | `string?` | The label of the button. Without it the localized "Export". |
| `LoadAll` | `Func<DataListRequest, CancellationToken, Task<IReadOnlyList<TItem>>>?` | Loads every row a request matches, for `DataListExportScope.All` on a list over a provider or a queryable (the list never holds those rows). It receives the request without paging and facets, as the list sent it: send it to your server and apply it there with `schema.Apply(query, request)`. A list of rows in memory does not need it. |
| `MaxRows` | `int` | The most rows one export may hold, whatever the scope (100,000 by default). A bigger export is refused before the rows are loaded when the source knows how many there are, with a message under the button (and `KlyDataListExport.OnError` receives a `DataListExportTooLargeException`): the file is built in memory on the server and sent to the browser, which a million rows would not survive. |
| `OnError` | `EventCallback<Exception>` | Raised when loading the rows or saving the file threw. Without a handler the failure is logged. |
| `OnExport` | `EventCallback<DataListExportResult>` | Raised with the finished file instead of the browser's download: save it yourself (a file picker in a MAUI app, an upload, a copy to the clipboard). Without a handler the browser downloads the file. |
| `Scopes` | `IReadOnlyList<DataListExportScope>?` | The sources of rows the user can pick from, in the order of the menu. By default this page, the selection and every result; each is offered only while it applies (the selection needs a ticked row, every result needs a list of rows in memory or `KlyDataListExport.LoadAll`). With one source left the button exports at once. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListFieldColumn<TItem>`

`Klyroo.Table` · component · inherits `KlyDataListColumnBase<TItem>`

A column that shows a field of the schema by its key, `<KlyDataListFieldColumn Field="status" />`: the cell shows the text of the field (formatted with `KlyDataListColumnBase.Format`) and the header sorts by it when the schema allows. It needs a schema on the list; use `KlyDataListColumn` for a typed value of the row.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Field` | `string` | The key of the field of the schema. |

Also accepts the parameters of `KlyDataListColumnBase<TItem>`.

#### `KlyDataListFilterEditor<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The editor of one filter of the schema, chosen by its kind: a text field, a list of options (with search, counts and remote loading), yes / no / any, a number or a range of numbers, a day or a range of days. The filter tags, the add filter menu and the filter panel use it; place it yourself to put a filter anywhere inside a `KlyDataList`.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether a change applies at once (`DataListApplyMode.Live`, the default) or waits for the Apply button (`DataListApplyMode.Submit`). Statically rendered, a filter always applies on a link or a submit. |
| `Key` | `string` | The key of the filter to edit: one of the filters of the schema of the list (any kind; a date filter exact to the second has no editor). |
| `OnDone` | `EventCallback` | Raised after Apply or Cancel (the Submit apply mode), so that the popover around the editor closes. |
| `ResetToken` | `int` | Change it to drop what was typed and show the filter as the list has it: the popover around the editor does it each time it closes. |
| `ShowTitle` | `bool` | Shows the name of the filter above the editor. True by default. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListFilterPanel<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

Every filter of the schema as a collapsible section, with the same editors as the filter tags and the add filter menu. Place it anywhere inside a list: next to the table as a sidebar, or inside a `KlySlideover` on a small screen. A section that has a filter on starts expanded.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether a change applies at once (`DataListApplyMode.Live`, the default) or waits for Apply, with Reset to drop it (`DataListApplyMode.Submit`). Statically rendered, the form always waits for its Apply button. |
| `CloseOnApply` | `bool` | Adds `data-kly-close` to the Apply button, so that a `KlySlideover` (or a modal) around the panel closes when the filters apply, with no round trip. It has no effect on the Live apply mode, which has no Apply button when interactive. |
| `Description` | `string?` | A line under the title of the panel. Without it nothing is shown. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListFilterTags<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

One tag per active filter ("Status is Active, Paused +2"). A click opens the editor of the filter in a popover, and the remove button takes the filter off.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether the editors apply each change at once (`DataListApplyMode.Live`) or wait for an Apply button (`DataListApplyMode.Submit`). |
| `ShowAdd` | `bool` | Shows the add filter button (`KlyDataListAddFilter`) after the tags. False by default; the default layout of the list shows it. |
| `ShowClear` | `bool` | Shows the clear filters button (`KlyDataListClearFilters`) after the tags. False by default; the default layout of the list shows it. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListGrid<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The rows of the list as a grid of cards, the alternative to `KlyDataListTable`. Place both inside one list with a `KlyDataListLayoutSwitch` to let the user choose; each card shows the visible columns as label and value pairs, or an `KlyDataListGrid.ItemTemplate`.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AriaLabel` | `string?` | Accessible name of the grid region. Defaults to the label of the list, then the localized "Results". |
| `Columns` | `int` | Maximum number of card columns on wide screens (1 to 4, 3 by default). |
| `Empty` | `RenderFragment<DataListContext<TItem>>?` | Shown when the list has no rows. Without it, `KlyDataListGrid.EmptyState` or `KlyDataListEmpty` (with the filtered-empty variant when search or filters are active). |
| `EmptyState` | `DataListEmptyState?` | Declarative configuration for the unfiltered empty state (icon, title, description, and an optional first-use `DataListEmptyState.Action`). When search or filters are active, the filtered empty state is still displayed instead and the first-use action is omitted. |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional visual grouping of the loaded cards under collapsible `KlyDataListGroupHeader` headers with per-group selection checkboxes. |
| `ItemTemplate` | `RenderFragment<TItem>?` | Template for rendering the body of each card in the grid. When `null`, renders the visible columns or schema fields as label/value pairs. |
| `OnRowActionError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when a row action threw or reported a failure. |
| `RowActions` | `IReadOnlyList<DataListRowAction<TItem>>?` | Per-card actions (`KlyDataListRowActions`). |
| `RowActionsMaxVisible` | `int` | How many row actions show as inline buttons on each card (0 by default, keeping one menu trigger button per card). |
| `ShowProgressLine` | `bool` | Draws a subtle top progress indicator while the cards on screen are being refreshed or replaced (`true` by default). |
| `ShowSelectionControl` | `bool` | Draws a selection checkbox on each card and a select-all header when selection is enabled (`true` by default). |
| `SkeletonTemplate` | `RenderFragment?` | Custom content shown for each placeholder card while the initial result loads. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListGroupHeader<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The header of a group of rows in a table, a grid or a summary list: an optional checkbox that ticks the group, and a button with the label and the summary that folds it.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Collapsed` | `bool?` | Overrides whether the group is currently collapsed. When `null`, reads `DataListContext.IsGroupCollapsed`. |
| `Group` | `DataListGroup<TItem>` | The group rendered by this header. |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional grouping definition supplying the label and summary templates of `DataListGroupBy`. |
| `OnToggle` | `EventCallback<bool>` | Raised when the fold/unfold button is clicked. When not bound, toggles the group on `DataListContext`. |
| `Selectable` | `bool?` | Overrides whether the group header displays a select-all-in-group checkbox. Defaults to `DataListContext.SelectionVisible`. |
| `ToggleLabel` | `string?` | Accessible name of the fold/unfold button. Defaults to the localized `KlyToolsMessages.GroupToggle` formatted with `DataListGroup.Label`. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListInfiniteLoader<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

Cursor paging: loads the next page when the user scrolls near the end of the list (`KlyDataListInfiniteLoader.Auto`) or clicks Load more. Statically rendered, the button is a link to the next cursor. Use it with `DataListPagingMode.Cursor`.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Auto` | `bool` | Automatically loads the next page when the sentinel approaches the viewport (`true` by default). Set to `false` for a manual Load more button only. |
| `ErrorTemplate` | `RenderFragment<DataListContext<TItem>>?` | Custom content shown when loading the next page fails (replaces the default message and Retry button). |
| `Label` | `string?` | Text of the Load more button. Defaults to the localized "Load more". |
| `Root` | `string?` | Optional CSS selector of the scrolling element when `KlyDataListInfiniteLoader.Auto` is `true`. Without it, watches the closest scrollable ancestor or the viewport; next to a table with `Virtualize` the loader is drawn inside the scrolling box of the table and watches it. |
| `RootMargin` | `string` | Margin of the `IntersectionObserver` when `KlyDataListInfiniteLoader.Auto` is `true` (`"0px 0px 320px 0px"` by default). |
| `ShowEnd` | `bool` | Shows the localized "No more items" text once the last page has been loaded. It is announced to assistive technologies either way. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListLayoutSwitch<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A button group that switches the list between the table and the grid layout. The choice is part of the state of the list, so it goes in the URL.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AriaLabel` | `string?` | Accessible name of the layout switch group. Defaults to the localized "View layout". |
| `GridIcon` | `string` | Icon name for the Grid layout (`"layout-grid"` by default). |
| `Order` | `IReadOnlyList<DataListLayout>?` | Order of the layout options (`[Table, Grid]` by default). |
| `ShowLabels` | `bool` | Shows the localized text labels ("Table view", "Grid view") next to the icons (`false` by default). |
| `TableIcon` | `string` | Icon name for the Table layout (`"table-2"` by default). |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListPagination<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

Numbered pagination for a list with `DataListPagingMode.Offset` paging: page links or buttons, optional first and last, and a page size picker.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AriaLabel` | `string?` | The accessible name of the pagination landmark. Give each list of a page its own: two landmarks with the same name cannot be told apart. Without it the landmark is "Pagination, <label>" for a list with a label, else the localized "Pagination". |
| `PageSizes` | `int[]` | The page sizes of the picker. The current size and sizes above the schema's maximum are handled for you. |
| `ShowFirstLast` | `bool` | Buttons that jump to the first and the last page. |
| `ShowPageSize` | `bool` | Shows the page size picker. |
| `SiblingCount` | `int` | Pages shown on each side of the current one. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListRefresh<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A button that loads the rows again. Without interactivity it is a link to the current address, which the server answers by loading the list again.

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListResultCount<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The range of rows on screen as a live region, with the total when the source knows it, and "No results" (naming the search text, if any) when nothing matches.

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListRowActions<TItem>`

`Klyroo.Table` · component · inherits `KlyToolsComponentBase`

The actions of one row: buttons up to `KlyDataListRowActions.MaxVisible`, then a menu, and below the `md` breakpoint one button that opens a sheet. The table and the grid place it for you when they have `RowActions`; use it directly in a template of your own.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Actions` | `IReadOnlyList<DataListRowAction<TItem>>` | The actions, in the order they are shown. Their keys are unique, children included. |
| `Item` | `TItem` | The row. |
| `MaxVisible` | `int` | How many actions show as buttons on a wide screen; the others go into a menu (3 by default). 0 puts them all behind one button. Below the `md` breakpoint every action is in a sheet. |
| `OnError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when an action threw or reported a failure; the list is not refreshed and nothing is rethrown. Without a handler an exception is logged. |
| `Position` | `int` | The place of the row on the page, from 1: it names the actions of a row that has no `RowLabel` in the schema. |
| `Size` | `KlySize` | The size of the buttons. |
| `Ui` | `DataListUi?` | Per-instance classes for each slot. |

Also accepts the parameters of `KlyToolsComponentBase`.

#### `KlyDataListSearch<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The search field of the list. It searches after a pause in the typing (`KlyDataListSearch.Debounce`) or on Enter; statically rendered it is a GET form with a search button.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AriaLabel` | `string?` | The accessible name of the field and of its search landmark. Without it, "Search <label of the list>" for a list with a label, else the localized "Search". |
| `Debounce` | `int?` | Milliseconds of quiet after the last keystroke before the list searches. Without it, `SearchDebounce` of `KlyrooTableOptions` (300). Enter searches at once. Statically rendered, nothing is searched before Enter or the search button. |
| `MinLength` | `int` | The fewest characters (once the spaces around the text are trimmed) a search must have: a shorter text searches nothing, so the rows come back as if the field were empty. Zero by default. Statically rendered, the browser refuses to submit a shorter text. |
| `Placeholder` | `string?` | The placeholder of the field. Without it, the localized "Search…". |
| `Size` | `KlySize?` | The size of the field. Defaults to medium. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListSelectionActions<TItem>`

`Klyroo.Table` · component · inherits `DataListActionPartBase<TItem>`

The bar of bulk actions on the selected rows: the number of rows ticked, a switch to act on every result, and the buttons of the actions. It appears when rows are ticked and its presence turns the selection on.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `MaxVisible` | `int` | How many actions show as buttons; the others go into a "More actions" menu (3 by default). |
| `Position` | `DataListSelectionBarPosition` | Whether the bar sticks to the bottom of its list while the list is on screen (the default) or is fixed to the bottom of the window. |
| `Scope` | `bool` | Shows the switch between the ticked rows and every result when the source knows how many results there are (true by default). |

Also accepts the parameters of `DataListActionPartBase<TItem>`.

#### `KlyDataListSortMenu<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A menu that picks the sort field and its direction, or restores the default sort. Useful in the grid layout, which has no column headers, and on small screens.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Items` | `IReadOnlyList<DataListSortOption>?` | Explicit sortable field options. When `null`, the menu lists the sortable `KlyDataListColumnBase`s in user order followed by any other sortable fields of the schema. |
| `Label` | `string?` | Label of the trigger button and heading of the popover. Defaults to the localized "Sort". |
| `Layouts` | `IReadOnlyList<DataListLayout>?` | Layouts in which the sort menu is shown (for example `[DataListLayout.Grid]` when the table headers already sort). When `null`, shown in every layout. |
| `Multiple` | `bool?` | Whether the menu edits a list of sorts: `true` always shows the active sorts in order, with a button to reverse, move and remove each, and the fields that can still be added; `false` keeps the menu that picks one field and its direction; `null` (the default) is the second until the list sorts by more than one field (a Shift+click on a header), then the first, so a sort of several keys can always be seen and changed. |
| `Size` | `KlySize` | Size of the trigger button (small by default). |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListTable<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

The table of the list: `KlyDataListColumnBase` children define the columns, and the table adds sorting, selection, row actions, pinned and resizable columns, grouping, a tree, summary footers, virtualization and the compact summary rows of a phone. Without columns it shows one per field of the schema.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AriaLabel` | `string?` | The accessible name of the table when there is no `KlyDataListTable.Caption`. |
| `Caption` | `string?` | Names the table for assistive technologies (visually hidden), and the scrolling region around it. Without it, the `AriaLabel` of the table, then the label of the list, then the localized "Results". |
| `ChildContent` | `RenderFragment?` | The columns: `KlyDataListColumnBase` components. Without any, the table shows one column per field of the schema. |
| `DetailTemplate` | `RenderFragment<TItem>?` | What to show under a row when it is expanded: a full-width row (master-detail) with this content. Each row gets a first column with a button (`aria-expanded`, named "Show details for {row label}") that opens and closes it; the state is the row expansion state of the list (`DataListContext.IsRowExpanded` and `DataListContext.SetRowExpanded`, by row key, so give the schema a `RowKey` to keep a row open across reloads). Interactive only (a static render draws the button disabled and no detail). Not supported with `KlyDataListTable.Virtualize` (its rows need one fixed height, a detail row has none: the detail is switched off), with `KlyDataListTable.Tree` (which uses the same expansion state for child rows), and not drawn in the compact summary view. |
| `Empty` | `RenderFragment<DataListContext<TItem>>?` | Shown when the list has no rows. It receives the list, whose `DataListContext.IsFiltered` tells "nothing matches" from "nothing exists". Without it, `KlyDataListTable.EmptyState` or `KlyDataListEmpty`. |
| `EmptyState` | `DataListEmptyState?` | Declarative configuration for the unfiltered empty state (icon, title, description, and an optional first-use `DataListEmptyState.Action`). When search or filters are active, the filtered empty state is still displayed instead and the first-use action is omitted. |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional visual grouping of the loaded rows under collapsible `KlyDataListGroupHeader` headers with per-group selection checkboxes. Ignored when `KlyDataListTable.Tree` is set. |
| `HeaderMenu` | `bool` | Adds a dropdown menu button to each column header in interactive mode with quick actions to sort ascending/descending, stop sorting, pin/unpin, or hide the column. Individual columns can override this via `KlyDataListColumnBase.HeaderMenu`. |
| `ItemSize` | `float?` | Height in pixels of one row when `KlyDataListTable.Virtualize` is on. Without it, follows `DataListDensity` (36, 44 or 60 px). |
| `OnRowActionError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when a row action threw or reported a failure. |
| `OnRowClick` | `EventCallback<DataListRowClickArgs<TItem>>` | Raised when the user activates a row: a click anywhere on it that did not land on a link, button, box, field or other control inside the row (the padding of the selection, detail and actions cells counts as inside, and so does anything marked `data-row-ignore`; a click that ends a text selection is ignored too), or Enter or Space while the row itself has the focus. The browser decides this, so an ignored click costs no round trip. The row is then reachable with Tab, but it has no role: a screen reader announces its cells, not an action. `KlyDataListTable.RowHref` is the accessible way to open a row, and the click is best kept as a shortcut for it. The event carries the row and the modifier keys (for Ctrl or Shift to open in another way). Interactive only: a static render has no row click; use `KlyDataListTable.RowHref`. |
| `OverscanCount` | `int` | Extra rows rendered above and below the viewport when `KlyDataListTable.Virtualize` is on (3 by default). |
| `Resizable` | `bool` | Puts a resize handle (a vertical separator) on the end edge of the header of each `KlyDataListColumnBase.Resizable` column (false by default): drag it, or focus it and press ArrowLeft and ArrowRight (16 px, 64 with Shift; in right-to-left text the arrow that points away from the start edge grows the column), Home and End for the limits, Delete or a double click for the declared width. The browser changes the width while the gesture runs and reports it once; the list keeps it in its column preferences (`KlyDataList.PreferencesKey`). Interactive only: a static render has no handle but still applies a saved width. |
| `RowActions` | `IReadOnlyList<DataListRowAction<TItem>>?` | The actions of each row: a last column with buttons up to `KlyDataListTable.RowActionsMaxVisible`, then a menu, and below the `md` breakpoint one button that opens a sheet (`KlyDataListRowActions`). Interactive only, except the actions that are links. |
| `RowActionsMaxVisible` | `int` | How many row actions show as buttons on a wide screen (3 by default); 0 keeps a single "Actions for ..." button per row. |
| `RowAttributes` | `Func<TItem, IReadOnlyDictionary<string, object>?>?` | Attributes added to the `<tr>` of each item (`data-id`, `title`, `aria-*`). `class` and `data-slot` are ignored (use `KlyDataListTable.RowClass`), and so are the row's own event attributes. It runs on every render of the row: keep it cheap. |
| `RowClass` | `Func<TItem, string?>?` | Classes added to the row for each item (a muted style for an archived row). It runs on every render of the row: keep it cheap. `KlyDataListColumnBase.CellClass` does the same for one cell. |
| `RowHref` | `Func<TItem, string?>?` | The address a row leads to. A row whose address is not empty gets a link that covers the row (an empty link named "Open {row label}", stretched over the row with CSS), so the whole row opens the page, the link is reachable with Tab and activated with Enter, and it works in a static render. Links, buttons, boxes and fields inside the row stay on top and keep their own clicks. A row with a link covers its text: the text of such a row cannot be selected with the mouse. Rows with an address are not activated through `KlyDataListTable.OnRowClick` (the link is), except that `KlyDataListTable.OnRowClick` is also raised when the link is clicked with the pointer or Enter. The address is written as given: do not pass text a user typed without checking its scheme. The link goes in the first column that is not pinned, and a pinned cell that floats above it (from `sm` up, after the link) does not take the row click. |
| `RowSummary` | `DataListRowSummary<TItem>?` | Optional compact summary-row presentation (leading tile, title, status, details joined with ` · `, selection checkbox and row actions), active automatically below the `md` breakpoint when `DataListRowSummary.AutoPhone` is `true`, when `DataListRowSummary.DefaultActive` is `true`, or when toggled with `KlyDataListViewToggle`. |
| `ShowProgressLine` | `bool` | Draws a subtle top progress indicator while the rows on screen are being refreshed or replaced (`true` by default). |
| `Summaries` | `DataListSummaries<TItem>?` | Optional footer summary (`<tfoot>`) configuration: target `DataListSummaryScope`(s) (`Filtered`, `Page`, `Selection`), custom row label, and optional `DataListSummaries.ResolveAsync` delegate for remote or asynchronous totals. When `null`, a single `DataListSummaryScope.Filtered` summary row is still rendered automatically whenever any column declares a summary. |
| `Tree` | `DataListTree<TItem>?` | Optional hierarchical tree configuration: expands and collapses nested child rows with a chevron button (click, Enter/Space, or ArrowRight/ArrowLeft) and visual tree rail connectors. Disables `KlyDataListTable.RowSummary` and `KlyDataListTable.GroupBy` when set. |
| `Virtualize` | `bool` | Renders only the rows around the scroll viewport with Blazor's `Virtualize`, using `tr` spacers, a sticky header and fixed single-line row heights matching `DataListDensity` (36 px compact, 44 px default, 60 px comfortable). Meant for `DataListPagingMode.None` and `DataListPagingMode.Cursor` lists with many loaded rows. Interactive only: static renders and prerenders draw regular rows without virtualization. |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyDataListTemplateColumn<TItem>`

`Klyroo.Table` · component · inherits `KlyDataListColumnBase<TItem>`

A column whose cell is a template, `<KlyDataListTemplateColumn Title="Actions">...</KlyDataListTemplateColumn>`. It has no value of its own: give it a `KlyDataListColumnBase.Key` naming a field of the schema and the header sorts by that field, and a CSV export and a summary read it too.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Template` | `RenderFragment<TItem>` | The content of a cell. |

Also accepts the parameters of `KlyDataListColumnBase<TItem>`.

#### `KlyDataListViewToggle<TItem>`

`Klyroo.Table` · component · inherits `DataListPartBase<TItem>`

A toggle that switches the table between its columns and the compact summary rows configured by `RowSummary`. It renders nothing when the table has no row summary.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Icon` | `string` | Icon name shown on the toggle button (`"list"` by default). |
| `Label` | `string?` | Text and accessible name of the toggle button. Defaults to the localized "Summary view". |
| `ShowLabel` | `bool` | Shows the text label next to the icon (`true` by default). |

Also accepts the parameters of `DataListPartBase<TItem>`.

#### `KlyToolsComponentBase`

`Klyroo.Table` · component · inherits `KlyComponentBase`

Base of the components of Klyroo.Table: the theme resolution of `KlyComponentBase`, plus the options and the texts of this package.

Also accepts the parameters of `KlyComponentBase`.

### Static classes and extensions

#### `KlyrooTableServiceCollectionExtensions`

`Klyroo.Table` · service

Registration of Klyroo.Table in the service container.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static AddKlyrooTable(IServiceCollection services, Action<KlyrooTableOptions>? configure = null)` | `IServiceCollection` | Registers Klyroo.Table, and Klyroo.UI and Klyroo.Query with it. It also registers `SynchronousDataListQueryExecutor` as the `IDataListQueryExecutor` unless one is registered already. Call it in the `Program.cs` of the server and of the WebAssembly client, or in `MauiProgram.cs`. |

#### `KlyToolsLocales`

`Klyroo.Table` · service

The message tables of Klyroo.Table, one per locale of `KlyLocales.All` and keyed by the same codes. A component picks the table of the current `KlyLocale.Code` with `KlyLocales.ResolveCode`, so "pt-PT" gets "pt" and an unknown language gets English. Change a table with `KlyrooTableOptions.Messages`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Ar` | `KlyToolsMessages` | العربية (ar). Needs review. |
| `static De` | `KlyToolsMessages` | Deutsch (de). Needs review. |
| `static En` | `KlyToolsMessages` | English. Reviewed. |
| `static Es` | `KlyToolsMessages` | Español (es). Needs review. |
| `static Fr` | `KlyToolsMessages` | Français. Reviewed. |
| `static It` | `KlyToolsMessages` | Italiano (it). Needs review. |
| `static Ja` | `KlyToolsMessages` | 日本語 (ja). Needs review. |
| `static Nl` | `KlyToolsMessages` | Nederlands (nl). Needs review. |
| `static Pt` | `KlyToolsMessages` | Português (pt). Needs review. |
| `static PtBr` | `KlyToolsMessages` | Português (Brasil) (pt-BR). Needs review. |
| `static ReviewedCodes` | `IReadOnlyList<string>` | The codes whose texts were reviewed by a native speaker. The other tables are drafts waiting for review. |
| `static Ru` | `KlyToolsMessages` | Русский (ru). Needs review. |
| `static Tables` | `IReadOnlyDictionary<string, KlyToolsMessages>` | Every table by locale code: the codes of `KlyLocales.All`. |
| `static ZhCn` | `KlyToolsMessages` | 简体中文 (zh-Hans). Needs review. |

#### `TableEventHandlers`

`Klyroo.Table` · service

Custom events of Klyroo.Table, registered on the browser side by `Klyroo.Table.lib.module.js`.

### Classes

#### `DataListAction<TItem>`

`Klyroo.Table` · class

An action of a data list: a button of the `KlyDataListSelectionActions` bar (it acts on the selection) or an entry of the `KlyDataListActionsDropdown` menu. While it runs its button shows a spinner and the action cannot start twice.

| Constructor | Description |
| :--- | :--- |
| `DataListAction<TItem>()` | Creates an action. Set `DataListAction.Key`, `DataListAction.Label` and `DataListAction.OnExecute`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `ClearSelection` (init) | `DataListSelectionClear` | When the ticked rows are unticked: never by default. |
| `Color` (init) | `KlyColor` | The color of the button; `KlyColor.Error` for a destructive action. |
| `Confirm` (init) | `DataListActionConfirm<TItem>?` | Asks before the action runs. Without it the action runs on the click. |
| `Disabled` (init) | `Func<DataListActionContext<TItem>, bool>?` | Disables the action when it returns `true`. It runs on every render: keep it cheap. |
| `Icon` (init) | `string?` | The name of an icon shown before the label (`trash`), one of the icons of Klyroo.UI. |
| `Key` (init) | `string` | Identifies the action: tests and the menu use it. |
| `Label` (init) | `string` | The text of the button. |
| `OnExecute` (init) | `Func<DataListActionContext<TItem>, Task>?` | What the action does. An exception marks the run as failed: the selection stays, the list is not refreshed, `OnError` of the part is raised and nothing is thrown to the page. |
| `RefreshOnSuccess` (init) | `bool` | Loads the list again once the action succeeded (the rows it changed or removed). Off by default; an action can also call `DataListContext.RefreshAsync` itself. |
| `Variant` (init) | `ButtonVariant` | The variant of the button in the selection bar. |
| `Visible` (init) | `Func<DataListActionContext<TItem>, bool>?` | Hides the action when it returns `false` (an action that does not apply to the selected rows). It runs on every render: keep it cheap. |

#### `DataListActionConfirm<TItem>`

`Klyroo.Table` · class

The question an action asks before it runs, in a dialog. The text can say how many rows are affected.

| Constructor | Description |
| :--- | :--- |
| `DataListActionConfirm<TItem>()` | Creates a confirmation. Set `DataListActionConfirm.Title`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Color` (init) | `KlyColor?` | The color of the confirm button; `KlyColor.Error` for a destructive action. Without it the color of the action. |
| `ConfirmLabel` (init) | `string?` | The label of the confirm button. Without it the localized "Confirm". |
| `Description` (init) | `Func<DataListActionContext<TItem>, string?>?` | The text under the title, built from the context so that it can count the rows (`c => $"{c.TargetCount} customers will be removed."`, which is right for the ticked rows and for every result). |
| `Title` (init) | `string` | The title of the dialog. |

#### `DataListActionContext<TItem>`

`Klyroo.Table` · class

What an action receives when it runs: the rows it targets, the scope and the request that stands for "every result".

| Property | Type | Description |
| :--- | :--- | :--- |
| `CancellationToken` | `CancellationToken` | Cancelled when the list goes away. |
| `List` | `DataListContext<TItem>` | The list, to read its state or to load it again (`DataListContext.RefreshAsync`). |
| `MatchingCount` | `long?` | How many rows the search and the filters match, when the source says (`DataListResult.TotalCount`); `null` when it does not. |
| `Request` | `DataListRequest?` | The request for every row the list matches: its search, filters and sort, with no paging and no facets, as the list sent it. For `DataListActionScope.All` over a provider, send it to your server and apply it there (`schema.Apply(query, request)`), so a bulk action never needs the rows on the client. `null` before the first load. |
| `Scope` | `DataListActionScope` | Whether the action targets the ticked rows or every result. |
| `SelectedItems` | `IReadOnlyList<TItem>` | The rows the action targets. With `DataListActionScope.Selection`: the ticked rows (those that are on screen or were ticked on another page of a list in memory). With `DataListActionScope.All` on a list of rows in memory, when the action runs: every row the search and the filters match. With `DataListActionScope.All` over a provider or a queryable: empty, because the list never holds those rows; apply `DataListActionContext.Request` instead. |
| `SelectedKeys` | `IReadOnlyList<string>` | The row keys (`RowKey` of the schema) of `DataListActionContext.SelectedItems`; empty where those are empty too. |
| `TargetCount` | `long` | How many rows the action will act on: `DataListActionContext.MatchingCount` when `DataListActionContext.Scope` is `DataListActionScope.All` (and the source knows it), else the number of ticked rows. Use it, not `SelectedItems.Count`, in the text of a confirmation and in `Visible` and `Disabled`: before the action runs, `DataListActionContext.SelectedItems` holds only the ticked rows (a list is never loaded just to describe an action), while the action itself gets every matching row of a list in memory. |

#### `DataListContext<TItem>`

`Klyroo.Table` · class

What the parts of a `KlyDataList` (the table, the pagination, the result count...) read and change: the state of the list, its rows and its loading flags. The list cascades one for its whole life (the value is fixed), so a part subscribes to `Changed` once and renders again when it is raised. Write your own part by taking it as a cascading parameter.

| Property | Type | Description |
| :--- | :--- | :--- |
| `ActiveGroupBy` | `DataListGroupBy<TItem>?` | The active grouping configuration for the current layout (`KlyDataListTable.GroupBy` or `KlyDataListGrid.GroupBy`), or `null`. |
| `AllExpanded` | `bool` | Whether every branch node in the loaded tree is currently expanded. |
| `AllResultsRequest` | `DataListRequest?` | The request for every row the list matches (the loaded request without paging or facets), or `null` before the first load. |
| `BulkScope` | `DataListActionScope` | What a bulk action targets: the ticked rows, or every result of the query on screen. |
| `CanLink` | `bool` | The state lives in the URL and the renderer is not interactive: controls are links (`DataListContext.HrefWith`) and GET forms. |
| `CanSelectAllResults` | `bool` | Whether the bulk scope can switch to every result: the source knows how many results there are and there are more than the ticked rows. |
| `Columns` | `IReadOnlyList<KlyDataListColumnBase<TItem>>` | Every column the list's table declares, visible or not (a columns panel lists them all), by `KlyDataListColumnBase.Order` and then in the order they first rendered: the order of the markup, except for a column that an `@if` adds later, which comes after the others unless it has an `Order`. Prefer `KlyDataListColumnBase.Visible` to an `@if`. |
| `ColumnStates` | `IReadOnlyList<DataListColumnState>` | Every keyed column of the table in its active order, with its effective visibility, pin edge and width. |
| `Density` | `DataListDensity` | The density of the rows. |
| `EffectiveLayout` | `DataListLayout` | The layout currently shown: `DataListState.Layout` when both table and grid exist, otherwise the one that is present. |
| `EffectiveSort` | `IReadOnlyList<DataListSort>` | The sort that applies: the state's, or the schema's default one when the state has none. |
| `Error` | `Exception?` | Why the newest load failed. |
| `ExpandedKeys` | `IReadOnlySet<string>` | Keys of all currently expanded branch nodes in the loaded tree; with a detail row, the keys of the rows the user opened. |
| `HasCustomColumnPreferences` | `bool` | Whether any column visibility, order, pinning or width currently differs from the columns' defaults. |
| `HasDetail` | `bool` | Whether the table shows a detail row under each row (`KlyDataListTable.DetailTemplate`). Its rows are opened and closed with `DataListContext.SetRowExpanded`. |
| `HasError` | `bool` | The newest load failed. The previous rows, if any, are still in `DataListContext.Rows`. |
| `HasLoadMoreError` | `bool` | The newest load-more request failed while the previously loaded rows stayed on screen. |
| `HasMore` | `bool` | Whether the source has another page after the rows loaded so far (cursor paging only). |
| `HasRowSummary` | `bool` | Whether the table has a `DataListRowSummary` configured. |
| `HasTree` | `bool` | Whether the table has a `DataListTree` configured. |
| `HoldsAllRows` | `bool` | Whether the list holds every row its query matches (a list of rows in memory, `Items`), so that `DataListContext.ExportRows` can return them for `DataListExportScope.All`. A list over a provider or a queryable does not: export those rows from the request in `DataListContext.AllResultsRequest`. |
| `IsFiltered` | `bool` | The rows have a search or filters applied, so an empty result means "nothing matches" rather than "nothing exists". |
| `IsInitial` | `bool` | No result yet and nothing failed: show placeholders. |
| `IsInteractive` | `bool` | The renderer can run handlers: controls are buttons. Otherwise (static rendering, prerendering) they are links and forms when `DataListContext.CanLink`. |
| `IsLoading` | `bool` | A load is running. |
| `IsLoadingMore` | `bool` | The next page of a cursor-paged list is being loaded to append to the rows on screen. |
| `IsRefreshing` | `bool` | The rows on screen are being loaded again (refresh or retry). |
| `IsReplacing` | `bool` | Other rows are being loaded (a new page, sort or filter) while the previous ones stay on screen. |
| `Label` | `string?` | The name of the list, for the accessible names of its table and its pagination (`Label` of the list). |
| `LoadedState` | `DataListState?` | The state `DataListContext.Result` was loaded for: the page and page size of the rows on screen. |
| `LoadMoreError` | `Exception?` | Why the newest load-more request failed; `null` when it did not. |
| `MatchingCount` | `long?` | How many rows the search and the filters match, when the source says; `null` when it does not. |
| `NextCursor` | `string?` | The opaque cursor for the next page (cursor paging only), or `null` after the last one. |
| `PageSelection` | `bool?` | The state of the select-all box for the rows on screen: `true` when all are ticked, `false` when none is (or there are no rows), `null` when only some are (the mixed state). |
| `PagingMode` | `DataListPagingMode` | How the list pages its rows. |
| `Path` | `string` | The path of the current page, the action of a GET form. |
| `Result` | `DataListResult<TItem>?` | The result on screen: the last one that loaded, kept while the next one loads. `null` before the first load. |
| `ResultVersion` | `int` | Counts the results shown so far; it does not change when only a flag does, which lets rows skip a render. |
| `Rows` | `IReadOnlyList<TItem>` | The rows of `DataListContext.Result`; empty before the first load. |
| `Schema` | `DataListSchema<TItem>?` | The schema, once the list has one (an explicit one, or the one its columns describe after they registered). |
| `SelectedCount` | `int` | How many rows are ticked. |
| `SelectedItems` | `IReadOnlyList<TItem>` | The ticked rows that the list holds: those on screen and, for a list of rows in memory, those ticked on other pages. |
| `SelectedKeys` | `IReadOnlySet<string>` | The keys of the ticked rows (`RowKey` of the schema). A set that later changes do not touch. |
| `SelectionDisabled` | `bool` | The boxes cannot be used yet: the render is the prerender of a page that will be interactive. |
| `SelectionEnabled` | `bool` | The list has a selection: `Selectable` is on, or it has a `KlyDataListSelectionActions` bar, or `SelectedKeys` is bound. |
| `SelectionVisible` | `bool` | The selection is drawn: it is enabled and the renderer can run handlers (or will, once a prerendered page turns interactive). A static render has no checkboxes. |
| `State` | `DataListState` | The state the list shows: search, filters, sort, page, page size. Immutable; change it with `DataListContext.SetState`. |
| `SummaryViewActive` | `bool` | Whether the table currently renders compact summary rows instead of the multi-column table. |
| `Tree` | `DataListTree<TItem>?` | The active hierarchical tree configuration on `KlyDataListTable`, or `null`. |
| `TreeBranchCount` | `int` | Total number of branch nodes (nodes with children) in the loaded tree. |
| `UrlKey` | `string?` | The key prefix of the URL (`UrlKey`), or `null`. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `AddSort(DataListState state, string key, bool descending = false)` | `DataListState` | The state with `key` added to the end of the sort in the given direction, or turned to that direction in place when it is already sorted. First page. |
| `ClearSelection()` | `void` | Unticks every row, and goes back to acting on the selection when the bulk scope was every result. |
| `ClearSort(DataListState state)` | `DataListState` | The state with the sort restored to the schema's default sort (or no sort when the schema has none), on the first page. |
| `CollapseAll()` | `void` | Collapses every branch node in the tree. |
| `ExpandAll()` | `void` | Expands every branch node in the tree. |
| `ExportCsv(DataListExportScope scope, DataListCsvOptions? options = null)` | `string` | Writes the rows of `scope` as CSV, see `DataListContext.ExportRows` and `DataListContext.ToCsv`. For `DataListExportScope.All` over a provider or a queryable, load the rows from `DataListContext.AllResultsRequest` and pass them to `DataListContext.ToCsv`. |
| `ExportHeaders()` | `IReadOnlyList<string>` | The columns an export writes: the visible columns of the table in the order the user sees them (pinned ones first and last, hidden ones left out), without the columns that only have a template (they have no value to write), or without any declared column the fields of the schema that are not collections. Each has the title of the header. |
| `ExportRows(DataListExportScope scope)` | `IReadOnlyList<TItem>` | The rows an export takes: `DataListContext.Rows` for `DataListExportScope.Page`, the ticked rows for `DataListExportScope.Selection`, and for `DataListExportScope.All` every row the search and the filters match when `DataListContext.HoldsAllRows` (empty otherwise). |
| `FieldName(string key)` | `string` | The name of a form field for a key of the list (`ps` becomes `users.ps` with `UrlKey="users"`). |
| `FilteredRows()` | `IReadOnlyList<TItem>` | Every row matching the active search and filters for an in-memory list (`Items`), or the loaded `DataListContext.Rows` for a provider or queryable source. |
| `FormFields(DataListState state, params string[] exclude)` | `IReadOnlyList<KeyValuePair<string, string>>` | The fields of a GET form that submits `state`: every parameter of the URL that is not the list's own, then the list's own for the state, so submitting the form does not drop anything. Keys the form supplies itself (`exclude`, names relative to `DataListContext.UrlKey`: `ps`, `p`) are left out. |
| `GetGroupSelection(DataListGroup<TItem> group)` | `bool?` | The selection state of a visual row group: `true` when all rows in `group` are ticked, `false` when none is (or the group is empty), and `null` when only some are ticked (indeterminate). |
| `Groups()` | `IReadOnlyList<DataListGroup<TItem>>` | Builds the visual groups of `DataListContext.Rows` for the active layout when `DataListContext.ActiveGroupBy` is configured; empty otherwise. |
| `HrefWith(Func<DataListState, DataListState> change)` | `string?` | The URL of the current page with the state `change` returns written in it, for a link of a static render; `null` when the list is not synced with the URL. |
| `IsDefaultSort(DataListState state)` | `bool` | Whether the active sort of `state` equals the schema's default sort. |
| `IsFilteredFor(DataListState state)` | `bool` | Whether `state` has a search or filters applied: `DataListContext.IsFiltered` for the state the rows on screen were loaded for. |
| `IsGroupCollapsed(DataListGroup<TItem> group)` | `bool` | Whether `group` is currently collapsed (folded). |
| `IsKeySelected(string key)` | `bool` | Whether a key is ticked. |
| `IsNodeExpanded(DataListTreeNode<TItem> node)` | `bool` | Whether `node` is currently expanded. |
| `IsRowExpanded(string key, int depth = 0)` | `bool` | Whether the row with `key` is currently expanded: for a tree the row at `depth` shows its children, for a table with a `KlyDataListTable.DetailTemplate` (`depth` stays 0) it shows its detail row. Always `false` when the table has neither. |
| `IsSelected(TItem item)` | `bool` | Whether a row is ticked. |
| `LoadMoreAsync()` | `Task` | Appends the next page of a cursor-paged list. Does nothing outside cursor mode, while a load is running, when there is no next cursor, or when the renderer is not interactive. |
| `MoveColumn(string key, int delta)` | `void` | Moves a column earlier (`-1`) or later (`+1`) in the column order. |
| `MoveColumnNextTo(string key, string targetKey, bool after)` | `void` | Puts a column right before or right after another one in the column order: what dropping a dragged column on another does. |
| `MoveSort(DataListState state, string key, int offset)` | `DataListState` | The state with `key` moved `offset` places in the sort (negative is earlier), kept inside the list. First page. |
| `NextSort(DataListState state, string key)` | `DataListState` | The state after a plain click on the sort control of `key`, which keeps one sort: ascending, then descending, then back to the default sort. A key that is in the sort but not first becomes the only sort, ascending. The page goes back to the first. |
| `NextSort(DataListState state, string key, bool append)` | `DataListState` | The state after a click on the sort control of `key`. Without `append` it keeps one sort: ascending, then descending, then the default sort. With `append` (Shift held) the other sorts stay: a key that is not sorted yet joins the end of the list ascending, then turns descending in place, then leaves the list. The page goes back to the first. |
| `RefreshAsync()` | `Task` | Loads the rows on screen again, or the failed request again after an error. |
| `RemoveSort(DataListState state, string key)` | `DataListState` | The state without `key` in its sort (the default sort when nothing else is left). First page. |
| `Reset(QueryHistory history = Push)` | `void` | Takes the search and the filters off and goes back to the first page (the sort, the page size and the layout stay), with a history entry by default. Nothing happens in a renderer that is not interactive: link to `DataListContext.HrefWith` with `DataListContext.ResetState`. |
| `ResetAll(QueryHistory history = Push)` | `void` | Resets the search and filters to the schema defaults (on page 1), clears the row selection, and resets tree expansion to its defaults. |
| `ResetColumns()` | `void` | Restores every column's visibility, order, pinning and width to its declared defaults and clears any saved cookie. |
| `ResetExpansion()` | `void` | Resets tree expansion back to the `DataListTree` defaults. |
| `ResetState(DataListState state)` | `DataListState` | `state` without its search and its filters (those of the schema's default state), on the first page: what `DataListContext.Reset` sets. |
| `ReverseSort(DataListState state, string key)` | `DataListState` | The state with the direction of `key` reversed, in the same place of the sort. First page. |
| `RowKey(TItem item)` | `string` | The row key of a row, as text. Throws when the schema has no `RowKey`. |
| `SetAllColumnsVisible(bool visible)` | `void` | Shows every column (`visible` true) or hides every column that is not `KlyDataListColumnBase.Required`; when none is, the first one stays so the table is never empty. |
| `SetBulkScope(DataListActionScope scope)` | `void` | Sets what a bulk action targets. `DataListActionScope.All` needs `DataListContext.CanSelectAllResults`; it is dropped when the rows or the query change. |
| `SetColumnPinned(string key, DataListPin pin)` | `void` | Pins a column to the start or end edge, or unpins it. |
| `SetColumnVisible(string key, bool visible)` | `void` | Shows or hides a column by key. Does nothing for a `KlyDataListColumnBase.Required` column. |
| `SetColumnWidth(string key, int? width)` | `void` | Sets a custom pixel width for a resizable column, or clears it when `null`. |
| `SetGroupCollapsed(DataListGroup<TItem> group, bool collapsed)` | `void` | Sets whether `group` is collapsed (folded), persisting the toggle when `PreferencesKey` is set. |
| `SetGroupSelected(DataListGroup<TItem> group, bool selected)` | `void` | Ticks or unticks every row in `group`. |
| `SetLayout(DataListLayout layout)` | `void` | Switches the active layout (`DataListLayout.Table` or `DataListLayout.Grid`) as a history entry. |
| `SetNodeExpanded(DataListTreeNode<TItem> node, bool expanded)` | `void` | Sets whether `node` is expanded. |
| `SetPageSelected(bool selected)` | `void` | Ticks or unticks the rows on screen. |
| `SetRowExpanded(string key, bool expanded, int depth = 0)` | `void` | Sets whether the row with `key` is expanded: its children in a tree, its detail row in a table with a `KlyDataListTable.DetailTemplate`. Nothing happens when the table has neither. |
| `SetSelected(TItem item, bool selected, bool range = false)` | `void` | Ticks or unticks a row. With `range` (a click with Shift held) every row on screen between the row ticked last and this one gets the same state. Nothing happens in a renderer that is not interactive. |
| `SetState(Func<DataListState, DataListState> change, QueryHistory history = Push)` | `void` | Changes the state: `change` receives the current one and returns the new one. With URL sync the change is written to the URL (`history` says whether it adds an entry: push for a page or a sort, replace for typing) and the list follows the URL; without it the list keeps the state itself and reports it through `StateChanged`. Nothing happens in a renderer that is not interactive: use `DataListContext.HrefWith`. |
| `SetSummaryView(bool active)` | `void` | Switches between compact summary rows (`true`) and the full table (`false`), persisting the choice when `PreferencesKey` is set. |
| `SortDescending(string key)` | `bool?` | The sort of a key as it stands: `null` when the list is not sorted by it, else whether it is descending. The key can be anywhere in `DataListContext.EffectiveSort`, not only first. |
| `SortIndex(string key)` | `int?` | The place of a key in `DataListContext.EffectiveSort`, from 0 for the first sort; `null` when the list is not sorted by it. |
| `SortOptions(IReadOnlyList<DataListSortOption>? explicitOptions = null)` | `IReadOnlyList<DataListSortOption>` | The sortable fields of the list for `KlyDataListSortMenu`: from `explicitOptions` when given (filtered to the schema's sortable keys), otherwise from the sortable `KlyDataListColumnBase`s in user order followed by any remaining sortable fields of the schema. |
| `ToCsv(IEnumerable<TItem> rows, DataListCsvOptions? options = null)` | `string` | Writes `rows` as CSV: one cell per `DataListContext.ExportHeaders` entry, formatted like the table shows it (the `Format` of the column, the culture of the file). A cell that is text is protected against formula injection (see `DataListCsvOptions.ProtectFormulas`); numbers and dates are not, so a negative number stays a number. |
| `ToggleGroup(DataListGroup<TItem> group)` | `void` | Folds or unfolds `group`, persisting the toggle when `PreferencesKey` is set. |
| `ToggleNodeExpanded(DataListTreeNode<TItem> node)` | `void` | Expands or collapses `node`. |
| `UpdateRow(TItem item)` | `bool` | Replaces a single row in the loaded result in place by its schema `RowKey`, without querying the source again. |
| `UpdateRows(IEnumerable<TItem> items)` | `int` | Replaces matching rows in the loaded result in place by their schema `RowKey`, without querying the source again. |
| `VisibleTreeNodes()` | `IReadOnlyList<DataListTreeNode<TItem>>` | The visible (unfolded) tree nodes in display order when `DataListContext.Tree` is configured; empty otherwise. |
| `WithLayout(DataListState state, DataListLayout layout)` | `DataListState` | The state with `DataListState.Layout` set to `layout`. |
| `WithSort(DataListState state, string key, bool descending = false)` | `DataListState` | The state with `key` sorted in the given direction, on the first page. |

#### `DataListController<TItem>`

`Klyroo.Table` · class

Loads the rows of a data list: turns a `DataListState` into a request, runs it against the source, cancels the request a newer one replaces and keeps the rows of the previous result while the next one loads. A plain class, so it is tested without a renderer; the `KlyDataList` component owns one and publishes its flags to the parts through `DataListContext`.

| Constructor | Description |
| :--- | :--- |
| `DataListController<TItem>(DataListSchema<TItem> schema, DataListProvider<TItem> provider)` | Creates a controller. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Error` | `Exception?` | Why the newest load failed; `null` when it did not. The rows of the previous result stay in `DataListController.Rows`. |
| `HasError` | `bool` | The newest load failed. |
| `HasLoadMoreError` | `bool` | The newest `DataListController.LoadMoreAsync` failed. |
| `HasMore` | `bool` | Whether there is another page to append in cursor mode. |
| `IsInitial` | `bool` | No result yet and nothing failed: the first load is running or has not started. A table shows placeholder rows. |
| `IsLoading` | `bool` | A load is running. |
| `IsLoadingMore` | `bool` | The next page of a cursor-paged list is being loaded (`DataListController.LoadMoreAsync`) to append to `DataListController.Rows`. |
| `IsRefreshing` | `bool` | A load of the request that is already on screen is running (`DataListController.RefreshAsync`). |
| `IsReplacing` | `bool` | A load of another request than the one on screen is running, with the previous rows still shown. |
| `LoadedRequest` | `DataListRequest?` | The request the last result was loaded with. |
| `LoadedState` | `DataListState?` | The state the last result was loaded for: the requested one with the page and page size brought into range. |
| `LoadMoreError` | `Exception?` | Why the newest `DataListController.LoadMoreAsync` failed; `null` when it did not. The previously loaded rows and `DataListController.NextCursor` stay intact so retrying works. |
| `MaxRefreshPages` (get, set) | `int` | The most cursor pages a `DataListController.RefreshAsync` loads again (20 by default, at least 1). A refresh of a cursor-paged list asks for the pages that are on screen one after the other, starting from the first cursor, and replaces the rows with what they return, so the list stays where it was; when more pages than this were loaded, the first `DataListController.MaxRefreshPages` come back and the list goes on from the cursor after them. This bounds the requests a refresh makes and the memory it holds (the old rows and the new ones for the time it runs). |
| `NextCursor` | `string?` | The opaque cursor of the next page (cursor paging only), or `null` after the last one. |
| `PagesLoaded` | `int` | How many cursor pages `DataListController.Rows` holds right now: 1 after a load, one more after each `DataListController.LoadMoreAsync` that appended a page (cursor paging only). |
| `PagingMode` (get, set) | `DataListPagingMode` | How requests page: numbered pages (`DataListPagingMode.Offset`), keyset cursors (`DataListPagingMode.Cursor`) or all matching rows (`DataListPagingMode.None`). |
| `Provider` (get, set) | `DataListProvider<TItem>` | Where the rows come from. Replace it when the source of the list changes; the next load uses it. |
| `RequestedState` | `DataListState?` | The state of the newest load, finished or not. |
| `Result` | `DataListResult<TItem>?` | The last result that loaded, kept while the next one loads. `null` until the first one. |
| `ResultVersion` | `int` | Counts the results shown so far: it changes when a load or an adoption replaces `DataListController.Result`, not when only a flag changes. |
| `Rows` | `IReadOnlyList<TItem>` | The rows of `DataListController.Result`, empty before the first load. |
| `Schema` | `DataListSchema<TItem>` | The schema of the rows. |
| `TimeZone` (get, set) | `string?` | The IANA id of the time zone of the user (`Europe/Paris`), sent with each request (`DataListRequest.TimeZone`). `null` means UTC. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Adopt(DataListState state, DataListState loaded, DataListRequest request, DataListResult<TItem> result)` | `void` | Sets a result that was loaded elsewhere (the prerender), without calling the provider. |
| `BuildRequest(DataListState state)` | `DataListRequest` | The request for a state: sanitized by the schema, carrying the default sort when the state has none and the time zone. |
| `Cancel()` | `void` | Cancels the load that is running, if any. Its result is ignored. |
| `static CreateProvider(DataListSchema<TItem> schema, IEnumerable<TItem>? items, DataListProvider<TItem>? provider, IQueryable<TItem>? queryable = null, IDataListQueryExecutor? executor = null)` | `DataListProvider<TItem>` | The one source of a list from what the component was given. |
| `Dispose()` | `void` | Cancels the running load. The controller cannot load afterwards. |
| `LoadAsync(DataListState state, bool refresh = false)` | `Task` | Loads the rows for a state. A load that is still running is cancelled and its result, should it arrive, is ignored. The rows of the previous result stay until this one replaces them; a failure keeps them and sets `DataListController.Error`. A page past the last one is replaced by the last page when the total is known (read it back as `DataListController.LoadedState`). |
| `LoadMoreAsync()` | `Task` | Appends the next page of a cursor-paged list using `DataListController.NextCursor`. Does nothing outside `DataListPagingMode.Cursor`, while a load is running or when there is no next cursor. A newer `DataListController.LoadAsync` cancels this load and ignores its result; a failure keeps the rows loaded so far and `DataListController.NextCursor`, sets `DataListController.LoadMoreError` and lets the caller retry. |
| `Normalize(DataListState state)` | `DataListState` | Brings a state into the range the schema allows: a page of at least 1, a page size from 1 to `DataListSchema.MaxPageSize` (the default one for zero or less), a search without the spaces around it and no cursor unless the list pages by cursor (a stray `c` in the address of a list with numbered pages, or with no paging, is dropped). |
| `RefreshAsync()` | `Task` | Loads the state of the newest load again: the retry after an error and the refresh button. |
| `SetLoadedTimeZone(string? timeZone)` | `void` | Sets the time zone of the request an adopted result stands for, once the browser told it (the prerender ran in UTC). |
| `UpdateRow(TItem item)` | `bool` | Replaces a single row in the loaded `DataListController.Result` in place by matching its schema `RowKey`, without querying the provider again. Returns `true` when a matching row was found and replaced. |
| `UpdateRows(IEnumerable<TItem> items)` | `int` | Replaces matching rows in the loaded `DataListController.Result` in place by their schema `RowKey`, without querying the provider again. Returns the number of rows replaced. |

#### `DataListEmptyAction`

`Klyroo.Table` · class

First-use action button or link displayed inside the unfiltered empty state of `KlyDataListTable` or `KlyDataListGrid` when `DataListEmptyState.Action` is configured. Never shown on a filtered empty state.

| Constructor | Description |
| :--- | :--- |
| `DataListEmptyAction()` | Creates an empty-state action. Set `DataListEmptyAction.Label`, then `DataListEmptyAction.Href` or `DataListEmptyAction.OnClick`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Color` (init) | `KlyColor` | Button color (`KlyColor.Primary` by default). |
| `Href` (init) | `string?` | Optional navigation link URL. When set, renders a link button (works in static SSR too). |
| `Icon` (init) | `string?` | Optional leading icon name. |
| `Label` (init) | `string` | Label of the action button or link. |
| `OnClick` (init) | `EventCallback` | Callback invoked when the button is clicked in an interactive render. |
| `Variant` (init) | `ButtonVariant` | Button visual variant (`ButtonVariant.Solid` by default). |

#### `DataListEmptyState`

`Klyroo.Table` · class

Declarative configuration for the unfiltered empty state of `KlyDataListTable` or `KlyDataListGrid`. When search or filters are active and match nothing, the filtered empty state (with Clear search / Reset filters) is still shown instead, and `DataListEmptyState.Action` is omitted so a first-use creation action is never offered while results are merely filtered out.

| Constructor | Description |
| :--- | :--- |
| `DataListEmptyState()` | Creates an empty-state configuration that changes nothing: the default icon, title and description. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Action` (init) | `DataListEmptyAction?` | Optional first-use call-to-action shown only when the list is empty without active search or filters. |
| `Description` (init) | `string?` | Optional description overriding the localized `KlyToolsMessages.NoResultsDescription`. |
| `HideDescription` (init) | `bool` | When `true`, hides the description line on the unfiltered empty state. |
| `Icon` (init) | `string?` | Optional icon name overriding the default empty-state icon (or empty string to hide the icon). |
| `Title` (init) | `string?` | Optional title overriding the localized `KlyToolsMessages.NoResults`. |

#### `DataListGroup<TItem>`

`Klyroo.Table` · class

One visual group of rows produced by a `DataListGroupBy` over the loaded rows of a data list. The group of a `DataListGroupBy.By`` grouping is a `DataListGroup`, which also carries the typed value.

| Property | Type | Description |
| :--- | :--- | :--- |
| `DefaultCollapsed` | `bool` | Whether the group starts folded before any user toggle. |
| `IsEmptyValue` | `bool` | Whether the group holds rows whose group value is `null` or blank. |
| `Items` | `IReadOnlyList<TItem>` | The rows belonging to this group, in the active sort order. |
| `Key` | `string` | Stable identifier of the group (`"~empty"` when the group value is null or blank). |
| `Label` | `string` | The display label of the group header. |
| `RowKeys` | `IReadOnlyList<string>` | The string keys of `DataListGroup.Items` when the schema defines a `RowKey`; empty otherwise. |
| `Summary` | `string?` | The summary text shown beside the group label (the row count by default). |

#### `DataListGroup<TItem, TKey>`

`Klyroo.Table` · class · inherits `DataListGroup<TItem>`

A group of a grouping by a value of type `TKey`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` | `TKey` | The group value; `default` when `DataListGroup.IsEmptyValue` is `true` (a null or blank value). |

#### `DataListGroupBy<TItem>`

`Klyroo.Table` · class

Visual grouping configuration for `KlyDataListTable` and `KlyDataListGrid`. Create one with `DataListGroupBy.By`` (a value read from the row) or `DataListGroupBy.ByField` (a field of the schema). Groups are visual only: filtering, sorting, pagination and actions continue to operate on the flat rows, while rows on screen are partitioned under collapsible `KlyDataListGroupHeader` headers with optional per-group selection checkboxes.

| Field | Type | Description |
| :--- | :--- | :--- |
| `const EmptyGroupKey = "~empty"` | `string` | The stable key used for the group of rows whose value is `null` or empty. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Collapsed` (get, set) | `bool` | Whether groups start collapsed (folded) by default (`false` by default). The `DefaultCollapsed` predicate of the grouping, when set, decides instead. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static By<TKey>(Func<TItem, TKey> key, IReadOnlyList<TKey>? order = null, IEqualityComparer<TKey>? comparer = null)` | `DataListGroupBy<TItem, TKey>` | Groups the rows by a value read from each row: `DataListGroupBy<Order>.By(o => o.Status)`. A `null` or blank value goes to the empty group, which comes last. |
| `static ByField(string fieldKey, IReadOnlyList<string>? order = null)` | `DataListGroupBy<TItem, string>` | Groups the rows by a field of the schema: `DataListGroupBy<Order>.ByField("status")`. The value of a group is the text of the field in the invariant culture; the label is formatted with the culture of the page. Groups come in the order of the field (numbers and dates by value). |

#### `DataListGroupBy<TItem, TKey>`

`Klyroo.Table` · class · inherits `DataListGroupBy<TItem>`

A grouping by a value of type `TKey`, from `DataListGroupBy.By`` or `DataListGroupBy.ByField`.

| Constructor | Description |
| :--- | :--- |
| `DataListGroupBy<TItem, TKey>(Func<TItem, TKey> key, IReadOnlyList<TKey>? order = null, IEqualityComparer<TKey>? comparer = null)` | Creates a grouping by a value read from each row, to set the label, summary and collapsing in an object initializer. `DataListGroupBy.By`` is the shorter form when none is needed. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Comparer` | `IEqualityComparer<TKey>?` | Tells when two values are the same group; `null` for the default equality of the type. |
| `DefaultCollapsed` (get, set) | `Func<DataListGroup<TItem, TKey>, bool>?` | Optional per-group predicate determining whether a group starts collapsed (folded) by default. Wins over `DataListGroupBy.Collapsed`. |
| `Label` (get, set) | `Func<DataListGroup<TItem, TKey>, string?>?` | Formats the heading label of a group. When `null`, formats `DataListGroup.Value` or uses the localized "Other" label (`KlyToolsMessages.GroupEmpty`) for empty values. |
| `LabelTemplate` (get, set) | `RenderFragment<DataListGroup<TItem, TKey>>?` | Custom template for the group heading label inside `KlyDataListGroupHeader`. |
| `Order` | `IReadOnlyList<TKey>?` | Group values placed first, in this order. Remaining non-empty groups follow in ascending value order, and the empty group (`null` or blank) comes last unless a blank value is listed. |
| `Summary` (get, set) | `Func<DataListGroup<TItem, TKey>, string?>?` | Formats the summary badge shown beside the group label. When `null`, shows the number of rows in the group. Return an empty string to hide the badge. |
| `SummaryTemplate` (get, set) | `RenderFragment<DataListGroup<TItem, TKey>>?` | Custom template for the summary badge shown beside the group label inside `KlyDataListGroupHeader`. |

#### `DataListRowAction<TItem>`

`Klyroo.Table` · class

An action on one row of a data list (`KlyDataListRowActions`): a button of the row, an entry of its menu or of its sheet on a phone. Actions with the same `DataListRowAction.Group` are kept together, separated from the others by a divider; an action with `DataListRowAction.Children` opens a submenu.

| Constructor | Description |
| :--- | :--- |
| `DataListRowAction<TItem>()` | Creates a row action. Set `DataListRowAction.Key` and `DataListRowAction.Label`, then `DataListRowAction.OnExecute` or `DataListRowAction.Href`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Children` (init) | `IReadOnlyList<DataListRowAction<TItem>>?` | The entries of a submenu. An action with children opens them instead of running, and only shows when one of them does. It always sits in the menu, never in a button of the row. |
| `Color` (init) | `KlyColor` | The color; `KlyColor.Error` for a destructive action. |
| `Confirm` (init) | `DataListRowActionConfirm<TItem>?` | Asks before the action runs. Without it the action runs on the click. |
| `Disabled` (init) | `Func<DataListRowActionContext<TItem>, bool>?` | Disables the action when it returns `true`; the children of a disabled action cannot run either. It runs on every render: keep it cheap. |
| `DisabledReason` (init) | `Func<DataListRowActionContext<TItem>, string?>?` | Why the action is disabled: shown under its label in the sheet of a phone, and on a wide screen as the title (the tooltip) and the accessible description of its button or menu entry. The reason of an action with `DataListRowAction.Children` is the one its children show while it is disabled. |
| `Group` (init) | `string?` | The group the action belongs to. Groups are shown in the order of their first visible action, with a divider between them; actions without a group form one of their own. |
| `Href` (init) | `Func<DataListRowActionContext<TItem>, string?>?` | Makes the action a link to the address it returns (`null`: no link for this row). A link works in a statically rendered list too, where it is the only kind of row action that is shown. `DataListRowAction.OnExecute` is not called for it. |
| `Icon` (init) | `string?` | The name of an icon shown before the label (`copy`), one of the icons of Klyroo.UI. |
| `Key` (init) | `string` | Identifies the action; unique among the actions of the part, children included. |
| `Label` (init) | `string` | The text of the button or of the menu entry. |
| `Loading` (init) | `Func<DataListRowActionContext<TItem>, bool>?` | Shows a spinner and blocks the action while it returns `true` (work started elsewhere). The action also shows one while it runs. |
| `OnExecute` (init) | `Func<DataListRowActionContext<TItem>, Task>?` | What the action does. An exception, or a call to `DataListRowActionContext.Fail`, marks the run as failed: the list is not refreshed, `OnError` of the part is raised and nothing is thrown to the page. |
| `RefreshOnSuccess` (init) | `bool` | Loads the list again once the action succeeded (the row it changed or removed). Off by default; an action can also call `DataListContext.RefreshAsync` itself. |
| `Visible` (init) | `Func<DataListRowActionContext<TItem>, bool>?` | Hides the action when it returns `false` (an action that does not apply to this row); the children of a hidden action are hidden with it. It runs on every render: keep it cheap. |

#### `DataListRowActionConfirm<TItem>`

`Klyroo.Table` · class

The question a row action asks before it runs, in a dialog. The text can name the row.

| Constructor | Description |
| :--- | :--- |
| `DataListRowActionConfirm<TItem>()` | Creates a confirmation. Set `DataListRowActionConfirm.Title`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Color` (init) | `KlyColor?` | The color of the confirm button; `KlyColor.Error` for a destructive action. Without it the color of the action. |
| `ConfirmLabel` (init) | `string?` | The label of the confirm button. Without it the localized "Confirm". |
| `Description` (init) | `Func<DataListRowActionContext<TItem>, string?>?` | The text under the title, built from the context (`c => $"{c.Item.Name} will be removed."`). |
| `Title` (init) | `string` | The title of the dialog. |

#### `DataListRowActionContext<TItem>`

`Klyroo.Table` · class

What a row action receives when it runs: the row, its key and place, and a way to report a failure without throwing.

| Property | Type | Description |
| :--- | :--- | :--- |
| `CancellationToken` | `CancellationToken` | Cancelled when the list goes away. |
| `Failed` | `bool` | Whether `DataListRowActionContext.Fail` was called. |
| `FailureMessage` | `string?` | The message given to `DataListRowActionContext.Fail`. |
| `Item` | `TItem` | The row. |
| `Key` | `string` | The row key (`RowKey` of the schema, else the identity of the row). |
| `List` | `DataListContext<TItem>` | The list, to read its state or to load it again (`DataListContext.RefreshAsync`). |
| `Position` | `int` | The place of the row on the page, from 1; 0 when the part was placed outside a table row. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Fail(string? message = null)` | `void` | Reports that the action did not do what it was asked (a refusal of the server, a conflict) without throwing: the list is not refreshed and `OnError` of the part is raised. Call it, then return. |

#### `DataListSummaries<TItem>`

`Klyroo.Table` · class

Footer summary configuration (`<tfoot>`) for `KlyDataListTable`. When omitted on `KlyDataListTable`, a single summary row with `DataListSummaryScope.Filtered` is still rendered automatically whenever at least one `KlyDataListColumnBase` sets `KlyDataListColumnBase.Summary`, `KlyDataListColumn.SummaryValue` or `KlyDataListColumnBase.SummaryTemplate`.

| Constructor | Description |
| :--- | :--- |
| `DataListSummaries<TItem>()` | Creates a summary configuration with one row for the `DataListSummaryScope.Filtered` scope. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Label` (init) | `string?` | Label of the summary row (defaults to the localized `KlyToolsMessages.SummaryTotal` "Total" when a single scope is shown, or the localized scope name when multiple `DataListSummaries.Scopes` are shown). |
| `ResolveAsync` (init) | `Func<DataListSummaryResolveContext<TItem>, ValueTask<IReadOnlyDictionary<string, DataListSummaryValue>>>?` | Optional asynchronous resolver returning a map of column key to aggregated value (`DataListSummaryValue`, converted implicitly from a number or a text) for a scope (for remote sources or custom async totals). Columns not present in the returned dictionary fall back to their declared `KlyDataListColumnBase.Summary` or `KlyDataListColumn.SummaryValue`. |
| `Scope` (init) | `DataListSummaryScope` | The default scope aggregated when `DataListSummaries.Scopes` is not set (`DataListSummaryScope.Filtered` by default). |
| `Scopes` (init) | `IReadOnlyList<DataListSummaryScope>?` | Optional list of scopes to render as separate summary rows in `<tfoot>` (for example `[DataListSummaryScope.Page, DataListSummaryScope.Filtered]`). When `null` or empty, renders one row for `DataListSummaries.Scope`. |

#### `KlyrooTableOptions`

`Klyroo.Table` · class

Configuration of Klyroo.Table: its component themes and its message tables. It is an extension of `KlyrooOptions` (`KlyrooOptions.Extension``), so its themes freeze with the ones of Klyroo.UI and the Tailwind class extractor and the docs bundle find them. Configure it with `services.AddKlyrooTable(t => ...)`, in code shared by the server and the WebAssembly client.

| Constructor | Description |
| :--- | :--- |
| `KlyrooTableOptions()` | Creates the options with the default themes and message tables, a search delay of 300 ms and the default prerender persistence limits. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `DataList` | `ComponentTheme` | The data list and its parts: toolbar, result count, refresh button, table, pagination and error state. One theme for all of them, so a look is changed in one place. |
| `DataListEmpty` | `ComponentTheme` | The empty state of a data list. |
| `PersistMaxBytes` (get, set) | `int` | The most bytes, as sent, one data list hands from its prerender to the interactive render (24 KB by default): its state, loaded state and result with its facets and labels, counted with the escaping and the two base64 encodings Blazor adds, so the JSON itself is about a third of it. A bigger result is not persisted: the interactive render loads it again once, and a warning names the list and the size. Every list, and the other Klyroo libraries of the page, also share `KlyrooOptions.PersistedStateMaxBytes` (28 KB as sent by default), because the client sends everything persisted to the server in the message that starts a Server circuit, which SignalR refuses past 32 KB. Set it in `AddKlyrooTable`. |
| `SearchDebounce` (get, set) | `int` | Milliseconds of quiet after the last keystroke before the search field of a data list searches (300 by default), unless the field sets its own `Debounce`. Enter searches at once. Set it in `AddKlyrooTable`. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Messages(string localeCode, Func<KlyToolsMessages, KlyToolsMessages> change)` | `KlyrooTableOptions` | Changes the texts of a locale, like `KlyLocales.Extend` does for Klyroo.UI: `change` receives the table the locale resolves to and returns the one to use, usually `m => m with { ... }`. `localeCode` is a code of `KlyLocales.All` ("fr"), or the code of a locale you added ("en-AU", "tlh"): a locale without a table of its own starts from the table `KlyLocales.ResolveCode` picks for it, then its changes apply. Like the themes, the messages become read-only the first time a component resolves a text. |

#### `KlyRowEventArgs`

`Klyroo.Table` · class · inherits `EventArgs`

Payload of the `klyrowactivate` browser event of a clickable row. The browser side works out whether a click or a key is an activation of the row at all, because .NET cannot see which element a click landed on, and only sends the ones that are: a click on a control inside the row, or a key typed in a field, never reaches .NET.

| Property | Type | Description |
| :--- | :--- | :--- |
| `AltKey` (get, set) | `bool` | Alt was held. |
| `CtrlKey` (get, set) | `bool` | Ctrl was held. |
| `Key` (get, set) | `string?` | `Enter` or a space for an activation with the keyboard; `null` for a click. |
| `MetaKey` (get, set) | `bool` | Meta was held. |
| `ShiftKey` (get, set) | `bool` | Shift was held. |

#### `SynchronousDataListQueryExecutor`

`Klyroo.Table` · class

The default executor: `Count()` and `ToList()`, on the thread that asks. It blocks that thread while the database answers: in Blazor Server that is the thread of the circuit, in WebAssembly the only thread of the page. Use it for tests, small local databases and providers with no asynchronous API; register your own asynchronous `IDataListQueryExecutor` for a real database (for EF Core, one that calls `CountAsync` and `ToListAsync` with the token). The cancellation token is checked before each query, not during it.

| Constructor | Description |
| :--- | :--- |
| `SynchronousDataListQueryExecutor()` | Creates an executor. Use `SynchronousDataListQueryExecutor.Instance` rather than your own. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Instance` | `SynchronousDataListQueryExecutor` | The shared instance. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<int>` | Counts the rows of a query with `Count()`, on the calling thread. The token is checked once, before the query runs. |
| `ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<List<T>>` | Loads the rows of a query with `ToList()`, on the calling thread. The token is checked once, before the query runs. |

### Records

#### `DataListActionFailure<TItem>`

`Klyroo.Table` · record

An action that threw.

| Constructor | Description |
| :--- | :--- |
| `DataListActionFailure<TItem>(DataListAction<TItem> Action, Exception Exception)` | An action that threw. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Action` (init) | `DataListAction<TItem>` | The action. |
| `Exception` (init) | `Exception` | What it threw. |

#### `DataListColumnState`

`Klyroo.Table` · record

The effective state of one column of a data list: visibility, pinning, width and limits.

| Constructor | Description |
| :--- | :--- |
| `DataListColumnState(string Key, string Title, bool Visible, bool Required, DataListPin Pinned, int? Width, int MinWidth, int MaxWidth, bool Resizable)` | The effective state of one column of a data list: visibility, pinning, width and limits. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Key` (init) | `string` | The column key (`KlyDataListColumnBase.EffectiveKey`). |
| `MaxWidth` (init) | `int` | Maximum width in pixels when resized. |
| `MinWidth` (init) | `int` | Minimum width in pixels when resized. |
| `Pinned` (init) | `DataListPin` | The active horizontal pin edge. |
| `Required` (init) | `bool` | Whether the column cannot be hidden. |
| `Resizable` (init) | `bool` | Whether the user can resize the column. |
| `Title` (init) | `string` | The display title of the column. |
| `Visible` (init) | `bool` | Whether the column is currently shown. |
| `Width` (init) | `int?` | The active pixel width, or `null` when automatic. |

#### `DataListEmptyUi`

`Klyroo.Table` · record

Per-instance class overrides of `KlyDataListEmpty` slots.

| Constructor | Description |
| :--- | :--- |
| `DataListEmptyUi()` | Creates an override set that changes no slot. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Actions` (init) | `string?` | The container of the actions of the empty state. Classes given here are merged with the theme classes of the slot. |
| `Description` (init) | `string?` | The text under the title of the empty state. Classes given here are merged with the theme classes of the slot. |
| `Icon` (init) | `string?` | The icon of the empty state. Classes given here are merged with the theme classes of the slot. |
| `Root` (init) | `string?` | The outer element of the empty state. Classes given here are merged with the theme classes of the slot. |
| `Title` (init) | `string?` | The title of the empty state. Classes given here are merged with the theme classes of the slot. |

#### `DataListRowActionFailure<TItem>`

`Klyroo.Table` · record

A row action that failed: it threw, or called `DataListRowActionContext.Fail`.

| Constructor | Description |
| :--- | :--- |
| `DataListRowActionFailure<TItem>(DataListRowAction<TItem> Action, TItem Item, Exception? Exception, string? Message)` | A row action that failed: it threw, or called `DataListRowActionContext.Fail`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Action` (init) | `DataListRowAction<TItem>` | The action. |
| `Exception` (init) | `Exception?` | What it threw; `null` when it reported the failure itself. |
| `Item` (init) | `TItem` | The row. |
| `Message` (init) | `string?` | What `DataListRowActionContext.Fail` was given. |

#### `DataListRowClickArgs<TItem>`

`Klyroo.Table` · record

What a row of `KlyDataListTable` reports when the user activates it (`KlyDataListTable.OnRowClick`): a click anywhere on the row that did not land on a control inside it, or Enter or Space while the row itself has the focus.

| Constructor | Description |
| :--- | :--- |
| `DataListRowClickArgs<TItem>(TItem Item, string Key, int Position, bool ShiftKey = false, bool CtrlKey = false, bool AltKey = false, bool MetaKey = false, bool FromKeyboard = false)` | What a row of `KlyDataListTable` reports when the user activates it (`KlyDataListTable.OnRowClick`): a click anywhere on the row that did not land on a control inside it, or Enter or Space while the row itself has the focus. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `AltKey` (init) | `bool` | Alt was held. |
| `CtrlKey` (init) | `bool` | Ctrl was held. |
| `FromKeyboard` (init) | `bool` | The row was activated with Enter or Space, not with the pointer. |
| `Item` (init) | `TItem` | The row. |
| `Key` (init) | `string` | The row key (`RowKey` of the schema) as text; `#3` (the position) for a list whose schema has none. |
| `MetaKey` (init) | `bool` | Meta (Command on a Mac, the Windows key) was held. |
| `Position` (init) | `int` | The place of the row on the page, from 1. |
| `ShiftKey` (init) | `bool` | Shift was held. |

#### `DataListRowSummary<TItem>`

`Klyroo.Table` · record

Compact summary-row presentation for `KlyDataListTable{TItem}`: one scannable row per record with an optional leading visual, a primary title (optionally a link or button), a status badge/pill, secondary details joined with ` · `, selection checkbox and row actions. Shown automatically below the `md` breakpoint when `AutoPhone` is `true`, when `DefaultActive` is `true`, or when toggled with `KlyDataListViewToggle{TItem}`.

| Constructor | Description |
| :--- | :--- |
| `DataListRowSummary<TItem>()` | Creates a row summary with every option at its default: the title and details come from the schema and the columns, and the summary rows show below the `md` breakpoint. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `AutoPhone` (init) | `bool` | Automatically switches to summary-row view below the `md` breakpoint (768 px) when the user has not explicitly chosen a view (`true` by default). |
| `DefaultActive` (init) | `bool` | Starts in summary-row view on every screen size until the user toggles `KlyDataListViewToggle{TItem}` (`false` by default). |
| `Details` (init) | `Func<TItem, IEnumerable<string?>>?` | Secondary detail texts joined with ` · ` on the second line (blank entries are skipped). When `null`, uses the remaining visible columns. |
| `Href` (init) | `Func<TItem, string?>?` | Optional link destination (`href`) for the title of the row. |
| `Leading` (init) | `RenderFragment<TItem>?` | Optional leading content (a 32 px tile, icon or avatar) shown before the title. |
| `OnSelect` (init) | `EventCallback<TItem>` | Optional click callback when the summary row's title button is activated (interactive only, when `Href` is `null`). |
| `Status` (init) | `RenderFragment<TItem>?` | Optional status content (a badge or pill) shown at the start of the second line. |
| `Template` (init) | `RenderFragment<TItem>?` | Optional custom template for the body of the summary row, replacing `Title`, `Leading`, `Status` and `Details`. |
| `Title` (init) | `Func<TItem, string?>?` | Primary one-line title of the row. When `null`, uses `Schema.GetRowLabel(item)` or the first visible column's formatted value. |

#### `DataListSortOption`

`Klyroo.Table` · record

An option offered by `KlyDataListSortMenu`.

| Constructor | Description |
| :--- | :--- |
| `DataListSortOption(string Key, string? Label = null)` | An option offered by `KlyDataListSortMenu`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Key` (init) | `string` | The key of the sortable field in the schema. |
| `Label` (init) | `string?` | The display label; when `null`, uses the title of the matching column or the humanized key. |

#### `DataListSummaryCellContext<TItem>`

`Klyroo.Table` · record

Context passed to `KlyDataListColumnBase.SummaryTemplate` when rendering a summary footer cell.

| Constructor | Description |
| :--- | :--- |
| `DataListSummaryCellContext<TItem>(DataListContext<TItem> List, string? ColumnKey, DataListSummaryScope Scope, IReadOnlyList<TItem> Rows, DataListSummaryValue Value, string? FormattedValue, bool IsLoading)` | Context passed to `KlyDataListColumnBase.SummaryTemplate` when rendering a summary footer cell. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `ColumnKey` (init) | `string?` | The key of the column of the cell. |
| `FormattedValue` (init) | `string?` | The value as text, with the format of the column and the culture of the page. |
| `IsLoading` (init) | `bool` | Whether an asynchronous `DataListSummaries.ResolveAsync` is still running for this scope. |
| `List` (init) | `DataListContext<TItem>` | The list the cell belongs to. |
| `Rows` (init) | `IReadOnlyList<TItem>` | The rows of that scope. |
| `Scope` (init) | `DataListSummaryScope` | The set of rows the value aggregates. |
| `Value` (init) | `DataListSummaryValue` | The aggregated value: a number (formatted with the format of the column) or a text that is already formatted. |

#### `DataListSummaryResolveContext<TItem>`

`Klyroo.Table` · record

Context passed to `DataListSummaries.ResolveAsync` when resolving summary values asynchronously.

| Constructor | Description |
| :--- | :--- |
| `DataListSummaryResolveContext<TItem>(DataListContext<TItem> List, DataListSummaryScope Scope, IReadOnlyList<TItem> Rows, DataListRequest? Request, CancellationToken CancellationToken)` | Context passed to `DataListSummaries.ResolveAsync` when resolving summary values asynchronously. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `CancellationToken` (init) | `CancellationToken` | Cancelled when the list goes away or a newer resolution replaces this one. |
| `List` (init) | `DataListContext<TItem>` | The list the summary belongs to. |
| `Request` (init) | `DataListRequest?` | The request for every row the list matches, to send to a server that aggregates; `null` before the first load. |
| `Rows` (init) | `IReadOnlyList<TItem>` | The rows of that scope that the list holds. |
| `Scope` (init) | `DataListSummaryScope` | The set of rows to aggregate. |

#### `DataListUi`

`Klyroo.Table` · record

Per-instance class overrides of the slots of the data list parts (`KlyDataList`, `KlyDataListTable`, `KlyDataListPagination`...).

| Constructor | Description |
| :--- | :--- |
| `DataListUi()` | Creates an override set that changes no slot. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `ActionsDropdown` (init) | `string?` | The trigger button of `KlyDataListActionsDropdown`. Classes given here are merged with the theme classes of the slot. |
| `AddFilter` (init) | `string?` | The add filter button of `KlyDataListAddFilter`. Classes given here are merged with the theme classes of the slot. |
| `Caption` (init) | `string?` | The visually hidden caption of the table. Classes given here are merged with the theme classes of the slot. |
| `ClearFilters` (init) | `string?` | The clear filters button of `KlyDataListClearFilters`. Classes given here are merged with the theme classes of the slot. |
| `ColResize` (init) | `string?` | The resize handle at the end edge of a header cell. Classes given here are merged with the theme classes of the slot. |
| `ColSized` (init) | `string?` | Added to the cells of a column that has a fixed width. Classes given here are merged with the theme classes of the slot. |
| `ColumnsActions` (init) | `string?` | The pin and move controls of a column. Classes given here are merged with the theme classes of the slot. |
| `ColumnsAll` (init) | `string?` | The row with the show all and reset buttons. Classes given here are merged with the theme classes of the slot. |
| `ColumnsContent` (init) | `string?` | The content of the columns popover. Classes given here are merged with the theme classes of the slot. |
| `ColumnsCount` (init) | `string?` | The count of visible columns in the header of the popover. Classes given here are merged with the theme classes of the slot. |
| `ColumnsGrip` (init) | `string?` | The drag grip of a column. Classes given here are merged with the theme classes of the slot. |
| `ColumnsHeader` (init) | `string?` | The header of the columns popover. Classes given here are merged with the theme classes of the slot. |
| `ColumnsHint` (init) | `string?` | The hint that says one column must stay visible. Classes given here are merged with the theme classes of the slot. |
| `ColumnsIconBtn` (init) | `string?` | An icon button of the panel. Classes given here are merged with the theme classes of the slot. |
| `ColumnsIconBtnActive` (init) | `string?` | Added to an icon button that is on, such as a pinned column. Classes given here are merged with the theme classes of the slot. |
| `ColumnsItem` (init) | `string?` | One column of the list. Classes given here are merged with the theme classes of the slot. |
| `ColumnsLabel` (init) | `string?` | The label of a column, with its checkbox. Classes given here are merged with the theme classes of the slot. |
| `ColumnsList` (init) | `string?` | The list of columns. Classes given here are merged with the theme classes of the slot. |
| `ColumnsLive` (init) | `string?` | The live region that announces column changes. Classes given here are merged with the theme classes of the slot. |
| `ColumnsPanel` (init) | `string?` | The root of `KlyDataListColumnPanel`. Classes given here are merged with the theme classes of the slot. |
| `ColumnsPinGroup` (init) | `string?` | The group of pin buttons of a column. Classes given here are merged with the theme classes of the slot. |
| `ColumnsPopover` (init) | `string?` | The popover of the columns panel. Classes given here are merged with the theme classes of the slot. |
| `ColumnsSearch` (init) | `string?` | The search field of the panel. Classes given here are merged with the theme classes of the slot. |
| `ColumnsTitle` (init) | `string?` | The title of the columns popover. Classes given here are merged with the theme classes of the slot. |
| `CursorReset` (init) | `string?` | The button that goes back to the first rows when a cursor in the address matches nothing. Classes given here are merged with the theme classes of the slot. |
| `Empty` (init) | `string?` | The cell that holds the empty state of the table. Classes given here are merged with the theme classes of the slot. |
| `Error` (init) | `string?` | The error state shown when a load failed. Classes given here are merged with the theme classes of the slot. |
| `FilterActions` (init) | `string?` | The row of Apply, Cancel and Reset buttons of an editor. Classes given here are merged with the theme classes of the slot. |
| `FilterBack` (init) | `string?` | The button that goes back from an editor to the add filter menu. Classes given here are merged with the theme classes of the slot. |
| `FilterChoices` (init) | `string?` | The group of operator choices of an editor. Classes given here are merged with the theme classes of the slot. |
| `FilterEditor` (init) | `string?` | The root of `KlyDataListFilterEditor`. Classes given here are merged with the theme classes of the slot. |
| `FilterEditorTitle` (init) | `string?` | The name of the filter above its editor. Classes given here are merged with the theme classes of the slot. |
| `FilterEmpty` (init) | `string?` | The text shown when an option filter has no options. Classes given here are merged with the theme classes of the slot. |
| `FilterField` (init) | `string?` | A labelled field inside a filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterFieldLabel` (init) | `string?` | The label of a field inside a filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterHint` (init) | `string?` | The line that reads back the range of a filter. Classes given here are merged with the theme classes of the slot. |
| `FilterInput` (init) | `string?` | The value input of a filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterMenu` (init) | `string?` | The menu that lists the filters in `KlyDataListAddFilter`. Classes given here are merged with the theme classes of the slot. |
| `FilterMenuHeader` (init) | `string?` | The header of the add filter menu. Classes given here are merged with the theme classes of the slot. |
| `FilterMenuItem` (init) | `string?` | One entry of the add filter menu. Classes given here are merged with the theme classes of the slot. |
| `FilterNativeInput` (init) | `string?` | A native input of the statically rendered filter forms. Classes given here are merged with the theme classes of the slot. |
| `FilterNativeLabel` (init) | `string?` | The label of a native input of the statically rendered filter forms. Classes given here are merged with the theme classes of the slot. |
| `FilterOperator` (init) | `string?` | The operator select of a filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterOption` (init) | `string?` | One option of an option filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterOptionCheck` (init) | `string?` | The checkbox of an option. Classes given here are merged with the theme classes of the slot. |
| `FilterOptionCount` (init) | `string?` | The count shown next to an option when its filter has facets. Classes given here are merged with the theme classes of the slot. |
| `FilterOptionLink` (init) | `string?` | The link of an option in a statically rendered editor. Classes given here are merged with the theme classes of the slot. |
| `FilterOptions` (init) | `string?` | The list of options of an option filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterPanel` (init) | `string?` | The root of `KlyDataListFilterPanel`. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelContent` (init) | `string?` | The content of a section of the panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelCount` (init) | `string?` | The count of active filters in the header of the panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelDescription` (init) | `string?` | The line under the title of the filter panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelEditor` (init) | `string?` | The editor inside a section of the panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelEmpty` (init) | `string?` | The text shown when the schema has no filters. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelFooter` (init) | `string?` | The footer of the panel that holds the Apply and Reset buttons. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelHeader` (init) | `string?` | The header of the filter panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelResults` (init) | `string?` | The live region that announces the number of results. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelSection` (init) | `string?` | One collapsible section of the filter panel, one per filter. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelTitle` (init) | `string?` | The title of the filter panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPanelTrigger` (init) | `string?` | The button that opens or closes a section of the panel. Classes given here are merged with the theme classes of the slot. |
| `FilterPopover` (init) | `string?` | The popover that holds a filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterPopoverWide` (init) | `string?` | Added to the popover of an editor that needs more room. Classes given here are merged with the theme classes of the slot. |
| `FilterPresets` (init) | `string?` | The group of preset buttons of a date filter editor. Classes given here are merged with the theme classes of the slot. |
| `FilterRange` (init) | `string?` | The container of the inputs of a range filter. Classes given here are merged with the theme classes of the slot. |
| `FilterRangeSlider` (init) | `string?` | The slider of a number range filter. Classes given here are merged with the theme classes of the slot. |
| `Filters` (init) | `string?` | The container of the filter tags and of the add and clear buttons. Classes given here are merged with the theme classes of the slot. |
| `FilterSection` (init) | `string?` | A section of an editor. Classes given here are merged with the theme classes of the slot. |
| `FilterTag` (init) | `string?` | One filter tag. Classes given here are merged with the theme classes of the slot. |
| `FilterTagButton` (init) | `string?` | The button of a filter tag that opens its editor. Classes given here are merged with the theme classes of the slot. |
| `FilterTagLabel` (init) | `string?` | The name of the filter inside a tag. Classes given here are merged with the theme classes of the slot. |
| `FilterTagOperator` (init) | `string?` | The operator text inside a tag. Classes given here are merged with the theme classes of the slot. |
| `FilterTagRemove` (init) | `string?` | The button that removes a filter from its tag. Classes given here are merged with the theme classes of the slot. |
| `FilterTagRemoveIcon` (init) | `string?` | The icon of the remove button of a tag. Classes given here are merged with the theme classes of the slot. |
| `FilterTags` (init) | `string?` | The list of filter tags. Classes given here are merged with the theme classes of the slot. |
| `FilterTagValue` (init) | `string?` | The value text inside a tag. Classes given here are merged with the theme classes of the slot. |
| `Grid` (init) | `string?` | The container of the cards. Classes given here are merged with the theme classes of the slot. |
| `GridCard` (init) | `string?` | One card. Classes given here are merged with the theme classes of the slot. |
| `GridCardBody` (init) | `string?` | The body of a card. Classes given here are merged with the theme classes of the slot. |
| `GridCardField` (init) | `string?` | One field of a card. Classes given here are merged with the theme classes of the slot. |
| `GridCardFieldLabel` (init) | `string?` | The label of a field of a card. Classes given here are merged with the theme classes of the slot. |
| `GridCardFields` (init) | `string?` | The list of fields of a card. Classes given here are merged with the theme classes of the slot. |
| `GridCardFieldValue` (init) | `string?` | The value of a field of a card. Classes given here are merged with the theme classes of the slot. |
| `GridCardTop` (init) | `string?` | The top of a card: selection box and row actions. Classes given here are merged with the theme classes of the slot. |
| `GridEmpty` (init) | `string?` | The container of the empty state of the grid. Classes given here are merged with the theme classes of the slot. |
| `GridGroup` (init) | `string?` | The container of one group of cards. Classes given here are merged with the theme classes of the slot. |
| `GridGroupHeader` (init) | `string?` | The header of one group of cards. Classes given here are merged with the theme classes of the slot. |
| `GridHeader` (init) | `string?` | The header row of the grid that holds the select all box. Classes given here are merged with the theme classes of the slot. |
| `GridRoot` (init) | `string?` | The root of `KlyDataListGrid`. Classes given here are merged with the theme classes of the slot. |
| `GroupCell` (init) | `string?` | The cell of a group header row. Classes given here are merged with the theme classes of the slot. |
| `GroupChevron` (init) | `string?` | The chevron of the fold button. Classes given here are merged with the theme classes of the slot. |
| `GroupChevronExpanded` (init) | `string?` | Added to the chevron while the group is open. Classes given here are merged with the theme classes of the slot. |
| `GroupHeader` (init) | `string?` | The root of `KlyDataListGroupHeader`. Classes given here are merged with the theme classes of the slot. |
| `GroupLabel` (init) | `string?` | The label of a group. Classes given here are merged with the theme classes of the slot. |
| `GroupRow` (init) | `string?` | The group header row of the table. Classes given here are merged with the theme classes of the slot. |
| `GroupSummary` (init) | `string?` | The summary badge beside the label of a group. Classes given here are merged with the theme classes of the slot. |
| `GroupToggle` (init) | `string?` | The fold button of a group header. Classes given here are merged with the theme classes of the slot. |
| `HeaderCell` (init) | `string?` | The content wrapper of a header cell. Classes given here are merged with the theme classes of the slot. |
| `HeaderMenu` (init) | `string?` | The wrapper of the header menu of a column. Classes given here are merged with the theme classes of the slot. |
| `HeaderMenuTrigger` (init) | `string?` | The button that opens the header menu of a column. Classes given here are merged with the theme classes of the slot. |
| `InfiniteEnd` (init) | `string?` | The text shown after the last page has loaded. Classes given here are merged with the theme classes of the slot. |
| `InfiniteLoader` (init) | `string?` | The root of `KlyDataListInfiniteLoader`. Classes given here are merged with the theme classes of the slot. |
| `InfiniteStatus` (init) | `string?` | The live region of the Load more button. Classes given here are merged with the theme classes of the slot. |
| `LayoutOption` (init) | `string?` | One button of the layout switch. Classes given here are merged with the theme classes of the slot. |
| `LayoutSwitch` (init) | `string?` | The root of `KlyDataListLayoutSwitch`. Classes given here are merged with the theme classes of the slot. |
| `LoadMoreError` (init) | `string?` | The block shown when loading the next page failed. Classes given here are merged with the theme classes of the slot. |
| `LoadMoreErrorText` (init) | `string?` | The message inside that block. Classes given here are merged with the theme classes of the slot. |
| `PageSize` (init) | `string?` | The page size picker with its label. Classes given here are merged with the theme classes of the slot. |
| `PageSizeForm` (init) | `string?` | The form around the page size picker in a statically rendered list. Classes given here are merged with the theme classes of the slot. |
| `PageSizeLabel` (init) | `string?` | The label of the page size picker. Classes given here are merged with the theme classes of the slot. |
| `PageSizeSelect` (init) | `string?` | The select of the page size picker. Classes given here are merged with the theme classes of the slot. |
| `Pagination` (init) | `string?` | The container of `KlyDataListPagination`. Classes given here are merged with the theme classes of the slot. |
| `ProgressLine` (init) | `string?` | The progress indicator drawn at the top of the table while rows reload. Classes given here are merged with the theme classes of the slot. |
| `ProgressLineBar` (init) | `string?` | The moving bar inside the progress indicator. Classes given here are merged with the theme classes of the slot. |
| `Refresh` (init) | `string?` | The refresh button of `KlyDataListRefresh`. Classes given here are merged with the theme classes of the slot. |
| `RefreshIcon` (init) | `string?` | The icon of the refresh button. Classes given here are merged with the theme classes of the slot. |
| `ResultCount` (init) | `string?` | The text that gives the number of results. Classes given here are merged with the theme classes of the slot. |
| `Root` (init) | `string?` | The outer element of the list. Classes given here are merged with the theme classes of the slot. |
| `RowActions` (init) | `string?` | The root of `KlyDataListRowActions`. Classes given here are merged with the theme classes of the slot. |
| `RowActionsCell` (init) | `string?` | The cell of the row actions column, header and body. Classes given here are merged with the theme classes of the slot. |
| `RowActionsInline` (init) | `string?` | The inline buttons of the row actions. Classes given here are merged with the theme classes of the slot. |
| `RowActionsMenu` (init) | `string?` | The menu of the row actions that do not fit inline. Classes given here are merged with the theme classes of the slot. |
| `RowActionsPhone` (init) | `string?` | The single button that opens the row actions sheet on a phone. Classes given here are merged with the theme classes of the slot. |
| `RowSelected` (init) | `string?` | Added to a row while it is ticked. Classes given here are merged with the theme classes of the slot. |
| `Search` (init) | `string?` | The search form of `KlyDataListSearch`. Classes given here are merged with the theme classes of the slot. |
| `SearchClear` (init) | `string?` | The button that clears the search text. Classes given here are merged with the theme classes of the slot. |
| `SearchClearIcon` (init) | `string?` | The icon of the clear button of the search field. Classes given here are merged with the theme classes of the slot. |
| `SearchField` (init) | `string?` | The wrapper around the search input and its clear button. Classes given here are merged with the theme classes of the slot. |
| `SearchInput` (init) | `string?` | The search input. Classes given here are merged with the theme classes of the slot. |
| `SearchSubmit` (init) | `string?` | The submit button of the search form, shown in a statically rendered list. Classes given here are merged with the theme classes of the slot. |
| `SelectCell` (init) | `string?` | The cell of the selection checkbox column, header and body. Classes given here are merged with the theme classes of the slot. |
| `SelectionActions` (init) | `string?` | The root of `KlyDataListSelectionActions`. Classes given here are merged with the theme classes of the slot. |
| `SelectionBar` (init) | `string?` | The bar that shows the selection count and the bulk actions. Classes given here are merged with the theme classes of the slot. |
| `SelectionCount` (init) | `string?` | The text that gives the number of selected rows. Classes given here are merged with the theme classes of the slot. |
| `SelectionGroup` (init) | `string?` | The group of action buttons in the selection bar. Classes given here are merged with the theme classes of the slot. |
| `SelectionLive` (init) | `string?` | The live region that announces the selection to assistive technologies. Classes given here are merged with the theme classes of the slot. |
| `SelectionScope` (init) | `string?` | The buttons that switch between the ticked rows and every result. Classes given here are merged with the theme classes of the slot. |
| `SheetDivider` (init) | `string?` | The divider between groups of actions in the sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetEntry` (init) | `string?` | One entry of the sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetHeading` (init) | `string?` | The heading of a submenu in the sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetIcon` (init) | `string?` | The icon of an action in the sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetItem` (init) | `string?` | The button or link of an action in the sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetList` (init) | `string?` | The list of actions inside the phone sheet. Classes given here are merged with the theme classes of the slot. |
| `SheetReason` (init) | `string?` | The text that says why an action is disabled, in the sheet. Classes given here are merged with the theme classes of the slot. |
| `Skeleton` (init) | `string?` | A placeholder bar shown in a cell while the first rows load. Classes given here are merged with the theme classes of the slot. |
| `SortButton` (init) | `string?` | The button of a header cell that sorts by its column. Classes given here are merged with the theme classes of the slot. |
| `SortIcon` (init) | `string?` | The sort direction icon of a header cell. Classes given here are merged with the theme classes of the slot. |
| `SortMenu` (init) | `string?` | The trigger button of `KlyDataListSortMenu`. Classes given here are merged with the theme classes of the slot. |
| `SortMenuContent` (init) | `string?` | The content of the sort popover. Classes given here are merged with the theme classes of the slot. |
| `SortMenuDirection` (init) | `string?` | One direction button. Classes given here are merged with the theme classes of the slot. |
| `SortMenuDirections` (init) | `string?` | The group of the ascending and descending buttons. Classes given here are merged with the theme classes of the slot. |
| `SortMenuHeader` (init) | `string?` | The header of the sort popover. Classes given here are merged with the theme classes of the slot. |
| `SortMenuItem` (init) | `string?` | One sortable field of the list. Classes given here are merged with the theme classes of the slot. |
| `SortMenuList` (init) | `string?` | The list of sortable fields. Classes given here are merged with the theme classes of the slot. |
| `SortMenuPopover` (init) | `string?` | The popover of the sort menu. Classes given here are merged with the theme classes of the slot. |
| `SortMenuTitle` (init) | `string?` | The title of the sort popover. Classes given here are merged with the theme classes of the slot. |
| `SummaryActions` (init) | `string?` | The row actions of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryBody` (init) | `string?` | The text block of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryDetails` (init) | `string?` | The details text of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryGroupRow` (init) | `string?` | The group header in the summary row list. Classes given here are merged with the theme classes of the slot. |
| `SummaryHeader` (init) | `string?` | The header of the summary row list that holds the select all box. Classes given here are merged with the theme classes of the slot. |
| `SummaryLeading` (init) | `string?` | The leading content of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryList` (init) | `string?` | The list of summary rows. Classes given here are merged with the theme classes of the slot. |
| `SummaryMeta` (init) | `string?` | The second line of a summary row: status and details. Classes given here are merged with the theme classes of the slot. |
| `SummaryRow` (init) | `string?` | One summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryStatus` (init) | `string?` | The status content of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryTitle` (init) | `string?` | The title of a summary row. Classes given here are merged with the theme classes of the slot. |
| `SummaryTitleButton` (init) | `string?` | The title of a summary row when it is a button. Classes given here are merged with the theme classes of the slot. |
| `SummaryTitleLink` (init) | `string?` | The title of a summary row when it is a link. Classes given here are merged with the theme classes of the slot. |
| `Table` (init) | `string?` | The `table` element. Classes given here are merged with the theme classes of the slot. |
| `TableRoot` (init) | `string?` | The scrolling region around the table. Classes given here are merged with the theme classes of the slot. |
| `Tbody` (init) | `string?` | The table body. Classes given here are merged with the theme classes of the slot. |
| `Td` (init) | `string?` | A body cell. Classes given here are merged with the theme classes of the slot. |
| `TdPinnedEnd` (init) | `string?` | Added to a body cell pinned to the end edge. Classes given here are merged with the theme classes of the slot. |
| `TdPinnedStart` (init) | `string?` | Added to a body cell pinned to the start edge. Classes given here are merged with the theme classes of the slot. |
| `Tfoot` (init) | `string?` | The table footer section that holds the summary rows. Classes given here are merged with the theme classes of the slot. |
| `TfootCell` (init) | `string?` | A cell of a summary row. Classes given here are merged with the theme classes of the slot. |
| `TfootCellPinnedEnd` (init) | `string?` | Added to a footer cell pinned to the end edge. Classes given here are merged with the theme classes of the slot. |
| `TfootCellPinnedStart` (init) | `string?` | Added to a footer cell pinned to the start edge. Classes given here are merged with the theme classes of the slot. |
| `TfootLabel` (init) | `string?` | The label of a summary row. Classes given here are merged with the theme classes of the slot. |
| `TfootRow` (init) | `string?` | A summary row of the footer. Classes given here are merged with the theme classes of the slot. |
| `Th` (init) | `string?` | A header cell. Classes given here are merged with the theme classes of the slot. |
| `Thead` (init) | `string?` | The table header section. Classes given here are merged with the theme classes of the slot. |
| `ThPinnedEnd` (init) | `string?` | Added to a header cell pinned to the end edge. Classes given here are merged with the theme classes of the slot. |
| `ThPinnedStart` (init) | `string?` | Added to a header cell pinned to the start edge. Classes given here are merged with the theme classes of the slot. |
| `Toolbar` (init) | `string?` | The row that holds the search field, the result count and the refresh button in the default layout. Classes given here are merged with the theme classes of the slot. |
| `Tr` (init) | `string?` | A table row. Classes given here are merged with the theme classes of the slot. |
| `TreeCell` (init) | `string?` | The cell that holds the tree rails, the toggle and the content. Classes given here are merged with the theme classes of the slot. |
| `TreeChevron` (init) | `string?` | The chevron of the toggle. Classes given here are merged with the theme classes of the slot. |
| `TreeChevronExpanded` (init) | `string?` | Added to the chevron while the node is open. Classes given here are merged with the theme classes of the slot. |
| `TreeContent` (init) | `string?` | The cell content next to the toggle. Classes given here are merged with the theme classes of the slot. |
| `TreeRailBlank` (init) | `string?` | A rail segment with no line. Classes given here are merged with the theme classes of the slot. |
| `TreeRailBranch` (init) | `string?` | A rail segment with a branch to a node that has later siblings. Classes given here are merged with the theme classes of the slot. |
| `TreeRailLast` (init) | `string?` | A rail segment with a branch to the last node of its level. Classes given here are merged with the theme classes of the slot. |
| `TreeRailPass` (init) | `string?` | A rail segment with a vertical line that continues to the next sibling of an ancestor. Classes given here are merged with the theme classes of the slot. |
| `TreeSpacer` (init) | `string?` | The spacer that takes the place of the toggle on a leaf node. Classes given here are merged with the theme classes of the slot. |
| `TreeToggle` (init) | `string?` | The expand and collapse button of a branch node. Classes given here are merged with the theme classes of the slot. |
| `ViewToggle` (init) | `string?` | The button of `KlyDataListViewToggle`. Classes given here are merged with the theme classes of the slot. |

#### `KlyToolsMessages`

`Klyroo.Table` · record

Every text the components of Klyroo.Table render: accessible names, labels, hints. One set per locale of Klyroo.UI (`KlyToolsLocales`); build a variant with a `with` expression and register it with `KlyrooTableOptions.Messages`.

| Constructor | Description |
| :--- | :--- |
| `KlyToolsMessages()` | Creates the table with the English texts. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `ActionCancel` (init) | `string` | Cancel button of the dialog an action asks before it runs. |
| `ActionConfirm` (init) | `string` | Confirm button of the dialog an action asks before it runs, when the action gives no label. |
| `ActionsMenu` (init) | `string` | Label of the button that opens the menu of the toolbar actions of a data list. |
| `ActionsMore` (init) | `string` | Label of the button of the selection bar that opens the actions that did not fit. |
| `AddFilter` (init) | `string` | Label of the button that opens the menu of filters to add. |
| `AddFilterBack` (init) | `string` | Label of the button of an editor opened from the add filter menu that goes back to the list of filters. |
| `AddFilterNone` (init) | `string` | Said in the menu of filters to add when every filter is already in use. |
| `AddFilterSearch` (init) | `string` | Label and placeholder of the field that narrows the menu of filters to add (shown when the list has many). |
| `ApplyPageSize` (init) | `string` | Submit button of the page size form of a statically rendered data list. |
| `BackToStart` (init) | `string` | Link shown by a statically rendered cursor-paged list to go back to the first page. |
| `ClearFilters` (init) | `string` | Label of the button that takes every filter off. |
| `ClearSearch` (init) | `string` | Accessible name of the button that empties the search field, and label of the action of the empty state that clears the search. |
| `ColumnHidden` (init) | `string` | Announced after a column was hidden. {0} is the column title. |
| `ColumnMenuClearSort` (init) | `string` | Item in the column header menu that clears sorting on the column. |
| `ColumnMenuHide` (init) | `string` | Item in the column header menu that hides the column. |
| `ColumnMenuOpen` (init) | `string` | Accessible name of the column header menu button. {0} is the column title. |
| `ColumnMenuSortAsc` (init) | `string` | Item in the column header menu that sorts ascending. |
| `ColumnMenuSortDesc` (init) | `string` | Item in the column header menu that sorts descending. |
| `ColumnMoved` (init) | `string` | Announced after a column moved. {0} is the column title, {1} its new position and {2} the number of columns. |
| `ColumnMoveDown` (init) | `string` | Accessible name of the button moving a column later in the order. {0} is the column title. |
| `ColumnMoveUp` (init) | `string` | Accessible name of the button moving a column earlier in the order. {0} is the column title. |
| `ColumnPinEnd` (init) | `string` | Accessible name of the button pinning a column to the end edge. {0} is the column title. |
| `ColumnPinnedToEnd` (init) | `string` | Announced after a column was pinned to the end edge. {0} is the column title. |
| `ColumnPinnedToStart` (init) | `string` | Announced after a column was pinned to the start edge. {0} is the column title. |
| `ColumnPinStart` (init) | `string` | Accessible name of the button pinning a column to the start edge. {0} is the column title. |
| `ColumnResize` (init) | `string` | Accessible name of the column resize handle. {0} is the column title. |
| `Columns` (init) | `string` | Label of the column configurator button and heading. |
| `ColumnsAllHidden` (init) | `string` | Announced after Hide all. |
| `ColumnsAllShown` (init) | `string` | Announced after Show all. |
| `ColumnsHideAll` (init) | `string` | Button of the column panel that hides every column that is not required. |
| `ColumnShown` (init) | `string` | Announced after a column was shown. {0} is the column title. |
| `ColumnsKeepOne` (init) | `string` | Hint of the column panel shown when only one column is left, which cannot be hidden. |
| `ColumnsReset` (init) | `string` | Button in the column panel that restores default column visibility, order, pinning and widths. |
| `ColumnsSearch` (init) | `string` | Placeholder and accessible name of the search field inside the column panel. |
| `ColumnsShowAll` (init) | `string` | Button of the column panel that shows every column. |
| `ColumnsVisible` (init) | `string` | Visible column count in the column panel. {0} is visible count, {1} is total count. |
| `ColumnsWereReset` (init) | `string` | Announced after the columns went back to their defaults. |
| `ColumnUnpin` (init) | `string` | Accessible name of the button unpinning a column. {0} is the column title. |
| `ColumnUnpinned` (init) | `string` | Announced after a column was unpinned. {0} is the column title. |
| `ColumnWidthChanged` (init) | `string` | Announced after a column was resized. {0} is the column title and {1} the width in pixels. |
| `ColumnWidthPixels` (init) | `string` | Value text of a column resize handle. {0} is the width in pixels. |
| `ColumnWidthReset` (init) | `string` | Announced after a column went back to its default width. {0} is the column title. |
| `DatePreset7DaysAgo` (init) | `string` | Date preset: the day seven days before today. |
| `DatePresetLast30Days` (init) | `string` | Range preset: the last thirty days, today included. |
| `DatePresetLast7Days` (init) | `string` | Range preset: the last seven days, today included. |
| `DatePresetLastMonth` (init) | `string` | Range preset: the whole of the previous month. |
| `DatePresetStartOfMonth` (init) | `string` | Date preset: the first day of the current month. |
| `DatePresetStartOfYear` (init) | `string` | Date preset: the first day of the current year. |
| `DatePresetThisMonth` (init) | `string` | Range preset: the current month, from its first day to its last. |
| `DatePresetToday` (init) | `string` | Date preset: the current day. |
| `DatePresetYearToDate` (init) | `string` | Range preset: from the first day of the year to today. |
| `DatePresetYesterday` (init) | `string` | Date preset: the day before today. |
| `DetailCollapse` (init) | `string` | Accessible name of the button that closes the detail row of a row. {0} is the label of the row, or its position (`#3`) when it has none. |
| `DetailColumn` (init) | `string` | Header, read by assistive technology only, of the column of the buttons that open the detail rows. |
| `DetailExpand` (init) | `string` | Accessible name of the button that opens the detail row of a row. {0} is the label of the row (`RowLabel` of the schema), or its position (`#3`) when it has none. |
| `EditFilter` (init) | `string` | Accessible name of the editor panel of a filter. {0} is the name of the filter. |
| `ErrorDescription` (init) | `string` | Text of the error state of a data list. |
| `ErrorTitle` (init) | `string` | Title of the error state of a data list. |
| `ExportAll` (init) | `string` | Menu entry that exports every result of the search and the filters. {0} is how many results match. |
| `ExportAllUnknown` (init) | `string` | Menu entry that exports every result when the source does not say how many there are. |
| `ExportLabel` (init) | `string` | Label of the button that exports a data list as a CSV file. |
| `ExportPage` (init) | `string` | Menu entry that exports the rows of the current page. |
| `ExportSelection` (init) | `string` | Menu entry that exports the selected rows. {0} is how many are selected. |
| `ExportTooLarge` (init) | `string` | Shown under the export button when the export holds more rows than `MaxRows` allows. {0} is how many rows it would hold, {1} the limit. |
| `FilterAny` (init) | `string` | The choice of a boolean filter that takes the filter off. |
| `FilterApply` (init) | `string` | Button of a filter editor that applies the changes (the Submit apply mode). |
| `FilterCalendarFor` (init) | `string` | Accessible name of the calendar of a date filter editor. {0} is the name of the filter. |
| `FilterCancel` (init) | `string` | Button of a filter editor that drops the changes (the Submit apply mode). |
| `FilterEmpty` (init) | `string` | The value of a filter tag that matches only empty rows, after the operator: "Status is empty". |
| `FilterFrom` (init) | `string` | Label of the first field of a range editor (the start of a number or date range). |
| `FilterFromFor` (init) | `string` | Accessible name of the start field of a range. {0} is the name of the filter. |
| `FilterFromTimeFor` (init) | `string` | Accessible name of the time field of the start of a range. {0} is the name of the filter. |
| `FilterIncludeEmpty` (init) | `string` | Label of the switch that also matches the rows where the field is empty. |
| `FilterMaxRangeDays` (init) | `string` | Hint under the range of a date filter that limits it. {0} is the number of days. |
| `FilterMore` (init) | `string` | Said after the values a filter tag shows when it has more. {0} is how many more. |
| `FilterNo` (init) | `string` | The "false" choice of a boolean filter. |
| `FilterNoOptions` (init) | `string` | Said in the editor of an option filter that has no option to list. |
| `FilterNoOptionsMatch` (init) | `string` | Said in the editor of an option filter when the search matches no option. |
| `FilterOperatorFor` (init) | `string` | Accessible name of the operator picker of a filter editor. {0} is the name of the filter. |
| `FilterOptionsMore` (init) | `string` | Label of the button that lists the options an option filter kept back. {0} is how many more there are. |
| `FilterOptionsNeedScript` (init) | `string` | Said in a statically rendered editor of an option filter whose options come from a remote source: the options that are listed are the selected ones and the counted ones, searching the rest runs in the browser. |
| `FilterOptionsSearch` (init) | `string` | Label and placeholder of the field that narrows the options of an option filter. |
| `FilterOrEmpty` (init) | `string` | Added to the values of a filter tag that also matches empty rows: "Status is Active or empty". |
| `FilterPanelActive` (init) | `string` | Said to screen readers after the name of a filter panel section (or of the panel) that has filters on. {0} is how many are active. |
| `FilterPanelNone` (init) | `string` | Said in the filter panel when the list has no filter to edit. |
| `FilterPanelResults` (init) | `string` | The number of rows that match the filters, in the filter panel (a live region). {0} is the count. Written so that it reads right for one row too. |
| `FilterPanelTitle` (init) | `string` | Title of the filter panel, and its accessible name. |
| `FilterPresetsFor` (init) | `string` | Accessible name of the group of preset buttons of a date filter (Today, Last 7 days...). {0} is the name of the filter. |
| `FilterReset` (init) | `string` | Button of the filter panel (the Submit apply mode) that drops the changes that were not applied. |
| `FilterSliderFor` (init) | `string` | Accessible name of the slider of a number filter editor, which sits next to the number field. {0} is the name of the filter. |
| `FilterTagsLabel` (init) | `string` | Accessible name of the list of filter tags. {0} is the number of active filters, so a screen reader announces the count. |
| `FilterTextPlaceholder` (init) | `string` | Placeholder of the value field of a text filter. |
| `FilterTimeFor` (init) | `string` | Accessible name of the time field of a date filter that is exact to the hour or the minute. {0} is the name of the filter. |
| `FilterTo` (init) | `string` | Label of the second field of a range editor (the end of a number or date range). |
| `FilterToFor` (init) | `string` | Accessible name of the end field of a range. {0} is the name of the filter. |
| `FilterToTimeFor` (init) | `string` | Accessible name of the time field of the end of a range. {0} is the name of the filter. |
| `FilterValueFor` (init) | `string` | Accessible name of the value field of a filter editor. {0} is the name of the filter. |
| `FilterYes` (init) | `string` | The "true" choice of a boolean filter. |
| `GridView` (init) | `string` | Accessible name and label of the grid layout option. |
| `GroupEmpty` (init) | `string` | Heading of the group that holds rows whose group field is null or empty. |
| `GroupSelect` (init) | `string` | Accessible name of the checkbox in a group header that selects or unselects every row in the group. {0} is the group label. |
| `GroupToggle` (init) | `string` | Accessible name of the button in a group header that folds or unfolds the group. {0} is the group label. |
| `LayoutSwitch` (init) | `string` | Accessible name of the layout switch group. |
| `Loading` (init) | `string` | Said while the first rows of a data list load. |
| `LoadMoreError` (init) | `string` | Error shown inline by the infinite loader when fetching the next cursor page failed. |
| `NoResults` (init) | `string` | Title of the empty state of a data list. |
| `NoResultsDescription` (init) | `string` | Hint under the title of the empty state of a data list. |
| `NoResultsFiltered` (init) | `string` | Title of the empty state of a data list when its filters match nothing. |
| `NoResultsFor` (init) | `string` | Title of the empty state of a data list when a search matches nothing, and the text its result count announces. {0} is the search text. |
| `NoResultsForDescription` (init) | `string` | Hint under the title of the empty state of a data list when a search matches nothing. |
| `OperatorAfter` (init) | `string` | The `after` operator of a date filter. |
| `OperatorBefore` (init) | `string` | The `before` operator of a date filter. |
| `OperatorBetween` (init) | `string` | The `between` operator of a number or date filter. |
| `OperatorContains` (init) | `string` | The `contains` operator of a text filter: "Name contains ann". |
| `OperatorGt` (init) | `string` | The `gt` operator of a number filter. |
| `OperatorGte` (init) | `string` | The `gte` operator of a number filter. |
| `OperatorIs` (init) | `string` | The `is` operator. Also what a filter tag says for `isAnyOf`: "Status is Active, Paused". |
| `OperatorIsAnyOf` (init) | `string` | The `isAnyOf` operator in the operator picker of an option filter. |
| `OperatorIsEmpty` (init) | `string` | The `isEmpty` operator. |
| `OperatorIsNot` (init) | `string` | The `isNot` operator. |
| `OperatorIsNotEmpty` (init) | `string` | The `isNotEmpty` operator. |
| `OperatorLt` (init) | `string` | The `lt` operator of a number filter. |
| `OperatorLte` (init) | `string` | The `lte` operator of a number filter. |
| `PageSize` (init) | `string` | Label of the page size picker. |
| `PaginationFor` (init) | `string` | The accessible name of the pagination of a labelled data list. {0} is the label of the list. |
| `Refresh` (init) | `string` | Button that loads the rows again. |
| `RemoveFilter` (init) | `string` | Accessible name of the button of a filter tag that removes the filter. {0} is the name of the filter. |
| `ResetFilters` (init) | `string` | Action of the empty state of a data list that removes its search and its filters. |
| `ResultCount` (init) | `string` | The rows shown out of the total, announced when they change. {0} is the first row number, {1} the last, {2} the total. |
| `ResultCountLoaded` (init) | `string` | Rows on a cursor page out of the total, when the start offset is not known (a static link). {0} is the row count of the page, {1} the total. |
| `ResultCountShown` (init) | `string` | Rows on a cursor page when neither the start offset nor the total is known. {0} is the row count of the page. |
| `ResultRange` (init) | `string` | The rows shown when the total is not known. {0} is the first row number, {1} the last. |
| `Results` (init) | `string` | The default accessible name of the table of a data list that has no label of its own. |
| `Retry` (init) | `string` | Button of the error state that sends the request again. |
| `RowActionsLabel` (init) | `string` | Accessible name of the button that opens the actions of one row of a data list; {0} is the label of the row. |
| `RowActionsLabelAt` (init) | `string` | Accessible name of the button that opens the actions of a row that has no label; {0} is its position on the page. |
| `RowOpen` (init) | `string` | Accessible name of the link that covers a row of a table with `RowHref`. {0} is the label of the row (`RowLabel` of the schema). |
| `RowOpenAt` (init) | `string` | Accessible name of the link that covers a row that has no label. {0} is the position of the row on the page, from 1. |
| `Search` (init) | `string` | Accessible name of the search field of a data list that has no label. It also labels the button that submits the search of a statically rendered list. |
| `SearchFor` (init) | `string` | Accessible name of the search field of a labelled data list. {0} is the label of the list. |
| `SearchPlaceholder` (init) | `string` | Placeholder of the search field of a data list. |
| `SelectAllRows` (init) | `string` | Accessible name of the checkbox in the header of a data list that selects the rows of the page. |
| `SelectionActionsLabel` (init) | `string` | Accessible name of the selection bar, the group of the actions on the selected rows. |
| `SelectionAllResults` (init) | `string` | Said instead of the count when the actions apply to every result. {0} is how many results match. |
| `SelectionClear` (init) | `string` | Button of the selection bar that unticks every row. |
| `SelectionCount` (init) | `string` | How many rows are selected, announced when it changes. {0} is the count. |
| `SelectionOnlySelected` (init) | `string` | Button of the selection bar that goes back from every result to the rows that were ticked. {0} is how many. |
| `SelectionSelectAllResults` (init) | `string` | Button of the selection bar that makes the actions apply to every result. {0} is how many results match. |
| `SelectRow` (init) | `string` | Accessible name of the checkbox of a row that has a label. {0} is the label of the row (`RowLabel` of the schema). |
| `SelectRowAt` (init) | `string` | Accessible name of the checkbox of a row that has no label. {0} is the position of the row on the page, from 1. |
| `Sort` (init) | `string` | Label of the sort menu trigger button when no sort is active, and heading of the sort popover. |
| `SortAddHeading` (init) | `string` | Heading of the list of the fields that can still be added to the sort in the sort menu. |
| `SortAppendHint` (init) | `string` | Tooltip of a sort button: how to add a column to the sort instead of replacing it. |
| `SortAscending` (init) | `string` | Accessible name of a sort button that sorts a column from the smallest to the largest. {0} is the title of the column. |
| `SortBy` (init) | `string` | Label of the sort menu trigger button when sorted by {0}, and accessible name of a sort field option. |
| `SortClear` (init) | `string` | Accessible name of a sort button that removes the sort of a column. {0} is the title of the column. |
| `SortDefault` (init) | `string` | Button in the sort menu that restores the default sort order. |
| `SortDescending` (init) | `string` | Accessible name of a sort button that sorts a column from the largest to the smallest. {0} is the title of the column. |
| `SortDirectionAscending` (init) | `string` | Ascending direction option in the sort menu. |
| `SortDirectionDescending` (init) | `string` | Descending direction option in the sort menu. |
| `SortMoveEarlier` (init) | `string` | Accessible name of the button that makes a sort apply earlier. {0} is the label of the field. |
| `SortMoveLater` (init) | `string` | Accessible name of the button that makes a sort apply later. {0} is the label of the field. |
| `SortMultiple` (init) | `string` | Text of the sort menu trigger when the list sorts by several fields. {0} is how many. |
| `SortOrderHeading` (init) | `string` | Heading of the list of the active sorts in the sort menu, in the order they apply. |
| `SortPriority` (init) | `string` | Added to the accessible name of a sort button when the list sorts by several columns. {0} is the place of the column in the sort, from 1. |
| `SortRemove` (init) | `string` | Accessible name of the button that takes a field out of the sort. {0} is the label of the field. |
| `SortReverse` (init) | `string` | Accessible name of the button that reverses the direction of one sort. {0} is the label of the field. |
| `SummaryFiltered` (init) | `string` | Label of a summary footer row scoped to all filtered rows. |
| `SummaryPage` (init) | `string` | Label of a summary footer row scoped to the current page. |
| `SummaryRowOne` (init) | `string` | Formatted count of a single row in a summary footer cell. {0} is 1. |
| `SummaryRowOther` (init) | `string` | Formatted count of multiple rows in a summary footer cell. {0} is the row count. |
| `SummarySelection` (init) | `string` | Label of a summary footer row scoped to the selected rows. |
| `SummaryTotal` (init) | `string` | Default label of the summary footer row (`<tfoot>`) of a table. |
| `SummaryView` (init) | `string` | Accessible name and label of the compact summary-rows toggle button. |
| `TableView` (init) | `string` | Accessible name and label of the table layout option. |
| `TreeCollapse` (init) | `string` | Accessible name of the chevron button that collapses an expanded tree row. {0} is the row label or position. |
| `TreeCollapseAll` (init) | `string` | Label of an action that collapses every branch row of a tree table. |
| `TreeExpand` (init) | `string` | Accessible name of the chevron button that expands a collapsed tree row. {0} is the row label or position. |
| `TreeExpandAll` (init) | `string` | Label of an action that expands every branch row of a tree table. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `OperatorLabel(FilterOperator op, bool forTag = false)` | `string` | The text of an operator for a filter tag and an operator picker. |

### Structs

#### `DataListSummaryValue`

`Klyroo.Table` · struct

The value of a summary footer cell: a number, which the column formats with its `SummaryFormat` or `Format` and the culture of the page, or a text shown as it is. A `Decimal`, an integer or a `String` converts (use `DataListSummaryValue.FromDouble` for a double) to it implicitly, so a resolver can write `["amount"] = 1234.5m`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Empty` | `DataListSummaryValue` | The value with nothing to show: the footer cell stays empty. |
| `IsEmpty` | `bool` | Whether there is nothing to show. |
| `Number` | `decimal?` | The number, or `null` when the value is a text or empty. |
| `Text` | `string?` | The text, or `null` when the value is a number or empty. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static FromDouble(double number)` | `DataListSummaryValue` | A number from a `Double`; a value that is not finite or does not fit a decimal is `DataListSummaryValue.Empty`. |
| `static FromNumber(decimal number)` | `DataListSummaryValue` | A number. |
| `static FromText(string? text)` | `DataListSummaryValue` | A text shown as it is; a null or empty text is `DataListSummaryValue.Empty`. |

### Interfaces

#### `IDataListQueryExecutor`

`Klyroo.Table` · interface

Runs the queries a data list builds for a `Queryable` source (`QueryableDataList.Apply``): the count of the matching rows and the page. The list asks the service container for one, so an adapter package replaces how the queries run (EF Core's `CountAsync` and `ToListAsync`, with the cancellation token of the request) without the list referencing the provider. `SynchronousDataListQueryExecutor` is the default.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<int>` | Counts the rows of a query. |
| `ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<List<T>>` | Loads the rows of a query. A cursor needs the sort values the database computed next to each row: the list passes `DataListQuery.Rows` (an `IQueryable<DataListRow<T>>`) here, so the projection is part of the query and needs nothing from the executor. |

### Enums

#### `DataListActionScope`

`Klyroo.Table` · enum

Which rows a bulk or toolbar action targets.

| Value | Description |
| :--- | :--- |
| `Selection` | The rows that are ticked. |
| `All` | Every row the search and the filters match, on every page. A list over a provider or a query does not hold them: the action gets the `DataListActionContext.Request` and applies it where the data lives. |

#### `DataListAlign`

`Klyroo.Table` · enum

Horizontal alignment of a column.

| Value | Description |
| :--- | :--- |
| `Start` | At the start of the line: left in a left-to-right language. |
| `Center` | Centered. |
| `End` | At the end of the line, for numbers. |

#### `DataListApplyMode`

`Klyroo.Table` · enum

How a filter editor applies what the user changes.

| Value | Description |
| :--- | :--- |
| `Live` | Each change applies at once (a text filter after a pause in the typing). The history gets an entry per change. |
| `Submit` | Changes wait in the editor until the Apply button; Cancel, Escape or a click outside drop them. |

#### `DataListDensity`

`Klyroo.Table` · enum

How tight the rows of a data list are.

| Value | Description |
| :--- | :--- |
| `Default` | Normal rows. |
| `Compact` | Little padding, for long lists. |
| `Comfortable` | Generous padding. |

#### `DataListPagingMode`

`Klyroo.Table` · enum

How a data list pages its rows.

| Value | Description |
| :--- | :--- |
| `Offset` | Numbered pages (`p`, `ps`) with `KlyDataListPagination`. |
| `Cursor` | Keyset cursor paging (`c`): pages append in place with `KlyDataListInfiniteLoader`, and degrade to a link in static rendering. |
| `None` | No paging: the source returns every matching row at once. Pairs with `KlyDataListTable.Virtualize`. |

#### `DataListPin`

`Klyroo.Table` · enum

Which horizontal edge a column of `KlyDataListTable` is pinned to.

| Value | Description |
| :--- | :--- |
| `None` | Scrolls horizontally with the table. |
| `Start` | Sticks to the start edge (left in left-to-right languages). |
| `End` | Sticks to the end edge (right in left-to-right languages). |

#### `DataListSelectionBarPosition`

`Klyroo.Table` · enum

Where the selection bar sits.

| Value | Description |
| :--- | :--- |
| `Sticky` | At the bottom of the window while the list is on screen, and no lower than the end of the list (the default). |
| `Fixed` | Fixed to the bottom of the window, centered. |

#### `DataListSelectionClear`

`Klyroo.Table` · enum

When running an action unticks the rows.

| Value | Description |
| :--- | :--- |
| `Never` | The selection is kept (the default). |
| `Trigger` | The rows are unticked as the action starts. |
| `Success` | The rows are unticked once the action finished without failing; a failure keeps them so that the user can try again. |

#### `DataListSelectionScope`

`Klyroo.Table` · enum

Where the rows of a selection survive: across pages, or only on the page they were ticked on.

| Value | Description |
| :--- | :--- |
| `Auto` | The list decides: a list of rows in memory (`Items`) keeps the selection across pages, a list over a provider or a queryable keeps it for the rows of the page. |
| `Page` | Rows that leave the page (a new page, a new search or filter, another sort) are no longer selected. |
| `All` | Selected rows stay selected on other pages while the search and the filters still match them. Only a list of rows in memory (`Items`) can do it: the others behave as `DataListSelectionScope.Page`. |

#### `DataListSkeletonKind`

`Klyroo.Table` · enum

Placeholder shape drawn inside a column cell while the initial rows of `KlyDataListTable` load.

| Value | Description |
| :--- | :--- |
| `Text` | A single text bar (`h-4 w-3/4`). |
| `Avatar` | A circular avatar next to a text bar. |
| `Icon` | A small square icon placeholder. |
| `Dot` | A status dot next to a short text bar. |
| `Check` | A checkbox square placeholder. |
| `Badge` | A pill badge placeholder. |
| `Number` | A short numeric bar aligned to the end edge. |
| `Progress` | A horizontal progress track placeholder. |
| `None` | Leaves the placeholder cell blank. |

#### `DataListSummaryKind`

`Klyroo.Table` · enum

Built-in column aggregation kinds for the summary footer (`<tfoot>`) of `KlyDataListTable`.

| Value | Description |
| :--- | :--- |
| `None` | No summary value in this column. |
| `Sum` | Sum of numeric values in the column across the summary scope. |
| `Average` | Arithmetic mean of numeric values in the column across the summary scope. |
| `Count` | Count of rows with a non-empty value in the column across the summary scope. |
| `Min` | Minimum value in the column across the summary scope. |
| `Max` | Maximum value in the column across the summary scope. |

#### `DataListSummaryScope`

`Klyroo.Table` · enum

Which set of rows a summary footer row aggregates.

| Value | Description |
| :--- | :--- |
| `Filtered` | Every row matching the active search and filters (or the loaded rows on screen for remote sources without `DataListSummaries.ResolveAsync`). |
| `Page` | The rows currently loaded on screen (`DataListContext.Rows`). |
| `Selection` | The currently selected rows (`DataListContext.SelectedItems`). |
