# Lux.Table (LuxStack)

> **Headless tree hierarchy engine & high-density enterprise BxDataList**

Headless-powered data tables and card grids for .NET: NativeAOT tree hierarchy (DataListTree<TItem>), visual grouping, footer aggregations, 2px loading progress line, automatic Sort/Hide HeaderMenu, 60fps virtualization, and responsive mobile summary rows.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![NativeAOT](https://img.shields.io/badge/Core-NativeAOT%20Ready-10b981.svg)](./docs/maui-and-aot.md)
[![MAUI Hybrid](https://img.shields.io/badge/.NET%20MAUI-Blazor%20Hybrid-8b5cf6.svg)](./docs/maui-and-aot.md)

---

## Architecture (Two-Layer Headless Split)

- **`Lux.Table.Core`** — 100% headless C#, zero UI dependencies, `<IsAotCompatible>true</IsAotCompatible>`, `<IsTrimmable>true</IsTrimmable>`. Runs in Minimal APIs, background workers, CLI apps, and native C# .NET MAUI ViewModels.
- **`Lux.Table`** — Multiplatform Blazor Web (Static SSR, Interactive Server, WebAssembly, Auto) & `.NET MAUI Blazor Hybrid` (`browser`, `ios`, `android`, `maccatalyst`, `windows`).

## Highlights

- **NativeAOT Tree Hierarchy** (`Headless Tree`) — Lux.Table.Core builds and flattens hierarchical trees (DataListTree<TItem>) with depth tracking and continuation rails without any UI dependency.
- **Built-In UX Polish** (`UX Polish`) — 2px indeterminate progress line during reloads, automatic Sort Asc/Desc/Hide HeaderMenu, adaptive skeletons, and first-use EmptyState CTAs.
- **Grouping, Summaries & Pinning** (`Enterprise Grid`) — Visual row grouping (DataListGroupBy), footer Sum/Avg/Min/Max aggregations, column resizing, and left/right sticky column pinning.
- **Responsive Summary Rows on Phone** (`Mobile Ready`) — Automatically switches to scannable summary rows (DataListRowSummary) below 768px on iOS and Android phones.

## Installation

```bash
dotnet add package Lux.Table.Core   # Headless C# / NativeAOT / MAUI C#
dotnet add package Lux.Table        # Blazor Web & .NET MAUI Blazor Hybrid
```

Register services in `Program.cs` (Blazor Web App) or `MauiProgram.cs` (.NET MAUI Blazor Hybrid):

```csharp
builder.Services.AddLuxTable();
```

## Quick Start

### Headless Core (`Lux.Table.Core`)

```csharp
using Blux.UI.Tools;

var tree = new DataListTree<Employee>
{
    Children = e => e.Reports,
    DefaultExpandedDepth = 1
};

var (roots, allNodes) = tree.BuildTree(employees, schema);
// Every node carries Depth, Parent, IsLast & AncestorContinuations
```

### Blazor Web & .NET MAUI Hybrid (`Lux.Table`)

```razor
<BxDataList TItem="Employee" Schema="@_schema" Items="@_employees" UrlSync="false">
    <BxDataListTable TItem="Employee"
                     HeaderMenu="true"
                     ShowProgressLine="true"
                     Summaries="@(new DataListSummaries<Employee>())">
        <BxDataListColumn TItem="Employee" Field="name" Title="Name" />
        <BxDataListColumn TItem="Employee" Field="salary" Title="Salary"
                          Summary="DataListSummaryKind.Sum" />
    </BxDataListTable>
</BxDataList>
```

## Documentation

| Guide | Description |
| :--- | :--- |
| [Getting Started](./docs/getting-started.md) | Installation, DI registration, stylesheets, and first working example |
| [Architecture](./docs/architecture.md) | Headless `.Core` vs UI split, NativeAOT guarantees, and platform matrix |
| [API Reference](./docs/api-reference.md) | Exhaustive reference of every public class, enum, property, method, and Razor parameter |
| [MAUI & NativeAOT](./docs/maui-and-aot.md) | `.NET MAUI Blazor Hybrid` (`iOS`, `Android`, `macOS`, `Windows`) and NativeAOT deployment |
| [Recipes & Testing](./docs/recipes.md) | End-to-end production patterns and `xUnit` / `bUnit` testing recipes |
| [AI Agent Skill](./skills/lux-table/SKILL.md) | Machine-readable guidance for coding agents |

## Key Public Types

| Type | Kind | Package | Summary |
| :--- | :--- | :--- | :--- |
| `DataListTreeSelectionMode` | `enum` | `Lux.Table.Core` | Which rows of a hierarchical tree table can be selected with checkboxes. |
| `DataListTreeNode<TItem>` | `class` | `Lux.Table.Core` | One node in the flattened hierarchy produced by `DataListTree{TItem}` over the loaded top-level rows of a data list. The row type. |
| `DataListTree<TItem>` | `class` | `Lux.Table.Core` | Hierarchical tree configuration for `BxDataListTable{TItem}`. Child rows travel with their parent item (`Children`), expand and collapse with a chevron button (mouse, Enter/Space, or ArrowRight/ArrowLeft) and visual rail connectors, and participate in row selection according to `Selectable`. When a tree is configured on `BxDataListTable{TItem}`, summary rows and visual groups are disabled on the table. The row type. |
| `BluxToolsOptions` | `class` | `Lux.Table` | Configuration of Blux.UI.Tools: its component themes and its message tables. It is an extension of `BluxOptions` (`Extension{T}`), so its themes freeze with the ones of Blux.UI and the Tailwind class extractor and the docs bundle find them. Configure it with `services.AddBluxUiTools(t =&gt; ...)`, in code shared by the server and the WebAssembly client. Partial: each family of components declares its themes in its own file. |
| `BxToolsComponentBase` | `class` | `Lux.Table` | Base of the components of Blux.UI.Tools: the theme resolution of `BxComponentBase`, plus the options and the texts of this package. |
| `BxDataList<TItem>` | `class` | `Lux.Table` | Public API type |
| `BxDataList<TItem>` | `class` | `Lux.Table` | Public API type |
| `BxDataListColumn<TItem>` | `class` | `Lux.Table` | A column of `BxDataListTable{TItem}`. Renders nothing itself: it registers with its list (`Columns`). What a cell shows comes from one of three places: `Template`, `Property` or `Field`. The row type. |
| `BxDataListFilterEditor<TItem>` | `class` | `Lux.Table` | Public API type |
| `DataListActionPartBase<TItem>` | `class` | `Lux.Table` | Public API type |
| `DataListActionScope` | `enum` | `Lux.Table` | Which rows a bulk or toolbar action targets. |
| `DataListSelectionScope` | `enum` | `Lux.Table` | Where the rows of a selection survive: across pages, or only on the page they were ticked on. |
| `DataListSelectionClear` | `enum` | `Lux.Table` | When running an action unticks the rows. |
| `DataListSelectionBarPosition` | `enum` | `Lux.Table` | Where the selection bar sits. |
| `DataListActionContext<TItem>` | `class` | `Lux.Table` | What an action receives when it runs: the rows it targets, the scope and the request that stands for "every result". The row type. |
| `DataListActionConfirm<TItem>` | `class` | `Lux.Table` | The question an action asks before it runs, in a dialog. The text can say how many rows are affected. The row type. |
| `DataListAction<TItem>` | `class` | `Lux.Table` | An action of a data list: a button of the `BxDataListSelectionActions{TItem}` bar (it acts on the selection) or an entry of the `BxDataListActionsDropdown{TItem}` menu. While it runs its button shows a spinner and the action cannot start twice. The row type. |
| `DataListActionFailure<TItem>` | `record` | `Lux.Table` | An action that threw. The action. What it threw. The row type. |

## Razor Components

| Component | Package | Parameters | Source |
| :--- | :--- | :--- | :--- |
| `<BxDataList>` | `Lux.Table` | 15 parameters | `src/Lux.Table/Components/DataList/BxDataList.razor` |
| `<BxDataListActionsDropdown>` | `Lux.Table` | 2 parameters | `src/Lux.Table/Components/DataList/BxDataListActionsDropdown.razor` |
| `<BxDataListAddFilter>` | `Lux.Table` | 3 parameters | `src/Lux.Table/Components/DataList/BxDataListAddFilter.razor` |
| `<BxDataListClearFilters>` | `Lux.Table` | 2 parameters | `src/Lux.Table/Components/DataList/BxDataListClearFilters.razor` |
| `<BxDataListColumnPanel>` | `Lux.Table` | 5 parameters | `src/Lux.Table/Components/DataList/BxDataListColumnPanel.razor` |
| `<BxDataListEmpty>` | `Lux.Table` | 6 parameters | `src/Lux.Table/Components/DataList/BxDataListEmpty.razor` |
| `<BxDataListErrorState>` | `Lux.Table` | 2 parameters | `src/Lux.Table/Components/DataList/BxDataListErrorState.razor` |
| `<BxDataListFilterEditor>` | `Lux.Table` | 5 parameters | `src/Lux.Table/Components/DataList/BxDataListFilterEditor.razor` |
| `<BxDataListFilterPanel>` | `Lux.Table` | 3 parameters | `src/Lux.Table/Components/DataList/BxDataListFilterPanel.razor` |
| `<BxDataListFilterTags>` | `Lux.Table` | 3 parameters | `src/Lux.Table/Components/DataList/BxDataListFilterTags.razor` |
| `<BxDataListGrid>` | `Lux.Table` | 12 parameters | `src/Lux.Table/Components/DataList/BxDataListGrid.razor` |
| `<BxDataListGroupHeader>` | `Lux.Table` | 6 parameters | `src/Lux.Table/Components/DataList/BxDataListGroupHeader.razor` |
| `<BxDataListInfiniteLoader>` | `Lux.Table` | 6 parameters | `src/Lux.Table/Components/DataList/BxDataListInfiniteLoader.razor` |
| `<BxDataListLayoutSwitch>` | `Lux.Table` | 5 parameters | `src/Lux.Table/Components/DataList/BxDataListLayoutSwitch.razor` |
| `<BxDataListPagination>` | `Lux.Table` | 5 parameters | `src/Lux.Table/Components/DataList/BxDataListPagination.razor` |
| `<BxDataListRefresh>` | `Lux.Table` | 0 parameters | `src/Lux.Table/Components/DataList/BxDataListRefresh.razor` |
| `<BxDataListResultCount>` | `Lux.Table` | 0 parameters | `src/Lux.Table/Components/DataList/BxDataListResultCount.razor` |
| `<BxDataListRowActions>` | `Lux.Table` | 7 parameters | `src/Lux.Table/Components/DataList/BxDataListRowActions.razor` |
| `<BxDataListSearch>` | `Lux.Table` | 5 parameters | `src/Lux.Table/Components/DataList/BxDataListSearch.razor` |
| `<BxDataListSelectionActions>` | `Lux.Table` | 3 parameters | `src/Lux.Table/Components/DataList/BxDataListSelectionActions.razor` |
| `<BxDataListSortMenu>` | `Lux.Table` | 4 parameters | `src/Lux.Table/Components/DataList/BxDataListSortMenu.razor` |
| `<BxDataListTable>` | `Lux.Table` | 18 parameters | `src/Lux.Table/Components/DataList/BxDataListTable.razor` |
| `<BxDataListViewToggle>` | `Lux.Table` | 3 parameters | `src/Lux.Table/Components/DataList/BxDataListViewToggle.razor` |
| `<DataListActionDialog>` | `Lux.Table` | 7 parameters | `src/Lux.Table/Components/DataList/DataListActionDialog.razor` |
| `<DataListRowSelect>` | `Lux.Table` | 2 parameters | `src/Lux.Table/Components/DataList/DataListRowSelect.razor` |
| `<DataListSelectAll>` | `Lux.Table` | 0 parameters | `src/Lux.Table/Components/DataList/DataListSelectAll.razor` |

## Building & Testing

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## License

MIT — see [LICENSE](./LICENSE). Part of the **[LuxStack](../lux.stack/README.md)** ecosystem.
