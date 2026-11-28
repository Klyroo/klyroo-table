# Klyroo.Table.Core

The UI-free part of [Klyroo.Table](https://github.com/klyroo/klyroo-table): `DataListController<TItem>` (loads the rows of a state from an in-memory list, a query or a provider, cancels what a newer request replaces, pages by offset or cursor), `DataListTree<TItem>` (a tree model that flattens rows carrying their children into nodes with depth, parent and rail information), the CSV writer and the message tables of the components in 12 languages. The Blazor components and the native MAUI controls (`Klyroo.Table.Maui`) are views over it. It builds on `Klyroo.Query.Core`, targets .NET 10 and is NativeAOT compatible and trimmable.

## Install

```bash
dotnet add package Klyroo.Table.Core --prerelease
```

## Use

```csharp
using Klyroo.Query;
using Klyroo.Table;

Employee[] staff = [new(1, "Ada", [new(2, "Grace")])];

var schema = DataListSchema.For<Employee>(s => s.RowKey(e => e.Id));
var tree = new DataListTree<Employee> { Children = e => e.Reports, DefaultExpandedDepth = 1 };

var (roots, nodes) = tree.BuildTree(staff, schema);

public sealed record Employee(int Id, string Name, IReadOnlyList<Employee>? Reports = null);
```

`BuildTree` takes any `IReadOnlyList<Employee>` of top-level rows. For the Blazor components (the data list, table and grid), install `Klyroo.Table` instead. See the [repository README](https://github.com/klyroo/klyroo-table#readme) and the [documentation](https://github.com/klyroo/klyroo-table/tree/main/docs).
