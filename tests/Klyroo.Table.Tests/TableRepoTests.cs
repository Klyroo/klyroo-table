using Klyroo.Table;
using Klyroo.Query;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class TableRepoTests : BunitContext
{
    public sealed record Employee(int Id, string Name, string Department, decimal Salary, IReadOnlyList<Employee>? Reports = null);

    [Fact]
    public void TableCore_builds_tree_nodes_and_groups_rows()
    {
        var tree = new DataListTree<Employee>
        {
            Children = e => e.Reports,
            DefaultExpanded = true
        };
        var roots = new[]
        {
            new Employee(1, "Alice", "Engineering", 120000m, [new Employee(2, "Bob", "Engineering", 95000m)])
        };
        var (_, allNodes) = tree.BuildTree(roots);
        Assert.Equal(2, allNodes.Count);
        Assert.Equal(0, allNodes[0].Depth);
        Assert.Equal(1, allNodes[1].Depth);

        var groupBy = new DataListGroupBy<Employee> { Key = e => e.Department };
        var groups = groupBy.BuildGroups(allNodes.Select(n => n.Item).ToList(), null, new KlyToolsMessages(), System.Globalization.CultureInfo.InvariantCulture);
        Assert.Single(groups);
        Assert.Equal("Engineering", groups[0].Key);
        Assert.Equal(2, groups[0].Items.Count);
    }

    [Fact]
    public void KlyDataList_renders_columns_header_menu_and_footer_summaries()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));

        var schema = DataListSchema.For<Employee>(s => s
            .RowKey(e => e.Id)
            .Field(e => e.Name, f => f.Key("name").Sortable())
            .Field(e => e.Salary, f => f.Key("salary").Sortable()));

        var items = new[]
        {
            new Employee(1, "Alice", "Engineering", 100m),
            new Employee(2, "Bob", "Sales", 200m),
        };

        var cut = Render<KlyDataList<Employee>>(p => p
            .Add(x => x.Schema, schema)
            .Add(x => x.Items, items)
            .Add(x => x.UrlSync, false)
            .AddChildContent<KlyDataListTable<Employee>>(t => t
                .Add(x => x.HeaderMenu, true)
                .Add(x => x.Summaries, new DataListSummaries<Employee>())
                .AddChildContent(b =>
                {
                    b.OpenComponent<KlyDataListColumn<Employee>>(0);
                    b.AddComponentParameter(1, nameof(KlyDataListColumn<Employee>.Field), "name");
                    b.AddComponentParameter(2, nameof(KlyDataListColumn<Employee>.Title), "Name");
                    b.CloseComponent();

                    b.OpenComponent<KlyDataListColumn<Employee>>(3);
                    b.AddComponentParameter(4, nameof(KlyDataListColumn<Employee>.Field), "salary");
                    b.AddComponentParameter(5, nameof(KlyDataListColumn<Employee>.Title), "Salary");
                    b.AddComponentParameter(6, nameof(KlyDataListColumn<Employee>.Summary), DataListSummaryKind.Sum);
                    b.CloseComponent();
                })));

        Assert.Equal(2, cut.FindAll("tbody tr").Count);
        Assert.NotNull(cut.Find("tfoot"));
    }
}
