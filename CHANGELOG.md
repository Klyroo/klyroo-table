# Changelog

All notable changes to this project are documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Remembered filters: `KlyDataList` now honors `DataListFilter.WithPersist()`. A change of such a filter is stored in `localStorage` (key scoped by row type, `PersistKey` or `UrlKey` and filter key, value in the URL form of `DataListStateQuery`) through the new `klyroo-table-storage.js` module; clearing the filter removes the entry, and the first interactive render restores the value of every marked filter the address does not name, then writes it to the address. Static renders and prerenders never touch storage, and every storage call is guarded.
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
