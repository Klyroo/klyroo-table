# API Reference — Klyroo.Table

Complete public API reference extracted directly from the source code of `Klyroo.Table.Core` and `Klyroo.Table`.

## Razor Components

### `<KlyDataList>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataList.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Schema` | `DataListSchema<TItem>?` | What a request may ask for: which fields sort, which filter, the default sort and page size. The schema is fixed for the life of the list: a new instance given after the first render is ignored (build it once, in a static field or a cached service; an inline `DataListSchema.For&lt;T&gt;(...)` in markup would create a new one on every render), and giving another schema means re-keying the list with `@@key`. Optional for a list of rows in memory whose `KlyDataListColumn{TItem}`s name a `Property`: those columns then describe the schema (every one sortable unless it says otherwise). A provider or a queryable needs a schema, because it is the schema that drops what a request must not ask for. |
| `Items` | `IEnumerable<TItem>?` | The rows, kept in memory: the list sorts and pages them itself, with the semantics of `InMemoryDataList`. Give a stable collection (a field, not a new list on every render): a new reference loads the list again. A list takes one source: `Items`, `Queryable` or `Provider`. |
| `Queryable` | `IQueryable<TItem>?` | A query of a database provider (an EF Core `DbSet`, with the filters that are not the user's already applied): the list turns each request into queries with `QueryableDataList` and runs them through the `IDataListQueryExecutor` of the container. Needs a `Schema`. The default executor blocks the thread while the database answers: register an asynchronous one (`AddKlyrooTableEntityFrameworkAdapter()` for EF Core). An `AsQueryable()` over a list is rejected: use `Items`. A DbContext allows one query at a time: give the list a context of its own (a factory) when the page has several. |
| `Provider` | `DataListProvider<TItem>?` | Loads a page of rows for a request, from a database, an HTTP API or anything else. It receives a token that is cancelled when a newer request replaces it. |
| `State` | `DataListState?` | The search, filters, sort, page and page size. Bind it with `@bind-State` to read or set it from outside: a value you set (a reset button) is written to the URL (a history entry) with `UrlSync`, or loaded directly without it. With `UrlSync` the URL wins at the start: this value only starts the list when the URL holds none of its keys. |
| `StateChanged` | `EventCallback<DataListState>` | Raised when the state changes, from the list or, with URL sync, from the URL (a link, Back and Forward, a typed address). |
| `UrlKey` | `string?` | A prefix for the URL keys of this list (`users` gives `users.s`, `users.p`): needed when a page has several lists that sync with the URL. Without it the keys are `q`, `p`, `ps`, `s`, `f.&lt;key&gt;`, `c` and `l`. |
| `UrlSync` | `bool` | Keeps the state in the URL (true by default): a page or a sort is pushed as a history entry, so Back restores it, and a typed or shared address loads its state. In static server rendering the controls are links and GET forms. Turn it off for a list in a dialog. |
| `ResultJson` | `JsonTypeInfo<DataListResult<TItem>>?` | Metadata to serialize the result of the prerender (`MyJson.Default.DataListResultMyRow`, from a `JsonSerializerContext` of the app with `[JsonSerializable(typeof(DataListResult&lt;MyRow&gt;))]`). The result of a prerendered list is then handed to the interactive render (WebAssembly, Server or Auto), which shows it at once and does not load it again. Without it nothing is persisted and the interactive render loads the list a second time. Lists that persist need distinct `UrlKey` or `PersistKey` values (two of the same row type under the same key throw). |
| `PersistKey` | `string?` | The key the prerendered result is persisted under, when the list has no `UrlKey` or two lists share it. The row type is part of the key. |
| `Density` | `DataListDensity` | How tight the rows are. |
| `PagingMode` | `DataListPagingMode` | How the list pages its rows: numbered pages (`Offset`, the default), keyset cursor paging (`Cursor`, with `KlyDataListInfiniteLoader{TItem}`) or no paging (`None`, every matching row at once, for virtualized tables). Changing it after the first render starts the list again from its first rows (page 1, no cursor) and loads it in the new mode; a cursor in the address of a list that does not page by cursor is dropped. |
| `Label` | `string?` | The name of the list. Its table is named by it (`Results` when it has none), and its pagination landmark is "Pagination, &lt;label&gt;": give the lists of a page distinct labels, because two landmarks with the same name cannot be told apart. |
| `ChildContent` | `RenderFragment?` | The parts of the list: toolbar items, the table with its columns, the pagination. Without it the list renders a default layout: result count, refresh, error state, table and pagination. |
| `Ui` | `DataListUi?` | Per-instance classes for each slot. |

### `<KlyDataListActionsDropdown>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListActionsDropdown.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | The label of the button that opens the menu. Without it the localized "Actions". |
| `Icon` | `string` | The name of the icon of the button (`ellipsis` by default). |

### `<KlyDataListAddFilter>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListAddFilter.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether the editors apply each change at once (`Live`) or wait for an Apply button (`Submit`). |
| `Size` | `KlySize?` | The size of the button. Defaults to small. |
| `SearchFrom` | `int` | The number of filters from which the menu has a search field. Defaults to 8. |

### `<KlyDataListClearFilters>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListClearFilters.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | The text of the button. Without it, the localized "Clear filters". |
| `Size` | `KlySize?` | The size of the button. Defaults to small. |

### `<KlyDataListColumnPanel>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListColumnPanel.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | Label of the trigger button and heading of the popover. Defaults to the localized "Columns". |
| `Size` | `KlySize` | Size of the trigger button (small by default). |
| `AllowReorder` | `bool` | Shows the drag grip and the Move up / Move down buttons to reorder columns (true by default). |
| `AllowPinning` | `bool` | Shows Pin to start / Pin to end controls (true by default). |
| `SearchThreshold` | `int` | Shows a search box inside the panel when at least this many configurable columns exist (30 by default; 0 always shows it; a negative number never). |

### `<KlyDataListEmpty>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListEmpty.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string?` | The title. Without it, the localized "No results". |
| `Description` | `string?` | The text under the title. Without it, the localized hint to change the search or the filters; an empty string shows none. |
| `Icon` | `string?` | The icon above the title, by name (any icon name Klyroo.UI accepts). Defaults to `search-x`; an empty string shows none. |
| `ChildContent` | `RenderFragment?` | Extra content between the description and the actions. |
| `Actions` | `RenderFragment?` | Buttons or links shown under the text, for example one that clears the filters. |
| `Ui` | `DataListEmptyUi?` | Per-instance classes for each slot. |

### `<KlyDataListErrorState>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListErrorState.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string?` | The title. Without it, the localized "Could not load the data". |
| `Description` | `string?` | The text under the title. Without it, the localized hint to try again. |

### `<KlyDataListFilterEditor>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListFilterEditor.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | The key of the filter to edit: one of the filters of the schema of the list (any kind; a date filter exact to the second has no editor). |
| `ApplyMode` | `DataListApplyMode` | Whether a change applies at once (`Live`, the default) or waits for the Apply button (`Submit`). Statically rendered, a filter always applies on a link or a submit. |
| `ShowTitle` | `bool` | Shows the name of the filter above the editor. True by default. |
| `OnDone` | `EventCallback` | Raised after Apply or Cancel (the Submit apply mode), so that the popover around the editor closes. |
| `ResetToken` | `int` | Change it to drop what was typed and show the filter as the list has it: the popover around the editor does it each time it closes. |

### `<KlyDataListFilterPanel>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListFilterPanel.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ApplyMode` | `DataListApplyMode` | Whether a change applies at once (`Live`, the default) or waits for Apply, with Reset to drop it (`Submit`). Statically rendered, the form always waits for its Apply button. |
| `Description` | `string?` | A line under the title of the panel. Without it nothing is shown. |
| `CloseOnApply` | `bool` | Adds `data-kly-close` to the Apply button, so that a `KlySlideover` (or a modal) around the panel closes when the filters apply, with no round trip. It has no effect on the Live apply mode, which has no Apply button when interactive. |

### `<KlyDataListFilterTags>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListFilterTags.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ShowAdd` | `bool` | Shows the add filter button (`KlyDataListAddFilter{TItem}`) after the tags. False by default; the default layout of the list shows it. |
| `ShowClear` | `bool` | Shows the clear filters button (`KlyDataListClearFilters{TItem}`) after the tags. False by default; the default layout of the list shows it. |
| `ApplyMode` | `DataListApplyMode` | Whether the editors apply each change at once (`Live`) or wait for an Apply button (`Submit`). |

### `<KlyDataListGrid>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListGrid.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ItemTemplate` | `RenderFragment<TItem>?` | Template for rendering the body of each card in the grid. When `null`, renders the visible columns or schema fields as label/value pairs. |
| `SkeletonTemplate` | `RenderFragment?` | Custom content shown for each placeholder card while the initial result loads. |
| `Empty` | `RenderFragment<DataListContext<TItem>>?` | Shown when the list has no rows. Without it, `EmptyState` or `KlyDataListEmpty` (with the filtered-empty variant when search or filters are active). |
| `EmptyState` | `DataListEmptyState?` | Declarative configuration for the unfiltered empty state (icon, title, description, and an optional first-use `Action`). When search or filters are active, the filtered empty state is still displayed instead and the first-use action is omitted. |
| `ShowProgressLine` | `bool` | Draws a subtle top progress indicator while the cards on screen are being refreshed or replaced (`true` by default). |
| `Columns` | `int` | Maximum number of card columns on wide screens (1 to 4, 3 by default). |
| `ShowSelectionControl` | `bool` | Draws a selection checkbox on each card and a select-all header when selection is enabled (`true` by default). |
| `RowActions` | `IReadOnlyList<DataListRowAction<TItem>>?` | Per-card actions (`KlyDataListRowActions{TItem}`). |
| `RowActionsMaxVisible` | `int` | How many row actions show as inline buttons on each card (0 by default, keeping one menu trigger button per card). |
| `OnRowActionError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when a row action threw or reported a failure. |
| `AriaLabel` | `string?` | Accessible name of the grid region. Defaults to the label of the list, then the localized "Results". |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional visual grouping of the loaded cards under collapsible `KlyDataListGroupHeader{TItem}` headers with per-group selection checkboxes. |

### `<KlyDataListGroupHeader>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListGroupHeader.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Group` | `DataListGroup<TItem>` | The group rendered by this header. |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional grouping definition supplying `LabelTemplate` or `SummaryTemplate`. |
| `Collapsed` | `bool?` | Overrides whether the group is currently collapsed. When `null`, reads `IsGroupCollapsed(DataListGroup{TItem})`. |
| `OnToggle` | `EventCallback<bool>` | Raised when the fold/unfold button is clicked. When not bound, toggles the group on `DataListContext{TItem}`. |
| `Selectable` | `bool?` | Overrides whether the group header displays a select-all-in-group checkbox. Defaults to `SelectionVisible`. |
| `ToggleLabel` | `string?` | Accessible name of the fold/unfold button. Defaults to the localized `GroupToggle` formatted with `Label`. |

### `<KlyDataListInfiniteLoader>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListInfiniteLoader.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Auto` | `bool` | Automatically loads the next page when the sentinel approaches the viewport (`true` by default). Set to `false` for a manual Load more button only. |
| `RootMargin` | `string` | Margin of the `IntersectionObserver` when `Auto` is `true` (`"0px 0px 320px 0px"` by default). |
| `Root` | `string?` | Optional CSS selector of the scrolling element when `Auto` is `true`. Without it, watches the closest scrollable ancestor or the viewport; next to a table with `Virtualize` the loader is drawn inside the scrolling box of the table and watches it. |
| `Label` | `string?` | Text of the Load more button. Defaults to the localized "Load more". |
| `ShowEnd` | `bool` | Shows the localized "No more items" text once the last page has been loaded. It is announced to assistive technologies either way. |
| `ErrorTemplate` | `RenderFragment<DataListContext<TItem>>?` | Custom content shown when loading the next page fails (replaces the default message and Retry button). |

### `<KlyDataListLayoutSwitch>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListLayoutSwitch.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ShowLabels` | `bool` | Shows the localized text labels ("Table view", "Grid view") next to the icons (`false` by default). |
| `Order` | `IReadOnlyList<DataListLayout>?` | Order of the layout options (`[Table, Grid]` by default). |
| `TableIcon` | `string` | Icon name for the Table layout (`"table-2"` by default). |
| `GridIcon` | `string` | Icon name for the Grid layout (`"layout-grid"` by default). |
| `AriaLabel` | `string?` | Accessible name of the layout switch group. Defaults to the localized "View layout". |

### `<KlyDataListPagination>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListPagination.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `PageSizes` | `int[]` | The page sizes of the picker. The current size and sizes above the schema's maximum are handled for you. |
| `ShowPageSize` | `bool` | Shows the page size picker. |
| `SiblingCount` | `int` | Pages shown on each side of the current one. |
| `ShowFirstLast` | `bool` | Buttons that jump to the first and the last page. |
| `AriaLabel` | `string?` | The accessible name of the pagination landmark. Give each list of a page its own: two landmarks with the same name cannot be told apart. Without it the landmark is "Pagination, &lt;label&gt;" for a list with a label, else the localized "Pagination". |

### `<KlyDataListRefresh>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListRefresh.razor`

_No public `[Parameter]` properties._

### `<KlyDataListResultCount>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListResultCount.razor`

_No public `[Parameter]` properties._

### `<KlyDataListRowActions>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListRowActions.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Item` | `TItem` | The row. |
| `Position` | `int` | The place of the row on the page, from 1: it names the actions of a row that has no `RowLabel` in the schema. |
| `Actions` | `IReadOnlyList<DataListRowAction<TItem>>` | The actions, in the order they are shown. Their keys are unique, children included. |
| `MaxVisible` | `int` | How many actions show as buttons on a wide screen; the others go into a menu (3 by default). 0 puts them all behind one button. Below the `md` breakpoint every action is in a sheet. |
| `Size` | `KlySize` | The size of the buttons. |
| `OnError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when an action threw or reported a failure; the list is not refreshed and nothing is rethrown. Without a handler an exception is logged. |
| `Ui` | `DataListUi?` | Per-instance classes for each slot. |

### `<KlyDataListSearch>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListSearch.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Placeholder` | `string?` | The placeholder of the field. Without it, the localized "Search…". |
| `AriaLabel` | `string?` | The accessible name of the field and of its search landmark. Without it, "Search &lt;label of the list&gt;" for a list with a label, else the localized "Search". |
| `Debounce` | `int?` | Milliseconds of quiet after the last keystroke before the list searches. Without it, `SearchDebounce` of `KlyrooTableOptions` (300). Enter searches at once. Statically rendered, nothing is searched before Enter or the search button. |
| `MinLength` | `int` | The fewest characters (once the spaces around the text are trimmed) a search must have: a shorter text searches nothing, so the rows come back as if the field were empty. Zero by default. Statically rendered, the browser refuses to submit a shorter text. |
| `Size` | `KlySize?` | The size of the field. Defaults to medium. |

### `<KlyDataListSelectionActions>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListSelectionActions.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `MaxVisible` | `int` | How many actions show as buttons; the others go into a "More actions" menu (3 by default). |
| `Scope` | `bool` | Shows the switch between the ticked rows and every result when the source knows how many results there are (true by default). |
| `Position` | `DataListSelectionBarPosition` | Whether the bar sticks to the bottom of its list while the list is on screen (the default) or is fixed to the bottom of the window. |

### `<KlyDataListSortMenu>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListSortMenu.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | Label of the trigger button and heading of the popover. Defaults to the localized "Sort". |
| `Size` | `KlySize` | Size of the trigger button (small by default). |
| `Items` | `IReadOnlyList<DataListSortOption>?` | Explicit sortable field options. When `null`, the menu lists the sortable `KlyDataListColumn{TItem}`s in user order followed by any other sortable fields of the schema. |
| `Layouts` | `IReadOnlyList<DataListLayout>?` | Layouts in which the sort menu is shown (for example `[DataListLayout.Grid]` when the table headers already sort). When `null`, shown in every layout. |

### `<KlyDataListTable>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListTable.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ChildContent` | `RenderFragment?` | The columns: `KlyDataListColumn{TItem}` components. Without any, the table shows one column per field of the schema. |
| `Caption` | `string?` | Names the table for assistive technologies (visually hidden), and the scrolling region around it. Without it, the `AriaLabel` of the table, then the label of the list, then the localized "Results". |
| `RowActions` | `IReadOnlyList<DataListRowAction<TItem>>?` | The actions of each row: a last column with buttons up to `RowActionsMaxVisible`, then a menu, and below the `md` breakpoint one button that opens a sheet (`KlyDataListRowActions{TItem}`). Interactive only, except the actions that are links. |
| `RowActionsMaxVisible` | `int` | How many row actions show as buttons on a wide screen (3 by default); 0 keeps a single "Actions for ..." button per row. |
| `OnRowActionError` | `EventCallback<DataListRowActionFailure<TItem>>` | Raised when a row action threw or reported a failure. |
| `AriaLabel` | `string?` | The accessible name of the table when there is no `Caption`. |
| `Empty` | `RenderFragment<DataListContext<TItem>>?` | Shown when the list has no rows. It receives the list, whose `IsFiltered` tells "nothing matches" from "nothing exists". Without it, `EmptyState` or `KlyDataListEmpty`. |
| `EmptyState` | `DataListEmptyState?` | Declarative configuration for the unfiltered empty state (icon, title, description, and an optional first-use `Action`). When search or filters are active, the filtered empty state is still displayed instead and the first-use action is omitted. |
| `HeaderMenu` | `bool` | Adds a dropdown menu button to each column header in interactive mode with quick actions to sort ascending/descending, stop sorting, pin/unpin, or hide the column. Individual columns can override this via `HeaderMenu`. |
| `ShowProgressLine` | `bool` | Draws a subtle top progress indicator while the rows on screen are being refreshed or replaced (`true` by default). |
| `Virtualize` | `bool` | Renders only the rows around the scroll viewport with Blazor's `Virtualize`, using `tr` spacers, a sticky header and fixed single-line row heights matching `DataListDensity` (36 px compact, 44 px default, 60 px comfortable). Meant for `None` and `Cursor` lists with many loaded rows. Interactive only: static renders and prerenders draw regular rows without virtualization. |
| `ItemSize` | `float?` | Height in pixels of one row when `Virtualize` is on. Without it, follows `DataListDensity` (36, 44 or 60 px). |
| `OverscanCount` | `int` | Extra rows rendered above and below the viewport when `Virtualize` is on (3 by default). |
| `Resizable` | `bool` | Puts a resize handle (a vertical separator) on the end edge of the header of each `Resizable` column (false by default): drag it, or focus it and press ArrowLeft and ArrowRight (16 px, 64 with Shift; in right-to-left text the arrow that points away from the start edge grows the column), Home and End for the limits, Delete or a double click for the declared width. The browser changes the width while the gesture runs and reports it once; the list keeps it in its column preferences (`PreferencesKey`). Interactive only: a static render has no handle but still applies a saved width. |
| `RowSummary` | `DataListRowSummary<TItem>?` | Optional compact summary-row presentation (leading tile, title, status, details joined with ` · `, selection checkbox and row actions), active automatically below the `md` breakpoint when `AutoPhone` is `true`, when `DefaultActive` is `true`, or when toggled with `KlyDataListViewToggle{TItem}`. |
| `GroupBy` | `DataListGroupBy<TItem>?` | Optional visual grouping of the loaded rows under collapsible `KlyDataListGroupHeader{TItem}` headers with per-group selection checkboxes. Ignored when `Tree` is set. |
| `Tree` | `DataListTree<TItem>?` | Optional hierarchical tree configuration: expands and collapses nested child rows with a chevron button (click, Enter/Space, or ArrowRight/ArrowLeft) and visual tree rail connectors. Disables `RowSummary` and `GroupBy` when set. |
| `Summaries` | `DataListSummaries<TItem>?` | Optional footer summary (`&lt;tfoot&gt;`) configuration: target `DataListSummaryScope`(s) (`Filtered`, `Page`, `Selection`), custom row label, and optional `ResolveAsync` delegate for remote or asynchronous totals. When `null`, a single `Filtered` summary row is still rendered automatically whenever any column declares a summary. |

### `<KlyDataListViewToggle>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListViewToggle.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string?` | Text and accessible name of the toggle button. Defaults to the localized "Summary view". |
| `ShowLabel` | `bool` | Shows the text label next to the icon (`true` by default). |
| `Icon` | `string` | Icon name shown on the toggle button (`"list"` by default). |

### `<DataListActionDialog>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActionDialog.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Open` | `bool` | — |
| `OpenChanged` | `EventCallback<bool>` | — |
| `Title` | `string` | — |
| `Description` | `string?` | — |
| `ConfirmLabel` | `string` | — |
| `Color` | `KlyColor` | — |
| `OnConfirm` | `EventCallback` | — |

### `<DataListRowSelect>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowSelect.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Item` | `TItem` | The row. |
| `Position` | `int` | The place of the row on the page, from 1: the name of its box when the schema gives no `RowLabel`. |

### `<DataListSelectAll>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSelectAll.razor`

_No public `[Parameter]` properties._

## C# Types, Controllers, Models & Enums

### `enum DataListTreeSelectionMode`
*Package*: `Klyroo.Table.Core` · *Source*: `src/Klyroo.Table.Core/DataListTree.cs`

Which rows of a hierarchical tree table can be selected with checkboxes.

| Member | Type | Description |
| :--- | :--- | :--- |
| `All` | `enum member` | — |
| `Roots` | `enum member` | — |

### `class DataListTreeNode<TItem>`
*Package*: `Klyroo.Table.Core` · *Source*: `src/Klyroo.Table.Core/DataListTree.cs`

One node in the flattened hierarchy produced by `DataListTree{TItem}` over the loaded top-level rows of a data list. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | Unique key of the row across the tree. |
| `Item` | `TItem` | The row data item. |
| `Depth` | `int` | Nesting depth: `0` for top-level root rows, `1` for immediate children, etc. |
| `Parent` | `DataListTreeNode<TItem>?` | Parent tree node, or `null` for top-level root rows. |
| `RootIndex` | `int` | Zero-based index of the top-level root ancestor. |
| `Children` | `IReadOnlyList<DataListTreeNode<TItem>>` | Child tree nodes. |
| `HasChildren` | `bool` | Whether this node has at least one child node. |
| `IsLast` | `bool` | Whether this node is the last sibling of its parent. |
| `AncestorContinuations` | `IReadOnlyList<bool>` | For each ancestor depth from `1` to `Depth - 1`, whether that ancestor has further siblings below it (used to draw vertical continuation rails). |

### `class DataListTree<TItem>`
*Package*: `Klyroo.Table.Core` · *Source*: `src/Klyroo.Table.Core/DataListTree.cs`

Hierarchical tree configuration for `KlyDataListTable{TItem}`. Child rows travel with their parent item (`Children`), expand and collapse with a chevron button (mouse, Enter/Space, or ArrowRight/ArrowLeft) and visual rail connectors, and participate in row selection according to `Selectable`. When a tree is configured on `KlyDataListTable{TItem}`, summary rows and visual groups are disabled on the table. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Children` | `Func<TItem, IEnumerable<TItem>?>` | Extracts the child items of a row. Null items and nodes whose key duplicates an earlier node in the tree are skipped. |
| `DefaultExpanded` | `bool` | Whether all branch nodes start expanded by default (`false` by default). When `DefaultExpandedDepth` is set, it takes precedence over this flag. |
| `DefaultExpandedDepth` | `int?` | Optional number of nesting levels expanded by default (`0` = none, `1` = root branches expanded, `2` = roots and their children expanded, etc.). Overrides `DefaultExpanded` when not `null`. |
| `Selectable` | `DataListTreeSelectionMode` | Which rows in the tree display a selection checkbox (`DataListTreeSelectionMode.All` by default). |
| `IsDefaultExpanded(int depth)` | `bool` | Whether a branch node at `depth` starts expanded according to the baseline settings. |
| `BuildTree(
        IReadOnlyList<TItem> rootItems,
        DataListSchema<TItem>? schema = null)` | `(IReadOnlyList<DataListTreeNode<TItem>> Roots, IReadOnlyList<DataListTreeNode<TItem>> AllNodes)` | — |

### `class KlyrooTableOptions`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/KlyrooTableOptions.cs`

Configuration of Klyroo.Table: its component themes and its message tables. It is an extension of `KlyrooOptions` (`Extension{T}`), so its themes freeze with the ones of Klyroo.UI and the Tailwind class extractor and the docs bundle find them. Configure it with `services.AddKlyrooTable(t =&gt; ...)`, in code shared by the server and the WebAssembly client. Partial: each family of components declares its themes in its own file.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Messages(string localeCode, Func<KlyToolsMessages, KlyToolsMessages> change)` | `KlyrooTableOptions` | Changes the texts of a locale, like `Extend` does for Klyroo.UI: `change` receives the table the locale resolves to and returns the one to use, usually `m =&gt; m with { ... }`. `localeCode` is a code of `All` ("fr"), or the code of a locale you added ("en-AU", "tlh"): a locale without a table of its own starts from the table `ResolveCode` picks for it, then its changes apply. Like the themes, the messages become read-only the first time a component resolves a text. A component has already resolved a text. |

### `class KlyToolsComponentBase`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/KlyToolsComponentBase.cs`

Base of the components of Klyroo.Table: the theme resolution of `KlyComponentBase`, plus the options and the texts of this package.

### `class KlyDataList<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataList.Columns.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `ExpandAll()` | `void` | Expands every branch node in the table's hierarchical tree (`Tree`). |
| `CollapseAll()` | `void` | Collapses every branch node in the table's hierarchical tree (`Tree`). |
| `ResetExpansion()` | `void` | Resets tree expansion back to the defaults declared on `Tree`. |

### `class KlyDataList<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataList.Selection.cs`

### `class KlyDataListColumn<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListColumn.cs`

A column of `KlyDataListTable{TItem}`. Renders nothing itself: it registers with its list (`Columns`). What a cell shows comes from one of three places: `Template`, `Property` or `Field`. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `EffectiveKey` | `string?` | The key of the column: `Key`, `Field` or the member path of `Property` in camel case. |
| `EffectiveTitle` | `string` | The header of the column: `Title`, or its key written as words. |
| `Dispose()` | `void` | Takes the column out of its table. |
| `Humanize(string key)` | `string` | A key written as words: `firstName` becomes "First name", `department.name` becomes "Name" (the last segment). |
| `ReadField<T>(this DataListSchema<T> schema, T item, string key)` | `object?` | The value of a field of the schema for a row, null-safe along its path; the elements of a collection field joined by commas; `null` for an unknown key. |

### `class KlyDataListFilterEditor<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/KlyDataListFilterEditor.Remote.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Dispose()` | `void` | Stops a search or a page that is on its way. |

### `class DataListActionPartBase<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActionPartBase.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Dispose()` | `void` | — |

### `enum DataListActionScope`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

Which rows a bulk or toolbar action targets.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Selection` | `enum member` | — |

### `enum DataListSelectionScope`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

Where the rows of a selection survive: across pages, or only on the page they were ticked on.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Auto` | `enum member` | — |
| `Page` | `enum member` | — |
| `All` | `enum member` | — |

### `enum DataListSelectionClear`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

When running an action unticks the rows.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Never` | `enum member` | — |
| `Trigger` | `enum member` | — |
| `Success` | `enum member` | — |

### `enum DataListSelectionBarPosition`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

Where the selection bar sits.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Sticky` | `enum member` | — |
| `Fixed` | `enum member` | — |

### `class DataListActionContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

What an action receives when it runs: the rows it targets, the scope and the request that stands for "every result". The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `List` | `DataListContext<TItem>` | The list, to read its state or to load it again (`RefreshAsync`). |
| `Scope` | `DataListActionScope` | Whether the action targets the ticked rows or every result. |
| `SelectedItems` | `IReadOnlyList<TItem>` | The rows the action targets. With `Selection`: the ticked rows (those that are on screen or were ticked on another page of a list in memory). With `All` on a list of rows in memory, when the action runs: every row the search and the filters match. With `All` over a provider or a queryable: empty, because the list never holds those rows; apply `Request` instead. |
| `TargetCount` | `long` | How many rows the action will act on: `MatchingCount` when `Scope` is `All` (and the source knows it), else the number of ticked rows. Use it, not `SelectedItems.Count`, in the text of a confirmation and in `Visible` and `Disabled`: before the action runs, `SelectedItems` holds only the ticked rows (a list is never loaded just to describe an action), while the action itself gets every matching row of a list in memory. |
| `SelectedKeys` | `IReadOnlyList<string>` | The row keys (`RowKey` of the schema) of `SelectedItems`; empty where those are empty too. |
| `MatchingCount` | `long?` | How many rows the search and the filters match, when the source says (`TotalCount`); `null` when it does not. |
| `Request` | `DataListRequest?` | The request for every row the list matches: its search, filters and sort, with no paging and no facets, as the list sent it. For `All` over a provider, send it to your server and apply it there (`schema.Apply(query, request)`), so a bulk action never needs the rows on the client. `null` before the first load. |
| `CancellationToken` | `CancellationToken` | Cancelled when the list goes away. |

### `class DataListActionConfirm<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

The question an action asks before it runs, in a dialog. The text can say how many rows are affected. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The title of the dialog. |
| `Description` | `Func<DataListActionContext<TItem>, string?>?` | The text under the title, built from the context so that it can count the rows (`c =&gt; $"{c.TargetCount} customers will be removed."`, which is right for the ticked rows and for every result). |
| `ConfirmLabel` | `string?` | The label of the confirm button. Without it the localized "Confirm". |
| `Color` | `KlyColor?` | The color of the confirm button; `Error` for a destructive action. Without it the color of the action. |

### `class DataListAction<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

An action of a data list: a button of the `KlyDataListSelectionActions{TItem}` bar (it acts on the selection) or an entry of the `KlyDataListActionsDropdown{TItem}` menu. While it runs its button shows a spinner and the action cannot start twice. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | Identifies the action: tests and the menu use it. |
| `Label` | `string` | The text of the button. |
| `Icon` | `string?` | The name of an icon shown before the label (`trash`), one of the icons of Klyroo.UI. |
| `Color` | `KlyColor` | The color of the button; `Error` for a destructive action. |
| `Variant` | `ButtonVariant` | The variant of the button in the selection bar. |
| `Visible` | `Func<DataListActionContext<TItem>, bool>?` | Hides the action when it returns `false` (an action that does not apply to the selected rows). It runs on every render: keep it cheap. |
| `Disabled` | `Func<DataListActionContext<TItem>, bool>?` | Disables the action when it returns `true`. It runs on every render: keep it cheap. |
| `Confirm` | `DataListActionConfirm<TItem>?` | Asks before the action runs. Without it the action runs on the click. |
| `ClearSelection` | `DataListSelectionClear` | When the ticked rows are unticked: never by default. |
| `RefreshOnSuccess` | `bool` | Loads the list again once the action succeeded (the rows it changed or removed). Off by default; an action can also call `RefreshAsync` itself. |
| `OnExecute` | `Func<DataListActionContext<TItem>, Task>?` | What the action does. An exception marks the run as failed: the selection stays, the list is not refreshed, `OnError` of the part is raised and nothing is thrown to the page. |

### `record DataListActionFailure<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListActions.cs`

An action that threw. The action. What it threw. The row type.

### `enum DataListPin`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListColumns.cs`

Which horizontal edge a column of `KlyDataListTable{TItem}` is pinned to.

| Member | Type | Description |
| :--- | :--- | :--- |
| `None` | `enum member` | — |
| `Start` | `enum member` | — |
| `End` | `enum member` | — |

### `record DataListColumnState`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListColumns.cs`

The effective state of one column of a data list: visibility, pinning, width and limits. The column key (`EffectiveKey`). The display title of the column. Whether the column is currently shown. Whether the column cannot be hidden. The active horizontal pin edge. The active pixel width, or `null` when automatic. Minimum width in pixels when resized. Maximum width in pixels when resized. Whether the user can resize the column.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Order` | `List<string>` | — |
| `Hidden` | `HashSet<string>` | — |
| `Shown` | `HashSet<string>` | — |
| `PinnedStart` | `HashSet<string>` | — |
| `PinnedEnd` | `HashSet<string>` | — |
| `Unpinned` | `HashSet<string>` | — |
| `Widths` | `Dictionary<string, int>` | — |
| `SummaryView` | `bool?` | — |
| `ToggledGroups` | `HashSet<string>` | — |
| `HasColumnOverrides` | `bool` | — |
| `IsEmpty` | `bool` | — |
| `ClearColumns()` | `void` | — |
| `Clear()` | `void` | — |
| `Clone()` | `DataListColumnPreferences` | — |
| `Prune(IReadOnlySet<string> knownKeys, IReadOnlySet<string> requiredKeys)` | `void` | Drops every key that is not in `knownKeys`, every key that is in two sections that exclude each other, and the visibility of `requiredKeys`. |
| `NormalizeKey(string? key)` | `string?` | Sanitizes a `PreferencesKey` to the cookie-safe subset (`[A-Za-z0-9_-]`, max 64 chars), or `null` when blank. |
| `Encode()` | `string?` | Encodes the preferences as a compact cookie-safe token, or empty string when nothing overrides defaults. Returns `null` when any key exceeds `MaxKeyLength`, any section exceeds `MaxKeysPerSection`, or the result would pass `MaxEncodedLength`: the caller keeps the preferences in memory and leaves the cookie as it was. |
| `DecodeFromCookie(string? rawCookie, string? normalizedKey)` | `DataListColumnPreferences` | Extracts and decodes the preferences of `normalizedKey` from the grouped `CookieName` cookie value (`key1.v1!...!!key2.v1!...`). |
| `Decode(string? raw)` | `DataListColumnPreferences` | Parses an encoded cookie string. Never throws on malformed input: invalid tokens are skipped. |
| `Claim(IServiceProvider services, string normalizedKey)` | `string` | — |
| `Release(IServiceProvider services, string? claimedKey)` | `void` | — |
| `Read(IServiceProvider services, string? normalizedKey, bool interactive)` | `(DataListColumnPreferences Preferences, bool ReadFromServer)` | The preferences the grouped cookie of the current request holds for `normalizedKey`. `ReadFromServer` is true when there was a request to ask (even if it carried no cookie): the browser has nothing to add for this render. An interactive render has no request (a circuit is not an HTTP request) and the browser has no `IKlyRequestCookies`: both read the cookie in the browser instead. |

### `class DataListContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.Columns.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `ColumnStates` | `IReadOnlyList<DataListColumnState>` | Every keyed column of the table in its active order, with its effective visibility, pin edge and width. |
| `HasCustomColumnPreferences` | `bool` | Whether any column visibility, order, pinning or width currently differs from the columns' defaults. |
| `SetColumnVisible(string key, bool visible)` | `void` | Shows or hides a column by key. Does nothing for a `Required` column. |
| `SetAllColumnsVisible(bool visible)` | `void` | Shows every column (`visible` true) or hides every column that is not `Required`; when none is, the first one stays so the table is never empty. |
| `SetColumnPinned(string key, DataListPin pin)` | `void` | Pins a column to the start or end edge, or unpins it. |
| `MoveColumn(string key, int delta)` | `void` | Moves a column earlier (`-1`) or later (`+1`) in the column order. |
| `MoveColumnNextTo(string key, string targetKey, bool after)` | `void` | Puts a column right before or right after another one in the column order: what dropping a dragged column on another does. |
| `SetColumnWidth(string key, int? width)` | `void` | Sets a custom pixel width for a resizable column, or clears it when `null`. |
| `ResetColumns()` | `void` | Restores every column's visibility, order, pinning and width to its declared defaults and clears any saved cookie. |

### `enum DataListApplyMode`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.Filters.cs`

How a filter editor applies what the user changes.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Live` | `enum member` | — |
| `Submit` | `enum member` | — |

### `class DataListContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.Filters.cs`

### `class DataListContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.Selection.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `SelectionChanged` | `event Action?` | Raised when the ticked rows, or what a bulk action targets, change. Cheaper than `Changed` for a part that only draws a checkbox. |
| `SelectionEnabled` | `bool` | The list has a selection: `Selectable` is on, or it has a `KlyDataListSelectionActions{TItem}` bar, or `SelectedKeys` is bound. |
| `SelectionVisible` | `bool` | The selection is drawn: it is enabled and the renderer can run handlers (or will, once a prerendered page turns interactive). A static render has no checkboxes. |
| `SelectionDisabled` | `bool` | The boxes cannot be used yet: the render is the prerender of a page that will be interactive. |
| `SelectedKeys` | `IReadOnlySet<string>` | The keys of the ticked rows (`RowKey` of the schema). A set that later changes do not touch. |
| `SelectedCount` | `int` | How many rows are ticked. |
| `SelectedItems` | `IReadOnlyList<TItem>` | The ticked rows that the list holds: those on screen and, for a list of rows in memory, those ticked on other pages. |
| `IsSelected(TItem item)` | `bool` | Whether a row is ticked. |
| `RowKey(TItem item)` | `string` | The row key of a row, as text. Throws when the schema has no `RowKey`. |
| `IsKeySelected(string key)` | `bool` | Whether a key is ticked. |
| `PageSelection` | `bool?` | The state of the select-all box for the rows on screen: `true` when all are ticked, `false` when none is (or there are no rows), `null` when only some are (the mixed state). |
| `SetSelected(TItem item, bool selected, bool range = false)` | `void` | Ticks or unticks a row. With `range` (a click with Shift held) every row on screen between the row ticked last and this one gets the same state. Nothing happens in a renderer that is not interactive. |
| `SetPageSelected(bool selected)` | `void` | Ticks or unticks the rows on screen. |
| `GetGroupSelection(DataListGroup<TItem> group)` | `bool?` | The selection state of a visual row group: `true` when all rows in `group` are ticked, `false` when none is (or the group is empty), and `null` when only some are ticked (indeterminate). |
| `SetGroupSelected(DataListGroup<TItem> group, bool selected)` | `void` | Ticks or unticks every row in `group`. |
| `ClearSelection()` | `void` | Unticks every row, and goes back to acting on the selection when the bulk scope was every result. |
| `BulkScope` | `DataListActionScope` | What a bulk action targets: the ticked rows, or every result of the query on screen. |
| `MatchingCount` | `long?` | How many rows the search and the filters match, when the source says; `null` when it does not. |
| `CanSelectAllResults` | `bool` | Whether the bulk scope can switch to every result: the source knows how many results there are and there are more than the ticked rows. |
| `SetBulkScope(DataListActionScope scope)` | `void` | Sets what a bulk action targets. `All` needs `CanSelectAllResults`; it is dropped when the rows or the query change. |

### `enum DataListPagingMode`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.cs`

How a data list pages its rows.

### `enum DataListDensity`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.cs`

How tight the rows of a data list are.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Default` | `enum member` | — |
| `Compact` | `enum member` | — |
| `Comfortable` | `enum member` | — |

### `enum DataListAlign`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.cs`

Horizontal alignment of a column.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Start` | `enum member` | — |
| `Center` | `enum member` | — |
| `End` | `enum member` | — |

### `record DataListSortOption`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.cs`

An option offered by `KlyDataListSortMenu{TItem}`. The key of the sortable field in the schema. The display label; when `null`, uses the title of the matching column or the humanized key.

### `class DataListContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListContext.cs`

What the parts of a `KlyDataList{TItem}` (the table, the pagination, the result count...) read and change: the state of the list, its rows and its loading flags. The list cascades one for its whole life (the value is fixed), so a part subscribes to `Changed` once and renders again when it is raised. Write your own part by taking it as a cascading parameter. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `EffectiveLayout` | `DataListLayout` | The layout currently shown: `Layout` when both table and grid exist, otherwise the one that is present. |
| `HasRowSummary` | `bool` | Whether the table has a `DataListRowSummary{TItem}` configured. |
| `SummaryViewActive` | `bool` | Whether the table currently renders compact summary rows instead of the multi-column table. |
| `SetSummaryView(bool active)` | `void` | Switches between compact summary rows (`true`) and the full table (`false`), persisting the choice when `PreferencesKey` is set. |
| `ActiveGroupBy` | `DataListGroupBy<TItem>?` | The active grouping configuration for the current layout (`GroupBy` or `GroupBy`), or `null`. |
| `Groups()` | `IReadOnlyList<DataListGroup<TItem>>` | Builds the visual groups of `Rows` for the active layout when `ActiveGroupBy` is configured; empty otherwise. |
| `IsGroupCollapsed(DataListGroup<TItem> group)` | `bool` | Whether `group` is currently collapsed (folded). |
| `ToggleGroup(DataListGroup<TItem> group)` | `void` | Folds or unfolds `group`, persisting the toggle when `PreferencesKey` is set. |
| `SetGroupCollapsed(DataListGroup<TItem> group, bool collapsed)` | `void` | Sets whether `group` is collapsed (folded), persisting the toggle when `PreferencesKey` is set. |
| `Tree` | `DataListTree<TItem>?` | The active hierarchical tree configuration on `KlyDataListTable{TItem}`, or `null`. |
| `HasTree` | `bool` | Whether the table has a `DataListTree{TItem}` configured. |
| `VisibleTreeNodes()` | `IReadOnlyList<DataListTreeNode<TItem>>` | The visible (unfolded) tree nodes in display order when `Tree` is configured; empty otherwise. |
| `IsNodeExpanded(DataListTreeNode<TItem> node)` | `bool` | Whether `node` is currently expanded. |
| `IsRowExpanded(string key, int depth = 0)` | `bool` | Whether the tree row with `key` at `depth` is currently expanded. |
| `ToggleNodeExpanded(DataListTreeNode<TItem> node)` | `void` | Expands or collapses `node`. |
| `SetNodeExpanded(DataListTreeNode<TItem> node, bool expanded)` | `void` | Sets whether `node` is expanded. |
| `SetRowExpanded(string key, bool expanded, int depth = 0)` | `void` | Sets whether the tree row with `key` is expanded. |
| `ExpandAll()` | `void` | Expands every branch node in the tree. |
| `CollapseAll()` | `void` | Collapses every branch node in the tree. |
| `ResetExpansion()` | `void` | Resets tree expansion back to the `DataListTree{TItem}` defaults. |
| `ExpandedKeys` | `IReadOnlySet<string>` | Keys of all currently expanded branch nodes in the loaded tree. |
| `TreeBranchCount` | `int` | Total number of branch nodes (nodes with children) in the loaded tree. |
| `AllExpanded` | `bool` | Whether every branch node in the loaded tree is currently expanded. |
| `Columns` | `IReadOnlyList<KlyDataListColumn<TItem>>` | Every column the list's table declares, visible or not (a columns panel lists them all), by `Order` and then in the order they first rendered: the order of the markup, except for a column that an `@if` adds later, which comes after the others unless it has an `Order`. Prefer `Visible` to an `@if`. |
| `Label` | `string?` | The name of the list, for the accessible names of its table and its pagination (`Label` of the list). |
| `ResultVersion` | `int` | Counts the results shown so far; it does not change when only a flag does, which lets rows skip a render. |
| `Schema` | `DataListSchema<TItem>?` | The schema, once the list has one (an explicit one, or the one its columns describe after they registered). |
| `State` | `DataListState` | The state the list shows: search, filters, sort, page, page size. Immutable; change it with `SetState`. |
| `Result` | `DataListResult<TItem>?` | The result on screen: the last one that loaded, kept while the next one loads. `null` before the first load. |
| `Rows` | `IReadOnlyList<TItem>` | The rows of `Result`; empty before the first load. |
| `FilteredRows()` | `IReadOnlyList<TItem>` | Every row matching the active search and filters for an in-memory list (`Items`), or the loaded `Rows` for a provider or queryable source. |
| `AllResultsRequest` | `DataListRequest?` | The request for every row the list matches (the loaded request without paging or facets), or `null` before the first load. |
| `LoadedState` | `DataListState?` | The state `Result` was loaded for: the page and page size of the rows on screen. |
| `IsInitial` | `bool` | No result yet and nothing failed: show placeholders. |
| `IsLoading` | `bool` | A load is running. |
| `IsRefreshing` | `bool` | The rows on screen are being loaded again (refresh or retry). |
| `IsReplacing` | `bool` | Other rows are being loaded (a new page, sort or filter) while the previous ones stay on screen. |
| `IsLoadingMore` | `bool` | The next page of a cursor-paged list is being loaded to append to the rows on screen. |
| `HasMore` | `bool` | Whether the source has another page after the rows loaded so far (cursor paging only). |
| `NextCursor` | `string?` | The opaque cursor for the next page (cursor paging only), or `null` after the last one. |

### `class DataListController<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListController.cs`

Loads the rows of a data list: turns a `DataListState` into a request, runs it against the source, cancels the request a newer one replaces and keeps the rows of the previous result while the next one loads. A plain class, so it is tested without a renderer; the `KlyDataList{TItem}` component owns one and publishes its flags to the parts through `DataListContext{TItem}`. Exactly one source feeds a list: `Items` (run in memory by `InMemoryDataList`), a `Queryable` (a database query, translated by `QueryableDataList` and run by an `IDataListQueryExecutor`) or a `DataListProvider{T}`. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Schema` | `DataListSchema<TItem>` | The schema of the rows. |
| `Provider` | `DataListProvider<TItem>` | Where the rows come from. Replace it when the source of the list changes; the next load uses it. |
| `TimeZone` | `string?` | The IANA id of the time zone of the user (`Europe/Paris`), sent with each request (`TimeZone`). `null` means UTC. |
| `PagingMode` | `DataListPagingMode` | How requests page: numbered pages (`Offset`), keyset cursors (`Cursor`) or all matching rows (`None`). |
| `Result` | `DataListResult<TItem>?` | The last result that loaded, kept while the next one loads. `null` until the first one. |
| `LoadedState` | `DataListState?` | The state the last result was loaded for: the requested one with the page and page size brought into range. |
| `LoadedRequest` | `DataListRequest?` | The request the last result was loaded with. |
| `RequestedState` | `DataListState?` | The state of the newest load, finished or not. |
| `Error` | `Exception?` | Why the newest load failed; `null` when it did not. The rows of the previous result stay in `Rows`. |
| `ResultVersion` | `int` | Counts the results shown so far: it changes when a load or an adoption replaces `Result`, not when only a flag changes. |
| `Rows` | `IReadOnlyList<TItem>` | The rows of `Result`, empty before the first load. |
| `IsLoading` | `bool` | A load is running. |
| `IsInitial` | `bool` | No result yet and nothing failed: the first load is running or has not started. A table shows placeholder rows. |
| `IsRefreshing` | `bool` | A load of the request that is already on screen is running (`RefreshAsync`). |
| `IsReplacing` | `bool` | A load of another request than the one on screen is running, with the previous rows still shown. |
| `IsLoadingMore` | `bool` | The next page of a cursor-paged list is being loaded (`LoadMoreAsync`) to append to `Rows`. |
| `LoadMoreError` | `Exception?` | Why the newest `LoadMoreAsync` failed; `null` when it did not. The previously loaded rows and `NextCursor` stay intact so retrying works. |
| `HasLoadMoreError` | `bool` | The newest `LoadMoreAsync` failed. |
| `NextCursor` | `string?` | The opaque cursor of the next page (cursor paging only), or `null` after the last one. |
| `HasMore` | `bool` | Whether there is another page to append in cursor mode. |
| `HasError` | `bool` | The newest load failed. |
| `PagesLoaded` | `int` | How many cursor pages `Rows` holds right now: 1 after a load, one more after each `LoadMoreAsync` that appended a page (cursor paging only). |
| `MaxRefreshPages` | `int` | The most cursor pages a `RefreshAsync` loads again (20 by default, at least 1). A refresh of a cursor-paged list asks for the pages that are on screen one after the other, starting from the first cursor, and replaces the rows with what they return, so the list stays where it was; when more pages than this were loaded, the first `MaxRefreshPages` come back and the list goes on from the cursor after them. This bounds the requests a refresh makes and the memory it holds (the old rows and the new ones for the time it runs). |
| `CreateProvider(
        DataListSchema<TItem> schema,
        IEnumerable<TItem>? items,
        DataListProvider<TItem>? provider,
        IQueryable<TItem>? queryable = null,
        IDataListQueryExecutor? executor = null)` | `DataListProvider<TItem>` | The one source of a list from what the component was given. The schema, used to run `items`. Rows in memory. A provider. None or both were given. A query of a database provider, run through `QueryableDataList` with `executor`. Runs the queries of `queryable`; the synchronous one when `null`. |
| `Normalize(DataListState state)` | `DataListState` | Brings a state into the range the schema allows: a page of at least 1, a page size from 1 to `MaxPageSize` (the default one for zero or less), a search without the spaces around it and no cursor unless the list pages by cursor (a stray `c` in the address of a list with numbered pages, or with no paging, is dropped). |
| `BuildRequest(DataListState state)` | `DataListRequest` | The request for a state: sanitized by the schema, carrying the default sort when the state has none and the time zone. |
| `Adopt(DataListState state, DataListState loaded, DataListRequest request, DataListResult<TItem> result)` | `void` | Sets a result that was loaded elsewhere (the prerender), without calling the provider. The state it was requested for. The state it was loaded for (the requested one with the page brought into range). The request it was loaded with. The result. |
| `LoadAsync(DataListState state, bool refresh = false)` | `Task` | Loads the rows for a state. A load that is still running is cancelled and its result, should it arrive, is ignored. The rows of the previous result stay until this one replaces them; a failure keeps them and sets `Error`. A page past the last one is replaced by the last page when the total is known (read it back as `LoadedState`). The state to load. Whether this is a reload of what is on screen (`IsRefreshing`). |
| `SetLoadedTimeZone(string? timeZone)` | `void` | Sets the time zone of the request an adopted result stands for, once the browser told it (the prerender ran in UTC). |
| `RefreshAsync()` | `Task` | Loads the state of the newest load again: the retry after an error and the refresh button. |
| `UpdateRow(TItem item)` | `bool` | Replaces a single row in the loaded `Result` in place by matching its schema `RowKey`, without querying the provider again. Returns `true` when a matching row was found and replaced. |
| `UpdateRows(IEnumerable<TItem> items)` | `int` | Replaces matching rows in the loaded `Result` in place by their schema `RowKey`, without querying the provider again. Returns the number of rows replaced. |
| `LoadMoreAsync()` | `Task` | Appends the next page of a cursor-paged list using `NextCursor`. Does nothing outside `Cursor`, while a load is running or when there is no next cursor. A newer `LoadAsync` cancels this load and ignores its result; a failure keeps the rows loaded so far and `NextCursor`, sets `LoadMoreError` and lets the caller retry. |
| `Cancel()` | `void` | Cancels the load that is running, if any. Its result is ignored. |
| `Dispose()` | `void` | Cancels the running load. The controller cannot load afterwards. |

### `record DataListEmptyUi`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListEmptyUi.cs`

Per-instance class overrides of `KlyDataListEmpty` slots.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Root` | `string?` | — |
| `Icon` | `string?` | — |
| `Title` | `string?` | — |
| `Description` | `string?` | — |
| `Actions` | `string?` | — |

### `class DataListGroup<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListGroupBy.cs`

One visual group of rows produced by `DataListGroupBy{TItem}` over the loaded rows of a data list. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | Stable identifier of the group (`"~empty"` when the group value is null or blank). |
| `Value` | `object?` | The raw group value (`null` when empty). |
| `IsEmptyValue` | `bool` | Whether the group holds rows whose group field is `null` or blank. |
| `Label` | `string` | The display label of the group header. |
| `Summary` | `string?` | The summary text shown beside the group label (the row count by default). |
| `Items` | `IReadOnlyList<TItem>` | The rows belonging to this group, in the active sort order. |
| `RowKeys` | `IReadOnlyList<string>` | The string keys of `Items` when the schema defines a `RowKey`; empty otherwise. |
| `DefaultCollapsed` | `bool` | Whether the group starts folded before any user toggle. |

### `class DataListGroupBy<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListGroupBy.cs`

Visual grouping configuration for `KlyDataListTable{TItem}` and `KlyDataListGrid{TItem}`. Groups are visual only: filtering, sorting, pagination and actions continue to operate on the flat rows, while rows on screen are partitioned under collapsible `KlyDataListGroupHeader{TItem}` headers with optional per-group selection checkboxes. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Field` | `string?` | Key of the field in the schema to group rows by when `Key` is not provided. |
| `Key` | `Func<TItem, object?>?` | Extracts the group value for a row. When `null`, reads `Field` from the schema. |
| `Order` | `IReadOnlyList<object?>?` | Explicit group values placed first, in this order. Remaining non-empty groups follow in ascending value order, and the empty group (`null` or blank) comes last. |
| `Label` | `Func<DataListGroup<TItem>, string?>?` | Formats the heading label of a group. When `null`, formats `Value` or uses the localized "Other" label (`GroupEmpty`) for empty values. |
| `LabelTemplate` | `RenderFragment<DataListGroup<TItem>>?` | Custom template for the group heading label inside `KlyDataListGroupHeader{TItem}`. |
| `Summary` | `Func<DataListGroup<TItem>, string?>?` | Formats the summary badge shown beside the group label. When `null`, shows the number of rows in the group. Return an empty string to hide the badge. |
| `SummaryTemplate` | `RenderFragment<DataListGroup<TItem>>?` | Custom template for the summary badge shown beside the group label inside `KlyDataListGroupHeader{TItem}`. |
| `Collapsed` | `bool` | Whether groups start collapsed (folded) by default (`false` by default). Overridden by `DefaultCollapsed` when set. |
| `DefaultCollapsed` | `Func<DataListGroup<TItem>, bool>?` | Optional per-group predicate determining whether a group starts collapsed (folded) by default. |
| `GroupKey` | `string` | — |
| `Value` | `object?` | — |
| `IsEmpty` | `bool` | — |
| `DiscoveryIndex` | `int` | — |
| `Items` | `List<TItem>` | — |
| `RowKeys` | `List<string>` | — |

### `class DataListLoadTrigger<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListLoadTrigger.cs`

### `class DataListPartBase<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListPartBase.cs`

Base of the parts of a `KlyDataList{TItem}`: takes the cascaded `DataListContext{TItem}` and renders again when it changes. Derive from it to write a part of your own. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Dispose()` | `void` | Unsubscribes from the list. |

### `enum DataListSkeletonKind`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListPolish.cs`

Placeholder shape drawn inside a column cell while the initial rows of `KlyDataListTable{TItem}` load.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Text` | `enum member` | — |
| `Avatar` | `enum member` | — |
| `Icon` | `enum member` | — |
| `Dot` | `enum member` | — |
| `Check` | `enum member` | — |
| `Badge` | `enum member` | — |
| `Number` | `enum member` | — |
| `Progress` | `enum member` | — |
| `None` | `enum member` | — |

### `class DataListEmptyAction`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListPolish.cs`

First-use action button or link displayed inside the unfiltered empty state of `KlyDataListTable{TItem}` or `KlyDataListGrid{TItem}` when `Action` is configured. Never shown on a filtered empty state.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string` | Label of the action button or link. |
| `Icon` | `string?` | Optional leading icon name. |
| `Href` | `string?` | Optional navigation link URL. When set, renders a link button (works in static SSR too). |
| `OnClick` | `EventCallback` | Callback invoked when the button is clicked in an interactive render. |
| `Color` | `KlyColor` | Button color (`Primary` by default). |
| `Variant` | `ButtonVariant` | Button visual variant (`Solid` by default). |

### `class DataListEmptyState`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListPolish.cs`

Declarative configuration for the unfiltered empty state of `KlyDataListTable{TItem}` or `KlyDataListGrid{TItem}`. When search or filters are active and match nothing, the filtered empty state (with Clear search / Reset filters) is still shown instead, and `Action` is omitted so a first-use creation action is never offered while results are merely filtered out.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Icon` | `string?` | Optional icon name overriding the default empty-state icon (or empty string to hide the icon). |
| `Title` | `string?` | Optional title overriding the localized `NoResults`. |
| `Description` | `string?` | Optional description overriding the localized `NoResultsDescription`. |
| `HideDescription` | `bool` | When `true`, hides the description line on the unfiltered empty state. |
| `Action` | `DataListEmptyAction?` | Optional first-use call-to-action shown only when the list is empty without active search or filters. |

### `class DataListRowActionContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowActions.cs`

What a row action receives when it runs: the row, its key and place, and a way to report a failure without throwing. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `List` | `DataListContext<TItem>` | The list, to read its state or to load it again (`RefreshAsync`). |
| `Item` | `TItem` | The row. |
| `Key` | `string` | The row key (`RowKey` of the schema, else the identity of the row). |
| `Position` | `int` | The place of the row on the page, from 1; 0 when the part was placed outside a table row. |
| `CancellationToken` | `CancellationToken` | Cancelled when the list goes away. |
| `FailureMessage` | `string?` | The message given to `Fail`. |
| `Failed` | `bool` | Whether `Fail` was called. |
| `Fail(string? message = null)` | `void` | Reports that the action did not do what it was asked (a refusal of the server, a conflict) without throwing: the list is not refreshed and `OnError` of the part is raised. Call it, then return. What went wrong, for `OnError`. |

### `class DataListRowActionConfirm<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowActions.cs`

The question a row action asks before it runs, in a dialog. The text can name the row. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The title of the dialog. |
| `Description` | `Func<DataListRowActionContext<TItem>, string?>?` | The text under the title, built from the context (`c =&gt; $"{c.Item.Name} will be removed."`). |
| `ConfirmLabel` | `string?` | The label of the confirm button. Without it the localized "Confirm". |
| `Color` | `KlyColor?` | The color of the confirm button; `Error` for a destructive action. Without it the color of the action. |

### `class DataListRowAction<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowActions.cs`

An action on one row of a data list (`KlyDataListRowActions{TItem}`): a button of the row, an entry of its menu or of its sheet on a phone. Actions with the same `Group` are kept together, separated from the others by a divider; an action with `Children` opens a submenu. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | Identifies the action; unique among the actions of the part, children included. |
| `Label` | `string` | The text of the button or of the menu entry. |
| `Icon` | `string?` | The name of an icon shown before the label (`copy`), one of the icons of Klyroo.UI. |
| `Color` | `KlyColor` | The color; `Error` for a destructive action. |
| `Group` | `string?` | The group the action belongs to. Groups are shown in the order of their first visible action, with a divider between them; actions without a group form one of their own. |
| `Children` | `IReadOnlyList<DataListRowAction<TItem>>?` | The entries of a submenu. An action with children opens them instead of running, and only shows when one of them does. It always sits in the menu, never in a button of the row. |
| `Href` | `Func<DataListRowActionContext<TItem>, string?>?` | Makes the action a link to the address it returns (`null`: no link for this row). A link works in a statically rendered list too, where it is the only kind of row action that is shown. `OnExecute` is not called for it. |
| `Visible` | `Func<DataListRowActionContext<TItem>, bool>?` | Hides the action when it returns `false` (an action that does not apply to this row); the children of a hidden action are hidden with it. It runs on every render: keep it cheap. |
| `Disabled` | `Func<DataListRowActionContext<TItem>, bool>?` | Disables the action when it returns `true`; the children of a disabled action cannot run either. It runs on every render: keep it cheap. |
| `DisabledReason` | `Func<DataListRowActionContext<TItem>, string?>?` | Why the action is disabled: shown under its label in the sheet of a phone, and on a wide screen as the title (the tooltip) and the accessible description of its button or menu entry. The reason of an action with `Children` is the one its children show while it is disabled. |
| `Loading` | `Func<DataListRowActionContext<TItem>, bool>?` | Shows a spinner and blocks the action while it returns `true` (work started elsewhere). The action also shows one while it runs. |
| `Confirm` | `DataListRowActionConfirm<TItem>?` | Asks before the action runs. Without it the action runs on the click. |
| `RefreshOnSuccess` | `bool` | Loads the list again once the action succeeded (the row it changed or removed). Off by default; an action can also call `RefreshAsync` itself. |
| `OnExecute` | `Func<DataListRowActionContext<TItem>, Task>?` | What the action does. An exception, or a call to `Fail`, marks the run as failed: the list is not refreshed, `OnError` of the part is raised and nothing is thrown to the page. |

### `record DataListRowActionFailure<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowActions.cs`

A row action that failed: it threw, or called `Fail`. The action. The row. What it threw; `null` when it reported the failure itself. What `Fail` was given. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `IsRunning(string row, string action)` | `bool` | — |
| `StateOf(string row, DataListRowAction<TItem> action, DataListRowActionContext<TItem> context, IReadOnlyList<DataListRowAction<TItem>>? ancestors = null)` | `RowActionState` | What an action shows for a row. `ancestors` are the actions it sits under, outermost first: a hidden ancestor hides it and a disabled one disables it (with the reason of the ancestor), so a child of a disabled or hidden action never runs and is never offered as runnable. |
| `Locate(IEnumerable<DataListRowAction<TItem>> roots, string? key)` | `(DataListRowAction<TItem> Action, IReadOnlyList<DataListRowAction<TItem>> Ancestors)?` | The action with this key among `roots` and the actions it sits under (outermost first), or `null`. |
| `RunAsync(string row, DataListRowAction<TItem> action, DataListRowActionContext<TItem> context, Action changed, Func<Exception?, string?, Task> onFailure, CancellationToken lifetime,
        IReadOnlyList<DataListRowAction<TItem>>? ancestors = null)` | `Task<RowActionOutcome>` | Runs an action. `changed` is called when it starts and when it ends, so that the row shows its spinner. |
| `Group(IEnumerable<DataListRowAction<TItem>> visible)` | `List<List<DataListRowAction<TItem>>>` | The visible actions grouped for a menu: groups in the order of their first visible action, actions without a group together. |

### `class DataListRowView<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListRowView.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Equals(object? obj)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `enum DataListSummaryKind`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSummaries.cs`

Built-in column aggregation kinds for the summary footer (`&lt;tfoot&gt;`) of `KlyDataListTable{TItem}`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `None` | `enum member` | — |
| `Sum` | `enum member` | — |
| `Average` | `enum member` | — |
| `Count` | `enum member` | — |
| `Min` | `enum member` | — |
| `Max` | `enum member` | — |

### `enum DataListSummaryScope`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSummaries.cs`

Which set of rows a summary footer row aggregates.

### `record DataListSummaryCellContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSummaries.cs`

Context passed to `SummaryTemplate` when rendering a summary footer cell. The row type.

### `record DataListSummaryResolveContext<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSummaries.cs`

Context passed to `ResolveAsync` when resolving summary values asynchronously. The row type.

### `class DataListSummaries<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListSummaries.cs`

Footer summary configuration (`&lt;tfoot&gt;`) for `KlyDataListTable{TItem}`. When omitted on `KlyDataListTable{TItem}`, a single summary row with `Filtered` is still rendered automatically whenever at least one `KlyDataListColumn{TItem}` sets `Summary`, `SummaryValue` or `SummaryTemplate`. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Scope` | `DataListSummaryScope` | The default scope aggregated when `Scopes` is not set (`Filtered` by default). |
| `Scopes` | `IReadOnlyList<DataListSummaryScope>?` | Optional list of scopes to render as separate summary rows in `&lt;tfoot&gt;` (for example `[DataListSummaryScope.Page, DataListSummaryScope.Filtered]`). When `null` or empty, renders one row for `Scope`. |
| `Label` | `string?` | Label of the summary row (defaults to the localized `SummaryTotal` "Total" when a single scope is shown, or the localized scope name when multiple `Scopes` are shown). |
| `ResolveAsync` | `Func<DataListSummaryResolveContext<TItem>, ValueTask<IReadOnlyDictionary<string, object?>>>?` | Optional asynchronous resolver returning a map of column key to aggregated value for a scope (for remote sources or custom async totals). Columns not present in the returned dictionary fall back to their declared `Summary` or `SummaryValue`. |
| `Compute<TItem>(
        DataListSummaryKind kind,
        IReadOnlyList<TItem> rows,
        Func<TItem, object?> selector)` | `object?` | — |

### `record DataListUi`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/DataListUi.cs`

Per-instance class overrides of the slots of the data list parts (`KlyDataList{TItem}`, `KlyDataListTable{TItem}`, `KlyDataListPagination{TItem}`...).

| Member | Type | Description |
| :--- | :--- | :--- |
| `Root` | `string?` | — |
| `Toolbar` | `string?` | — |
| `Search` | `string?` | — |
| `SearchField` | `string?` | — |
| `SearchInput` | `string?` | — |
| `SearchClear` | `string?` | — |
| `SearchClearIcon` | `string?` | — |
| `SearchSubmit` | `string?` | — |
| `ResultCount` | `string?` | — |
| `Refresh` | `string?` | — |
| `RefreshIcon` | `string?` | — |
| `TableRoot` | `string?` | — |
| `Table` | `string?` | — |
| `Caption` | `string?` | — |
| `Thead` | `string?` | — |
| `Tr` | `string?` | — |
| `Th` | `string?` | — |
| `SortButton` | `string?` | — |
| `SortIcon` | `string?` | — |
| `Tbody` | `string?` | — |
| `Td` | `string?` | — |
| `Empty` | `string?` | — |
| `Skeleton` | `string?` | — |
| `Pagination` | `string?` | — |
| `PageSize` | `string?` | — |
| `PageSizeLabel` | `string?` | — |
| `PageSizeSelect` | `string?` | — |
| `PageSizeForm` | `string?` | — |
| `Error` | `string?` | — |
| `Filters` | `string?` | — |
| `FilterTags` | `string?` | — |
| `FilterTag` | `string?` | — |
| `FilterTagButton` | `string?` | — |
| `FilterTagLabel` | `string?` | — |
| `FilterTagOperator` | `string?` | — |
| `FilterTagValue` | `string?` | — |
| `FilterTagRemove` | `string?` | — |
| `FilterTagRemoveIcon` | `string?` | — |
| `AddFilter` | `string?` | — |
| `ClearFilters` | `string?` | — |

### `interface IDataListQueryExecutor`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/IDataListQueryExecutor.cs`

Runs the queries a data list builds for a `Queryable` source (`Apply{T}`): the count of the matching rows and the page. The list asks the service container for one, so an adapter package replaces how the queries run (EF Core's `CountAsync` and `ToListAsync`, with the cancellation token of the request) without the list referencing the provider. `SynchronousDataListQueryExecutor` is the default.

### `class SynchronousDataListQueryExecutor`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Components/DataList/IDataListQueryExecutor.cs`

The default executor: `Count()` and `ToList()`, on the thread that asks. It blocks that thread while the database answers: in Blazor Server that is the thread of the circuit, in WebAssembly the only thread of the page. Use it for tests, small local databases and providers with no asynchronous API; register an asynchronous executor for a real database (`AddKlyrooTableEntityFrameworkAdapter()` of Klyroo.Table.EntityFrameworkCore for EF Core). The cancellation token is checked before each query, not during it.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Instance` | `SynchronousDataListQueryExecutor` | The shared instance. |
| `CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<int>` | — |
| `ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)` | `ValueTask<List<T>>` | — |
| `Create<T>(DataListSchema<T> schema, IQueryable<T> source, IDataListQueryExecutor executor)` | `DataListProvider<T>` | — |

### `record DataListRowSummary<TItem>`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/DataListRowSummary.cs`

Compact summary-row presentation for `KlyDataListTable{TItem}`: one scannable row per record with an optional leading visual, a primary title (optionally a link or button), a status badge/pill, secondary details joined with ` · `, selection checkbox and row actions. Shown automatically below the `md` breakpoint when `AutoPhone` is `true`, when `DefaultActive` is `true`, or when toggled with `KlyDataListViewToggle{TItem}`. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Title` | `Func<TItem, string?>?` | Primary one-line title of the row. When `null`, uses `Schema.GetRowLabel(item)` or the first visible column's formatted value. |
| `Leading` | `RenderFragment<TItem>?` | Optional leading content (a 32 px tile, icon or avatar) shown before the title. |
| `Status` | `RenderFragment<TItem>?` | Optional status content (a badge or pill) shown at the start of the second line. |
| `Details` | `Func<TItem, IEnumerable<string?>>?` | Secondary detail texts joined with ` · ` on the second line (blank entries are skipped). When `null`, uses the remaining visible columns. |
| `Href` | `Func<TItem, string?>?` | Optional link destination (`href`) for the title of the row. |
| `OnSelect` | `EventCallback<TItem>` | Optional click callback when the summary row's title button is activated (interactive only, when `Href` is `null`). |
| `Template` | `RenderFragment<TItem>?` | Optional custom template for the body of the summary row, replacing `Title`, `Leading`, `Status` and `Details`. |
| `DefaultActive` | `bool` | Starts in summary-row view on every screen size until the user toggles `KlyDataListViewToggle{TItem}` (`false` by default). |
| `AutoPhone` | `bool` | Automatically switches to summary-row view below the `md` breakpoint (768 px) when the user has not explicitly chosen a view (`true` by default). |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/KlyToolsLocales.cs`

The message tables of Klyroo.Table, one per locale of `All` and keyed by the same codes. A component picks the table of the current `Code` with `ResolveCode`, so "pt-PT" gets "pt" and an unknown language gets English. Change a table with `Messages`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Tables` | `IReadOnlyDictionary<string, KlyToolsMessages>` | Every table by locale code: the codes of `All`. |
| `ReviewedCodes` | `IReadOnlyList<string>` | The codes whose texts were reviewed by a native speaker. The other tables are drafts waiting for review. |

### `record KlyToolsMessages`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/KlyToolsMessages.DataList.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `NoResults` | `string` | Title of the empty state of a data list. |
| `NoResultsDescription` | `string` | Hint under the title of the empty state of a data list. |
| `SortAscending` | `string` | Accessible name of a sort button that sorts a column from the smallest to the largest. {0} is the title of the column. |
| `SortDescending` | `string` | Accessible name of a sort button that sorts a column from the largest to the smallest. {0} is the title of the column. |
| `SortClear` | `string` | Accessible name of a sort button that removes the sort of a column. {0} is the title of the column. |
| `Sort` | `string` | Label of the sort menu trigger button when no sort is active, and heading of the sort popover. |
| `SortBy` | `string` | Label of the sort menu trigger button when sorted by {0}, and accessible name of a sort field option. |
| `SortDirectionAscending` | `string` | Ascending direction option in the sort menu. |
| `SortDirectionDescending` | `string` | Descending direction option in the sort menu. |
| `SortDefault` | `string` | Button in the sort menu that restores the default sort order. |
| `LayoutSwitch` | `string` | Accessible name of the layout switch group. |
| `TableView` | `string` | Accessible name and label of the table layout option. |
| `GridView` | `string` | Accessible name and label of the grid layout option. |
| `SummaryView` | `string` | Accessible name and label of the compact summary-rows toggle button. |
| `GroupEmpty` | `string` | Heading of the group that holds rows whose group field is null or empty. |
| `GroupSelect` | `string` | Accessible name of the checkbox in a group header that selects or unselects every row in the group. {0} is the group label. |
| `GroupToggle` | `string` | Accessible name of the button in a group header that folds or unfolds the group. {0} is the group label. |
| `TreeExpand` | `string` | Accessible name of the chevron button that expands a collapsed tree row. {0} is the row label or position. |
| `TreeCollapse` | `string` | Accessible name of the chevron button that collapses an expanded tree row. {0} is the row label or position. |
| `TreeExpandAll` | `string` | Label of an action that expands every branch row of a tree table. |
| `TreeCollapseAll` | `string` | Label of an action that collapses every branch row of a tree table. |
| `SummaryTotal` | `string` | Default label of the summary footer row (`&lt;tfoot&gt;`) of a table. |
| `SummaryFiltered` | `string` | Label of a summary footer row scoped to all filtered rows. |
| `SummaryPage` | `string` | Label of a summary footer row scoped to the current page. |
| `SummarySelection` | `string` | Label of a summary footer row scoped to the selected rows. |
| `SummaryRowOne` | `string` | Formatted count of a single row in a summary footer cell. {0} is 1. |
| `SummaryRowOther` | `string` | Formatted count of multiple rows in a summary footer cell. {0} is the row count. |
| `ColumnMenuOpen` | `string` | Accessible name of the column header menu button. {0} is the column title. |
| `ColumnMenuSortAsc` | `string` | Item in the column header menu that sorts ascending. |
| `ColumnMenuSortDesc` | `string` | Item in the column header menu that sorts descending. |
| `ColumnMenuClearSort` | `string` | Item in the column header menu that clears sorting on the column. |
| `ColumnMenuHide` | `string` | Item in the column header menu that hides the column. |
| `ResultRange` | `string` | The rows shown when the total is not known. {0} is the first row number, {1} the last. |
| `ResultCount` | `string` | The rows shown out of the total, announced when they change. {0} is the first row number, {1} the last, {2} the total. |
| `ResultCountLoaded` | `string` | Rows on a cursor page out of the total, when the start offset is not known (a static link). {0} is the row count of the page, {1} the total. |
| `ResultCountShown` | `string` | Rows on a cursor page when neither the start offset nor the total is known. {0} is the row count of the page. |
| `LoadMoreError` | `string` | Error shown inline by the infinite loader when fetching the next cursor page failed. |
| `BackToStart` | `string` | Link shown by a statically rendered cursor-paged list to go back to the first page. |
| `Columns` | `string` | Label of the column configurator button and heading. |
| `ColumnsVisible` | `string` | Visible column count in the column panel. {0} is visible count, {1} is total count. |

### `record KlyToolsMessages`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/KlyToolsMessages.Filters.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `FilterTagsLabel` | `string` | Accessible name of the list of filter tags. {0} is the number of active filters, so a screen reader announces the count. |
| `AddFilter` | `string` | Label of the button that opens the menu of filters to add. |
| `AddFilterSearch` | `string` | Label and placeholder of the field that narrows the menu of filters to add (shown when the list has many). |
| `AddFilterNone` | `string` | Said in the menu of filters to add when every filter is already in use. |
| `AddFilterBack` | `string` | Label of the button of an editor opened from the add filter menu that goes back to the list of filters. |
| `ClearFilters` | `string` | Label of the button that takes every filter off. |
| `RemoveFilter` | `string` | Accessible name of the button of a filter tag that removes the filter. {0} is the name of the filter. |
| `EditFilter` | `string` | Accessible name of the editor panel of a filter. {0} is the name of the filter. |
| `FilterOperatorFor` | `string` | Accessible name of the operator picker of a filter editor. {0} is the name of the filter. |
| `FilterValueFor` | `string` | Accessible name of the value field of a filter editor. {0} is the name of the filter. |
| `FilterTextPlaceholder` | `string` | Placeholder of the value field of a text filter. |
| `FilterOptionsSearch` | `string` | Label and placeholder of the field that narrows the options of an option filter. |
| `FilterNoOptions` | `string` | Said in the editor of an option filter that has no option to list. |
| `FilterNoOptionsMatch` | `string` | Said in the editor of an option filter when the search matches no option. |
| `FilterOptionsMore` | `string` | Label of the button that lists the options an option filter kept back. {0} is how many more there are. |
| `FilterOptionsNeedScript` | `string` | Said in a statically rendered editor of an option filter whose options come from a remote source: the options that are listed are the selected ones and the counted ones, searching the rest runs in the browser. |
| `FilterIncludeEmpty` | `string` | Label of the switch that also matches the rows where the field is empty. |
| `FilterEmpty` | `string` | The value of a filter tag that matches only empty rows, after the operator: "Status is empty". |
| `FilterOrEmpty` | `string` | Added to the values of a filter tag that also matches empty rows: "Status is Active or empty". |
| `FilterYes` | `string` | The "true" choice of a boolean filter. |
| `FilterNo` | `string` | The "false" choice of a boolean filter. |
| `FilterAny` | `string` | The choice of a boolean filter that takes the filter off. |
| `FilterApply` | `string` | Button of a filter editor that applies the changes (the Submit apply mode). |
| `FilterCancel` | `string` | Button of a filter editor that drops the changes (the Submit apply mode). |
| `FilterPanelTitle` | `string` | Title of the filter panel, and its accessible name. |
| `FilterPanelActive` | `string` | Said to screen readers after the name of a filter panel section (or of the panel) that has filters on. {0} is how many are active. |
| `FilterPanelResults` | `string` | The number of rows that match the filters, in the filter panel (a live region). {0} is the count. Written so that it reads right for one row too. |
| `FilterPanelNone` | `string` | Said in the filter panel when the list has no filter to edit. |
| `FilterReset` | `string` | Button of the filter panel (the Submit apply mode) that drops the changes that were not applied. |
| `FilterMore` | `string` | Said after the values a filter tag shows when it has more. {0} is how many more. |
| `OperatorContains` | `string` | The `contains` operator of a text filter: "Name contains ann". |
| `OperatorIs` | `string` | The `is` operator. Also what a filter tag says for `isAnyOf`: "Status is Active, Paused". |
| `OperatorIsAnyOf` | `string` | The `isAnyOf` operator in the operator picker of an option filter. |
| `OperatorIsNot` | `string` | The `isNot` operator. |
| `OperatorGt` | `string` | The `gt` operator of a number filter. |
| `OperatorGte` | `string` | The `gte` operator of a number filter. |
| `OperatorLt` | `string` | The `lt` operator of a number filter. |
| `OperatorLte` | `string` | The `lte` operator of a number filter. |
| `OperatorBetween` | `string` | The `between` operator of a number or date filter. |
| `OperatorBefore` | `string` | The `before` operator of a date filter. |

### `record KlyToolsMessages`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/KlyToolsMessages.Selection.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `SelectAllRows` | `string` | Accessible name of the checkbox in the header of a data list that selects the rows of the page. |
| `SelectRow` | `string` | Accessible name of the checkbox of a row that has a label. {0} is the label of the row (`RowLabel` of the schema). |
| `SelectRowAt` | `string` | Accessible name of the checkbox of a row that has no label. {0} is the position of the row on the page, from 1. |
| `SelectionCount` | `string` | How many rows are selected, announced when it changes. {0} is the count. |
| `SelectionAllResults` | `string` | Said instead of the count when the actions apply to every result. {0} is how many results match. |
| `SelectionSelectAllResults` | `string` | Button of the selection bar that makes the actions apply to every result. {0} is how many results match. |
| `SelectionOnlySelected` | `string` | Button of the selection bar that goes back from every result to the rows that were ticked. {0} is how many. |
| `SelectionClear` | `string` | Button of the selection bar that unticks every row. |
| `SelectionActionsLabel` | `string` | Accessible name of the selection bar, the group of the actions on the selected rows. |
| `ActionsMenu` | `string` | Label of the button that opens the menu of the toolbar actions of a data list. |
| `ActionsMore` | `string` | Label of the button of the selection bar that opens the actions that did not fit. |
| `ActionConfirm` | `string` | Confirm button of the dialog an action asks before it runs, when the action gives no label. |
| `ActionCancel` | `string` | Cancel button of the dialog an action asks before it runs. |
| `RowActionsLabel` | `string` | Accessible name of the button that opens the actions of one row of a data list; {0} is the label of the row. |
| `RowActionsLabelAt` | `string` | Accessible name of the button that opens the actions of a row that has no label; {0} is its position on the page. |

### `record KlyToolsMessages`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/KlyToolsMessages.cs`

Every text the components of Klyroo.Table render: accessible names, labels, hints. One set per locale of Klyroo.UI (`KlyToolsLocales`); build a variant with a `with` expression and register it with `Messages`. Partial: each family of components declares its messages in its own file. A message with placeholders ({0}, {1}) is formatted by the component with `Format(string, object?[])`; keep the same placeholders when you translate it.

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Ar.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Ar` | `KlyToolsMessages` | العربية (ar). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/De.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `De` | `KlyToolsMessages` | Deutsch (de). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/En.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `En` | `KlyToolsMessages` | English. Reviewed. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Es.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Es` | `KlyToolsMessages` | Español (es). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Fr.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Fr` | `KlyToolsMessages` | Français. Reviewed. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/It.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `It` | `KlyToolsMessages` | Italiano (it). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Ja.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Ja` | `KlyToolsMessages` | 日本語 (ja). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Nl.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Nl` | `KlyToolsMessages` | Nederlands (nl). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Pt.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Pt` | `KlyToolsMessages` | Português (pt). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/PtBr.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `PtBr` | `KlyToolsMessages` | Português (Brasil) (pt-BR). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/Ru.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Ru` | `KlyToolsMessages` | Русский (ru). Needs review. |

### `class KlyToolsLocales`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Localization/Locales/ZhCn.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `ZhCn` | `KlyToolsMessages` | 简体中文 (zh-Hans). Needs review. |

### `class KlyrooTableServiceCollectionExtensions`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/KlyrooTableServiceCollectionExtensions.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `AddKlyrooTable(this IServiceCollection services, Action<KlyrooTableOptions>? configure = null)` | `IServiceCollection` | — |

### `class KlyrooTableOptions`
*Package*: `Klyroo.Table` · *Source*: `src/Klyroo.Table/Theming/KlyrooTableOptions.DataList.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `DataListEmpty` | `ComponentTheme` | The empty state of a data list. |
| `DataList` | `ComponentTheme` | The data list and its parts: toolbar, result count, refresh button, table, pagination and error state. One theme for all of them, so a look is changed in one place. |
| `SearchDebounce` | `int` | Milliseconds of quiet after the last keystroke before the search field of a data list searches (300 by default), unless the field sets its own `Debounce`. Enter searches at once. Set it in `AddKlyrooTable`. The value is negative. |
| `PersistMaxBytes` | `int` | The most bytes (state, loaded state and result with its facets, as JSON) one data list hands from its prerender to the interactive render (16 KB by default). A bigger result is not persisted: the interactive render loads it again once, and a warning names the list and the size. The limit exists because the client sends everything persisted to the server when a Server circuit starts, and SignalR refuses messages past 32 KB. Set it in `AddKlyrooTable`. The value is negative. |
| `PersistPageMaxBytes` | `int` | The most bytes all the data lists of one prerendered page hand over together (8 KB by default: what the browser sends is about 3.5 times bigger once escaped, encrypted and encoded, next to the rest of the page state, and SignalR refuses 32 KB): lists past it, in the order they persist, are not persisted. The value is negative. |
