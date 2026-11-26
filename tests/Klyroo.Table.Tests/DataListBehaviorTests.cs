using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class DataListBehaviorTests : BunitContext
{
    private sealed record TaskRow(int Id, string Title, decimal Hours);

    private static readonly DataListSchema<TaskRow> Schema = DataListSchema.For<TaskRow>(s => s
        .RowKey(t => t.Id)
        .RowLabel(t => t.Title)
        .Field(t => t.Title, f => f.Key("title").Sortable().Filter(DataListFilter.Text()))
        .Field(t => t.Hours, f => f.Key("hours").Sortable())
        .DefaultSort(t => t.Title)
        .DefaultPageSize(2));

    private static readonly TaskRow[] Tasks =
    [
        new(1, "Alpha", 12m),
        new(2, "Beta", 8m),
        new(3, "Gamma", 4m),
        new(4, "Delta", 16m),
        new(5, "Epsilon", 10m),
    ];

    public DataListBehaviorTests()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));
    }

    private IRenderedComponent<KlyDataList<TaskRow>> RenderList(RenderFragment childContent, IReadOnlySet<string>? selected = null) =>
        Render<KlyDataList<TaskRow>>(p => p
            .Add(x => x.Schema, Schema)
            .Add(x => x.Items, Tasks)
            .Add(x => x.UrlSync, false)
            .Add(x => x.SelectedKeys, selected)
            .Add(x => x.ChildContent, childContent));

    private static RenderFragment Table(DataListSummaries<TaskRow>? summaries) => b =>
    {
        b.OpenComponent<KlyDataListTable<TaskRow>>(0);
        b.AddComponentParameter(1, nameof(KlyDataListTable<TaskRow>.Summaries), summaries);
        b.AddComponentParameter(2, nameof(KlyDataListTable<TaskRow>.ChildContent), (RenderFragment)(cb =>
        {
            cb.OpenComponent<KlyDataListColumn<TaskRow>>(0);
            cb.AddComponentParameter(1, nameof(KlyDataListColumn<TaskRow>.Field), "title");
            cb.CloseComponent();
            cb.OpenComponent<KlyDataListColumn<TaskRow>>(2);
            cb.AddComponentParameter(3, nameof(KlyDataListColumn<TaskRow>.Field), "hours");
            cb.AddComponentParameter(4, nameof(KlyDataListColumn<TaskRow>.Summary), DataListSummaryKind.Sum);
            cb.CloseComponent();
        }));
        b.CloseComponent();
    };

    [Fact]
    public void Each_summary_scope_totals_its_own_rows()
    {
        var summaries = new DataListSummaries<TaskRow> { Scopes = [DataListSummaryScope.Filtered, DataListSummaryScope.Page, DataListSummaryScope.Selection] };

        var cut = RenderList(Table(summaries), new HashSet<string> { "4" });

        string Total(string scope) => cut.Find($"tfoot tr[data-scope={scope}] [data-key=hours]").TextContent.Trim();
        cut.WaitForAssertion(() =>
        {
            // Every matching row (50), the two rows of page 1 sorted by title (Alpha 12 + Beta 8), the ticked row (Delta 16).
            Assert.Equal("50", Total("filtered"));
            Assert.Equal("20", Total("page"));
            Assert.Equal("16", Total("selection"));
        });
    }

    [Fact]
    public void The_filtered_scope_follows_the_filters_and_the_page_scope_the_rows_on_screen()
    {
        var summaries = new DataListSummaries<TaskRow> { Scopes = [DataListSummaryScope.Filtered, DataListSummaryScope.Page] };
        var cut = RenderList(Table(summaries));
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("tbody tr").Count));

        cut.InvokeAsync(() => cut.Instance.SetState(s => s with { Filters = [new FilterRule("title", FilterOperator.Contains, new FilterValue.Text("ta"))] }, QueryHistory.Push));

        cut.WaitForAssertion(() =>
        {
            // Beta 8 + Delta 16 match "ta"; both are on page 1.
            Assert.Equal("24", cut.Find("tfoot tr[data-scope=filtered] [data-key=hours]").TextContent.Trim());
            Assert.Equal("24", cut.Find("tfoot tr[data-scope=page] [data-key=hours]").TextContent.Trim());
        });
    }

    [Fact]
    public void The_selection_summary_follows_the_ticked_rows()
    {
        var summaries = new DataListSummaries<TaskRow> { Scope = DataListSummaryScope.Selection };
        var cut = RenderList(Table(summaries), new HashSet<string>());
        string Total() => cut.Find("tfoot tr[data-scope=selection] [data-key=hours]").TextContent.Trim();
        cut.WaitForAssertion(() => Assert.Equal("0", Total()));

        cut.Find("input[data-select=row][data-key='2']").Change(true);

        cut.WaitForAssertion(() => Assert.Equal("8", Total()));
    }

    [Fact]
    public void The_selection_bar_is_sticky_by_default()
    {
        var cut = RenderList(b =>
        {
            b.OpenComponent<KlyDataListSelectionActions<TaskRow>>(0);
            b.CloseComponent();
            Table(null)(b);
        }, new HashSet<string> { "1" });

        var bar = cut.WaitForElement("[data-slot=selection-bar]");
        var classes = bar.GetAttribute("class")!.Split(' ');
        Assert.Contains("sticky", classes);
        Assert.DoesNotContain("fixed", classes);
    }

    [Fact]
    public void The_selection_bar_can_be_fixed_to_the_window()
    {
        var cut = RenderList(b =>
        {
            b.OpenComponent<KlyDataListSelectionActions<TaskRow>>(0);
            b.AddComponentParameter(1, nameof(KlyDataListSelectionActions<TaskRow>.Position), DataListSelectionBarPosition.Fixed);
            b.CloseComponent();
            Table(null)(b);
        }, new HashSet<string> { "1" });

        var classes = cut.WaitForElement("[data-slot=selection-bar]").GetAttribute("class")!.Split(' ');
        Assert.Contains("fixed", classes);
        Assert.DoesNotContain("sticky", classes);
    }

    [Fact]
    public async Task PagesLoaded_counts_the_cursor_pages_on_screen()
    {
        var failNext = false;
        DataListProvider<TaskRow> provider = (request, _) =>
        {
            if (failNext) throw new InvalidOperationException("offline");
            var cursor = (DataListPaging.Cursor)request.Paging;
            var skip = cursor.After is null ? 0 : int.Parse(cursor.After, System.Globalization.CultureInfo.InvariantCulture);
            var page = Tasks.Skip(skip).Take(cursor.Take).ToList();
            var next = skip + page.Count < Tasks.Length ? (skip + page.Count).ToString(System.Globalization.CultureInfo.InvariantCulture) : null;
            return ValueTask.FromResult(new DataListResult<TaskRow>(page, Tasks.Length, next));
        };
        using var controller = new DataListController<TaskRow>(Schema, provider) { PagingMode = DataListPagingMode.Cursor };

        await controller.LoadAsync(Schema.DefaultState);
        Assert.Equal(1, controller.PagesLoaded);

        await controller.LoadMoreAsync();
        await controller.LoadMoreAsync();
        Assert.Equal(3, controller.PagesLoaded);
        Assert.Equal(5, controller.Rows.Count);
        Assert.False(controller.HasMore);

        // Nothing left to append: no page is counted.
        await controller.LoadMoreAsync();
        Assert.Equal(3, controller.PagesLoaded);

        // A refresh asks for the pages on screen again and keeps the count.
        await controller.RefreshAsync();
        Assert.Equal(3, controller.PagesLoaded);
        Assert.Equal(5, controller.Rows.Count);

        // A new load starts again from one page; a page that fails to append is not counted.
        await controller.LoadAsync(Schema.DefaultState with { PageSize = 2 });
        Assert.Equal(1, controller.PagesLoaded);
        failNext = true;
        await controller.LoadMoreAsync();
        Assert.True(controller.HasLoadMoreError);
        Assert.Equal(1, controller.PagesLoaded);

        // Outside cursor paging there is one page.
        controller.PagingMode = DataListPagingMode.Offset;
        Assert.Equal(1, controller.PagesLoaded);
    }
}
