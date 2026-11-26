using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Klyroo.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class TableInteractionsAndEdgeCaseTests : BunitContext
{
    private sealed record TaskRow(int Id, string Title, string Team, decimal Hours, IReadOnlyList<TaskRow>? Subtasks = null);

    private static readonly DataListSchema<TaskRow> Schema = DataListSchema.For<TaskRow>(s => s
        .RowKey(t => t.Id)
        .RowLabel(t => t.Title)
        .Search(t => t.Title, t => t.Team)
        .Field(t => t.Title, f => f.Key("title").Sortable().Filter(DataListFilter.Text()))
        .Field(t => t.Team, f => f.Key("team").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(t => t.Hours, f => f.Key("hours").Sortable().Filter(DataListFilter.Number(min: 0)))
        .DefaultSort(t => t.Title)
        .DefaultPageSize(2)
        .MaxPageSize(20));

    private static TaskRow[] SampleTasks() =>
    [
        new(1, "Alpha API", "Backend", 12m),
        new(2, "Beta UI", "Frontend", 8m),
        new(3, "Gamma Docs", "DevRel", 4m),
        new(4, "Delta Tests", "Backend", 16m),
        new(5, "Epsilon Deploy", "Infra", 10m)
    ];

    private void SetupTableServices()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));
    }

    [Fact]
    public void DataListTree_BuildTree_handles_empty_input_deep_hierarchy_and_selection_modes()
    {
        var tree = new DataListTree<TaskRow>
        {
            Children = t => t.Subtasks,
            DefaultExpandedDepth = 1,
            Selectable = DataListTreeSelectionMode.Roots
        };

        var (emptyRoots, emptyAll) = tree.BuildTree([], Schema);
        Assert.Empty(emptyRoots);
        Assert.Empty(emptyAll);

        var grandChild = new TaskRow(3, "GrandChild", "Backend", 2m);
        var child = new TaskRow(2, "Child", "Backend", 4m, [grandChild]);
        var root1 = new TaskRow(1, "Root 1", "Backend", 8m, [child]);
        var root2 = new TaskRow(4, "Root 2", "Frontend", 6m);

        var (roots, all) = tree.BuildTree([root1, root2], Schema);
        Assert.Equal(2, roots.Count);
        Assert.Equal(4, all.Count);
        Assert.Equal(0, roots[0].Depth);
        Assert.True(roots[0].HasChildren);
        Assert.False(roots[1].HasChildren);
        Assert.Equal(2, all.Single(n => n.Item.Id == 3).Depth);
        Assert.Equal(DataListTreeSelectionMode.Roots, tree.Selectable);
    }

    [Fact]
    public void DataListTree_BuildTree_breaks_cyclic_references_without_infinite_recursion()
    {
        var list = new List<TaskRow>();
        var a = new TaskRow(1, "Node A", "Core", 1m, list);
        var b = new TaskRow(2, "Node B", "Core", 2m, [a]);
        list.Add(b); // Cycle: A -> B -> A

        var tree = new DataListTree<TaskRow> { Children = t => t.Subtasks };
        var (roots, all) = tree.BuildTree([a], Schema);

        Assert.Single(roots);
        Assert.Equal(2, all.Count);
        Assert.Equal([1, 2], all.Select(n => n.Item.Id).ToArray());
    }

    [Fact]
    public void KlyDataList_renders_empty_state_when_items_are_empty()
    {
        SetupTableServices();

        var cut = Render(builder =>
        {
            builder.OpenComponent<KlyDataList<TaskRow>>(0);
            builder.AddAttribute(1, nameof(KlyDataList<TaskRow>.Schema), Schema);
            builder.AddAttribute(2, nameof(KlyDataList<TaskRow>.Items), Array.Empty<TaskRow>());
            builder.AddAttribute(3, nameof(KlyDataList<TaskRow>.UrlSync), false);
            builder.AddAttribute(4, nameof(KlyDataList<TaskRow>.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<KlyDataListTable<TaskRow>>(0);
                b.AddAttribute(1, nameof(KlyDataListTable<TaskRow>.ChildContent), (RenderFragment)(cb =>
                {
                    cb.OpenComponent<KlyDataListFieldColumn<TaskRow>>(0);
                    cb.AddAttribute(1, nameof(KlyDataListFieldColumn<TaskRow>.Field), "title");
                    cb.AddAttribute(2, nameof(KlyDataListFieldColumn<TaskRow>.Title), "Title");
                    cb.CloseComponent();
                }));
                b.CloseComponent();
                b.OpenComponent<KlyDataListEmpty>(2);
                b.AddAttribute(3, nameof(KlyDataListEmpty.Title), "No tasks yet");
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains("No tasks yet", cut.Markup);
    }

    [Fact]
    public void KlyDataList_paginates_items_using_schema_DefaultPageSize()
    {
        SetupTableServices();

        var cut = Render(builder =>
        {
            builder.OpenComponent<KlyDataList<TaskRow>>(0);
            builder.AddAttribute(1, nameof(KlyDataList<TaskRow>.Schema), Schema);
            builder.AddAttribute(2, nameof(KlyDataList<TaskRow>.Items), SampleTasks());
            builder.AddAttribute(3, nameof(KlyDataList<TaskRow>.UrlSync), false);
            builder.AddAttribute(4, nameof(KlyDataList<TaskRow>.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<KlyDataListTable<TaskRow>>(0);
                b.AddAttribute(1, nameof(KlyDataListTable<TaskRow>.ChildContent), (RenderFragment)(cb =>
                {
                    cb.OpenComponent<KlyDataListFieldColumn<TaskRow>>(0);
                    cb.AddAttribute(1, nameof(KlyDataListFieldColumn<TaskRow>.Field), "title");
                    cb.AddAttribute(2, nameof(KlyDataListFieldColumn<TaskRow>.Title), "Title");
                    cb.CloseComponent();
                }));
                b.CloseComponent();
                b.OpenComponent<KlyDataListPagination<TaskRow>>(2);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains("Alpha API", cut.Markup);
        Assert.Contains("Beta UI", cut.Markup);
        Assert.DoesNotContain("Gamma Docs", cut.Markup);
    }

    [Fact]
    public void KlyDataList_renders_multiple_footer_summary_aggregations()
    {
        SetupTableServices();

        var cut = Render(builder =>
        {
            builder.OpenComponent<KlyDataList<TaskRow>>(0);
            builder.AddAttribute(1, nameof(KlyDataList<TaskRow>.Schema), Schema);
            builder.AddAttribute(2, nameof(KlyDataList<TaskRow>.Items), SampleTasks());
            builder.AddAttribute(3, nameof(KlyDataList<TaskRow>.UrlSync), false);
            builder.AddAttribute(4, nameof(KlyDataList<TaskRow>.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<KlyDataListTable<TaskRow>>(0);
                b.AddAttribute(1, nameof(KlyDataListTable<TaskRow>.Summaries), new DataListSummaries<TaskRow>());
                b.AddAttribute(2, nameof(KlyDataListTable<TaskRow>.ChildContent), (RenderFragment)(cb =>
                {
                    cb.OpenComponent<KlyDataListFieldColumn<TaskRow>>(0);
                    cb.AddAttribute(1, nameof(KlyDataListFieldColumn<TaskRow>.Field), "title");
                    cb.AddAttribute(2, nameof(KlyDataListFieldColumn<TaskRow>.Title), "Title");
                    cb.AddAttribute(3, nameof(KlyDataListFieldColumn<TaskRow>.Summary), DataListSummaryKind.Count);
                    cb.CloseComponent();
                    cb.OpenComponent<KlyDataListFieldColumn<TaskRow>>(4);
                    cb.AddAttribute(5, nameof(KlyDataListFieldColumn<TaskRow>.Field), "hours");
                    cb.AddAttribute(6, nameof(KlyDataListFieldColumn<TaskRow>.Title), "Hours");
                    cb.AddAttribute(7, nameof(KlyDataListFieldColumn<TaskRow>.Summary), DataListSummaryKind.Sum);
                    cb.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var tfoot = cut.Find("tfoot");
        Assert.NotNull(tfoot);
        Assert.False(string.IsNullOrWhiteSpace(tfoot.TextContent));
    }

    [Fact]
    public void KlyDataList_renders_search_and_grid_layout()
    {
        SetupTableServices();
        var gridState = new DataListState("", [], [], 1, 2, DataListLayout.Grid);

        var cut = Render(builder =>
        {
            builder.OpenComponent<KlyDataList<TaskRow>>(0);
            builder.AddAttribute(1, nameof(KlyDataList<TaskRow>.Schema), Schema);
            builder.AddAttribute(2, nameof(KlyDataList<TaskRow>.Items), SampleTasks());
            builder.AddAttribute(3, nameof(KlyDataList<TaskRow>.UrlSync), false);
            builder.AddAttribute(4, nameof(KlyDataList<TaskRow>.State), gridState);
            builder.AddAttribute(5, nameof(KlyDataList<TaskRow>.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<KlyDataListSearch<TaskRow>>(0);
                b.CloseComponent();
                b.OpenComponent<KlyDataListGrid<TaskRow>>(2);
                b.AddAttribute(3, nameof(KlyDataListGrid<TaskRow>.ItemTemplate), (RenderFragment<TaskRow>)(item => ib =>
                {
                    ib.OpenElement(0, "div");
                    ib.AddAttribute(1, "data-test-card", item.Id);
                    ib.AddContent(2, $"{item.Title} ({item.Team})");
                    ib.CloseElement();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Equal(2, cut.FindAll("[data-test-card]").Count);
        Assert.Contains("Alpha API (Backend)", cut.Markup);
    }

    [Fact]
    public void KlyToolsLocales_provides_English_and_French_messages()
    {
        Assert.NotNull(KlyToolsLocales.En);
        Assert.NotNull(KlyToolsLocales.Fr);
        Assert.NotEmpty(KlyToolsLocales.En.NoResults);
        Assert.NotEmpty(KlyToolsLocales.Fr.NoResults);
        Assert.NotEqual(KlyToolsLocales.En.NoResults, KlyToolsLocales.Fr.NoResults);
    }
}
