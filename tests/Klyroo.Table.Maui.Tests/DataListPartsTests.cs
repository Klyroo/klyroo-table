namespace Klyroo.Table.Maui.Tests;

/// <summary>The small parts of a data list: search, refresh, result count, empty and error states, filters buttons, layout and view switches, paging, sorting and group headers.</summary>
public class DataListPartsTests
{
    private static DataListProvider<Person> Counting(Action onCall, Func<int, bool>? fail = null)
    {
        var calls = 0;
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        return (request, token) =>
        {
            onCall();
            return fail?.Invoke(++calls) == true ? throw new InvalidOperationException("down") : inner(request, token);
        };
    }

    // ---- search

    [Fact]
    public async Task The_search_field_searches_when_the_typing_stops_and_goes_back_to_the_first_page()
    {
        var search = new KlyDataListSearch<Person> { Debounce = 0 };
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        search.Type("backend");
        await list.Context.WhenLoaded();
        Assert.Equal("backend", list.Context.State.Search);
        Assert.Equal(1, list.Context.State.Page);
        Assert.Equal(["Alice Martin", "Claire Petit"], list.Context.Rows.Select(p => p.Name));
        Assert.True(search.ClearButton.IsVisible);
    }

    [Fact]
    public async Task The_search_waits_for_the_pause_or_the_return_key()
    {
        var search = new KlyDataListSearch<Person> { Debounce = 60_000 };
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        search.Type("infra");
        Assert.Equal("", list.Context.State.Search);
        search.Input.Text = "infra ";
        search.ApplyNow();
        Assert.Equal("infra", list.Context.State.Search);
        search.Type("frontend");
        Assert.Equal("infra", list.Context.State.Search);
        ((IEntry)(Entry)search.Input.Content!).Completed();
        Assert.Equal("frontend", list.Context.State.Search);
    }

    [Fact]
    public async Task A_search_shorter_than_the_minimum_searches_nothing()
    {
        var search = new KlyDataListSearch<Person> { Debounce = 0, MinLength = 3 };
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        search.Type("ba");
        Assert.Equal("", list.Context.State.Search);
        search.Type("bac");
        Assert.Equal("bac", list.Context.State.Search);
    }

    [Fact]
    public async Task The_clear_button_empties_the_field_and_the_search()
    {
        var search = new KlyDataListSearch<Person> { Debounce = 0 };
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        search.Type("infra");
        search.ClearButton.Activate();
        Assert.Equal("", search.Input.Text);
        Assert.Equal("", list.Context.State.Search);
        Assert.False(search.ClearButton.IsVisible);
    }

    [Fact]
    public async Task A_search_set_from_outside_is_written_into_the_field()
    {
        var search = new KlyDataListSearch<Person> { Debounce = 0 };
        var list = await TableHarness.Mount(s => s.Children.Add(search));
        list.Context.SetState(s => s with { Search = "infra" });
        Assert.Equal("infra", search.Input.Text);
        list.Context.Reset();
        Assert.Equal("", search.Input.Text);
    }

