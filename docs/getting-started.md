# Getting Started with Klyroo.Table

Headless-powered data tables and card grids for .NET: NativeAOT tree hierarchy (DataListTree<TItem>), visual grouping, footer aggregations, 2px loading progress line, automatic Sort/Hide HeaderMenu, 60fps virtualization, and responsive mobile summary rows.

## 1. Choose Your Package

| Use Case | Package to Install |
| :--- | :--- |
| Headless C#, Minimal APIs, Workers, CLI, or native C# .NET MAUI ViewModels | `Klyroo.Table.Core` |
| Blazor Web App (Server, WebAssembly, Auto, Static SSR) or `.NET MAUI Blazor Hybrid` | `Klyroo.Table` |

```bash
dotnet add package Klyroo.Table.Core
dotnet add package Klyroo.Table
```

## 2. Register Services

Call `builder.Services.AddKlyrooTable();` in `Program.cs` (both Server and Client projects when using Blazor Interactive Auto) or `MauiProgram.cs` (after `AddMauiBlazorWebView()`):

```csharp
builder.Services.AddKlyrooTable();
```

## 3. Add Namespace Imports

In `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Table
@using Klyroo.Query
@using Klyroo.Query
```

## 4. First Example

### Headless C# (`Klyroo.Table.Core`)

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

### Blazor / MAUI Hybrid (`Klyroo.Table`)

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

## Next Steps

- [Architecture & Two-Layer Design](./architecture.md)
- [Complete API Reference](./api-reference.md)
- [.NET MAUI & NativeAOT Guide](./maui-and-aot.md)
- [Recipes & Testing](./recipes.md)
