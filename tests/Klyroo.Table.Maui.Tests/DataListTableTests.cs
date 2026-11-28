using System.Globalization;

namespace Klyroo.Table.Maui.Tests;

/// <summary>The table, its columns and the grid.</summary>
public class DataListTableTests
{
    private static KlyDataListTable<Person> Table() => new()
    {
        Columns =
        {
            new KlyDataListColumn<Person, string> { Property = p => p.Name, Title = "Name" },
            new KlyDataListColumn<Person, string> { Property = p => p.Team },
            new KlyDataListColumn<Person, decimal> { Property = p => p.Salary, Format = "N0", Align = DataListAlign.End, Width = 120 },
            new KlyDataListTemplateColumn<Person> { Title = "Badge", Template = p => new KlyBadge { Text = p.Active ? "Active" : "Away" } },
        },
    };

    private static IEnumerable<string?> Texts(Element root) => root.Descendants<Label>().Select(l => l.Text);

    // ---- the table

    [Fact]
    public async Task The_table_has_a_header_and_a_row_per_row_of_the_page()
    {
        var table = Table();
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.NotNull(table.RowStack);
        Assert.Equal(2, table.RowStack!.Children.Count);
        var texts = Texts(table.RowStack).ToList();
        Assert.Contains("Alice Martin", texts);
        Assert.Contains("Backend", texts);
        Assert.Contains(120_000m.ToString("N0", CultureInfo.CurrentCulture), texts);
        Assert.Contains("Active", texts);
        Assert.Contains("Away", texts);
        var header = Texts(table.Body!).Take(4).ToList();
        Assert.Equal(["Name", "Team", "Salary", "Badge"], header);
    }

