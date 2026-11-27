using System.Globalization;

namespace Klyroo.Table.Maui.Tests;

/// <summary>The selection, the columns, the groups, the filters and the export of the context of a list.</summary>
public class DataListStateTests
{
    private static KlyDataListTable<Person> Table() => new()
    {
        Columns =
        {
            new KlyDataListColumn<Person, string> { Property = p => p.Name, Title = "Name", Required = true },
            new KlyDataListColumn<Person, string> { Property = p => p.Team, Title = "Team" },
            new KlyDataListColumn<Person, decimal> { Property = p => p.Salary, Format = "N0", Align = DataListAlign.End },
            new KlyDataListTemplateColumn<Person> { Title = "Actions", Template = _ => new Label() },
        },
    };

    // ---- selection

    [Fact]
    public async Task The_selection_is_off_until_something_asks_for_it()
    {
        var list = await TableHarness.Mount();
        Assert.False(list.Context.SelectionEnabled);
        list.Selectable = true;
        Assert.True(list.Context.SelectionEnabled);
        list.Selectable = false;
        list.SelectedKeys = new HashSet<string>();
        Assert.False(list.Context.SelectionEnabled);
    }

    [Fact]
    public async Task Setting_the_keys_turns_the_selection_on_and_ticks_the_rows()
    {
        var list = await TableHarness.Mount();
        list.SelectedKeys = new HashSet<string> { "1" };
        Assert.True(list.Context.SelectionEnabled);
        Assert.True(list.Context.IsSelected(list.Context.Rows[0]));
        Assert.Equal(1, list.Context.SelectedCount);
    }

