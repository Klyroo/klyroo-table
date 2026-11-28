namespace Klyroo.Table.Maui.Tests;

/// <summary>The filter editor and the parts that show it: the tags, the add button and the panel.</summary>
public class DataListFilterTests
{
    private static KlyDataListFilterEditor<Person> Editor(string key, DataListApplyMode mode = DataListApplyMode.Live) => new() { Key = key, ApplyMode = mode };

    private static Task<KlyDataList<Person>> With(KlyDataListFilterEditor<Person> editor, int pageSize = 10) =>
        TableHarness.Mount(s => s.Children.Add(editor), pageSize: pageSize);

    // ---- the editor, by kind

    [Fact]
    public async Task A_text_editor_applies_a_rule_after_a_pause_in_the_typing()
    {
        var editor = Editor("name");
        var list = await With(editor);
        Assert.NotNull(editor.TextInput);
        editor.TextInput!.Text = "ali";
        Assert.Empty(list.Context.State.Filters);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await list.Context.WhenLoaded();
        var rule = Assert.Single(list.Context.State.Filters);
        Assert.Equal(new FilterRule("name", FilterOperator.Contains, new FilterValue.Text("ali")), rule);
        Assert.Equal(["Alice Martin"], list.Context.Rows.Select(p => p.Name));
        editor.TextInput!.Text = "";
        editor.Commit();
        Assert.Empty(list.Context.State.Filters);
    }

    [Fact]
    public async Task The_text_editor_keeps_its_field_while_the_list_changes()
    {
        var editor = Editor("name");
        var list = await With(editor);
        var field = editor.TextInput;
        editor.TextInput!.Text = "ali";
        editor.Commit();
        await list.Context.WhenLoaded();
        Assert.Same(field, editor.TextInput);
    }

    [Fact]
    public async Task The_editor_offers_the_operators_of_the_filter()
    {
        var editor = Editor("name");
        var list = await With(editor);
        Assert.NotNull(editor.OperatorSelect);
        editor.TextInput!.Text = "Alice Martin";
        editor.SetOperatorTo(FilterOperator.Is);
        Assert.Equal(new FilterRule("name", FilterOperator.Is, new FilterValue.Text("Alice Martin")), list.Context.State.Filters.Single());
        Assert.Equal("is", editor.OperatorSelect!.SelectedItem!.ToString());
    }

