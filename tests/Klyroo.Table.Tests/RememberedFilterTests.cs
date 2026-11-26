using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class RememberedFilterTests : BunitContext
{
    private sealed record TaskRow(int Id, string Title, string Team, decimal Hours);

    private static readonly DataListSchema<TaskRow> Schema = DataListSchema.For<TaskRow>(s => s
        .RowKey(t => t.Id)
        .Field(t => t.Title, f => f.Key("title").Sortable().Filter(DataListFilter.Text()))
        .Field(t => t.Team, f => f.Key("team").Sortable().Filter(DataListFilter.Option().WithPersist()))
        .Field(t => t.Hours, f => f.Key("hours").Sortable().Filter(DataListFilter.Number(min: 0)))
        .QueryFilter("team.lead", DataListFilter.Text())
        .DefaultSort(t => t.Title));

    private static readonly TaskRow[] Tasks =
    [
        new(1, "Alpha API", "Backend", 12m),
        new(2, "Beta UI", "Frontend", 8m),
        new(3, "Delta Tests", "Backend", 16m),
        new(4, "Epsilon Deploy", "Infra", 10m),
    ];

    private static readonly string StorageKey = "klyroo-dl-filter:" + typeof(TaskRow).FullName + ":tasks:team";

    private readonly BunitJSModuleInterop _storage;

    public RememberedFilterTests()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));
        _storage = JSInterop.SetupModule(KlyDataList<TaskRow>.FilterStorageModulePath);
        _storage.SetupVoid("write", _ => true).SetVoidResult();
    }

    private void Stored(string? value) => _storage.Setup<string?[]>("readAll", _ => true).SetResult([value]);

    private IRenderedComponent<KlyDataList<TaskRow>> RenderList() =>
        Render<KlyDataList<TaskRow>>(p => p
            .Add(x => x.Schema, Schema)
            .Add(x => x.Items, Tasks)
            .Add(x => x.UrlKey, "tasks")
            .AddChildContent<KlyDataListTable<TaskRow>>(t => t
                .AddChildContent<KlyDataListColumn<TaskRow>>(c => c.Add(x => x.Field, "title").Add(x => x.Title, "Title"))));

    private static IReadOnlyList<string> Titles(IRenderedComponent<KlyDataList<TaskRow>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.TextContent.Trim())];

    [Fact]
    public void Restores_the_stored_value_when_the_URL_has_none_and_writes_it_to_the_URL()
    {
        Stored("f.team=Backend");

        var cut = RenderList();

        cut.WaitForAssertion(() => Assert.Equal(["Alpha API", "Delta Tests"], Titles(cut)));
        var read = Assert.Single(_storage.Invocations, i => i.Identifier == "readAll");
        Assert.Equal([StorageKey], Assert.IsType<string[]>(read.Arguments[0]));
        Assert.Contains("tasks.f.team=Backend", Uri.UnescapeDataString(Services.GetRequiredService<NavigationManager>().Uri));
        // The value it starts with is not written back.
        Assert.DoesNotContain(_storage.Invocations, i => i.Identifier == "write");
    }

    [Fact]
    public void Does_not_restore_when_the_URL_has_a_value()
    {
        Stored("f.team=Backend");
        Services.GetRequiredService<NavigationManager>().NavigateTo("/?tasks.f.team=Infra");

        var cut = RenderList();

        cut.WaitForAssertion(() => Assert.Equal(["Epsilon Deploy"], Titles(cut)));
        Assert.DoesNotContain(_storage.Invocations, i => i.Identifier == "readAll");
    }

    [Fact]
    public void A_filter_whose_key_extends_the_remembered_one_does_not_count_as_its_value()
    {
        Stored("f.team=Backend");
        Services.GetRequiredService<NavigationManager>().NavigateTo("/?tasks.f.team.lead.is=Ann");

        var cut = RenderList();

        cut.WaitForAssertion(() => Assert.Equal(["Alpha API", "Delta Tests"], Titles(cut)));
        Assert.Single(_storage.Invocations, i => i.Identifier == "readAll");
    }

    [Fact]
    public void Saves_a_changed_value_and_removes_it_when_the_filter_is_cleared()
    {
        Stored(null);
        var cut = RenderList();
        cut.WaitForAssertion(() => Assert.Equal(4, Titles(cut).Count));

        cut.InvokeAsync(() => cut.Instance.SetState(s => s with { Filters = [new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Frontend", "Infra"]))] }, QueryHistory.Push));
        cut.WaitForAssertion(() => Assert.Equal(["Beta UI", "Epsilon Deploy"], Titles(cut)));
        var saved = Assert.Single(_storage.Invocations, i => i.Identifier == "write");
        Assert.Equal(StorageKey, saved.Arguments[0]);
        Assert.Equal("f.team=Frontend%2CInfra", saved.Arguments[1]);

        // Another filter does not write the remembered one again.
        cut.InvokeAsync(() => cut.Instance.SetState(s => s with { Filters = [.. s.Filters, new FilterRule("title", FilterOperator.Contains, new FilterValue.Text("Beta"))] }, QueryHistory.Push));
        cut.WaitForAssertion(() => Assert.Equal(["Beta UI"], Titles(cut)));
        Assert.Single(_storage.Invocations, i => i.Identifier == "write");

        cut.InvokeAsync(() => cut.Instance.ResetAll());
        cut.WaitForAssertion(() => Assert.Equal(2, _storage.Invocations.Count(i => i.Identifier == "write")));
        var cleared = _storage.Invocations.Last(i => i.Identifier == "write");
        Assert.Equal(StorageKey, cleared.Arguments[0]);
        Assert.Equal("", cleared.Arguments[1]);
    }

    [Fact]
    public void A_navigation_to_the_page_without_a_value_restores_the_stored_one()
    {
        Stored("f.team=Backend");
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/?tasks.f.team=Infra");
        var cut = RenderList();
        cut.WaitForAssertion(() => Assert.Equal(["Epsilon Deploy"], Titles(cut)));

        // A menu link back to the page: the address names no team, so the remembered one comes back and the address shows it.
        cut.InvokeAsync(() => navigation.NavigateTo("/"));

        cut.WaitForAssertion(() => Assert.Equal(["Alpha API", "Delta Tests"], Titles(cut)));
        Assert.Contains("tasks.f.team=Backend", Uri.UnescapeDataString(navigation.Uri));
    }

    [Fact]
    public void Clearing_the_filter_in_the_list_does_not_bring_the_stored_value_back()
    {
        Stored("f.team=Backend");
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/?tasks.f.team=Infra");
        var cut = RenderList();
        cut.WaitForAssertion(() => Assert.Equal(["Epsilon Deploy"], Titles(cut)));

        cut.InvokeAsync(() => cut.Instance.SetState(s => s with { Filters = [] }, QueryHistory.Push));

        cut.WaitForAssertion(() => Assert.Equal(4, Titles(cut).Count));
        Assert.DoesNotContain("f.team", navigation.Uri);
        Assert.DoesNotContain(_storage.Invocations, i => i.Identifier == "readAll");
        var cleared = Assert.Single(_storage.Invocations, i => i.Identifier == "write");
        Assert.Equal("", cleared.Arguments[1]);
    }

    [Fact]
    public void A_stored_value_is_read_like_the_address_and_keeps_only_its_own_filter()
    {
        var rules = KlyDataList<TaskRow>.DecodeRememberedFilter(Schema, "team", "q=x&f.hours.gt=3&f.team=Backend%2CInfra&f.team.bogus=1", null);

        var rule = Assert.Single(rules);
        Assert.Equal("team", rule.Key);
        Assert.Equal(FilterOperator.IsAnyOf, rule.Operator);
        Assert.Equal(["Backend", "Infra"], Assert.IsType<FilterValue.List>(rule.Value).Values);
        Assert.Empty(KlyDataList<TaskRow>.DecodeRememberedFilter(Schema, "team", "f.team.gt=3", null));
        Assert.Equal("", KlyDataList<TaskRow>.EncodeRememberedFilter(Schema, Schema.DefaultState, "team"));
    }

    [Fact]
    public void A_prerender_neither_reads_nor_writes_the_storage()
    {
        SetRendererInfo(new RendererInfo("Static", false));
        Stored("f.team=Backend");

        var cut = RenderList();

        Assert.Equal(4, Titles(cut).Count);
        Assert.Empty(_storage.Invocations);
    }

    [Fact]
    public void Without_URL_sync_a_bound_state_with_a_value_wins_and_one_without_takes_the_stored_value()
    {
        Stored("f.team=Infra");
        DataListState? reported = null;

        var cut = Render<KlyDataList<TaskRow>>(p => p
            .Add(x => x.Schema, Schema)
            .Add(x => x.Items, Tasks)
            .Add(x => x.UrlSync, false)
            .Add(x => x.UrlKey, "tasks")
            .Add(x => x.StateChanged, s => reported = s)
            .AddChildContent<KlyDataListTable<TaskRow>>(t => t
                .AddChildContent<KlyDataListColumn<TaskRow>>(c => c.Add(x => x.Field, "title"))));

        cut.WaitForAssertion(() => Assert.Equal(["Epsilon Deploy"], Titles(cut)));
        Assert.Contains(reported!.Filters, r => r.Key == "team");

        var given = Schema.DefaultState with { Filters = [new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Frontend"]))] };
        var bound = Render<KlyDataList<TaskRow>>(p => p
            .Add(x => x.Schema, Schema)
            .Add(x => x.Items, Tasks)
            .Add(x => x.UrlSync, false)
            .Add(x => x.State, given)
            .AddChildContent<KlyDataListTable<TaskRow>>(t => t
                .AddChildContent<KlyDataListColumn<TaskRow>>(c => c.Add(x => x.Field, "title"))));

        bound.WaitForAssertion(() => Assert.Equal(["Beta UI"], Titles(bound)));
        Assert.Single(_storage.Invocations, i => i.Identifier == "readAll");
    }
}
