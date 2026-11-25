# Klyroo.Table

*Part of [Klyroo](https://klyroo.dev): Build better. Compose freely.*

[![CI](https://github.com/klyroo/klyroo-table/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-table/actions/workflows/ci.yml) [![NuGet](https://img.shields.io/badge/NuGet-0.1.0--preview.1-004880.svg)](https://www.nuget.org/packages/Klyroo.Table) [![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/) [![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-10b981.svg)](https://klyroo.dev/why) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)

> **Headless tree hierarchy engine & high-density enterprise KlyDataList**

Headless-powered data tables and card grids for .NET: NativeAOT tree hierarchy (DataListTree<TItem>), visual grouping, footer aggregations, 2px loading progress line, automatic Sort/Hide HeaderMenu, 60fps virtualization, and responsive mobile summary rows.

[![MAUI Hybrid](https://img.shields.io/badge/.NET%20MAUI-Blazor%20Hybrid-8b5cf6.svg)](./docs/maui-and-aot.md)

---

## Architecture (Two-Layer Headless Split)

- **`Klyroo.Table.Core`** — 100% headless C#, zero UI dependencies, `<IsAotCompatible>true</IsAotCompatible>`, `<IsTrimmable>true</IsTrimmable>`. Runs in Minimal APIs, background workers, CLI apps, and native C# .NET MAUI ViewModels.
- **`Klyroo.Table`** — Multiplatform Blazor Web (Static SSR, Interactive Server, WebAssembly, Auto) & `.NET MAUI Blazor Hybrid` (`browser`, `ios`, `android`, `maccatalyst`, `windows`).

## Highlights

- **NativeAOT Tree Hierarchy** (`Headless Tree`) — Klyroo.Table.Core builds and flattens hierarchical trees (DataListTree<TItem>) with depth tracking and continuation rails without any UI dependency.
- **Built-In UX Polish** (`UX Polish`) — 2px indeterminate progress line during reloads, automatic Sort Asc/Desc/Hide HeaderMenu, adaptive skeletons, and first-use EmptyState CTAs.
- **Grouping, Summaries & Pinning** (`Enterprise Grid`) — Visual row grouping (DataListGroupBy), footer Sum/Avg/Min/Max aggregations, column resizing, and left/right sticky column pinning.
- **Responsive Summary Rows on Phone** (`Mobile Ready`) — Automatically switches to scannable summary rows (DataListRowSummary) below 768px on iOS and Android phones.

## Prerequisites

- .NET 10 SDK or later.
- A Blazor Web App (Static SSR, Interactive Server, WebAssembly or Auto) or a .NET MAUI Blazor Hybrid app for the UI
  package; the `.Core` package runs anywhere .NET 10 runs, NativeAOT included.
- For .NET MAUI: the `maui` workload (`dotnet workload install maui`).

## Installation

```bash
dotnet add package Klyroo.Table.Core   # Headless C# / NativeAOT / MAUI C#
dotnet add package Klyroo.Table        # Blazor Web & .NET MAUI Blazor Hybrid
```

Register services in `Program.cs` (Blazor Web App) or `MauiProgram.cs` (.NET MAUI Blazor Hybrid):

```csharp
builder.Services.AddKlyrooTable();
```

## Quick Start

### Headless Core (`Klyroo.Table.Core`)

```csharp
using Klyroo.Table;
using Klyroo.Query;

var tree = new DataListTree<Employee>
{
    Children = e => e.Reports,
    DefaultExpandedDepth = 1
};

var (roots, allNodes) = tree.BuildTree(employees, schema);
// Every node carries Depth, Parent, IsLast & AncestorContinuations
```

### Blazor Web & .NET MAUI Hybrid (`Klyroo.Table`)

```razor
<KlyDataList TItem="Employee" Schema="@_schema" Items="@_employees" UrlSync="false">
    <KlyDataListTable TItem="Employee"
                     HeaderMenu="true"
                     ShowProgressLine="true"
                     Summaries="@(new DataListSummaries<Employee>())">
        <KlyDataListColumn TItem="Employee" Field="name" Title="Name" />
        <KlyDataListColumn TItem="Employee" Field="salary" Title="Salary"
                          Summary="DataListSummaryKind.Sum" />
    </KlyDataListTable>
</KlyDataList>
```

## Documentation

| Guide | Description |
| :--- | :--- |
| [Online documentation](https://docs.klyroo.dev/table) | Guides, live examples and API reference |
| [Getting Started](./docs/getting-started.md) | Installation, DI registration, stylesheets, and first working example |
| [Architecture](./docs/architecture.md) | Headless `.Core` vs UI split, NativeAOT guarantees, and platform matrix |
| [API Reference](./docs/api-reference.md) | Exhaustive reference of every public class, enum, property, method, and Razor parameter |
| [MAUI & NativeAOT](./docs/maui-and-aot.md) | `.NET MAUI Blazor Hybrid` (`iOS`, `Android`, `macOS`, `Windows`) and NativeAOT deployment |
| [Recipes & Testing](./docs/recipes.md) | End-to-end production patterns and `xUnit` / `bUnit` testing recipes |
| [AI Agent Skill](./skills/klyroo-table/SKILL.md) | Machine-readable guidance for coding agents |

## Key Public Types

| Type | Kind | Package | Summary |
| :--- | :--- | :--- | :--- |
| `DataListTreeSelectionMode` | `enum` | `Klyroo.Table.Core` | Which rows of a hierarchical tree table can be selected with checkboxes. |
| `DataListTreeNode<TItem>` | `class` | `Klyroo.Table.Core` | One node in the flattened hierarchy produced by `DataListTree{TItem}` over the loaded top-level rows of a data list. The row type. |
| `DataListTree<TItem>` | `class` | `Klyroo.Table.Core` | Hierarchical tree configuration for `KlyDataListTable{TItem}`. Child rows travel with their parent item (`Children`), expand and collapse with a chevron button (mouse, Enter/Space, or ArrowRight/ArrowLeft) and visual rail connectors, and participate in row selection according to `Selectable`. When a tree is configured on `KlyDataListTable{TItem}`, summary rows and visual groups are disabled on the table. The row type. |
| `KlyrooTableOptions` | `class` | `Klyroo.Table` | Configuration of Klyroo.Table: its component themes and its message tables. It is an extension of `KlyrooOptions` (`Extension{T}`), so its themes freeze with the ones of Klyroo.UI and the Tailwind class extractor and the docs bundle find them. Configure it with `services.AddKlyrooTable(t =&gt; ...)`, in code shared by the server and the WebAssembly client. Partial: each family of components declares its themes in its own file. |
| `KlyToolsComponentBase` | `class` | `Klyroo.Table` | Base of the components of Klyroo.Table: the theme resolution of `KlyComponentBase`, plus the options and the texts of this package. |
| `KlyDataList<TItem>` | `class` | `Klyroo.Table` | Public API type |
| `KlyDataList<TItem>` | `class` | `Klyroo.Table` | Public API type |
| `KlyDataListColumn<TItem>` | `class` | `Klyroo.Table` | A column of `KlyDataListTable{TItem}`. Renders nothing itself: it registers with its list (`Columns`). What a cell shows comes from one of three places: `Template`, `Property` or `Field`. The row type. |
| `KlyDataListFilterEditor<TItem>` | `class` | `Klyroo.Table` | Public API type |
| `DataListActionPartBase<TItem>` | `class` | `Klyroo.Table` | Public API type |
| `DataListActionScope` | `enum` | `Klyroo.Table` | Which rows a bulk or toolbar action targets. |
| `DataListSelectionScope` | `enum` | `Klyroo.Table` | Where the rows of a selection survive: across pages, or only on the page they were ticked on. |
| `DataListSelectionClear` | `enum` | `Klyroo.Table` | When running an action unticks the rows. |
| `DataListSelectionBarPosition` | `enum` | `Klyroo.Table` | Where the selection bar sits. |
| `DataListActionContext<TItem>` | `class` | `Klyroo.Table` | What an action receives when it runs: the rows it targets, the scope and the request that stands for "every result". The row type. |
| `DataListActionConfirm<TItem>` | `class` | `Klyroo.Table` | The question an action asks before it runs, in a dialog. The text can say how many rows are affected. The row type. |
| `DataListAction<TItem>` | `class` | `Klyroo.Table` | An action of a data list: a button of the `KlyDataListSelectionActions{TItem}` bar (it acts on the selection) or an entry of the `KlyDataListActionsDropdown{TItem}` menu. While it runs its button shows a spinner and the action cannot start twice. The row type. |
| `DataListActionFailure<TItem>` | `record` | `Klyroo.Table` | An action that threw. The action. What it threw. The row type. |

## Razor Components

| Component | Package | Parameters | Source |
| :--- | :--- | :--- | :--- |
| `<KlyDataList>` | `Klyroo.Table` | 15 parameters | `src/Klyroo.Table/Components/DataList/KlyDataList.razor` |
| `<KlyDataListActionsDropdown>` | `Klyroo.Table` | 2 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListActionsDropdown.razor` |
| `<KlyDataListAddFilter>` | `Klyroo.Table` | 3 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListAddFilter.razor` |
| `<KlyDataListClearFilters>` | `Klyroo.Table` | 2 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListClearFilters.razor` |
| `<KlyDataListColumnPanel>` | `Klyroo.Table` | 5 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListColumnPanel.razor` |
| `<KlyDataListEmpty>` | `Klyroo.Table` | 6 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListEmpty.razor` |
| `<KlyDataListErrorState>` | `Klyroo.Table` | 2 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListErrorState.razor` |
| `<KlyDataListFilterEditor>` | `Klyroo.Table` | 5 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListFilterEditor.razor` |
| `<KlyDataListFilterPanel>` | `Klyroo.Table` | 3 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListFilterPanel.razor` |
| `<KlyDataListFilterTags>` | `Klyroo.Table` | 3 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListFilterTags.razor` |
| `<KlyDataListGrid>` | `Klyroo.Table` | 12 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListGrid.razor` |
| `<KlyDataListGroupHeader>` | `Klyroo.Table` | 6 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListGroupHeader.razor` |
| `<KlyDataListInfiniteLoader>` | `Klyroo.Table` | 6 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListInfiniteLoader.razor` |
| `<KlyDataListLayoutSwitch>` | `Klyroo.Table` | 5 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListLayoutSwitch.razor` |
| `<KlyDataListPagination>` | `Klyroo.Table` | 5 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListPagination.razor` |
| `<KlyDataListRefresh>` | `Klyroo.Table` | 0 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListRefresh.razor` |
| `<KlyDataListResultCount>` | `Klyroo.Table` | 0 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListResultCount.razor` |
| `<KlyDataListRowActions>` | `Klyroo.Table` | 7 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListRowActions.razor` |
| `<KlyDataListSearch>` | `Klyroo.Table` | 5 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListSearch.razor` |
| `<KlyDataListSelectionActions>` | `Klyroo.Table` | 3 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListSelectionActions.razor` |
| `<KlyDataListSortMenu>` | `Klyroo.Table` | 4 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListSortMenu.razor` |
| `<KlyDataListTable>` | `Klyroo.Table` | 18 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListTable.razor` |
| `<KlyDataListViewToggle>` | `Klyroo.Table` | 3 parameters | `src/Klyroo.Table/Components/DataList/KlyDataListViewToggle.razor` |
| `<DataListActionDialog>` | `Klyroo.Table` | 7 parameters | `src/Klyroo.Table/Components/DataList/DataListActionDialog.razor` |
| `<DataListRowSelect>` | `Klyroo.Table` | 2 parameters | `src/Klyroo.Table/Components/DataList/DataListRowSelect.razor` |
| `<DataListSelectAll>` | `Klyroo.Table` | 0 parameters | `src/Klyroo.Table/Components/DataList/DataListSelectAll.razor` |

## Building & Testing

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## Contributing

Issues and pull requests are welcome. Read [CONTRIBUTING.md](./CONTRIBUTING.md) for the workflow (Conventional
Commits, tests, documentation), follow the [Code of Conduct](./CODE_OF_CONDUCT.md), and report vulnerabilities
privately as described in [SECURITY.md](./SECURITY.md).

## Migrating from Lux

This library was previously named `lux-table`. [MIGRATION.md](./MIGRATION.md) lists every renamed package, namespace,
component, CSS token and build setting.

## License

MIT — see [LICENSE](./LICENSE). Part of the **[Klyroo](../klyroo-stack/README.md)** ecosystem.
