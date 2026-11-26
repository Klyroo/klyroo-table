# Changelog

All notable changes to this project are documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- The prerender persistence of a data list is counted as sent, with the escaping and the two base64 encodings Blazor adds, and draws on the budget every Klyroo library of the page shares (`KlyrooOptions.PersistedStateMaxBytes`, 28 KB). Before, the lists of a page counted raw JSON against their own 8 KB budget, so a page with a dashboard could pass SignalR's 32 KB message limit and fail to start its circuit. `PersistMaxBytes` now counts sent bytes (24 KB by default), and `PersistPageMaxBytes` is removed. Column preferences handed over by the prerender count in the same budget.

### Added

- Master-detail: `KlyDataListTable.DetailTemplate` draws a full-width detail row under each row, opened by a button in a new first column (`aria-expanded`, `aria-controls`, an accessible name with the row label, ArrowRight and ArrowLeft). It uses the existing row expansion state (`DataListContext.IsRowExpanded`, `SetRowExpanded`, `ExpandAll`, `CollapseAll`, `ExpandedKeys`), which now works without a `Tree`, and `DataListContext.HasDetail` says whether a table has one. Not supported with `Virtualize` or `Tree` (the detail is ignored).
- Multi-column sort in the table: Shift+click on a sortable header appends the column to the sort, cycles it ascending, descending and out while keeping the other sorts, and a plain click keeps one sort as before. Headers show the place of each column (1, 2, 3) while several sorts are active, `aria-sort` stays on the first, and `KlyDataListSortMenu` gains `Multiple` (automatic by default) to edit the list: reverse, move, remove and add. New `DataListContext` methods `NextSort(state, key, append)`, `AddSort`, `RemoveSort`, `ReverseSort`, `MoveSort` and `SortIndex`; `SortDescending(key)` now reads any position of the sort, not only the first.
- Row hooks on `KlyDataListTable`: `RowHref` (an accessible link stretched over the row, also in a static render, with the controls inside the row kept above it), `OnRowClick` (a `DataListRowClickArgs<TItem>` with the row and the modifier keys; Enter and Space activate a focused row, and clicks on controls inside the row or that end a text selection are ignored), `RowClass` and `RowAttributes`, plus `CellClass` on `KlyDataListColumn`. The click filter runs in the browser through two new custom events (`klyrowclick`, `klyrowkey`) that `Klyroo.Table.lib.module.js` registers.
- Remembered filters: `KlyDataList` now honors `DataListFilter.WithPersist()`. A change of such a filter is stored in `localStorage` (key scoped by row type, `PersistKey` or `UrlKey` and filter key, value in the URL form of `DataListStateQuery`) through the new `klyroo-table-storage.js` module; clearing the filter removes the entry, and the first interactive render, like any later navigation to the page whose address does not name a marked filter (a menu link, Back), restores its value, then writes it to the address. A filter cleared in the list stays cleared. Static renders and prerenders never touch storage, and every storage call is guarded.
- Tests for the summary scopes (`Filtered`, `Page`, `Selection`), the default sticky position of the selection bar and `DataListController.PagesLoaded` across appends, refreshes, new loads and failed appends.

## [0.1.0-preview.1] - 2026-11-24

### Added

- `KlyDataList<TItem>` data list that loads rows from in-memory `Items`, a database `Queryable` run by a replaceable `IDataListQueryExecutor`, or an asynchronous `Provider`, and keeps search, filters, sort, page, page size and layout in the query string.
- Composable parts for the list: table, card grid, search, refresh, result count, filter tags, add and clear filter buttons, filter panel, sort menu, layout switch, column panel, error and empty states, numbered pagination and a cursor-based infinite loader.
- Table features: sortable columns with a header menu, pinned and resizable columns with saved preferences, virtualized rows, collapsible row groups, footer summaries (sum, average, count, min, max), compact summary rows for phones and per-column templates.
- Selection with a bulk action bar that can target every result of a query, per-row actions with confirmation dialogs and a toolbar actions menu.
- `DataListTree<TItem>` in `Klyroo.Table.Core`: a UI-free, NativeAOT-compatible tree model that flattens rows carrying their children into nodes with depth and rail information, with expansion and selection settings for the table.
- Prerender hand-over (`ResultJson`) so an interactive list adopts the result of its prerender, and a time zone aware render for date filters in static, prerendered and interactive modes.
- Twelve message tables (English and French reviewed), per-instance and global theming through `DataListUi` and `KlyrooTableOptions`, and `AddKlyrooTable()` registration.

[Unreleased]: https://github.com/klyroo/klyroo-table/compare/v0.1.0-preview.1...HEAD
[0.1.0-preview.1]: https://github.com/klyroo/klyroo-table/releases/tag/v0.1.0-preview.1