    [Fact]
    public async Task An_option_editor_lists_the_options_with_their_counts_and_applies_the_ticked_ones()
    {
        var editor = Editor("team");
        var list = await With(editor);
        Assert.Equal(3, editor.OptionBoxes.Count);
        Assert.Contains(editor.OptionBoxes, b => b.Text == "Backend (2)");
        editor.SetOption("Backend", true);
        await list.Context.WhenLoaded();
        Assert.Equal(new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])), list.Context.State.Filters.Single());
        editor.SetOption("Infra", true);
        Assert.Equal(["Backend", "Infra"], ((FilterValue.List)list.Context.State.Filters.Single().Value!).Values);
        editor.SetOption("Backend", false);
        editor.SetOption("Infra", false);
        Assert.Empty(list.Context.State.Filters);
    }

    [Fact]
    public async Task Ticking_an_option_box_applies_the_rule()
    {
        var editor = Editor("team");
        var list = await With(editor);
        editor.OptionBoxes.First(b => b.Text!.StartsWith("Frontend", StringComparison.Ordinal)).IsChecked = true;
        Assert.Equal(["Frontend"], ((FilterValue.List)list.Context.State.Filters.Single().Value!).Values);
    }

    [Fact]
    public async Task With_the_operator_is_an_option_editor_keeps_one_value()
    {
        var editor = Editor("team");
        var list = await With(editor);
        editor.SetOperatorTo(FilterOperator.Is);
        editor.SetOption("Backend", true);
        editor.SetOption("Infra", true);
        Assert.Equal(new FilterRule("team", FilterOperator.Is, new FilterValue.Text("Infra")), list.Context.State.Filters.Single());
    }

    [Fact]
    public async Task An_option_editor_can_include_the_empty_values()
    {
        var items = TableHarness.People().Append(new Person(6, "Fay", "", 1m, new(2020, 1, 1))).ToArray();
        var editor = Editor("team");
        var list = await TableHarness.Mount(s => s.Children.Add(editor), items: items, pageSize: 10);
        Assert.NotNull(editor.IncludeEmptyBox);
        editor.IncludeEmptyBox!.IsChecked = true;
        var rule = list.Context.State.Filters.Single();
        Assert.True(rule.IncludeEmpty);
        Assert.Equal(new FilterValue.List([]), rule.Value);
    }

    [Fact]
    public async Task A_long_list_of_options_has_a_search_box_and_a_show_more_button()
    {
        var teams = Enumerable.Range(1, 12).Select(i => new Person(i, "P" + i, "Team " + i, 1m, new(2020, 1, 1))).ToArray();
        var editor = Editor("team");
        var list = await TableHarness.Mount(s => s.Children.Add(editor), items: teams, pageSize: 20);
        Assert.NotNull(editor.SearchInput);
        Assert.Equal(8, editor.OptionBoxes.Count);
        editor.SearchInput!.Text = "Team 1";
        Assert.Equal(4, editor.OptionBoxes.Count);
        editor.SearchInput.Text = "zzz";
        Assert.Empty(editor.OptionBoxes);
        Assert.Same(editor.SearchInput, editor.SearchInput);
        _ = list;
    }

    [Fact]
    public async Task A_boolean_editor_applies_yes_no_or_any()
    {
        var editor = Editor("active");
        var list = await With(editor);
        Assert.Null(editor.OperatorSelect);
        editor.SetBoolean(true);
        Assert.Equal(new FilterRule("active", FilterOperator.Is, new FilterValue.Boolean(true)), list.Context.State.Filters.Single());
        await list.Context.WhenLoaded();
        Assert.Equal(3, list.Context.Rows.Count);
        editor.SetBoolean(null);
        Assert.Empty(list.Context.State.Filters);
        editor.BooleanSelect!.SelectedItem = editor.BooleanSelect.ItemsSource![2];
        Assert.False(((FilterValue.Boolean)list.Context.State.Filters.Single().Value!).Value);
    }

    [Fact]
    public async Task A_number_editor_applies_a_value_or_a_range()
    {
        var editor = Editor("salary");
        var list = await With(editor);
        Assert.NotNull(editor.FromInput);
        Assert.Null(editor.ToInput);
        editor.SetFrom(100_000);
        Assert.Equal(new FilterValue.Number(100_000), list.Context.State.Filters.Single().Value);
        editor.SetOperatorTo(FilterOperator.Between);
        Assert.NotNull(editor.ToInput);
        editor.SetTo(115_000);
        var rule = list.Context.State.Filters.Single();
        Assert.Equal(FilterOperator.Between, rule.Operator);
        Assert.Equal(new FilterValue.Range(new FilterValue.Number(100_000), new FilterValue.Number(115_000)), rule.Value);
        await list.Context.WhenLoaded();
        Assert.Equal(["Claire Petit"], list.Context.Rows.Select(p => p.Name));
        editor.SetFrom(null);
        editor.SetTo(null);
        Assert.Empty(list.Context.State.Filters);
    }

    [Fact]
    public async Task A_number_field_reports_its_value_to_the_editor()
    {
        var editor = Editor("salary");
        var list = await With(editor);
        editor.FromInput!.Value = 50_000;
        Assert.Equal(new FilterValue.Number(50_000), list.Context.State.Filters.Single().Value);
    }

    [Fact]
    public async Task A_date_editor_applies_a_day_a_range_or_a_preset()
    {
        var editor = Editor("hired");
        var list = await With(editor);
        Assert.NotNull(editor.DayInput);
        Assert.Equal(6 - 3, editor.PresetButtons.Count);
        editor.SetDay(new DateOnly(2024, 1, 1));
        Assert.Equal(new FilterValue.Date(new DateOnly(2024, 1, 1)), list.Context.State.Filters.Single().Value);
        editor.SetOperatorTo(FilterOperator.Between);
        Assert.NotNull(editor.DayToInput);
        Assert.Equal(6, editor.PresetButtons.Count);
        editor.SetDay(new DateOnly(2024, 1, 1), end: false);
        editor.SetDay(new DateOnly(2025, 1, 1), end: true);
        var range = Assert.IsType<FilterValue.Range>(list.Context.State.Filters.Single().Value);
        Assert.Equal(new FilterValue.Date(new DateOnly(2025, 1, 1)), range.To);
        editor.ApplyPreset(DatePreset.Last7Days);
        var preset = Assert.IsType<FilterValue.Range>(list.Context.State.Filters.Single().Value);
        Assert.Equal(new FilterValue.Date(list.Context.Today), preset.To);
        Assert.Equal(new FilterValue.Date(list.Context.Today.AddDays(-6)), preset.From);
    }

    [Fact]
    public async Task A_date_picker_reports_its_day_to_the_editor()
    {
        var editor = Editor("hired");
        var list = await With(editor);
        editor.DayInput!.Value = new DateTime(2023, 6, 1);
        Assert.Equal(new FilterValue.Date(new DateOnly(2023, 6, 1)), list.Context.State.Filters.Single().Value);
    }

    [Fact]
    public async Task A_filter_without_an_editor_shows_nothing()
    {
        var unknown = Editor("nothing");
        var seconds = new KlyDataListFilterEditor<Person> { Key = "when" };
        var schema = DataListSchema.For<Person>(s => s
            .RowKey(p => p.Id)
            .Field(p => p.Hired, f => f.Key("when").Filter(DataListFilter.Date(DateGranularity.Second))));
        var list = new KlyDataList<Person> { Schema = schema, Items = TableHarness.People(), Content = new VerticalStackLayout { Children = { unknown, seconds } } };
        await list.LoadAsync();
        Assert.Null(unknown.Content);
        Assert.Null(seconds.Content);
    }

    [Fact]
    public async Task The_title_names_the_filter_unless_it_is_hidden()
    {
        var editor = Editor("team");
        await With(editor);
        var root = (VerticalStackLayout)editor.Content!;
        Assert.Equal("Team", ((Label)root.Children[0]).Text);
        editor.ShowTitle = false;
        Assert.IsNotType<Label>(((VerticalStackLayout)editor.Content!).Children[0]);
    }

    // ---- apply modes

    [Fact]
    public async Task In_submit_mode_the_changes_wait_for_apply_and_cancel_drops_them()
    {
        var editor = Editor("team", DataListApplyMode.Submit);
        var list = await With(editor);
        var done = 0;
        editor.Done += (_, _) => done++;
        editor.SetOption("Backend", true);
        Assert.Empty(list.Context.State.Filters);
        Assert.NotNull(editor.ApplyButton);
        editor.ApplyButton!.Activate();
        Assert.Equal(1, done);
        Assert.Equal(["Backend"], ((FilterValue.List)list.Context.State.Filters.Single().Value!).Values);
        editor.SetOption("Infra", true);
        editor.CancelButton!.Activate();
        Assert.Equal(2, done);
        Assert.Equal(["Backend"], ((FilterValue.List)list.Context.State.Filters.Single().Value!).Values);
        Assert.True(editor.OptionBoxes.First(b => b.Text!.StartsWith("Backend", StringComparison.Ordinal)).IsChecked);
        Assert.False(editor.OptionBoxes.First(b => b.Text!.StartsWith("Infra", StringComparison.Ordinal)).IsChecked);
    }

    [Fact]
    public async Task A_live_editor_has_no_apply_button()
    {
        var editor = Editor("team");
        await With(editor);
        Assert.Null(editor.ApplyButton);
    }

    [Fact]
    public async Task A_deferred_scope_stages_the_change_instead_of_applying_it()
    {
        var scope = new FilterDraftScope { Deferred = true };
        var editor = new KlyDataListFilterEditor<Person> { Key = "team", Scope = scope };
        var list = await With(editor);
        editor.SetOption("Backend", true);
        Assert.Empty(list.Context.State.Filters);
        Assert.True(scope.TryGetStaged("team", out var staged));
        Assert.Equal(["Backend"], ((FilterValue.List)staged!.Value!).Values);
        editor.SetOption("Backend", false);
        Assert.True(scope.TryGetStaged("team", out var removed) || !scope.HasPending);
        _ = removed;
    }

    [Fact]
    public async Task A_rule_that_changed_without_the_editor_is_shown_in_it()
    {
        var editor = Editor("team");
        var list = await With(editor);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Infra"])));
        await list.Context.WhenLoaded();
        Assert.True(editor.OptionBoxes.First(b => b.Text!.StartsWith("Infra", StringComparison.Ordinal)).IsChecked);
        list.Context.SetState(DataListContext<Person>.WithoutFilters);
        await list.Context.WhenLoaded();
        Assert.All(editor.OptionBoxes, b => Assert.False(b.IsChecked));
        Assert.Null(editor.Draft);
    }

    [Fact]
    public async Task Changing_the_key_shows_the_other_filter()
    {
        var editor = Editor("team");
        await With(editor);
        editor.Key = "name";
        Assert.NotNull(editor.TextInput);
        Assert.Empty(editor.OptionBoxes);
    }

    // ---- remote options

    [Fact]
    public async Task A_remote_option_editor_loads_a_page_and_searches_the_source()
    {
        var requests = new List<DataListOptionsRequest>();
        var schema = DataListSchema.For<Person>(s => s
            .RowKey(p => p.Id)
            .Field(p => p.Team, f => f.Key("team").Filter(DataListFilter.Option().WithRemoteOptions((request, _) =>
            {
                requests.Add(request);
                var all = new[] { "Backend", "Frontend", "Infra" }.Where(t => request.Search.Length == 0 || t.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).Select(t => new DataListFilterOption(t, t.ToUpperInvariant())).ToList();
                return ValueTask.FromResult(new DataListOptionsPage(all.Take(2).ToList(), HasMore: all.Count > 2));
            }, pageSize: 2))));
        var editor = Editor("team");
        var list = new KlyDataList<Person> { Schema = schema, Items = TableHarness.People(), Content = new VerticalStackLayout { Children = { editor } } };
        await list.LoadAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.Equal(["BACKEND", "FRONTEND"], editor.OptionBoxes.Select(b => b.Text));
        Assert.Equal(new DataListOptionsRequest("", 0, 2, null), requests[0]);
        editor.SetOption("Backend", true);
        Assert.Equal("BACKEND", list.Context.OptionLabelOf("team", "Backend"));
        editor.SearchInput!.Text = "inf";
        await Task.Delay(600, TestContext.Current.CancellationToken);
        Assert.Contains(requests, r => r.Search == "inf");
        Assert.Contains(editor.OptionBoxes, b => b.Text == "INFRA");
        Assert.Contains(editor.OptionBoxes, b => b.Text == "BACKEND" && b.IsChecked);
    }

    [Fact]
    public async Task A_remote_source_that_fails_shows_the_error_and_a_retry()
    {
        var calls = 0;
        var schema = DataListSchema.For<Person>(s => s
            .RowKey(p => p.Id)
            .Field(p => p.Team, f => f.Key("team").Filter(DataListFilter.Option().WithRemoteOptions((_, _) =>
            {
                calls++;
                return calls == 1 ? throw new InvalidOperationException("offline") : ValueTask.FromResult(new DataListOptionsPage([new DataListFilterOption("a", "A")]));
            }))));
        var editor = Editor("team");
        var list = new KlyDataList<Person> { Schema = schema, Items = TableHarness.People(), Content = new VerticalStackLayout { Children = { editor } } };
        await list.LoadAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var texts = ((VerticalStackLayout)editor.Content!).Descendants<Label>().Select(l => l.Text).ToList();
        Assert.Contains("offline", texts);
        var retry = ((VerticalStackLayout)editor.Content!).Descendants<TableButton>().First(b => b.TextView.Text == "Try again");
        retry.Activate();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.Equal(["A"], editor.OptionBoxes.Select(b => b.Text));
    }

    // ---- tags

    [Fact]
    public async Task There_is_a_tag_per_active_filter_and_it_survives_the_changes_of_the_list()
    {
        var tags = new KlyDataListFilterTags<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(tags));
        Assert.Null(tags.Content);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        var tag = tags.Tags["team"];
        Assert.Equal("Team is Backend", tag.Text.FormattedText.ToString());
        list.Context.ApplyFilter("name", new FilterRule("name", FilterOperator.Contains, new FilterValue.Text("a")));
        await list.Context.WhenLoaded();
        Assert.Same(tag, tags.Tags["team"]);
        Assert.Equal(["team", "name"], tags.Tags.Keys);
        Assert.Equal("Name contains a", tags.Tags["name"].Text.FormattedText.ToString());
        Assert.Equal("Filters, 2 active", SemanticProperties.GetDescription((View)tags.Content!));
        tags.Tags["team"].Remove.Activate();
        await list.Context.WhenLoaded();
        Assert.Equal(["name"], tags.Tags.Keys);
        Assert.Equal("Remove filter Name", SemanticProperties.GetDescription(tags.Tags["name"].Remove));
    }

    [Fact]
    public async Task A_tag_edits_its_filter_in_a_popover()
    {
        var tags = new KlyDataListFilterTags<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(tags));
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        var tag = tags.Tags["team"];
        Assert.NotNull(tag.Popover);
        Assert.Same(tag.Editor, tag.Popover!.PanelContent);
        Assert.Equal("team", tag.Editor!.Key);
        Assert.False(tag.Editor.ShowTitle);
        tag.Editor.SetOption("Infra", true);
        Assert.Equal(["Backend", "Infra"], ((FilterValue.List)list.Context.State.Filters.Single().Value!).Values);
        tags.ApplyMode = DataListApplyMode.Submit;
        Assert.Equal(DataListApplyMode.Submit, tag.Editor.ApplyMode);
    }

    [Fact]
    public async Task The_tags_can_show_the_add_and_clear_buttons()
    {
        var tags = new KlyDataListFilterTags<Person> { ShowAdd = true, ShowClear = true };
        var list = await TableHarness.Mount(s => s.Children.Add(tags));
        Assert.NotNull(tags.AddButton);
        Assert.NotNull(tags.Content);
        Assert.NotNull(tags.ClearButton);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        Assert.NotNull(tags.ClearButton!.Content);
    }

    // ---- add filter

    [Fact]
    public async Task The_add_button_lists_the_filters_that_are_not_on_and_shows_the_editor_of_the_one_picked()
    {
        var add = new KlyDataListAddFilter<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(add));
        Assert.Equal(["name", "team", "salary", "hired", "active"], add.Available);
        Assert.Equal("Add filter", add.Trigger!.TextView.Text);
        Assert.Equal(5, add.Choices.Count);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        Assert.DoesNotContain("team", add.Available);
        add.Pick("salary");
        Assert.NotNull(add.Editor);
        Assert.Equal("salary", add.Editor!.Key);
        Assert.Equal("salary", add.Picked);
        add.Editor.SetFrom(1);
        Assert.Equal(2, list.Context.State.Filters.Count);
    }

    [Fact]
    public async Task The_add_button_says_when_every_filter_is_in_use_and_can_search_the_filters()
    {
        var add = new KlyDataListAddFilter<Person> { SearchFrom = 3 };
        var list = await TableHarness.Mount(s => s.Children.Add(add));
        var panel = (VerticalStackLayout)add.Popover.PanelContent!;
        Assert.Contains(panel.Children, c => c is KlyInput);
        foreach (var key in add.Available) list.Context.ApplyFilter(key, new FilterRule(key, FilterOperator.Is, new FilterValue.Text("x")));
        await list.Context.WhenLoaded();
        Assert.Empty(add.Available);
        var none = ((VerticalStackLayout)add.Popover.PanelContent!).Children.OfType<Label>().Single();
        Assert.Equal("Every filter is in use", none.Text);
    }

    // ---- panel

    [Fact]
    public async Task The_panel_has_a_section_per_filter_and_opens_the_active_ones()
    {
        var panel = new KlyDataListFilterPanel<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(panel));
        Assert.Equal(["name", "team", "salary", "hired", "active"], panel.Headers.Keys);
        Assert.Empty(panel.Editors);
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        Assert.True(panel.IsExpanded("team"));
        Assert.Equal(["team"], panel.Editors.Keys);
        panel.Toggle("salary");
        Assert.Equal(["team", "salary"], panel.Editors.Keys.Order(StringComparer.Ordinal).Reverse());
        panel.Toggle("salary");
        Assert.Equal(["team"], panel.Editors.Keys);
    }

    [Fact]
    public async Task The_live_panel_applies_at_once_and_clears_every_filter()
    {
        var panel = new KlyDataListFilterPanel<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(panel));
        panel.Toggle("team");
        panel.Editors["team"].SetOption("Backend", true);
        Assert.Single(list.Context.State.Filters);
        Assert.Null(panel.ApplyButton);
        panel.ClearAll();
        await list.Context.WhenLoaded();
        Assert.Empty(list.Context.State.Filters);
    }

    [Fact]
    public async Task The_submit_panel_stages_the_editors_and_applies_them_in_one_change()
    {
        var panel = new KlyDataListFilterPanel<Person> { ApplyMode = DataListApplyMode.Submit, Description = "Narrow the list" };
        var changes = 0;
        var list = await TableHarness.Mount(s => s.Children.Add(panel));
        list.StateChanged += (_, _) => changes++;
        panel.Toggle("team");
        panel.Toggle("salary");
        Assert.False(panel.ApplyButton!.IsEnabled);
        var team = panel.Editors["team"];
        var salary = panel.Editors["salary"];
        team.SetOption("Backend", true);
        salary.SetOperatorTo(FilterOperator.Gte);
        salary.SetFrom(100_000);
        Assert.Empty(list.Context.State.Filters);
        Assert.True(panel.ApplyButton!.IsEnabled);
        Assert.Same(team, panel.Editors["team"]);
        panel.ApplyStaged();
        await list.Context.WhenLoaded();
        Assert.Equal(["team", "salary"], list.Context.State.Filters.Select(r => r.Key));
        Assert.Equal(1, changes);
        Assert.Equal(["Alice Martin", "Claire Petit"], list.Context.Rows.Select(p => p.Name));
    }

    [Fact]
    public async Task The_submit_panel_resets_what_was_staged_and_stages_the_removal_of_the_filters()
    {
        var panel = new KlyDataListFilterPanel<Person> { ApplyMode = DataListApplyMode.Submit };
        var list = await TableHarness.Mount(s => s.Children.Add(panel));
        list.Context.ApplyFilter("team", new FilterRule("team", FilterOperator.IsAnyOf, new FilterValue.List(["Backend"])));
        await list.Context.WhenLoaded();
        panel.Editors["team"].SetOption("Infra", true);
        panel.ResetStaged();
        Assert.True(panel.Editors["team"].OptionBoxes.First(b => b.Text!.StartsWith("Backend", StringComparison.Ordinal)).IsChecked);
        Assert.False(panel.Editors["team"].OptionBoxes.First(b => b.Text!.StartsWith("Infra", StringComparison.Ordinal)).IsChecked);
        panel.ClearAll();
        Assert.Single(list.Context.State.Filters);
        Assert.True(panel.ApplyButton!.IsEnabled);
        panel.ApplyStaged();
        await list.Context.WhenLoaded();
        Assert.Empty(list.Context.State.Filters);
    }
}