    [Fact]
    public async Task The_search_has_an_accessible_name_and_a_placeholder()
    {
        var search = new KlyDataListSearch<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(search), configure: l => l.Label = "Staff");
        Assert.Equal("Search Staff", search.Input.AccessibleName);
        Assert.Equal("Search…", search.Input.Placeholder);
        search.AccessibleName = "Find people";
        search.Placeholder = "Name or team";
        Assert.Equal("Find people", search.Input.AccessibleName);
        Assert.Equal("Name or team", search.Input.Placeholder);
        Assert.Equal(KlySize.Lg, (search.Size = KlySize.Lg) == KlySize.Lg ? search.Input.Size : KlySize.Md);
        _ = list;
    }

    // ---- refresh

    [Fact]
    public async Task The_refresh_button_loads_the_rows_again()
    {
        var calls = 0;
        var refresh = new KlyDataListRefresh<Person>();
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = Counting(() => calls++), Content = new VerticalStackLayout { Children = { refresh } } };
        await list.LoadAsync();
        Assert.Equal(1, calls);
        Assert.Equal("Refresh", refresh.Button.TextView.Text);
        refresh.Button.Activate();
        await list.Context.WhenLoaded();
        Assert.True(calls >= 2);
    }

    [Fact]
    public async Task The_refresh_button_shows_a_spinner_while_the_rows_are_loaded_again()
    {
        var gate = new TaskCompletionSource();
        var first = true;
        var refresh = new KlyDataListRefresh<Person>();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = async (request, token) =>
            {
                if (!first) await gate.Task;
                first = false;
                return await inner(request, token);
            },
            Content = new VerticalStackLayout { Children = { refresh } },
        };
        await list.LoadAsync();
        Assert.False(refresh.Button.IsBusy);
        var pending = list.Context.RefreshAsync();
        Assert.True(refresh.Button.IsBusy);
        gate.SetResult();
        await pending;
        Assert.False(refresh.Button.IsBusy);
    }

    // ---- result count

    [Fact]
    public async Task The_result_count_says_which_rows_are_on_screen()
    {
        var count = new KlyDataListResultCount<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(count));
        Assert.Equal("1–2 of 5", count.Text);
        list.Context.SetState(s => s with { Page = 3 });
        await list.Context.WhenLoaded();
        Assert.Equal("5–5 of 5", count.Text);
        list.Context.SetState(s => s with { Search = "zzz", Page = 1 });
        await list.Context.WhenLoaded();
        Assert.Equal("No results for “zzz”", count.Text);
        list.Context.SetState(s => s with { Search = "", Filters = [new FilterRule("name", FilterOperator.Contains, new FilterValue.Text("zzz"))] });
        await list.Context.WhenLoaded();
        Assert.Equal("No results", count.Text);
    }

    [Fact]
    public async Task The_result_count_says_loading_before_the_first_result()
    {
        var gate = new TaskCompletionSource();
        var count = new KlyDataListResultCount<Person>();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = async (r, t) => { await gate.Task; return await inner(r, t); }, Content = new VerticalStackLayout { Children = { count } } };
        var loading = list.LoadAsync();
        Assert.Equal("Loading…", count.Text);
        gate.SetResult();
        await loading;
        Assert.Equal("1–2 of 5", count.Text);
    }

    [Fact]
    public async Task The_result_count_of_a_cursor_list_counts_the_rows_loaded()
    {
        var count = new KlyDataListResultCount<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(count), configure: l => l.PagingMode = DataListPagingMode.Cursor);
        Assert.Equal("1–2 of 5", count.Text);
    }

    // ---- empty and error

    [Fact]
    public void The_empty_state_says_no_results_by_default_and_takes_its_own_texts()
    {
        var empty = new KlyDataListEmpty();
        var inner = (KlyEmpty)empty.Content!;
        Assert.Equal("No results", inner.Title);
        Assert.Equal("Try changing your search or your filters.", inner.Description);
        Assert.NotNull(inner.Icon);
        empty.Title = "Nobody here";
        empty.Description = "";
        empty.Icon = "";
        Assert.Equal("Nobody here", inner.Title);
        Assert.Equal("", inner.Description);
        Assert.Null(inner.Icon);
    }

    [Fact]
    public async Task The_error_state_shows_only_after_a_failure_and_its_button_tries_again()
    {
        var calls = 0;
        var error = new KlyDataListErrorState<Person>();
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = Counting(() => calls++, n => n == 1), Content = new VerticalStackLayout { Children = { error } } };
        await list.LoadAsync();
        Assert.NotNull(error.Content);
        Assert.Equal("Could not load the data", error.Alert.Title);
        Assert.Equal("Try again", error.RetryButton.TextView.Text);
        error.RetryButton.Activate();
        await list.Context.WhenLoaded();
        Assert.Null(error.Content);
        Assert.Equal(2, calls);
        error.Title = "Custom";
        Assert.Null(error.Content);
    }

    // ---- filters buttons

    [Fact]
    public async Task The_clear_filters_button_shows_only_while_a_filter_is_on()
    {
        var clear = new KlyDataListClearFilters<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(clear));
        Assert.Null(clear.Content);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Infra"])));
        await list.Context.WhenLoaded();
        Assert.NotNull(clear.Content);
        Assert.Equal("Clear filters", clear.Button.TextView.Text);
        clear.Text = "Reset";
        Assert.Equal("Reset", clear.Button.TextView.Text);
        clear.Button.Activate();
        await list.Context.WhenLoaded();
        Assert.Empty(list.Context.State.Filters);
        Assert.Null(clear.Content);
    }

    // ---- layout and view

    [Fact]
    public async Task The_layout_switch_toggles_between_the_table_and_the_grid()
    {
        var layout = new KlyDataListLayoutSwitch<Person>();
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(layout);
            s.Children.Add(new KlyDataListTable<Person>());
            s.Children.Add(new KlyDataListGrid<Person>());
        });
        Assert.Equal([DataListLayout.Table, DataListLayout.Grid], layout.Buttons.Keys);
        Assert.Equal("selected", layout.Buttons[DataListLayout.Table].AccessibleHint);
        layout.Buttons[DataListLayout.Grid].Activate();
        Assert.Equal(DataListLayout.Grid, list.Context.EffectiveLayout);
        Assert.Equal("selected", layout.Buttons[DataListLayout.Grid].AccessibleHint);
        Assert.Null(layout.Buttons[DataListLayout.Table].AccessibleHint);
        Assert.Equal("Table view", layout.Buttons[DataListLayout.Table].AccessibleName);
        Assert.Null(layout.Buttons[DataListLayout.Table].TextView.Text);
    }

    [Fact]
    public async Task The_layout_switch_can_show_labels_and_change_its_order()
    {
        var layout = new KlyDataListLayoutSwitch<Person> { ShowLabels = true, Order = [DataListLayout.Grid, DataListLayout.Table], TableIcon = "list" };
        await TableHarness.Mount(s =>
        {
            s.Children.Add(layout);
            s.Children.Add(new KlyDataListTable<Person>());
            s.Children.Add(new KlyDataListGrid<Person>());
        });
        Assert.Equal([DataListLayout.Grid, DataListLayout.Table], layout.Buttons.Keys);
        Assert.Equal("Grid view", layout.Buttons[DataListLayout.Grid].TextView.Text);
        Assert.Equal("list", layout.Buttons[DataListLayout.Table].IconView.Name);
    }

    [Fact]
    public async Task The_view_toggle_needs_a_table_with_a_row_summary()
    {
        var toggle = new KlyDataListViewToggle<Person>();
        var table = new KlyDataListTable<Person>();
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(toggle);
            s.Children.Add(table);
        });
        Assert.Null(toggle.Content);
        table.RowSummary = new DataListRowSummary<Person> { AutoPhone = false };
        Assert.NotNull(toggle.Button);
        Assert.False(list.Context.SummaryViewActive);
        toggle.Button!.Activate();
        Assert.True(list.Context.SummaryViewActive);
        Assert.Equal("selected", toggle.Button!.AccessibleHint);
        toggle.Button!.Activate();
        Assert.False(list.Context.SummaryViewActive);
        toggle.ShowText = false;
        Assert.Null(toggle.Button!.TextView.Text);
        Assert.Equal("Summary view", toggle.Button.AccessibleName);
    }

    // ---- pagination

    [Fact]
    public async Task The_pagination_follows_the_page_and_changes_it()
    {
        var pagination = new KlyDataListPagination<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(pagination));
        Assert.Equal(5, pagination.Pagination.Total);
        Assert.Equal(2, pagination.Pagination.PageSize);
        Assert.Equal(1, pagination.Pagination.Page);
        pagination.Pagination.Page = 2;
        await list.Context.WhenLoaded();
        Assert.Equal(2, list.Context.State.Page);
        Assert.Equal(2, pagination.Pagination.Page);
        list.Context.SetState(s => s with { Page = 3 });
        await list.Context.WhenLoaded();
        Assert.Equal(3, pagination.Pagination.Page);
    }

    [Fact]
    public async Task The_page_size_picker_lists_the_sizes_and_the_current_one()
    {
        var pagination = new KlyDataListPagination<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(pagination));
        Assert.Equal([2, 10, 25, 50], pagination.SizePicker.ItemsSource!.Cast<int>());
        Assert.Equal(2, pagination.SizePicker.SelectedItem);
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        pagination.SizePicker.SelectedItem = 10;
        await list.Context.WhenLoaded();
        Assert.Equal(10, list.Context.State.PageSize);
        Assert.Equal(1, list.Context.State.Page);
        pagination.ShowPageSize = false;
        Assert.False(((View)pagination.SizePicker.Parent!).IsVisible);
    }

    [Fact]
    public async Task The_pagination_is_named_after_the_list()
    {
        var pagination = new KlyDataListPagination<Person>();
        await TableHarness.Mount(s => s.Children.Add(pagination), configure: l => l.Label = "Staff");
        Assert.Equal("Pagination, Staff", pagination.Pagination.AccessibleName);
        pagination.AccessibleName = "Pages";
        Assert.Equal("Pages", pagination.Pagination.AccessibleName);
        pagination.ShowFirstLast = true;
        Assert.True(pagination.Pagination.ShowFirstLast);
        pagination.SiblingCount = 2;
        Assert.Equal(2, pagination.Pagination.SiblingCount);
    }

    [Fact]
    public async Task Without_a_count_the_pagination_knows_the_next_page_exists_while_the_page_is_full()
    {
        var pagination = new KlyDataListPagination<Person>();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = async (r, t) => (await inner(r, t)) with { TotalCount = null },
            Content = new VerticalStackLayout { Children = { pagination } },
            State = TableHarness.PersonSchema.DefaultState with { PageSize = 2 },
        };
        await list.LoadAsync();
        Assert.Equal(3, pagination.Pagination.Total);
    }

    // ---- infinite loader

    [Fact]
    public async Task The_loader_button_appends_the_next_page_of_a_cursor_list()
    {
        var loader = new KlyDataListInfiniteLoader<Person> { Auto = false };
        var list = await TableHarness.Mount(s => s.Children.Add(loader), configure: l => l.PagingMode = DataListPagingMode.Cursor);
        Assert.Equal(2, list.Context.Rows.Count);
        Assert.NotNull(loader.LoadMoreButton);
        Assert.Equal("Load more", loader.LoadMoreButton!.TextView.Text);
        loader.LoadMoreButton.Activate();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(4, list.Context.Rows.Count);
        loader.LoadMoreButton!.Activate();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(5, list.Context.Rows.Count);
        Assert.Null(loader.LoadMoreButton);
        Assert.False(list.Context.HasMore);
    }

    [Fact]
    public async Task The_loader_shows_the_end_when_asked()
    {
        var loader = new KlyDataListInfiniteLoader<Person> { Auto = false, ShowEnd = true };
        var list = await TableHarness.Mount(s => s.Children.Add(loader), configure: l => l.PagingMode = DataListPagingMode.Cursor, pageSize: 10);
        Assert.Null(loader.LoadMoreButton);
        var label = Assert.IsType<Label>(((VerticalStackLayout)loader.Content!).Children.Single());
        Assert.Equal("No more items", label.Text);
        Assert.False(list.Context.HasMore);
    }

    [Fact]
    public async Task The_loader_is_empty_outside_cursor_mode()
    {
        var loader = new KlyDataListInfiniteLoader<Person>();
        await TableHarness.Mount(s => s.Children.Add(loader));
        Assert.Null(loader.Content);
    }

    [Fact]
    public async Task A_page_that_fails_shows_the_error_and_a_retry_that_keeps_the_rows()
    {
        var calls = 0;
        var loader = new KlyDataListInfiniteLoader<Person> { Auto = false };
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = Counting(() => calls++, n => n == 2),
            PagingMode = DataListPagingMode.Cursor,
            Content = new VerticalStackLayout { Children = { loader } },
            State = TableHarness.PersonSchema.DefaultState with { PageSize = 2 },
        };
        await list.LoadAsync();
        loader.LoadMoreButton!.Activate();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.True(list.Context.HasLoadMoreError);
        Assert.Equal(2, list.Context.Rows.Count);
        Assert.NotNull(loader.RetryButton);
        loader.RetryButton!.Activate();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.False(list.Context.HasLoadMoreError);
        Assert.Equal(4, list.Context.Rows.Count);
    }

    [Fact]
    public async Task A_list_entered_mid_way_offers_the_way_back_to_the_first_page()
    {
        var loader = new KlyDataListInfiniteLoader<Person> { Auto = false };
        var list = await TableHarness.Mount(s => s.Children.Add(loader), configure: l => l.PagingMode = DataListPagingMode.Cursor);
        list.Context.SetState(s => s with { Cursor = list.Context.NextCursor });
        await list.Context.WhenLoaded();
        Assert.NotNull(loader.BackButton);
        loader.BackButton!.Activate();
        await list.Context.WhenLoaded();
        Assert.Null(list.Context.State.Cursor);
        Assert.Null(loader.BackButton);
    }

    [Fact]
    public async Task The_automatic_loader_tells_the_list_so_a_virtualized_table_can_ask_for_pages()
    {
        var loader = new KlyDataListInfiniteLoader<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(loader), configure: l => l.PagingMode = DataListPagingMode.Cursor);
        Assert.True(list.Context.AutoLoadMore);
        loader.Auto = false;
        Assert.False(list.Context.AutoLoadMore);
    }

    // ---- sort menu

    [Fact]
    public async Task The_sort_menu_lists_the_sortable_fields_and_sorts_by_the_one_chosen()
    {
        var menu = new KlyDataListSortMenu<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(menu));
        Assert.Equal("Sort by Name", menu.Trigger!.TextView.Text);
        var items = menu.Menu!.Items;
        Assert.Equal(KlyMenuItemKind.Heading, items[0].Kind);
        Assert.Equal(["name", "team", "salary", "hired"], items.Where(i => i.Kind == KlyMenuItemKind.Item && i.Value != "~default").Select(i => i.Value));
        Assert.Equal("Name  ↑", items[1].Label);
        menu.Choose("salary");
        await list.Context.WhenLoaded();
        Assert.Equal([new DataListSort("salary")], list.Context.State.Sort);
        Assert.Equal("Sort by Salary", menu.Trigger!.TextView.Text);
        menu.Choose("salary");
        await list.Context.WhenLoaded();
        Assert.Equal([new DataListSort("salary", true)], list.Context.State.Sort);
        menu.Choose("~default");
        await list.Context.WhenLoaded();
        Assert.Equal(list.Context.Schema!.DefaultSort, list.Context.EffectiveSort);
        Assert.False(menu.Menu!.Items[^1].IsEnabled);
    }

    [Fact]
    public async Task The_sort_menu_adds_to_the_sort_when_it_is_multiple()
    {
        var menu = new KlyDataListSortMenu<Person> { Multiple = true };
        var list = await TableHarness.Mount(s => s.Children.Add(menu));
        menu.Choose("team");
        await list.Context.WhenLoaded();
        Assert.Equal([new DataListSort("name"), new DataListSort("team")], list.Context.State.Sort);
        Assert.Equal("Sort by 2 fields", menu.Trigger!.TextView.Text);
    }

    [Fact]
    public async Task The_sort_menu_can_be_limited_to_some_layouts_and_some_fields()
    {
        var menu = new KlyDataListSortMenu<Person> { Layouts = [DataListLayout.Grid], Items = [new DataListSortOption("salary", "Pay")] };
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(menu);
            s.Children.Add(new KlyDataListTable<Person>());
            s.Children.Add(new KlyDataListGrid<Person>());
        });
        Assert.Null(menu.Content);
        list.Context.SetLayout(DataListLayout.Grid);
        Assert.NotNull(menu.Content);
        Assert.Equal("Pay", menu.Menu!.Items.First(i => i.Value == "salary").Label);
        Assert.Equal(3, menu.Menu.Items.Count + 0 - 1);
    }

    // ---- group header

    [Fact]
    public async Task A_group_header_folds_its_group_and_selects_its_rows()
    {
        var header = new KlyDataListGroupHeader<Person>();
        var table = new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team) };
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(header);
            s.Children.Add(table);
        }, configure: l => l.Selectable = true, pageSize: 10);
        var group = list.Context.Groups()[0];
        header.Group = group;
        Assert.Equal("Backend", header.LabelView!.Text);
        Assert.Equal("2", header.SummaryView!.Text);
        Assert.NotNull(header.Checkbox);
        Assert.False(list.Context.IsGroupCollapsed(group));
        header.Fold();
        Assert.True(list.Context.IsGroupCollapsed(group));
        header.Checkbox!.IsChecked = true;
        Assert.True(list.Context.GetGroupSelection(group));
        Assert.True(header.Checkbox!.IsChecked);
        header.Selectable = false;
        Assert.Null(header.Checkbox);
    }

    [Fact]
    public async Task A_group_header_reports_the_fold_instead_of_folding_when_it_has_a_handler()
    {
        var header = new KlyDataListGroupHeader<Person>();
        var table = new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team) };
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(header);
            s.Children.Add(table);
        }, pageSize: 10);
        var group = list.Context.Groups()[0];
        header.Group = group;
        bool? reported = null;
        header.Toggled += (_, collapsed) => reported = collapsed;
        header.Fold();
        Assert.True(reported);
        Assert.False(list.Context.IsGroupCollapsed(group));
        Assert.Equal("Expand or collapse Backend", header.Toggle!.AccessibleName);
        header.ToggleLabel = "Fold";
        Assert.Equal("Fold", header.Toggle!.AccessibleName);
    }
}
