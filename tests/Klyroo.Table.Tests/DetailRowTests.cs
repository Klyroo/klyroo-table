using Bunit;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class DetailRowTests : TableHarness
{
    private static readonly RenderFragment<Person> Detail = p => b => b.AddMarkupContent(0, $"<p class=\"detail-body\">Hired on {p.Hired:yyyy-MM-dd} in {p.Team}</p>");

    private DataListContext<Person>? _context;

    private IRenderedComponent<KlyDataList<Person>> MountDetail(
        Action<ComponentParameterCollectionBuilder<KlyDataListTable<Person>>>? table = null,
        Action<ComponentParameterCollectionBuilder<KlyDataList<Person>>>? list = null) =>
        Mount(
            t =>
            {
                t.Add(x => x.DetailTemplate, Detail);
                table?.Invoke(t);
            },
            list: list,
            before: b =>
            {
                b.OpenComponent<CaptureContext>(0);
                b.AddComponentParameter(1, nameof(CaptureContext.OnContext), (Action<DataListContext<Person>>)(c => _context = c));
                b.CloseComponent();
            });

    [Fact]
    public void Every_row_gets_a_toggle_in_a_first_column_and_no_detail_until_it_is_opened()
    {
        Prepare();
        var cut = MountDetail();

        var header = cut.Find("thead th");
        Assert.Equal("detail", header.GetAttribute("data-key"));
        Assert.Contains("Details", header.TextContent);
        var toggles = cut.FindAll("tbody button[data-slot=detail-toggle]");
        Assert.Equal(4, toggles.Count);
        Assert.Equal("false", toggles[0].GetAttribute("aria-expanded"));
        Assert.Equal("Show details for Alice Martin", toggles[0].GetAttribute("aria-label"));
        Assert.Null(toggles[0].GetAttribute("aria-controls"));
        Assert.Empty(cut.FindAll("tr[data-slot=detail-row]"));
        // The toggle is the first cell of its row.
        Assert.Same(toggles[0].ParentElement, cut.Find("tbody tr").QuerySelector("td"));
    }

    [Fact]
    public void Clicking_the_toggle_opens_a_full_width_row_under_its_row_and_clicking_again_closes_it()
    {
        Prepare();
        var cut = MountDetail();

        cut.FindAll("tbody button[data-slot=detail-toggle]")[1].Click();

        var detail = cut.Find("tr[data-slot=detail-row]");
        Assert.Equal("Bob Chen", detail.PreviousElementSibling!.QuerySelectorAll("td")[1].TextContent.Trim());
        Assert.Contains("Hired on 2025-01-10 in Frontend", detail.TextContent);
        // One cell for the toggle, three columns.
        Assert.Equal("4", detail.QuerySelector("td")!.GetAttribute("colspan"));
        var toggle = cut.FindAll("tbody button[data-slot=detail-toggle]")[1];
        Assert.Equal("true", toggle.GetAttribute("aria-expanded"));
        Assert.Equal("Hide details for Bob Chen", toggle.GetAttribute("aria-label"));
        Assert.Equal(detail.Id, toggle.GetAttribute("aria-controls"));
        Assert.True(_context!.IsRowExpanded("2"));
        Assert.False(_context.IsRowExpanded("1"));

        toggle.Click();
        Assert.Empty(cut.FindAll("tr[data-slot=detail-row]"));
        Assert.False(_context.IsRowExpanded("2"));
    }

    [Fact]
    public void The_detail_follows_the_row_expansion_state_of_the_list()
    {
        Prepare();
        var cut = MountDetail();
        Assert.True(_context!.HasDetail);

        cut.InvokeAsync(() => _context.SetRowExpanded("3", true));
        var detail = Assert.Single(cut.FindAll("tr[data-slot=detail-row]"));
        Assert.Contains("Claire Petit", detail.PreviousElementSibling!.TextContent);
        Assert.Contains("3", _context.ExpandedKeys);

        cut.InvokeAsync(_context.ExpandAll);
        Assert.Equal(4, cut.FindAll("tr[data-slot=detail-row]").Count);

        cut.InvokeAsync(_context.CollapseAll);
        Assert.Empty(cut.FindAll("tr[data-slot=detail-row]"));
    }

    [Fact]
    public void Arrow_keys_on_the_toggle_open_and_close_the_row()
    {
        Prepare();
        var cut = MountDetail();

        cut.FindAll("tbody button[data-slot=detail-toggle]")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Single(cut.FindAll("tr[data-slot=detail-row]"));
        cut.FindAll("tbody button[data-slot=detail-toggle]")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        Assert.Empty(cut.FindAll("tr[data-slot=detail-row]"));
    }

    [Fact]
    public void The_detail_spans_the_selection_and_actions_columns_too()
    {
        Prepare();
        var actions = new List<DataListRowAction<Person>> { new() { Key = "edit", Label = "Edit", OnExecute = _ => Task.CompletedTask } };
        var cut = MountDetail(t => t.Add(x => x.RowActions, actions), l => l.Add(x => x.Selectable, true));

        cut.FindAll("tbody button[data-slot=detail-toggle]")[0].Click();

        // Toggle, selection box, three columns, actions.
        Assert.Equal("6", cut.Find("tr[data-slot=detail-row] td").GetAttribute("colspan"));
        Assert.Equal(6, cut.FindAll("thead th").Count);
        Assert.Equal(6, cut.Find("tbody tr").QuerySelectorAll("td").Length);
    }

    [Fact]
    public void A_static_render_draws_disabled_toggles_and_no_detail()
    {
        Prepare(interactive: false);
        var cut = MountDetail();

        Assert.All(cut.FindAll("tbody button[data-slot=detail-toggle]"), b => Assert.True(b.HasAttribute("disabled")));
        Assert.Empty(cut.FindAll("tr[data-slot=detail-row]"));
    }

    [Fact]
    public void Virtualize_and_Tree_switch_the_detail_off()
    {
        Prepare();
        var virtualized = MountDetail(t => t.Add(x => x.Virtualize, true));
        Assert.Empty(virtualized.FindAll("button[data-slot=detail-toggle]"));
        Assert.False(_context!.HasDetail);
    }

    [Fact]
    public void A_table_without_a_detail_template_has_no_detail_column()
    {
        Prepare();
        var cut = Mount();
        Assert.Empty(cut.FindAll("button[data-slot=detail-toggle]"));
        Assert.DoesNotContain(cut.FindAll("thead th"), th => th.GetAttribute("data-key") == "detail");
    }
}