    [Fact]
    public async Task Ticking_a_row_reports_the_keys_once_and_the_page_box_is_mixed()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true);
        var reported = new List<IReadOnlySet<string>>();
        list.SelectedKeysChanged += (_, keys) => reported.Add(keys);
        var context = list.Context;
        Assert.False(context.PageSelection);
        context.SetSelected(context.Rows[0], true);
        Assert.Single(reported);
        Assert.Equal(["1"], reported[0]);
        Assert.Null(context.PageSelection);
        context.SetSelected(context.Rows[1], true);
        Assert.True(context.PageSelection);
        context.SetSelected(context.Rows[1], true);
        Assert.Equal(2, reported.Count);
        context.SetPageSelected(false);
        Assert.Equal(0, context.SelectedCount);
        Assert.False(context.PageSelection);
    }

    [Fact]
    public async Task A_shift_tick_ticks_the_rows_between()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true, pageSize: 5);
        var context = list.Context;
        context.SetSelected(context.Rows[0], true);
        context.SetSelected(context.Rows[3], true, range: true);
        Assert.Equal(4, context.SelectedCount);
    }

    [Fact]
    public async Task A_list_in_memory_keeps_the_selection_across_pages()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true);
        var context = list.Context;
        context.SetSelected(context.Rows[0], true);
        context.SetState(s => s with { Page = 2 });
        await context.WhenLoaded();
        Assert.Equal(1, context.SelectedCount);
        Assert.Equal("Alice Martin", context.SelectedItems.Single().Name);
    }

    [Fact]
    public async Task The_page_scope_forgets_rows_that_left_the_page()
    {
        var list = await TableHarness.Mount(configure: l =>
        {
            l.Selectable = true;
            l.SelectionScope = DataListSelectionScope.Page;
        });
        var context = list.Context;
        context.SetSelected(context.Rows[0], true);
        context.SetState(s => s with { Page = 2 });
        await context.WhenLoaded();
        Assert.Equal(0, context.SelectedCount);
    }

    [Fact]
    public async Task A_list_over_a_provider_keeps_only_the_rows_on_screen()
    {
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People()),
            Selectable = true,
            State = TableHarness.PersonSchema.DefaultState with { PageSize = 2 },
        };
        await list.LoadAsync();
        list.Context.SetSelected(list.Context.Rows[0], true);
        list.Context.SetState(s => s with { Page = 2 });
        await list.Context.WhenLoaded();
        Assert.Equal(0, list.Context.SelectedCount);
    }

    [Fact]
    public async Task Selecting_needs_a_row_key()
    {
        var schema = DataListSchema.For<Person>(s => s.Field(p => p.Name, f => f.Key("name").Sortable()));
        var list = new KlyDataList<Person> { Schema = schema, Items = TableHarness.People(), Selectable = true };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => list.LoadAsync());
        Assert.Contains("RowKey", error.Message);
    }

    [Fact]
    public async Task The_bulk_scope_switches_to_every_result_when_there_are_more_than_the_ticked_rows()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true);
        var context = list.Context;
        Assert.Equal(DataListActionScope.Selection, context.BulkScope);
        context.SetSelected(context.Rows[0], true);
        Assert.True(context.CanSelectAllResults);
        context.SetBulkScope(DataListActionScope.All);
        Assert.Equal(DataListActionScope.All, context.BulkScope);
        var action = await context.CreateActionContextAsync(DataListActionScope.All, resolveAll: true, CancellationToken.None);
        Assert.Equal(5, action.SelectedItems.Count);
        Assert.Equal(5, action.TargetCount);
        Assert.NotNull(action.Request);
        Assert.IsType<DataListPaging.None>(action.Request!.Paging);
        context.ClearSelection();
        Assert.Equal(DataListActionScope.Selection, context.BulkScope);
    }

    [Fact]
    public async Task Describing_an_action_for_every_result_does_not_load_the_rows()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true);
        list.Context.SetSelected(list.Context.Rows[0], true);
        list.Context.SetBulkScope(DataListActionScope.All);
        var described = list.Context.CreateActionContext(DataListActionScope.All);
        Assert.Empty(described.SelectedItems);
        Assert.Equal(5, described.TargetCount);
    }

    // ---- columns

    [Fact]
    public async Task The_columns_register_with_the_list_in_their_order()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        Assert.Equal(["name", "team", "salary", null], list.Context.Columns.Select(c => c.EffectiveKey));
        Assert.Equal(["Name", "Team", "Salary"], list.Context.ColumnStates.Select(c => c.Title));
    }

    [Fact]
    public async Task A_required_column_cannot_be_hidden_and_the_last_visible_one_stays()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        var context = list.Context;
        context.SetColumnVisible("name", false);
        Assert.True(context.ColumnStates.First(c => c.Key == "name").Visible);
        context.SetColumnVisible("team", false);
        context.SetColumnVisible("salary", false);
        Assert.False(context.ColumnStates.First(c => c.Key == "salary").Visible);
        Assert.Equal(["name"], context.TableColumns().Where(c => c.ResolvedKey is not null).Select(c => c.ResolvedKey));
        context.SetColumnVisible("team", true);
        Assert.Equal(["name", "team"], context.TableColumns().Where(c => c.ResolvedKey is not null).Select(c => c.ResolvedKey));
        Assert.True(context.HasCustomColumnPreferences);
        context.ResetColumns();
        Assert.False(context.HasCustomColumnPreferences);
        Assert.All(context.ColumnStates, c => Assert.True(c.Visible));
    }

    [Fact]
    public async Task Hiding_every_column_keeps_the_required_ones_and_showing_all_brings_them_back()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        var context = list.Context;
        context.SetAllColumnsVisible(false);
        Assert.Equal(["name"], context.ColumnStates.Where(c => c.Visible).Select(c => c.Key));
        context.SetAllColumnsVisible(true);
        Assert.Equal(3, context.ColumnStates.Count(c => c.Visible));
    }

    [Fact]
    public async Task A_column_moves_and_pins_and_the_table_shows_the_pinned_ones_at_the_edges()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        var context = list.Context;
        context.MoveColumn("salary", -1);
        Assert.Equal(["name", "salary", "team"], context.ColumnStates.Select(c => c.Key));
        context.MoveColumnNextTo("name", "team", after: true);
        Assert.Equal(["salary", "team", "name"], context.ColumnStates.Select(c => c.Key));
        context.SetColumnPinned("team", DataListPin.Start);
        Assert.Equal("team", context.TableColumns()[0].ResolvedKey);
        context.SetColumnPinned("salary", DataListPin.End);
        Assert.Equal("salary", context.TableColumns().Last(c => c.ResolvedKey is not null).ResolvedKey);
        context.SetColumnPinned("name", DataListPin.End);
        Assert.Equal(DataListPin.None, context.ColumnStates.First(c => c.Key == "name").Pinned);
    }

    [Fact]
    public async Task A_column_width_is_kept_between_its_limits_and_cleared_with_null()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        var context = list.Context;
        var team = context.Columns.First(c => c.ResolvedKey == "team");
        Assert.Equal(KlyDataListColumnBase<Person>.DefaultWidth, context.EffectiveWidth(team, true));
        context.SetColumnWidth("team", 10);
        Assert.Equal(64, context.EffectiveWidth(team, true));
        context.SetColumnWidth("team", 1200);
        Assert.Equal(900, context.EffectiveWidth(team, true));
        context.SetColumnWidth("team", null);
        Assert.Null(context.ColumnStates.First(c => c.Key == "team").Width);
    }

    [Fact]
    public async Task The_columns_changed_event_fires_for_a_change_of_the_columns()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        var count = 0;
        list.Context.ColumnsChanged += () => count++;
        list.Context.SetColumnVisible("team", false);
        Assert.Equal(1, count);
    }

    // ---- sort options

    [Fact]
    public async Task The_sort_options_are_the_sortable_columns_then_the_other_sortable_fields()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()));
        Assert.Equal(["name", "team", "salary", "hired"], list.Context.SortOptions().Select(o => o.Key));
        Assert.Equal("Name", list.Context.SortOptions()[0].Label);
        Assert.Equal("Hired", list.Context.SortOptions()[3].Label);
        var explicitOptions = list.Context.SortOptions([new DataListSortOption("salary", "Pay"), new DataListSortOption("note")]);
        Assert.Equal([("salary", "Pay")], explicitOptions.Select(o => (o.Key, o.Label)));
    }

    // ---- groups

    [Fact]
    public async Task Rows_are_grouped_by_a_value_with_the_empty_group_last()
    {
        var items = TableHarness.People().Append(new Person(6, "Fay", "", 1m, new(2020, 1, 1))).ToArray();
        var table = new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team) };
        var list = await TableHarness.Mount(s => s.Children.Add(table), items: items, pageSize: 10);
        var groups = list.Context.Groups();
        Assert.Equal(["Backend", "Frontend", "Infra", "Other"], groups.Select(g => g.Label));
        Assert.Equal(["2", "2", "1", "1"], groups.Select(g => g.Summary));
        Assert.True(groups[^1].IsEmptyValue);
        Assert.Equal(DataListGroupBy<Person>.EmptyGroupKey, groups[^1].Key);
        Assert.Equal(["1", "3"], groups[0].RowKeys);
    }

    [Fact]
    public async Task A_group_order_puts_the_listed_values_first()
    {
        var table = new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team, ["Infra", "Frontend"]) };
        var list = await TableHarness.Mount(s => s.Children.Add(table), pageSize: 10);
        Assert.Equal(["Infra", "Frontend", "Backend"], list.Context.Groups().Select(g => g.Label));
    }

    [Fact]
    public async Task A_grouping_by_a_field_of_the_schema_uses_its_text()
    {
        var table = new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.ByField("active") };
        var list = await TableHarness.Mount(s => s.Children.Add(table), pageSize: 10);
        Assert.Equal(["False", "True"], list.Context.Groups().Select(g => g.Label));
    }

    [Fact]
    public async Task Groups_fold_and_unfold_and_a_predicate_can_start_them_folded()
    {
        var grouping = DataListGroupBy<Person>.By(p => p.Team);
        grouping.DefaultCollapsed = g => g.Label == "INFRA";
        grouping.Label = g => g.Label.ToUpperInvariant();
        grouping.Summary = g => $"{g.Items.Count} people";
        var list = await TableHarness.Mount(s => s.Children.Add(new KlyDataListTable<Person> { GroupBy = grouping }), pageSize: 10);
        var groups = list.Context.Groups();
        Assert.Equal(["BACKEND", "FRONTEND", "INFRA"], groups.Select(g => g.Label));
        Assert.Equal("2 people", groups[0].Summary);
        Assert.False(list.Context.IsGroupCollapsed(groups[0]));
        Assert.True(list.Context.IsGroupCollapsed(groups[2]));
        list.Context.ToggleGroup(groups[0]);
        list.Context.ToggleGroup(groups[2]);
        Assert.True(list.Context.IsGroupCollapsed(groups[0]));
        Assert.False(list.Context.IsGroupCollapsed(groups[2]));
    }

    [Fact]
    public async Task A_group_selection_is_mixed_until_all_its_rows_are_ticked()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(new KlyDataListTable<Person> { GroupBy = DataListGroupBy<Person>.By(p => p.Team) }), configure: l => l.Selectable = true, pageSize: 10);
        var backend = list.Context.Groups()[0];
        Assert.False(list.Context.GetGroupSelection(backend));
        list.Context.SetSelected(backend.Items[0], true);
        Assert.Null(list.Context.GetGroupSelection(backend));
        list.Context.SetGroupSelected(backend, true);
        Assert.True(list.Context.GetGroupSelection(backend));
        list.Context.SetGroupSelected(backend, false);
        Assert.False(list.Context.GetGroupSelection(backend));
    }

    // ---- filters

    [Fact]
    public async Task A_filter_rule_replaces_the_one_of_its_key_and_goes_back_to_the_first_page()
    {
        var list = await TableHarness.Mount();
        var state = list.State! with { Page = 3 };
        var rule = new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"]));
        var next = DataListContext<Person>.WithFilter(state, "team", rule);
        Assert.Equal([rule], next.Filters);
        Assert.Equal(1, next.Page);
        var other = new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Infra"]));
        Assert.Equal([other], DataListContext<Person>.WithFilter(next, "team", other).Filters);
        Assert.Empty(DataListContext<Person>.WithFilter(next, "team", null).Filters);
        Assert.Empty(DataListContext<Person>.WithoutFilters(next).Filters);
    }

    [Fact]
    public async Task An_applied_filter_loads_the_matching_rows_and_is_summarized_for_its_tag()
    {
        var list = await TableHarness.Mount(pageSize: 10);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend", "Infra", "Frontend"])));
        await list.Context.WhenLoaded();
        Assert.Equal(5, list.Context.Rows.Count);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        Assert.Equal(2, list.Context.Rows.Count);
        var (op, value) = list.Context.Summarize(list.Context.ActiveFilters[0]);
        Assert.Equal("is", op);
        Assert.Equal("Backend", value);
        Assert.Equal("Team", list.Context.FilterLabel("team"));
        var summary = list.Context.Summarize(new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["a", "b", "c", "d"])));
        Assert.Equal("a, b +2", summary.Value);
        Assert.Equal(("is greater than", "1,000"), list.Context.Summarize(new FilterRule("salary", FilterOperator.Gt, new FilterValue.Number(1000))));
        Assert.Equal(("is", "Yes"), list.Context.Summarize(new FilterRule("active", FilterOperator.Is, new FilterValue.Boolean(true))));
        Assert.Equal(("is", "empty"), list.Context.Summarize(new FilterRule("team", FilterOperator.Is, null, IncludeEmpty: true)));
    }

    [Fact]
    public async Task The_options_of_an_option_filter_come_with_their_counts()
    {
        var list = await TableHarness.Mount(pageSize: 10);
        var choices = list.Context.FilterChoices("team", []);
        Assert.Equal([("Backend", 2), ("Frontend", 2), ("Infra", 1)], choices.Select(c => (c.Value, c.Count!.Value)).OrderBy(c => c.Item1));
        var withSelected = list.Context.FilterChoices("team", ["Gone"]);
        Assert.Equal("Gone", withSelected[^1].Value);
        Assert.Equal(["name", "team", "salary", "hired", "active"], list.Context.EditableFilterKeys);
    }

    [Fact]
    public async Task The_editor_operators_leave_out_is_next_to_is_any_of()
    {
        var list = await TableHarness.Mount();
        Assert.DoesNotContain(FilterOperator.Is, list.Context.EditorOperators("team", null));
        Assert.Contains(FilterOperator.Is, list.Context.EditorOperators("team", FilterOperator.Is));
        Assert.DoesNotContain(FilterOperator.IsEmpty, list.Context.EditorOperators("name", null));
    }

    // ---- export

    [Fact]
    public async Task The_export_writes_the_visible_columns_in_the_order_the_user_sees_them()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(Table()), pageSize: 10);
        var context = list.Context;
        Assert.Equal(["Name", "Team", "Salary"], context.ExportHeaders());
        context.SetColumnVisible("team", false);
        Assert.Equal(["Name", "Salary"], context.ExportHeaders());
        var csv = context.ToCsv(context.Rows, new DataListCsvOptions { Culture = CultureInfo.InvariantCulture, Utf8Bom = false, NewLine = DataListCsvNewLine.Lf });
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("Name,Salary", lines[0]);
        Assert.Equal("Alice Martin,\"120,000\"", lines[1]);
        Assert.Equal(6, lines.Length);
    }

    [Fact]
    public async Task Without_columns_the_export_writes_the_fields_of_the_schema()
    {
        var list = await TableHarness.Mount(pageSize: 10);
        Assert.Equal(["Name", "Team", "Salary", "Hired", "Active", "Note"], list.Context.ExportHeaders());
    }

    [Fact]
    public async Task A_text_cell_is_protected_against_formulas_and_a_number_is_not()
    {
        var list = await TableHarness.Mount(s => s.Children.Add(new KlyDataListTable<Person>
        {
            Columns =
            {
                new KlyDataListColumn<Person, string?> { Property = p => p.Note, Title = "Note" },
                new KlyDataListColumn<Person, decimal> { Property = p => -p.Salary, Title = "Debt", Key = "debt" },
            },
        }), pageSize: 10);
        var csv = list.Context.ToCsv(list.Context.Rows, new DataListCsvOptions { Culture = CultureInfo.InvariantCulture, Utf8Bom = false, NewLine = DataListCsvNewLine.Lf, Delimiter = ';' });
        Assert.Contains("'=HYPERLINK", csv);
        Assert.Contains(";-120000", csv);
    }

    [Fact]
    public async Task The_export_scopes_pick_the_page_the_selection_or_every_row()
    {
        var list = await TableHarness.Mount(configure: l => l.Selectable = true);
        var context = list.Context;
        Assert.Equal(2, (await context.ExportRowsAsync(DataListExportScope.Page, TestContext.Current.CancellationToken)).Count);
        context.SetSelected(context.Rows[1], true);
        Assert.Equal(["Bob Chen"], (await context.ExportRowsAsync(DataListExportScope.Selection, TestContext.Current.CancellationToken)).Select(p => p.Name));
        Assert.Equal(5, (await context.ExportRowsAsync(DataListExportScope.All, TestContext.Current.CancellationToken)).Count);
        Assert.True(context.HoldsAllRows);
    }
}
