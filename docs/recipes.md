# Recipes & Testing — Klyroo.Table

## Recipe 1: Mobile Summary Rows + Grouping

```csharp
private static readonly DataListRowSummary<Employee> PhoneSummary = new()
{
    Title = e => e.Name,
    Details = e => [e.Department, $"€{e.Salary:N0}"],
    AutoPhone = true
};

private static readonly DataListGroupBy<Employee> ByDepartment = new()
{
    Key = e => e.Department
};
```

## Recipe 2: Headless Unit Testing (`xUnit`)

Test all business logic in `Klyroo.Table.Core` without spinning up a browser or Blazor renderer:

```csharp
using Xunit;

public sealed class TableUnitTests
{
    [Fact]
    public void Headless_core_runs_without_ui_host()
    {
        // Arrange & Act using Klyroo.Table.Core directly
        // Assert deterministic outputs with zero JSInterop setup
    }
}
```

## Recipe 3: Component Testing (`bUnit`)

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class TableComponentTests : BunitContext
{
    public TableComponentTests()
    {
        Services.AddKlyrooTable();
    }
}
```

> **Tip**: Always register Klyroo services on `Services` **before** calling `SetRendererInfo(...)` in bUnit tests.
