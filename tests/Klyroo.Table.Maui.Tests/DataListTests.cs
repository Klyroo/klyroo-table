namespace Klyroo.Table.Maui.Tests;

public class DataListTests
{
    private static KlyDataListTable<Person> TableWithColumns() => new()
    {
        Columns =
        {
            new KlyDataListColumn<Person, string> { Property = p => p.Name, Title = "Name" },
            new KlyDataListColumn<Person, string> { Property = p => p.Team, Title = "Team" },
            new KlyDataListColumn<Person, decimal> { Property = p => p.Salary, Format = "N0", Align = DataListAlign.End },
        },
    };

    [Fact]
    public async Task A_part_below_the_list_finds_it()
    {
        var search = new KlyDataListSearch<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        Assert.Same(list.Context, search.Context);
    }

    [Fact]
    public async Task The_list_loads_its_first_page_with_the_schema_defaults()
    {
        var list = await TableHarness.Mount();
        var context = list.Context;
        Assert.False(context.IsInitial);
        Assert.Equal(["Alice Martin", "Bob Chen"], context.Rows.Select(p => p.Name));
        Assert.Equal(5, context.Result!.TotalCount);
        Assert.Equal(1, context.State.Page);
        Assert.Equal(2, context.State.PageSize);
    }

    [Fact]
    public async Task A_list_without_a_source_does_not_start()
    {
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema };
        await Assert.ThrowsAsync<InvalidOperationException>(() => list.LoadAsync());
    }

    [Fact]
    public async Task A_list_with_two_sources_does_not_start()
    {
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Items = TableHarness.People(), Provider = (_, _) => ValueTask.FromResult(new DataListResult<Person>([], 0, null, null)) };
        await Assert.ThrowsAsync<InvalidOperationException>(() => list.LoadAsync());
    }

    [Fact]
    public async Task A_provider_without_a_schema_is_refused()
    {
        var list = new KlyDataList<Person> { Provider = (_, _) => ValueTask.FromResult(new DataListResult<Person>([], 0, null, null)) };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => list.LoadAsync());
        Assert.Contains("Schema", error.Message);
    }

    [Fact]
    public async Task The_columns_describe_the_schema_of_a_list_without_one()
    {
        var table = TableWithColumns();
        var list = new KlyDataList<Person> { Items = TableHarness.People(), Content = new VerticalStackLayout { Children = { table } } };
        await list.LoadAsync();
        Assert.Equal(["name", "team", "salary"], list.Context.Schema!.SortKeys);
        Assert.Equal(5, list.Context.Result!.TotalCount);
    }

    [Fact]
    public async Task A_state_change_loads_the_new_page_and_reports_it_once()
    {
        var list = await TableHarness.Mount();
        var reported = new List<DataListState>();
        list.StateChanged += (_, state) => reported.Add(state);
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        Assert.Equal(["Claire Petit", "David Kim"], list.Context.Rows.Select(p => p.Name));
        Assert.Single(reported);
        Assert.Equal(2, list.State!.Page);
    }

    [Fact]
    public async Task A_state_set_from_outside_loads_the_list()
    {
        var list = await TableHarness.Mount();
        list.State = list.State! with { Page = 3 };
        await list.Context.WhenLoaded();
        Assert.Equal(["Emma Stone"], list.Context.Rows.Select(p => p.Name));
    }

    [Fact]
    public async Task A_page_past_the_end_goes_to_the_last_one_and_the_state_follows()
    {
        var list = await TableHarness.Mount();
        list.Context.SetState(s => s with { Page = 99 });
        await list.Context.WhenLoaded();
        Assert.Equal(3, list.Context.State.Page);
        Assert.Equal(3, list.State!.Page);
    }

    [Fact]
    public async Task A_search_filters_the_rows_and_goes_back_to_the_first_page()
    {
        var list = await TableHarness.Mount();
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        list.Context.SetState(s => s with { Search = "backend", Page = 1 });
        await list.Context.WhenLoaded();
        Assert.Equal(["Alice Martin", "Claire Petit"], list.Context.Rows.Select(p => p.Name));
        Assert.True(list.Context.IsFiltered);
        list.Context.Reset();
        await list.Context.WhenLoaded();
        Assert.False(list.Context.IsFiltered);
    }

    [Fact]
    public async Task A_plain_tap_on_a_sort_goes_ascending_then_descending_then_back_to_the_default()
    {
        var list = await TableHarness.Mount();
        var context = list.Context;
        var first = context.NextSort(context.State, "salary");
        Assert.Equal([new DataListSort("salary", false)], first.Sort);
        var second = context.NextSort(first, "salary");
        Assert.Equal([new DataListSort("salary", true)], second.Sort);
        var third = context.NextSort(second, "salary");
        Assert.Equal(context.Schema!.DefaultSort, third.Sort);
        Assert.All([first, second, third], s => Assert.Equal(1, s.Page));
    }

    [Fact]
    public async Task A_shift_tap_adds_to_the_sort_and_removes_it_after_descending()
    {
        var list = await TableHarness.Mount();
        var context = list.Context;
        var state = context.NextSort(context.State, "team", append: true);
        Assert.Equal([new DataListSort("name"), new DataListSort("team")], state.Sort);
        state = context.NextSort(state, "team", append: true);
        Assert.Equal([new DataListSort("name"), new DataListSort("team", true)], state.Sort);
        state = context.NextSort(state, "team", append: true);
        Assert.Equal([new DataListSort("name")], state.Sort);
        Assert.Equal(0, context.SortIndex("name"));
        Assert.Null(context.SortDescending("team"));
    }

    [Fact]
    public async Task The_layout_changes_without_loading_the_rows_again()
    {
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(new KlyDataListTable<Person>());
            s.Children.Add(new KlyDataListGrid<Person>());
        });
        var version = list.Context.ResultVersion;
        Assert.Equal(DataListLayout.Table, list.Context.EffectiveLayout);
        list.Context.SetLayout(DataListLayout.Grid);
        await list.Context.WhenLoaded();
        Assert.Equal(DataListLayout.Grid, list.Context.EffectiveLayout);
        Assert.Equal(version, list.Context.ResultVersion);
    }

    [Fact]
    public async Task With_only_a_table_the_layout_is_the_table()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(new KlyDataListTable<Person>()));
        list.Context.SetLayout(DataListLayout.Grid);
        Assert.Equal(DataListLayout.Table, list.Context.EffectiveLayout);
    }

    [Fact]
    public async Task A_queryable_over_a_list_in_memory_is_refused_like_in_the_blazor_list()
    {
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Queryable = TableHarness.People().AsQueryable(), Executor = new CountingExecutor() };
        await list.LoadAsync();
        Assert.True(list.Context.HasError);
        Assert.Contains("database provider", list.Context.Error!.Message);
    }

    private sealed class CountingExecutor : IDataListQueryExecutor
    {
        public int Calls;

        public ValueTask<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            Calls++;
            return ValueTask.FromResult(query.Count());
        }

        public ValueTask<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            Calls++;
            return ValueTask.FromResult(query.ToList());
        }
    }

    [Fact]
    public async Task A_failing_provider_sets_the_error_and_a_refresh_tries_again()
    {
        var fail = true;
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = (_, _) => fail ? throw new InvalidOperationException("down") : ValueTask.FromResult(new DataListResult<Person>(TableHarness.People(), 5, null, null)),
        };
        await list.LoadAsync();
        Assert.True(list.Context.HasError);
        Assert.Equal("down", list.Context.Error!.Message);
        fail = false;
        await list.Context.RefreshAsync();
        Assert.False(list.Context.HasError);
        Assert.Equal(5, list.Context.Rows.Count);
    }

    [Fact]
    public async Task A_collection_that_changes_loads_the_list_again()
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<Person>(TableHarness.People());
        var list = await TableHarness.Mount(items: items);
        Assert.Equal(5, list.Context.Result!.TotalCount);
        items.Add(new Person(6, "Zed", "Infra", 1m, new(2020, 1, 1)));
        await list.Context.WhenLoaded();
        Assert.Equal(6, list.Context.Result!.TotalCount);
    }

    [Fact]
    public async Task Changing_the_paging_mode_starts_again_from_the_first_rows()
    {
        var list = await TableHarness.Mount();
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        list.PagingMode = DataListPagingMode.Cursor;
        await list.Context.WhenLoaded();
        Assert.Equal(1, list.Context.State.Page);
        Assert.Equal(DataListPagingMode.Cursor, list.Context.PagingMode);
        Assert.True(list.Context.HasMore);
    }

    [Fact]
    public async Task The_texts_follow_the_messages_of_the_list()
    {
        var list = await TableHarness.Mount();
        Assert.Equal("No results", list.Context.Messages.NoResults);
        list.Messages = KlyToolsLocales.Fr;
        Assert.Equal(KlyToolsLocales.Fr.NoResults, list.Context.Messages.NoResults);
    }

    [Theory]
    [InlineData("fr-FR", "fr")]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("zh-TW", "zh-Hans")]
    [InlineData("sv-SE", "en")]
    public void The_messages_follow_the_language_of_the_user(string culture, string table)
    {
        Assert.Same(KlyToolsLocales.Tables[table], TableMessages.For(new System.Globalization.CultureInfo(culture)));
    }

    [Fact]
    public async Task A_row_is_replaced_in_place_by_its_key()
    {
        var list = await TableHarness.Mount();
        var version = list.Context.ResultVersion;
        Assert.True(list.Context.UpdateRow(list.Context.Rows[0] with { Team = "Infra" }));
        Assert.Equal("Infra", list.Context.Rows[0].Team);
        Assert.Equal(version + 1, list.Context.ResultVersion);
        Assert.False(list.Context.UpdateRow(new Person(99, "Nobody", "x", 0, new(2020, 1, 1))));
    }

    [Fact]
    public async Task The_part_given_a_list_by_code_takes_its_state_from_it()
    {
        var list = await TableHarness.Mount();
        var count = new KlyDataListResultCount<Person> { List = list };
        Assert.Same(list.Context, count.Context);
        Assert.Equal("1–2 of 5", count.Text);
    }

    [Fact]
    public async Task Without_content_the_list_builds_the_default_layout()
    {
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Items = TableHarness.People() };
        await list.LoadAsync();
        var parts = ((VerticalStackLayout)list.Content!).Descendants<Element>().ToList();
        Assert.NotEmpty(parts.OfType<KlyDataListSearch<Person>>());
        Assert.NotEmpty(parts.OfType<KlyDataListResultCount<Person>>());
        Assert.NotEmpty(parts.OfType<KlyDataListRefresh<Person>>());
        Assert.NotEmpty(parts.OfType<KlyDataListFilterTags<Person>>());
        Assert.NotEmpty(parts.OfType<KlyDataListErrorState<Person>>());
        Assert.NotEmpty(parts.OfType<KlyDataListPagination<Person>>());
        Assert.Empty(parts.OfType<KlyDataListInfiniteLoader<Person>>());
        var table = parts.OfType<KlyDataListTable<Person>>().Single();
        Assert.Contains("Alice Martin", table.Descendants<Label>().Select(l => l.Text));
        Assert.All(parts.OfType<KlyDataListResultCount<Person>>(), c => Assert.Equal("1–2 of 5", c.Text));
    }

    [Fact]
    public async Task The_default_layout_of_a_cursor_list_has_the_loader_and_no_pagination()
    {
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Items = TableHarness.People(), PagingMode = DataListPagingMode.Cursor };
        await list.LoadAsync();
        var parts = ((VerticalStackLayout)list.Content!).Descendants<Element>().ToList();
        Assert.NotEmpty(parts.OfType<KlyDataListInfiniteLoader<Person>>());
        Assert.Empty(parts.OfType<KlyDataListPagination<Person>>());
    }

    [Fact]
    public async Task A_list_without_a_search_field_in_its_schema_has_no_search_part()
    {
        var schema = DataListSchema.For<Person>(s => s.RowKey(p => p.Id).Field(p => p.Name, f => f.Key("name").Sortable()));
        var list = new KlyDataList<Person> { Schema = schema, Items = TableHarness.People() };
        await list.LoadAsync();
        Assert.Empty(((VerticalStackLayout)list.Content!).Descendants<KlyDataListSearch<Person>>());
        Assert.Empty(((VerticalStackLayout)list.Content!).Descendants<KlyDataListFilterTags<Person>>());
    }
}