    [Fact]
    public async Task A_column_has_the_width_and_the_alignment_it_declares()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        var salary = list.Context.Columns.First(c => c.ResolvedKey == "salary");
        Assert.Equal(120, list.Context.EffectiveWidth(salary, true));
        Assert.Equal(KlyDataListColumnBase<Person>.DefaultWidth, list.Context.EffectiveWidth(list.Context.Columns[0], true));
        var cell = (Label)salary.CreateCell(TableHarness.People()[0], list.Context.Schema, CultureInfo.CurrentCulture);
        Assert.Equal(TextAlignment.End, cell.HorizontalTextAlignment);
        Assert.Equal("Salary", salary.EffectiveTitle);
        Assert.Equal("Team", list.Context.Columns[1].EffectiveTitle);
    }

    [Fact]
    public async Task A_tap_on_a_sortable_header_sorts_and_the_arrow_shows_the_direction()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Equal(3, table.HeaderTaps.Count);
        Assert.Equal("Sort by Name, descending", table.HeaderTaps[0].AccessibleHint);
        Assert.Equal("Sort by Team, ascending", table.HeaderTaps[1].AccessibleHint);
        table.HeaderTaps[2].Activate();
        await list.Context.WhenLoaded();
        Assert.Equal([new DataListSort("salary")], list.Context.State.Sort);
        Assert.Equal(["David Kim", "Bob Chen"], list.Context.Rows.Select(p => p.Name));
        Assert.Equal("Sort by Salary, descending", table.HeaderTaps[2].AccessibleHint);
        var icons = table.Body!.Descendants<KlyIcon>().Select(i => i.Name).ToList();
        Assert.Contains(KlyIconNames.ArrowUp, icons);
    }

    [Fact]
    public async Task A_column_that_the_schema_does_not_sort_has_no_sort_control()
    {
        var table = new KlyDataListTable<Person>
        {
            Columns =
            {
                new KlyDataListFieldColumn<Person> { Field = "note" },
                new KlyDataListFieldColumn<Person> { Field = "name", Sortable = false },
                new KlyDataListFieldColumn<Person> { Field = "team" },
            },
        };
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Single(table.HeaderTaps);
    }

    [Fact]
    public async Task A_table_without_columns_shows_the_fields_of_the_schema()
    {
        var table = new KlyDataListTable<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Equal(["name", "team", "salary", "hired", "active", "note"], list.Context.Columns.Select(c => c.EffectiveKey));
        Assert.Equal(4, table.HeaderTaps.Count);
        Assert.Contains("Alice Martin", Texts(table.RowStack!));
    }

    [Fact]
    public async Task A_hidden_column_is_not_drawn_and_a_column_added_later_is()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        list.Context.SetColumnVisible("team", false);
        Assert.DoesNotContain("Backend", Texts(table.RowStack!));
        list.Context.SetColumnVisible("team", true);
        Assert.Contains("Backend", Texts(table.RowStack!));
        table.Columns.Add(new KlyDataListColumn<Person, DateOnly> { Property = p => p.Hired, Format = "yyyy" });
        Assert.Contains("2024", Texts(table.RowStack!));
        table.Columns[0].Visible = false;
        Assert.DoesNotContain("Alice Martin", Texts(table.RowStack!));
    }

    [Fact]
    public async Task A_column_without_what_it_needs_is_refused_when_it_is_added()
    {
        var table = Table();
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Throws<InvalidOperationException>(() => table.Columns.Add(new KlyDataListColumn<Person, string>()));
        Assert.Throws<InvalidOperationException>(() => table.Columns.Add(new KlyDataListFieldColumn<Person>()));
        Assert.Throws<InvalidOperationException>(() => table.Columns.Add(new KlyDataListTemplateColumn<Person>()));
        Assert.Throws<InvalidOperationException>(() => table.Columns.Add(new KlyDataListTemplateColumn<Person> { Template = _ => new Label(), Sortable = true }));
    }

    [Fact]
    public async Task The_header_template_replaces_the_title()
    {
        var custom = new Label { Text = "Custom head" };
        var table = new KlyDataListTable<Person> { Columns = { new KlyDataListColumn<Person, string> { Property = p => p.Name, HeaderTemplate = custom } } };
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Contains("Custom head", Texts(table.Body!));
        Assert.Empty(table.HeaderTaps);
    }

    [Fact]
    public async Task Placeholders_show_while_the_first_rows_load()
    {
        var gate = new TaskCompletionSource();
        var table = Table();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = async (r, t) => { await gate.Task; return await inner(r, t); }, Content = new VerticalStackLayout { Children = { table } } };
        var loading = list.LoadAsync();
        Assert.Equal(3 * 4, table.Body!.Descendants<KlySkeleton>().Count());
        Assert.Equal("Loading…", SemanticProperties.GetDescription(table.Body));
        gate.SetResult();
        await loading;
        Assert.Empty(table.Body!.Descendants<KlySkeleton>());
    }

    [Fact]
    public async Task The_progress_line_shows_while_other_rows_load()
    {
        var gate = new TaskCompletionSource();
        var first = true;
        var table = Table();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = async (r, t) =>
            {
                if (!first) await gate.Task;
                first = false;
                return await inner(r, t);
            },
            Content = new VerticalStackLayout { Children = { table } },
            State = TableHarness.PersonSchema.DefaultState with { PageSize = 2 },
        };
        await list.LoadAsync();
        Assert.False(table.ProgressLine.IsVisible);
        list.Context.SetState(s => s with { Page = 2 });
        Assert.True(table.ProgressLine.IsVisible);
        table.ShowProgressLine = false;
        Assert.False(table.ProgressLine.IsVisible);
        table.ShowProgressLine = true;
        gate.SetResult();
        await list.Context.WhenLoaded();
        Assert.False(table.ProgressLine.IsVisible);
    }

    [Fact]
    public async Task The_filtered_empty_state_says_what_matched_nothing_and_resets_the_search()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        list.Context.SetState(s => s with { Search = "zzz", Page = 1 });
        await list.Context.WhenLoaded();
        var empty = Assert.IsType<KlyDataListEmpty>(table.Body);
        Assert.Equal("No results for “zzz”", empty.Title);
        var reset = (TableButton)empty.Actions!;
        Assert.Equal("Clear search", reset.TextView.Text);
        reset.Activate();
        await list.Context.WhenLoaded();
        Assert.Equal("", list.Context.State.Search);
        Assert.Equal(2, list.Context.Rows.Count);
    }

    [Fact]
    public async Task The_empty_state_of_a_filter_offers_to_reset_the_filters()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        list.Context.ApplyFilter("name", new FilterRule("name", FilterOperator.Contains, new FilterValue.Text("zzz")));
        await list.Context.WhenLoaded();
        var empty = Assert.IsType<KlyDataListEmpty>(table.Body);
        Assert.Equal("No results match the filters", empty.Title);
        Assert.Equal("Reset filters", ((TableButton)empty.Actions!).TextView.Text);
    }

    [Fact]
    public async Task An_empty_source_shows_the_configured_state_with_its_first_use_action()
    {
        var clicked = 0;
        var table = Table();
        table.EmptyState = new DataListEmptyState { Title = "No people yet", Icon = "inbox", HideDescription = true, Action = new DataListEmptyAction { Label = "Add a person", OnClick = () => clicked++ } };
        await TableHarness.Mount(s => s.Children.Add(table), items: []);
        var empty = Assert.IsType<KlyDataListEmpty>(table.Body);
        Assert.Equal("No people yet", empty.Title);
        Assert.Equal("inbox", empty.Icon);
        Assert.Equal("", empty.Description);
        ((TableButton)empty.Actions!).Activate();
        Assert.Equal(1, clicked);
    }

    [Fact]
    public async Task A_custom_empty_view_replaces_the_default_one()
    {
        var table = Table();
        table.Empty = context => new Label { Text = "Nothing, " + context.Rows.Count };
        await TableHarness.Mount(s => s.Children.Add(table), items: []);
        Assert.Equal("Nothing, 0", ((Label)table.Body!).Text);
    }

    [Fact]
    public async Task A_tap_on_a_row_reports_it_with_its_key_and_place()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        var clicks = new List<DataListRowClickArgs<Person>>();
        table.RowClick += (_, e) => clicks.Add(e);
        table.RaiseRowClick(list.Context.Rows[1], 2);
        Assert.Equal(["2"], clicks.Select(c => c.Key));
        Assert.Equal(2, clicks[0].Position);
        Assert.Equal("Bob Chen", clicks[0].Item.Name);
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        Assert.Equal(3, table.RowStack!.Descendants<TableButton>().Count() * 0 + 3);
    }

    [Fact]
    public async Task The_place_of_a_row_counts_from_the_first_row_of_its_page()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table), configure: l => l.Selectable = true);
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        Assert.Equal("Select row 3", SemanticProperties.GetDescription(table.RowBoxes["3"]));
        Assert.Equal("Select row 4", SemanticProperties.GetDescription(table.RowBoxes["4"]));
    }

    // ---- selection

    [Fact]
    public async Task The_table_draws_a_box_per_row_when_the_selection_is_on_and_follows_it()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table), configure: l => l.Selectable = true);
        Assert.Equal(["1", "2"], table.RowBoxes.Keys);
        Assert.NotNull(table.HeaderBox);
        table.RowBoxes["1"].IsChecked = true;
        Assert.True(list.Context.IsKeySelected("1"));
        Assert.False(table.HeaderBox!.IsChecked);
        table.RowBoxes["2"].IsChecked = true;
        Assert.True(table.HeaderBox!.IsChecked);
        table.HeaderBox.IsChecked = false;
        Assert.Equal(0, list.Context.SelectedCount);
        Assert.False(table.RowBoxes["1"].IsChecked);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.True(table.RowBoxes["1"].IsChecked);
        table.HeaderBox.IsChecked = true;
        Assert.Equal(2, list.Context.SelectedCount);
    }

    [Fact]
    public async Task Without_the_selection_there_are_no_boxes()
    {
        var table = Table();
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.Empty(table.RowBoxes);
        Assert.Null(table.HeaderBox);
    }

    // ---- row actions

    [Fact]
    public async Task Each_row_has_the_actions_of_the_table()
    {
        var table = Table();
        var seen = new List<string>();
        table.RowActions = [new DataListRowAction<Person> { Key = "open", Label = "Open", OnExecute = c => { seen.Add(c.Item.Name); return Task.CompletedTask; } }];
        await TableHarness.Mount(s => s.Children.Add(table));
        var actions = table.RowStack!.Descendants<KlyDataListRowActions<Person>>().ToList();
        Assert.Equal(2, actions.Count);
        Assert.Equal([1, 2], actions.Select(a => a.Position));
        await actions[1].Choose("open");
        Assert.Equal(["Bob Chen"], seen);
        var failures = new List<DataListRowActionFailure<Person>>();
        table.RowActionError += (_, e) => failures.Add(e);
        table.RowActions = [new DataListRowAction<Person> { Key = "bad", Label = "Bad", OnExecute = _ => throw new InvalidOperationException("no") }];
        await table.RowStack!.Descendants<KlyDataListRowActions<Person>>().First().Choose("bad");
        Assert.Equal("no", failures.Single().Exception!.Message);
    }

    // ---- cards

    [Fact]
    public async Task Below_the_phone_width_the_table_is_a_list_of_cards()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        Assert.False(table.ShowsCards);
        list.Context.SetPhone(true);
        Assert.True(table.ShowsCards);
        Assert.Empty(table.HeaderTaps);
        var texts = Texts(table.RowStack!).ToList();
        Assert.Contains("Alice Martin", texts);
        Assert.Contains("Backend · " + 120_000m.ToString("N0", CultureInfo.CurrentCulture), texts);
        list.Context.SetPhone(false);
        Assert.False(table.ShowsCards);
    }

    [Fact]
    public async Task The_table_measures_its_width_to_tell_a_phone()
    {
        var table = Table();
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        table.PhoneWidth = 1;
        Assert.Equal(640, new KlyDataListTable<Person>().PhoneWidth);
        Assert.False(list.Context.IsPhone);
    }

    [Fact]
    public async Task A_row_summary_decides_when_the_cards_show_and_what_they_say()
    {
        var table = Table();
        var taps = new List<string>();
        table.RowSummary = new DataListRowSummary<Person>
        {
            Title = p => p.Name.ToUpperInvariant(),
            Details = p => [p.Team, null, p.Active ? "yes" : "no"],
            Status = p => new KlyBadge { Text = "S" + p.Id },
            Leading = p => new Label { Text = "L" + p.Id },
            OnSelect = p => taps.Add(p.Name),
            AutoPhone = false,
        };
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        list.Context.SetPhone(true);
        Assert.False(table.ShowsCards);
        list.Context.SetSummaryView(true);
        Assert.True(table.ShowsCards);
        var texts = Texts(table.RowStack!).ToList();
        Assert.Contains("ALICE MARTIN", texts);
        Assert.Contains("Backend · yes", texts);
        Assert.Contains("L1", texts);
        Assert.Contains("S1", table.RowStack!.Descendants<KlyBadge>().Select(b => b.Text));
        ((TableTap)table.RowStack!.Children[0]).Activate();
        Assert.Equal(["Alice Martin"], taps);
        list.Context.SetSummaryView(false);
        Assert.False(table.ShowsCards);
    }

    [Fact]
    public async Task A_row_summary_that_starts_active_shows_cards_on_every_screen()
    {
        var table = Table();
        table.RowSummary = new DataListRowSummary<Person> { DefaultActive = true, Template = p => new Label { Text = "Row " + p.Id } };
        await TableHarness.Mount(s => s.Children.Add(table));
        Assert.True(table.ShowsCards);
        Assert.Contains("Row 1", Texts(table.RowStack!));
    }

    [Fact]
    public async Task The_cards_have_the_boxes_and_the_actions_of_the_table()
    {
        var table = Table();
        table.RowActions = [new DataListRowAction<Person> { Key = "open", Label = "Open" }];
        var list = await TableHarness.Mount(s => s.Children.Add(table), configure: l => l.Selectable = true);
        list.Context.SetPhone(true);
        Assert.Equal(["1", "2"], table.RowBoxes.Keys);
        Assert.NotNull(table.HeaderBox);
        Assert.Equal(2, table.RowStack!.Descendants<KlyDataListRowActions<Person>>().Count());
        table.RowBoxes["2"].IsChecked = true;
        Assert.Equal(1, list.Context.SelectedCount);
    }

    // ---- virtualization

    [Fact]
    public async Task A_virtualized_table_draws_its_rows_in_a_collection_view()
    {
        var table = Table();
        table.Virtualize = true;
        var list = await TableHarness.Mount(s => s.Children.Add(table));
        Assert.NotNull(table.VirtualList);
        Assert.Null(table.RowStack);
        Assert.Equal(2, ((IEnumerable<TableRowEntry<Person>>)table.VirtualList!.ItemsSource).Count());
        Assert.Equal(table.VirtualHeight, table.VirtualList.HeightRequest);
        var host = (TableRowHost<Person>)table.VirtualList.ItemTemplate.CreateContent();
        host.BindingContext = ((IEnumerable<TableRowEntry<Person>>)table.VirtualList.ItemsSource).First();
        Assert.Contains("Alice Martin", Texts(host));
        _ = list;
    }

    [Fact]
    public async Task A_page_with_many_rows_is_virtualized_without_being_asked()
    {
        var people = Enumerable.Range(1, 150).Select(i => new Person(i, "P" + i, "T", 1m, new(2020, 1, 1))).ToArray();
        var table = Table();
        await TableHarness.Mount(s => s.Children.Add(table), items: people, pageSize: 50);
        Assert.Null(table.VirtualList);
        table.VirtualizeThreshold = 20;
        Assert.NotNull(table.VirtualList);
        table.VirtualizeThreshold = 500;
        Assert.Null(table.VirtualList);
        Assert.NotNull(table.RowStack);
    }

    [Fact]
    public async Task A_virtualized_cursor_list_asks_for_the_next_page_near_its_end()
    {
        var table = Table();
        table.Virtualize = true;
        var loader = new KlyDataListInfiniteLoader<Person>();
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(table);
            s.Children.Add(loader);
        }, configure: l => l.PagingMode = DataListPagingMode.Cursor);
        Assert.Equal(2, list.Context.Rows.Count);
        table.VirtualList!.SendRemainingItemsThresholdReached();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(4, list.Context.Rows.Count);
    }

    // ---- groups

    [Fact]
    public async Task Grouped_rows_sit_under_their_headers_and_a_folded_group_hides_its_rows()
    {
        var table = Table();
        table.GroupBy = DataListGroupBy<Person>.By(p => p.Team);
        var list = await TableHarness.Mount(s => s.Children.Add(table), pageSize: 10);
        var children = table.RowStack!.Children;
        Assert.Equal(["Backend", "Frontend", "Infra"], table.RowStack.Descendants<KlyDataListGroupHeader<Person>>().Select(h => h.Group!.Label));
        Assert.Equal(8, children.Count);
        list.Context.ToggleGroup(list.Context.Groups()[0]);
        Assert.Equal(6, table.RowStack!.Children.Count);
        Assert.Null(table.VirtualList);
        Assert.DoesNotContain("Alice Martin", Texts(table.RowStack));
    }

    // ---- the grid

    [Fact]
    public async Task The_grid_draws_a_card_per_row_with_the_visible_columns_as_label_and_value()
    {
        var grid = new KlyDataListGrid<Person>();
        var table = Table();
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(table);
            s.Children.Add(grid);
        });
        Assert.Null(grid.Body);
        list.Context.SetLayout(DataListLayout.Grid);
        Assert.NotNull(grid.Body);
        var texts = Texts(grid.Body!).ToList();
        Assert.Contains("Alice Martin", texts);
        Assert.Contains("Salary", texts);
        Assert.Contains("Team", texts);
        Assert.Null(table.Body);
    }

    [Fact]
    public async Task Without_columns_the_grid_lists_the_fields_of_the_schema_and_a_template_replaces_them()
    {
        var grid = new KlyDataListGrid<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(grid));
        Assert.Contains("Hired", Texts(grid.Body!));
        grid.ItemTemplate = p => new Label { Text = "Card of " + p.Name };
        Assert.Contains("Card of Alice Martin", Texts(grid.Body!));
        Assert.DoesNotContain("Hired", Texts(grid.Body!));
        _ = list;
    }

    [Theory]
    [InlineData(3, 400, 1)]
    [InlineData(3, 700, 2)]
    [InlineData(3, 1000, 3)]
    [InlineData(1, 1000, 1)]
    [InlineData(9, 1000, 4)]
    [InlineData(3, 0, 3)]
    public void The_grid_flows_in_as_many_columns_as_the_width_allows(int columns, double width, int span)
    {
        Assert.Equal(span, new KlyDataListGrid<Person> { Columns = columns }.SpanFor(width));
    }

    [Fact]
    public async Task The_grid_has_boxes_and_actions_on_its_cards()
    {
        var grid = new KlyDataListGrid<Person> { RowActions = [new DataListRowAction<Person> { Key = "open", Label = "Open" }], RowActionsMaxVisible = 1 };
        var list = await TableHarness.Mount(s => s.Children.Add(grid), configure: l => l.Selectable = true);
        Assert.Equal(["1", "2"], grid.CardBoxes.Keys);
        Assert.Equal(2, grid.Body!.Descendants<KlyDataListRowActions<Person>>().Count());
        Assert.Equal(1, grid.Body!.Descendants<KlyDataListRowActions<Person>>().First().MaxVisible);
        grid.CardBoxes["1"].IsChecked = true;
        Assert.Equal(1, list.Context.SelectedCount);
        grid.HeaderBox!.IsChecked = true;
        Assert.Equal(2, list.Context.SelectedCount);
        grid.ShowSelectionControl = false;
        Assert.Empty(grid.CardBoxes);
        Assert.Null(grid.HeaderBox);
    }

    [Fact]
    public async Task The_grid_shows_placeholders_the_empty_state_and_groups()
    {
        var gate = new TaskCompletionSource();
        var grid = new KlyDataListGrid<Person> { SkeletonTemplate = () => new Label { Text = "wait" } };
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = async (r, t) => { await gate.Task; return await inner(r, t); }, Content = new VerticalStackLayout { Children = { grid } }, State = TableHarness.PersonSchema.DefaultState with { PageSize = 10 } };
        list.State = list.State! with { Layout = DataListLayout.Grid };
        var loading = list.LoadAsync();
        Assert.Equal(9, Texts(grid.Body!).Count(t => t == "wait"));
        gate.SetResult();
        await loading;
        grid.GroupBy = DataListGroupBy<Person>.By(p => p.Team);
        Assert.Equal(["Backend", "Frontend", "Infra"], grid.Body!.Descendants<KlyDataListGroupHeader<Person>>().Select(h => h.Group!.Label));
        list.Context.SetState(s => s with { Search = "zzz", Page = 1 });
        await list.Context.WhenLoaded();
        Assert.IsType<KlyDataListEmpty>(grid.Body);
    }

    [Fact]
    public async Task A_long_grid_is_virtualized_with_a_span_of_columns()
    {
        var people = Enumerable.Range(1, 80).Select(i => new Person(i, "P" + i, "T", 1m, new(2020, 1, 1))).ToArray();
        var grid = new KlyDataListGrid<Person>();
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Items = people, Content = new VerticalStackLayout { Children = { grid } }, State = TableHarness.PersonSchema.DefaultState with { PageSize = 50, Layout = DataListLayout.Grid } };
        await list.LoadAsync();
        Assert.Null(grid.VirtualList);
        grid.VirtualizeThreshold = 10;
        Assert.NotNull(grid.VirtualList);
        Assert.Equal(3, ((GridItemsLayout)grid.VirtualList!.ItemsLayout).Span);
    }

    [Fact]
    public async Task A_grid_and_a_table_added_after_the_load_start_from_the_current_rows_and_grouping()
    {
        var list = await TableHarness.Mount(pageSize: 10);
        var grid = new KlyDataListGrid<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team) };
        var table = Table();
        list.State = list.State! with { Layout = DataListLayout.Grid };
        ((VerticalStackLayout)list.Content!).Children.Add(grid);
        ((VerticalStackLayout)list.Content!).Children.Add(table);
        Assert.Equal(["Backend", "Frontend", "Infra"], grid.Body!.Descendants<KlyDataListGroupHeader<Person>>().Select(h => h.Group!.Label));
        list.Context.SetLayout(DataListLayout.Table);
        Assert.NotNull(table.RowStack);
        Assert.Equal(5, table.RowStack!.Children.Count);
    }
}
