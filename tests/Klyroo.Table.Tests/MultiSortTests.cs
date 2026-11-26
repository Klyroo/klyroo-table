using AngleSharp.Dom;
using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class MultiSortTests : TableHarness
{
    private static IReadOnlyList<string> Names(IRenderedComponent<KlyDataList<Person>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.QuerySelectorAll("td")[0].TextContent.Trim())];

    private DataListState? _state;

    private IRenderedComponent<KlyDataList<Person>> MountSorted(IReadOnlyList<DataListSort>? sort = null, bool menu = false, bool? multiple = null, Person[]? items = null) =>
        Mount(
            list: l =>
            {
                l.Add(x => x.StateChanged, EventCallback.Factory.Create<DataListState>(this, s => _state = s));
                if (sort is not null) l.Add(x => x.State, PersonSchema.DefaultState with { Sort = sort });
            },
            parts: b =>
            {
                if (menu)
                {
                    b.OpenComponent<KlyDataListSortMenu<Person>>(0);
                    if (multiple is not null) b.AddComponentParameter(1, nameof(KlyDataListSortMenu<Person>.Multiple), multiple);
                    b.CloseComponent();
                }
                b.OpenComponent<KlyDataListTable<Person>>(2);
                b.AddComponentParameter(3, nameof(KlyDataListTable<Person>.ChildContent), (RenderFragment)(cb =>
                {
                    var seq = 0;
                    Column(cb, ref seq, "name", "Name");
                    Column(cb, ref seq, "team", "Team");
                    Column(cb, ref seq, "salary", "Salary");
                }));
                b.CloseComponent();
            },
            items: items);

    [Fact]
    public void A_plain_click_keeps_one_sort_and_Shift_click_appends_cycles_and_removes()
    {
        Prepare();
        var cut = MountSorted();
        string[] Keys() => [.. (_state ?? PersonSchema.DefaultState).Sort.Select(s => s.Key + (s.Descending ? "-" : "+"))];
        IElement Header(string title) => cut.FindAll("thead th button").Single(b => b.TextContent.Trim().StartsWith(title));

        // Plain click on Team: replaces the default sort (name) by team ascending.
        Header("Team").Click();
        Assert.Equal(["team+"], Keys());

        // Shift+click on Salary: keeps team, adds salary.
        Header("Salary").Click(new MouseEventArgs { ShiftKey = true });
        Assert.Equal(["team+", "salary+"], Keys());

        // Shift+click on Team: turns it descending in place; the order of the keys is kept.
        Header("Team").Click(new MouseEventArgs { ShiftKey = true });
        Assert.Equal(["team-", "salary+"], Keys());

        // Shift+click again: Team leaves the list.
        Header("Team").Click(new MouseEventArgs { ShiftKey = true });
        Assert.Equal(["salary+"], Keys());

        // A plain click on a key that is not the only one goes back to one sort.
        Header("Name").Click(new MouseEventArgs { ShiftKey = true });
        Assert.Equal(["salary+", "name+"], Keys());
        Header("Name").Click();
        Assert.Equal(["name-"], Keys());
    }

    [Fact]
    public void Headers_show_the_order_number_only_while_several_sorts_are_active()
    {
        Prepare();
        var cut = MountSorted([new("team"), new("salary", true)]);

        var badges = cut.FindAll("thead [data-slot=sort-order]");
        Assert.Equal(["1", "2"], badges.Select(b => b.TextContent.Trim()).ToArray());
        var team = cut.FindAll("thead th").Single(th => th.GetAttribute("data-key") == "team");
        var salary = cut.FindAll("thead th").Single(th => th.GetAttribute("data-key") == "salary");
        Assert.Equal("ascending", team.GetAttribute("aria-sort"));
        // Only the first sort is the sort of the table for assistive technology; the second names its place in the button.
        Assert.Equal("none", salary.GetAttribute("aria-sort"));
        Assert.Contains("sort order 2", salary.QuerySelector("button")!.GetAttribute("aria-label"));
        Assert.Contains("Shift", salary.QuerySelector("button")!.GetAttribute("title"));
        // The rows follow both keys: Backend (Alice 120k before Claire 110k when salary descends), then the other teams.
        Assert.Equal(["Alice Martin", "Claire Petit", "Bob Chen", "David Kim"], Names(cut));
    }

    [Fact]
    public void A_single_sort_has_no_order_number()
    {
        Prepare();
        var cut = MountSorted([new("team")]);
        Assert.Empty(cut.FindAll("thead [data-slot=sort-order]"));
    }

    [Fact]
    public void The_sort_menu_edits_the_list_reverses_moves_removes_and_adds()
    {
        Prepare();
        var cut = MountSorted([new("team"), new("salary", true)], menu: true, multiple: true);
        string[] Keys() => [.. (_state ?? PersonSchema.DefaultState).Sort.Select(s => s.Key + (s.Descending ? "-" : "+"))];
        Assert.Contains("2 fields", cut.Find("[data-slot=sort-trigger]").TextContent);

        var items = cut.FindAll("[data-slot=sort-order-item]");
        Assert.Equal(["team", "salary"], items.Select(i => i.GetAttribute("data-sort-key")).ToArray());
        // The first cannot move earlier, the last cannot move later.
        Assert.True(items[0].QuerySelector("[data-slot=sort-order-earlier]")!.HasAttribute("disabled"));
        Assert.True(items[1].QuerySelector("[data-slot=sort-order-later]")!.HasAttribute("disabled"));

        items[1].QuerySelector("[data-slot=sort-order-earlier]")!.Click();
        Assert.Equal(["salary-", "team+"], Keys());

        cut.FindAll("[data-slot=sort-order-item]")[0].QuerySelector("[data-slot=sort-order-reverse]")!.Click();
        Assert.Equal(["salary+", "team+"], Keys());

        cut.FindAll("[data-slot=sort-order-item]")[1].QuerySelector("[data-slot=sort-order-remove]")!.Click();
        Assert.Equal(["salary+"], Keys());

        // A field that is not sorted yet is offered to add; one that is, is not.
        var addable = cut.FindAll("[data-slot=sort-list] [data-slot=sort-item]").Select(b => b.GetAttribute("data-sort-key")).ToArray();
        Assert.DoesNotContain("salary", addable);
        Assert.Contains("name", addable);
        cut.Find("[data-slot=sort-list] [data-sort-key=name]").Click();
        Assert.Equal(["salary+", "name+"], Keys());
    }

    [Fact]
    public void The_sort_menu_keeps_picking_one_field_while_the_list_sorts_by_one()
    {
        Prepare();
        var single = MountSorted([new("team")], menu: true);
        Assert.Empty(single.FindAll("[data-slot=sort-order-item]"));
        Assert.NotEmpty(single.FindAll("[data-slot=sort-direction]"));
    }

    [Fact]
    public void The_sort_menu_turns_into_the_editor_by_itself_once_two_sorts_are_active()
    {
        Prepare();
        var cut = MountSorted([new("team"), new("salary")], menu: true);
        Assert.Equal(2, cut.FindAll("[data-slot=sort-order-item]").Count);
        Assert.Empty(cut.FindAll("[data-slot=sort-direction]"));
    }

    [Fact]
    public void The_sort_menu_edits_a_single_sort_when_asked_to()
    {
        Prepare();
        var forced = MountSorted([new("team")], menu: true, multiple: true);
        Assert.Single(forced.FindAll("[data-slot=sort-order-item]"));
        Assert.Empty(forced.FindAll("[data-slot=sort-direction]"));
    }

    [Fact]
    public void Context_sort_helpers_never_leave_a_key_twice_and_clamp_moves()
    {
        Prepare();
        DataListContext<Person>? context = null;
        Mount(parts: b =>
        {
            b.OpenComponent<CaptureContext>(0);
            b.AddComponentParameter(1, nameof(CaptureContext.OnContext), (Action<DataListContext<Person>>)(c => context = c));
            b.CloseComponent();
        });
        Assert.NotNull(context);
        var state = PersonSchema.DefaultState with { Sort = [new("team"), new("salary")], Page = 3 };

        var added = context!.AddSort(state, "team", descending: true);
        Assert.Equal(["team-", "salary+"], added.Sort.Select(s => s.Key + (s.Descending ? "-" : "+")).ToArray());
        Assert.Equal(1, added.Page);

        Assert.Equal(["salary", "team"], context.MoveSort(state, "team", 5).Sort.Select(s => s.Key).ToArray());
        Assert.Equal(["team", "salary"], context.MoveSort(state, "team", -5).Sort.Select(s => s.Key).ToArray());
        Assert.Same(state, context.MoveSort(state, "nope", 1));
        Assert.Null(context.SortIndex("team"));
        Assert.Equal(0, context.SortIndex("name"));

        // Removing the last sort falls back to the default sort of the schema.
        var cleared = context.RemoveSort(PersonSchema.DefaultState with { Sort = [new("team")] }, "team");
        Assert.Empty(cleared.Sort);
        Assert.Equal(["name"], context.EffectiveSort.Select(s => s.Key).ToArray());
    }

    private sealed class CaptureContext : DataListPartBase<Person>
    {
        [Parameter] public Action<DataListContext<Person>>? OnContext { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            OnContext?.Invoke(Context);
        }
    }
}
