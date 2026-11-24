# Getting Started with Lux.Table

Headless-powered data tables and card grids for .NET: NativeAOT tree hierarchy (DataListTree<TItem>), visual grouping, footer aggregations, 2px loading progress line, automatic Sort/Hide HeaderMenu, 60fps virtualization, and responsive mobile summary rows.

## 1. Choose Your Package

| Use Case | Package to Install |
| :--- | :--- |
| Headless C#, Minimal APIs, Workers, CLI, or native C# .NET MAUI ViewModels | `Lux.Table.Core` |
| Blazor Web App (Server, WebAssembly, Auto, Static SSR) or `.NET MAUI Blazor Hybrid` | `Lux.Table` |

```bash
dotnet add package Lux.Table.Core
dotnet add package Lux.Table
```

## 2. Register Services

Call `builder.Services.AddLuxTable();` in `Program.cs` (both Server and Client projects when using Blazor Interactive Auto) or `MauiProgram.cs` (after `AddMauiBlazorWebView()`):

```csharp
builder.Services.AddLuxTable();
```

## 3. Add Namespace Imports

In `_Imports.razor`:

```razor
@using Lux.UI
@using Lux.Table
@using Lux.Query
@using Lux.Query
```

## 4. First Example

### Headless C# (`Lux.Table.Core`)

```csharp
using Lux.Table;
using Lux.Query;

var tree = new DataListTree<Employee>
{
    Children = e => e.Reports,
    DefaultExpandedDepth = 1
};

var (roots, allNodes) = tree.BuildTree(employees, schema);
// Every node carries Depth, Parent, IsLast & AncestorContinuations
```

### Blazor / MAUI Hybrid (`Lux.Table`)

```razor
<LxDataList TItem="Employee" Schema="@_schema" Items="@_employees" UrlSync="false">
    <LxDataListTable TItem="Employee"
                     HeaderMenu="true"
                     ShowProgressLine="true"
                     Summaries="@(new DataListSummaries<Employee>())">
        <LxDataListColumn TItem="Employee" Field="name" Title="Name" />
        <LxDataListColumn TItem="Employee" Field="salary" Title="Salary"
                          Summary="DataListSummaryKind.Sum" />
    </LxDataListTable>
</LxDataList>
```

## Next Steps

- [Architecture & Two-Layer Design](./architecture.md)
- [Complete API Reference](./api-reference.md)
- [.NET MAUI & NativeAOT Guide](./maui-and-aot.md)
- [Recipes & Testing](./recipes.md)
