# Klyroo.Table.Maui

The Klyroo data list as native .NET MAUI controls (no WebView, no Blazor), drawn with the design tokens of [Klyroo.UI.Maui](https://github.com/klyroo/klyroo-ui): the same colors, sizes and radius as the web components, in light and dark.

The controls are views over `Klyroo.Table.Core` and `Klyroo.Query.Core`. The root control `KlyDataList<TItem>` owns the state of the list (search, filters, sort, page, layout, selection, columns) and a `DataListController<TItem>` that loads the rows from one source; it hands its `DataListContext<TItem>` to its parts, the same composition as the Blazor `KlyDataList`. A part finds the list above it in the visual tree (or takes it from its `List` property when it is shown in an overlay), subscribes once and draws again when the list changes. The schema, the query model, the filters, the paging, the CSV writer and the 12 message tables are the ones of the Blazor package: nothing of the logic is written twice.

## Controls

| Control | Built on | Notes |
| :--- | :--- | :--- |
| `KlyDataList<TItem>` | `ContentView` | `Schema`, one source (`Items`, `Queryable` + `Executor`, or `Provider`), two-way `State`, `PagingMode`, `Density`, `Label`, `Messages`, `Selectable`, `SelectionScope`, two-way `SelectedKeys`. Without content it builds the default layout (search, count, refresh, tags, error, table, paging). |
| `KlyDataListTable<TItem>` | `ScrollView`, `CollectionView` | Header with sort, selection column, row actions, groups, placeholders, empty state. Below 640 points it is a list of cards; a long page is virtualized. |
| `KlyDataListGrid<TItem>` | `Grid`, `CollectionView` | Cards in 1 to 4 columns, `ItemTemplate`, selection, row actions, groups. |
| `KlyDataListColumn<TItem, TValue>`, `KlyDataListFieldColumn<TItem>`, `KlyDataListTemplateColumn<TItem>` | `BindableObject` | `Property` (an expression), `Field` (a key of the schema) or `Template` (a view); `Key`, `Title`, `Format`, `Align`, `Sortable`, `Visible`, `Required`, `Pinned`, `Width`, `Order`, `HeaderTemplate`. |
| `KlyDataListSearch<TItem>` | `KlyInput` | Debounce, `MinLength`, clear button. |
| `KlyDataListPagination<TItem>` | `KlyPagination`, `KlySelect` | Numbered pages and the page size. |
| `KlyDataListInfiniteLoader<TItem>` | `ContentView` | Cursor paging: loads near the end of the scroll view, or a Load more button. |
| `KlyDataListResultCount<TItem>`, `KlyDataListRefresh<TItem>`, `KlyDataListErrorState<TItem>`, `KlyDataListClearFilters<TItem>` | `Label`, `KlyAlert` | |
| `KlyDataListEmpty` | `KlyEmpty` | The empty state, with its own texts. |
| `KlyDataListLayoutSwitch<TItem>`, `KlyDataListViewToggle<TItem>` | `Border` | Table / grid, and cards / table. |
| `KlyDataListSortMenu<TItem>` | `KlyDropdownMenu` | Sort fields with the direction, single or multiple. |
| `KlyDataListFilterTags<TItem>`, `KlyDataListAddFilter<TItem>`, `KlyDataListFilterPanel<TItem>` | `KlyPopover` | Tags with an editor in a popover, the add button, and a panel for a `KlySlideover` or a `KlyModal` (Live and Submit apply modes). |
| `KlyDataListFilterEditor<TItem>` | `KlyInput`, `KlyInputNumber`, `KlyInputDate`, `KlyInputTime`, `KlySelect`, `KlyCheckbox` | Text, option (counts, search, remote source), boolean, number and date (with presets) editors. |
| `KlyDataListColumnPanel<TItem>` | `KlyPopover` | Show, hide, move and pin columns. |
| `KlyDataListGroupHeader<TItem>` | `Border` | Fold and select a group of rows. |
| `KlyDataListRowActions<TItem>`, `KlyDataListSelectionActions<TItem>`, `KlyDataListActionsDropdown<TItem>` | `KlyDropdownMenu` | Row buttons and menu with confirmations, the bar of the ticked rows, the toolbar menu. |
| `KlyDataListExport<TItem>` | `KlyDropdownMenu` | CSV of this page, the selection or every result. |

## Use

```csharp
using Klyroo.Query;
using Klyroo.Table.Maui;

var schema = DataListSchema.For<Person>(s => s
    .RowKey(p => p.Id)
    .Search(p => p.Name)
    .Field(p => p.Name, f => f.Key("name").Sortable())
    .Field(p => p.Team, f => f.Key("team").Sortable().Filter(DataListFilter.Option().WithFacets())));

var list = new KlyDataList<Person>
{
    Schema = schema,
    Items = people,
    Selectable = true,
    Content = new VerticalStackLayout
    {
        Children =
        {
            new KlyDataListSearch<Person>(),
            new KlyDataListFilterTags<Person> { ShowAdd = true },
            new KlyDataListTable<Person>
            {
                Columns =
                {
                    new KlyDataListColumn<Person, string> { Property = p => p.Name },
                    new KlyDataListColumn<Person, string> { Property = p => p.Team },
                },
            },
            new KlyDataListPagination<Person>(),
        },
    },
};
```

The generic controls are written in C#. In XAML a generic type can only be the root element (`x:TypeArguments`), so build the list in code behind or give a `ContentView` of your own the parts.

## What the app provides

- A `KlyOverlayHost` above the list for the popovers, the menus and the confirmation dialogs (`KlyDataListFilterTags`, `KlyDataListAddFilter`, `KlyDataListColumnPanel`, `KlyDataListSortMenu`, `KlyDataListActionsDropdown`, `KlyDataListRowActions`, `KlyDataListSelectionActions`, `KlyDataListExport`).
- Nothing to register in the services: the texts follow `CultureInfo.CurrentUICulture` (the 12 tables of `KlyToolsLocales`), or the `Messages` property of the list.
- For `Queryable` sources an `IDataListQueryExecutor` set on `Executor`; the default one blocks the UI thread while the database answers.
- Export: without a handler for `Exported` the CSV is written to the cache directory and given to the system share sheet (`Share.RequestAsync`), which saves, mails or copies it; MAUI has no file saver built in.

## Differences from the Blazor controls

- No URL: the state lives in the list and in `State`/`StateChanged`; there is no `UrlSync`, `UrlKey`, link or form fallback of a static render, and no prerender persistence (`ResultJson`, `PersistKey`).
- `EventCallback` parameters are events (`StateChanged`, `RowClick`, `Exported`, `Error`...), `Disabled` is `IsEnabled`, render fragments are `Func<TItem, View>`.
- Row actions that are links raise `LinkActivated` for the app to route; a web, mail or phone address is also opened by the system.
- The selection bar, the row actions and the column panel use the dialogs, popovers and menus of the overlay host.
- A virtualized table draws its rows in a `CollectionView` of a fixed `VirtualHeight`; a grouping turns virtualization off.
- Not ported: the tree (`DataListTree`), the detail rows, the summary footer (`DataListSummaries`), the column header menu, the column resize by drag, the sticky pinned columns (pinned columns keep their place at the start or the end of the row), the user preferences cookie (`PreferencesKey`), the remembered filters (`WithPersist`), the filter slider display, the skeleton kinds per column and the CSS slots (`Ui`).

## Build

This project is not part of `Klyroo.Table.slnx`: it needs the MAUI workload. It targets `net10.0` (the unit tests run on it) and `net10.0-android`, plus iOS and Mac Catalyst outside Linux and Windows on Windows. A machine whose MAUI workload is the .NET 11 one builds with `-p:KlyrooMauiTfm=net11.0`.

```bash
dotnet test --project tests/Klyroo.Table.Maui.Tests
```
