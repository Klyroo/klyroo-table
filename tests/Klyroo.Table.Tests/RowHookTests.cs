using Bunit;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class RowHookTests : TableHarness
{
    [Fact]
    public void RowHref_draws_one_stretched_named_link_per_row_in_a_static_render()
    {
        Prepare(interactive: false);
        var cut = Mount(t => t.Add(x => x.RowHref, p => p.Id == 4 ? null : "/people/" + p.Id));

        var links = cut.FindAll("tbody a[data-slot=row-link]");
        Assert.Equal(3, links.Count);
        Assert.Equal("/people/1", links[0].GetAttribute("href"));
        Assert.Equal("Open Alice Martin", links[0].GetAttribute("aria-label"));
        // The link sits in the first column of its row and rows with no address get none.
        Assert.Equal("td", links[0].ParentElement!.TagName.ToLowerInvariant());
        Assert.Null(links[0].ParentElement!.PreviousElementSibling);
        Assert.Empty(cut.FindAll("tbody tr").Last().QuerySelectorAll("a"));
        Assert.All(cut.FindAll("tbody tr").Take(3), tr => Assert.Contains("relative", tr.GetAttribute("class")));
    }

    [Fact]
    public void RowHref_keeps_checkbox_and_action_clicks_out_of_the_link()
    {
        Prepare();
        IReadOnlySet<string>? selected = null;
        var cut = Mount(
            t => t.Add(x => x.RowHref, p => "/people/" + p.Id),
            list: l => l
                .Add(x => x.Selectable, true)
                .Add(x => x.SelectedKeysChanged, EventCallback.Factory.Create<IReadOnlySet<string>>(this, k => selected = k)));

        var box = cut.Find("tbody tr:first-child input[data-select=row]");
        var link = cut.Find("tbody tr:first-child a[data-slot=row-link]");
        // The box is not inside the link, and the link does not carry the box.
        Assert.False(link.Contains(box));
        box.Change(true);
        Assert.Equal(["1"], selected);
        Assert.Contains("data-select", cut.Find("tbody tr:first-child input").OuterHtml);
    }

    [Fact]
    public async Task OnRowClick_reports_the_row_and_the_modifier_keys_and_ignores_flagged_events()
    {
        Prepare();
        var clicks = new List<DataListRowClickArgs<Person>>();
        var cut = Mount(t => t.Add(x => x.OnRowClick, EventCallback.Factory.Create<DataListRowClickArgs<Person>>(this, a => clicks.Add(a))));

        var row = cut.Find("tbody tr:nth-child(2)");
        Assert.Equal("0", row.GetAttribute("tabindex"));
        Assert.Equal("true", row.GetAttribute("data-clickable"));

        await row.TriggerEventAsync("onklyrowclick", new KlyRowEventArgs { Ignore = true });
        Assert.Empty(clicks);

        await row.TriggerEventAsync("onklyrowclick", new KlyRowEventArgs { ShiftKey = true, CtrlKey = true });
        var click = Assert.Single(clicks);
        Assert.Equal(2, click.Item.Id);
        Assert.Equal("2", click.Key);
        Assert.Equal(2, click.Position);
        Assert.True(click.ShiftKey);
        Assert.True(click.CtrlKey);
        Assert.False(click.AltKey);
        Assert.False(click.FromKeyboard);
    }

    [Fact]
    public async Task A_clickable_row_is_activated_with_Enter_and_Space_on_the_row_itself()
    {
        Prepare();
        var clicks = new List<DataListRowClickArgs<Person>>();
        var cut = Mount(t => t.Add(x => x.OnRowClick, EventCallback.Factory.Create<DataListRowClickArgs<Person>>(this, a => clicks.Add(a))));

        var row = cut.Find("tbody tr:first-child");
        await row.TriggerEventAsync("onklyrowkey", new KlyRowEventArgs { Key = "Enter" });
        await row.TriggerEventAsync("onklyrowkey", new KlyRowEventArgs { Key = " " });
        await row.TriggerEventAsync("onklyrowkey", new KlyRowEventArgs { Key = "a", Ignore = true });

        Assert.Equal(2, clicks.Count);
        Assert.All(clicks, c => Assert.True(c.FromKeyboard));
        Assert.All(clicks, c => Assert.Equal(1, c.Item.Id));
    }

    [Fact]
    public void A_static_render_has_no_row_click()
    {
        Prepare(interactive: false);
        var cut = Mount(t => t.Add(x => x.OnRowClick, EventCallback.Factory.Create<DataListRowClickArgs<Person>>(this, _ => { })));

        var row = cut.Find("tbody tr:first-child");
        Assert.Null(row.GetAttribute("tabindex"));
        Assert.Null(row.GetAttribute("data-clickable"));
    }

    [Fact]
    public async Task A_row_with_an_address_also_raises_OnRowClick_from_its_link()
    {
        Prepare();
        var clicks = new List<DataListRowClickArgs<Person>>();
        var cut = Mount(t => t
            .Add(x => x.RowHref, p => "/people/" + p.Id)
            .Add(x => x.OnRowClick, EventCallback.Factory.Create<DataListRowClickArgs<Person>>(this, a => clicks.Add(a))));

        var row = cut.Find("tbody tr:first-child");
        // The row is not activated through the browser events: its link is.
        Assert.Null(row.GetAttribute("tabindex"));
        await cut.Find("tbody tr:first-child a[data-slot=row-link]").ClickAsync(new MouseEventArgs { CtrlKey = true });
        var click = Assert.Single(clicks);
        Assert.Equal(1, click.Item.Id);
        Assert.True(click.CtrlKey);
    }

    [Fact]
    public void RowClass_RowAttributes_and_CellClass_style_rows_and_cells()
    {
        Prepare();
        var cut = Mount(
            t => t
                .Add(x => x.RowClass, p => p.Team == "Backend" ? "is-backend" : null)
                .Add(x => x.RowAttributes, p => new Dictionary<string, object>
                {
                    ["data-id"] = p.Id,
                    ["title"] = p.Name,
                    ["class"] = "ignored",
                    ["data-slot"] = "ignored",
                }),
            columns: b =>
            {
                var seq = 0;
                Column(b, ref seq, "name", "Name");
                Column(b, ref seq, "salary", "Salary", (cb, s) => cb.AddComponentParameter(s, nameof(KlyDataListColumn<Person>.CellClass), (Func<Person, string?>)(p => p.Salary > 100_000m ? "high-pay" : null)));
            });

        var rows = cut.FindAll("tbody tr");
        Assert.Contains("is-backend", rows[0].GetAttribute("class"));
        Assert.DoesNotContain("is-backend", rows[1].GetAttribute("class"));
        Assert.DoesNotContain("ignored", rows[0].GetAttribute("class"));
        Assert.Equal("row", rows[0].GetAttribute("data-slot"));
        Assert.Equal("1", rows[0].GetAttribute("data-id"));
        Assert.Equal("Alice Martin", rows[0].GetAttribute("title"));

        var salaryCells = rows.Select(r => r.QuerySelectorAll("td")[1]).ToList();
        Assert.Contains("high-pay", salaryCells[0].GetAttribute("class"));
        Assert.DoesNotContain("high-pay", salaryCells[1].GetAttribute("class"));
        // The header is not touched.
        Assert.DoesNotContain("high-pay", cut.FindAll("thead th")[1].GetAttribute("class"));
    }
}
