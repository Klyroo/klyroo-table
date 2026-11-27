using System.Globalization;

namespace Klyroo.Table.Maui.Tests;

/// <summary>The row actions, the selection bar, the actions menu, the column panel and the export.</summary>
public class DataListActionsTests
{
    private static async Task<(KlyDataList<Person> List, KlyDataListRowActions<Person> Part)> Row(IReadOnlyList<DataListRowAction<Person>> actions, int maxVisible = 3)
    {
        var part = new KlyDataListRowActions<Person> { Actions = actions, MaxVisible = maxVisible };
        var list = await TableHarness.Mount(s => s.Children.Add(part));
        part.Item = list.Context.Rows[0];
        part.Position = 1;
        return (list, part);
    }

    private static DataListRowAction<Person> Action(string key, Func<DataListRowActionContext<Person>, Task>? run = null) =>
        new() { Key = key, Label = key.ToUpperInvariant(), OnExecute = run };

    // ---- row actions

    [Fact]
    public async Task The_first_actions_are_buttons_and_the_rest_are_in_a_menu_with_submenus()
    {
        var (_, part) = await Row(
        [
            Action("a"),
            Action("b"),
            new DataListRowAction<Person> { Key = "more", Label = "More", Children = [Action("c"), Action("d")] },
            Action("e"),
        ], maxVisible: 2);
        Assert.Equal(["a", "b"], part.Buttons.Keys);
        Assert.NotNull(part.Menu);
        var items = part.Menu!.Items;
        Assert.Equal(["More", "E"], items.Select(i => i.Label));
        Assert.Equal(["C", "D"], items[0].Children!.Select(i => i.Label));
        Assert.Equal("Actions for row 1", part.Menu.AccessibleName);
    }

    [Fact]
    public async Task With_no_visible_button_the_row_has_one_menu_button()
    {
        var (_, part) = await Row([Action("a"), Action("b")], maxVisible: 0);
        Assert.Empty(part.Buttons);
        Assert.Equal(["A", "B"], part.Menu!.Items.Select(i => i.Label));
    }

    [Fact]
    public async Task An_action_that_is_hidden_for_the_row_is_not_drawn_and_one_that_is_disabled_says_why()
    {
        var (_, part) = await Row(
        [
            new DataListRowAction<Person> { Key = "hide", Label = "Hide", Visible = c => c.Item.Id != 1 },
            new DataListRowAction<Person> { Key = "off", Label = "Off", Disabled = _ => true, DisabledReason = _ => "Locked" },
            new DataListRowAction<Person> { Key = "link", Label = "Link", Href = c => c.Item.Id == 1 ? null : "/x" },
        ]);
        Assert.Equal(["off"], part.Buttons.Keys);
        Assert.False(part.Buttons["off"].IsEnabled);
        Assert.Equal("Locked", part.Buttons["off"].AccessibleHint);
        Assert.Equal(0.6, part.Buttons["off"].Opacity);
    }

    [Fact]
    public async Task A_row_action_runs_with_the_row_its_key_and_its_place_and_does_not_start_twice()
    {
        var gate = new TaskCompletionSource();
        var contexts = new List<DataListRowActionContext<Person>>();
        var (_, part) = await Row([Action("go", async c =>
        {
            contexts.Add(c);
            await gate.Task;
        })]);
        var first = part.Choose("go");
        Assert.True(part.Buttons["go"].IsBusy);
        await part.Choose("go");
        Assert.Single(contexts);
        gate.SetResult();
        await first;
        Assert.False(part.Buttons["go"].IsBusy);
        Assert.Equal(("1", 1, "Alice Martin"), (contexts[0].Key, contexts[0].Position, contexts[0].Item.Name));
    }

    [Fact]
    public async Task A_row_action_that_throws_or_fails_is_reported_and_does_not_refresh_the_list()
    {
        var calls = 0;
        var part = new KlyDataListRowActions<Person>
        {
            Actions =
            [
                new DataListRowAction<Person> { Key = "throw", Label = "T", RefreshOnSuccess = true, OnExecute = _ => throw new InvalidOperationException("boom") },
                new DataListRowAction<Person> { Key = "fail", Label = "F", RefreshOnSuccess = true, OnExecute = c => { c.Fail("refused"); return Task.CompletedTask; } },
                new DataListRowAction<Person> { Key = "ok", Label = "O", RefreshOnSuccess = true, OnExecute = _ => Task.CompletedTask },
            ],
        };
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = (r, t) => { calls++; return inner(r, t); }, Content = new VerticalStackLayout { Children = { part } } };
        await list.LoadAsync();
        part.Item = list.Context.Rows[0];
        var failures = new List<DataListRowActionFailure<Person>>();
        part.Error += (_, e) => failures.Add(e);
        await part.Choose("throw");
        await part.Choose("fail");
        Assert.Equal(["boom", "refused"], failures.Select(f => f.Exception?.Message ?? f.Message));
        Assert.Equal(1, calls);
        await part.Choose("ok");
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task A_row_action_can_ask_first_and_a_refusal_stops_it()
    {
        var ran = 0;
        var action = new DataListRowAction<Person>
        {
            Key = "del",
            Label = "Delete",
            Color = KlyColor.Error,
            Confirm = new DataListRowActionConfirm<Person> { Title = "Delete?", Description = c => c.Item.Name + " will be removed." },
            OnExecute = _ => { ran++; return Task.CompletedTask; },
        };
        var (_, part) = await Row([action]);
        TableConfirmRequest? asked = null;
        var answer = false;
        part.Confirmer = request =>
        {
            asked = request;
            return Task.FromResult(answer);
        };
        await part.Choose("del");
        Assert.Equal(0, ran);
        Assert.Equal(new TableConfirmRequest("Delete?", "Alice Martin will be removed.", "Confirm", "Cancel", KlyColor.Error), asked);
        answer = true;
        await part.Choose("del");
        Assert.Equal(1, ran);
    }

    [Fact]
    public async Task Without_an_overlay_host_a_confirmation_says_so()
    {
        var action = new DataListRowAction<Person> { Key = "del", Label = "Delete", Confirm = new DataListRowActionConfirm<Person> { Title = "Delete?" }, OnExecute = _ => Task.CompletedTask };
        var (_, part) = await Row([action]);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => part.Choose("del"));
        Assert.Contains("KlyOverlayHost", error.Message);
    }

    [Fact]
    public async Task A_child_of_a_disabled_action_cannot_run_and_a_link_is_handed_to_the_app()
    {
        var ran = 0;
        var (_, part) = await Row(
        [
            new DataListRowAction<Person> { Key = "group", Label = "Group", Disabled = _ => true, DisabledReason = _ => "No", Children = [Action("child", _ => { ran++; return Task.CompletedTask; })] },
            new DataListRowAction<Person> { Key = "link", Label = "Open", Href = c => "/people/" + c.Key },
        ], maxVisible: 1);
        await part.Choose("child");
        Assert.Equal(0, ran);
        var links = new List<string>();
        part.LinkActivated += (_, href) => links.Add(href);
        await part.Choose("link");
        Assert.Equal(["/people/1"], links);
        Assert.Equal(["link"], part.Buttons.Keys);
        Assert.Equal(["Group"], part.Menu!.Items.Select(i => i.Label));
    }

    [Fact]
    public async Task A_row_action_that_asks_for_a_refresh_loads_the_list_again()
    {
        var action = new DataListRowAction<Person> { Key = "r", Label = "R", RefreshOnSuccess = true, OnExecute = _ => Task.CompletedTask };
        var (list, part) = await Row([action]);
        var version = list.Context.ResultVersion;
        await part.Choose("r");
        Assert.True(list.Context.ResultVersion > version);
    }

    // ---- selection actions

    private static async Task<(KlyDataList<Person> List, KlyDataListSelectionActions<Person> Bar)> Bar(IReadOnlyList<DataListAction<Person>> actions, Action<KlyDataListSelectionActions<Person>>? configure = null)
    {
        var bar = new KlyDataListSelectionActions<Person> { Actions = actions };
        configure?.Invoke(bar);
        var list = await TableHarness.Mount(s => s.Children.Add(bar));
        return (list, bar);
    }

    private static DataListAction<Person> Bulk(string key, Func<DataListActionContext<Person>, Task>? run = null) => new() { Key = key, Label = key.ToUpperInvariant(), OnExecute = run };

    [Fact]
    public async Task The_bar_turns_the_selection_on_and_shows_only_while_rows_are_ticked()
    {
        var (list, bar) = await Bar([Bulk("a")]);
        Assert.True(list.Context.SelectionEnabled);
        Assert.Null(bar.Content);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.NotNull(bar.Bar);
        Assert.Equal("1 selected", bar.CountLabel!.Text);
        list.Context.SetSelected(list.Context.Rows[1], true);
        Assert.Equal("2 selected", bar.CountLabel!.Text);
        bar.ClearButton!.Activate();
        Assert.Null(bar.Content);
        Assert.Equal(0, list.Context.SelectedCount);
    }

    [Fact]
    public async Task The_first_actions_are_buttons_and_the_others_are_in_the_more_menu()
    {
        var (list, bar) = await Bar([Bulk("a"), Bulk("b"), Bulk("c")], b => b.MaxVisible = 2);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.Equal(2, bar.ActionButtons.Count);
        Assert.Equal("A", bar.ActionButtons[0].TextView.Text);
        Assert.NotNull(bar.Overflow);
        Assert.Equal(["C"], bar.Overflow!.Items.Select(i => i.Label));
        bar.MaxVisible = 3;
        Assert.Null(bar.Overflow);
        Assert.Equal(3, bar.ActionButtons.Count);
    }

    [Fact]
    public async Task An_action_gets_the_ticked_rows_and_can_clear_the_selection()
    {
        var seen = new List<string>();
        var action = new DataListAction<Person> { Key = "x", Label = "X", ClearSelection = DataListSelectionClear.Success, OnExecute = c => { seen.AddRange(c.SelectedItems.Select(p => p.Name)); Assert.Equal(DataListActionScope.Selection, c.Scope); return Task.CompletedTask; } };
        var (list, bar) = await Bar([action]);
        list.Context.SetSelected(list.Context.Rows[0], true);
        list.Context.SetSelected(list.Context.Rows[1], true);
        await bar.Choose("x");
        Assert.Equal(["Alice Martin", "Bob Chen"], seen);
        Assert.Equal(0, list.Context.SelectedCount);
    }

    [Fact]
    public async Task A_failing_action_keeps_the_selection_and_reports_the_failure()
    {
        var action = new DataListAction<Person> { Key = "x", Label = "X", ClearSelection = DataListSelectionClear.Success, RefreshOnSuccess = true, OnExecute = _ => throw new InvalidOperationException("no") };
        var (list, bar) = await Bar([action]);
        var failures = new List<DataListActionFailure<Person>>();
        bar.Error += (_, e) => failures.Add(e);
        list.Context.SetSelected(list.Context.Rows[0], true);
        await bar.Choose("x");
        Assert.Equal("no", failures.Single().Exception.Message);
        Assert.Equal(1, list.Context.SelectedCount);
    }

    [Fact]
    public async Task An_action_that_clears_on_trigger_unticks_the_rows_as_it_starts()
    {
        var left = -1;
        DataListActionContext<Person>? context = null;
        DataListContext<Person>? list = null;
        var action = new DataListAction<Person> { Key = "x", Label = "X", ClearSelection = DataListSelectionClear.Trigger, OnExecute = c => { context = c; left = list!.SelectedCount; return Task.CompletedTask; } };
        var (kly, bar) = await Bar([action]);
        list = kly.Context;
        list.SetSelected(list.Rows[0], true);
        await bar.Choose("x");
        Assert.Equal(0, left);
        Assert.Single(context!.SelectedItems);
    }

    [Fact]
    public async Task An_action_that_is_hidden_disabled_or_running_does_not_start()
    {
        var gate = new TaskCompletionSource();
        var runs = 0;
        var actions = new[]
        {
            new DataListAction<Person> { Key = "hidden", Label = "H", Visible = c => c.TargetCount > 5, OnExecute = _ => { runs++; return Task.CompletedTask; } },
            new DataListAction<Person> { Key = "off", Label = "O", Disabled = c => c.TargetCount < 2, OnExecute = _ => { runs++; return Task.CompletedTask; } },
            new DataListAction<Person> { Key = "slow", Label = "S", OnExecute = async _ => { runs++; await gate.Task; } },
        };
        var (list, bar) = await Bar(actions);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.Equal(2, bar.ActionButtons.Count);
        await bar.Choose("hidden");
        await bar.Choose("off");
        Assert.Equal(0, runs);
        Assert.False(bar.ActionButtons[0].IsEnabled);
        var first = bar.Choose("slow");
        Assert.True(bar.ActionButtons[1].IsBusy);
        await bar.Choose("slow");
        Assert.Equal(1, runs);
        gate.SetResult();
        await first;
        Assert.False(bar.ActionButtons[1].IsBusy);
    }

    [Fact]
    public async Task An_action_can_ask_first()
    {
        var runs = 0;
        var action = new DataListAction<Person>
        {
            Key = "x",
            Label = "X",
            Color = KlyColor.Error,
            Confirm = new DataListActionConfirm<Person> { Title = "Remove?", Description = c => $"{c.TargetCount} will go.", ConfirmLabel = "Remove" },
            OnExecute = _ => { runs++; return Task.CompletedTask; },
        };
        var (list, bar) = await Bar([action]);
        TableConfirmRequest? asked = null;
        var answer = false;
        bar.Confirmer = r =>
        {
            asked = r;
            return Task.FromResult(answer);
        };
        list.Context.SetSelected(list.Context.Rows[0], true);
        await bar.Choose("x");
        Assert.Equal(0, runs);
        Assert.Equal(new TableConfirmRequest("Remove?", "1 will go.", "Remove", "Cancel", KlyColor.Error), asked);
        answer = true;
        await bar.Choose("x");
        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task The_bar_can_switch_to_every_result()
    {
        DataListActionContext<Person>? seen = null;
        var action = new DataListAction<Person> { Key = "x", Label = "X", OnExecute = c => { seen = c; return Task.CompletedTask; } };
        var (list, bar) = await Bar([action]);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.NotNull(bar.ScopeButton);
        Assert.Equal("Select all 5 results", bar.ScopeButton!.TextView.Text);
        bar.ScopeButton.Activate();
        Assert.Equal("All 5 results selected", bar.CountLabel!.Text);
        Assert.Equal("Only the 1 selected", bar.ScopeButton!.TextView.Text);
        await bar.Choose("x");
        Assert.Equal(DataListActionScope.All, seen!.Scope);
        Assert.Equal(5, seen.SelectedItems.Count);
        bar.ScopeButton!.Activate();
        Assert.Equal("1 selected", bar.CountLabel!.Text);
        bar.ShowScope = false;
        Assert.Null(bar.ScopeButton);
    }

    [Fact]
    public async Task The_bar_can_stick_to_the_bottom()
    {
        var (list, bar) = await Bar([Bulk("a")], b => b.Position = DataListSelectionBarPosition.Fixed);
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.Equal(LayoutOptions.End, bar.VerticalOptions);
        bar.Position = DataListSelectionBarPosition.Sticky;
        Assert.Equal(LayoutOptions.Fill, bar.VerticalOptions);
    }

    // ---- actions menu

    [Fact]
    public async Task The_actions_menu_lists_the_visible_actions_and_runs_the_one_chosen()
    {
        var ran = new List<string>();
        var menu = new KlyDataListActionsDropdown<Person>
        {
            Actions =
            [
                new DataListAction<Person> { Key = "new", Label = "New", Icon = "plus", OnExecute = _ => { ran.Add("new"); return Task.CompletedTask; } },
                new DataListAction<Person> { Key = "hide", Label = "Hide", Visible = _ => false },
                new DataListAction<Person> { Key = "off", Label = "Off", Disabled = _ => true, Color = KlyColor.Error },
            ],
        };
        var list = await TableHarness.Mount(s => s.Children.Add(menu));
        Assert.Equal(["New", "Off"], menu.Menu!.Items.Select(i => i.Label));
        Assert.Equal("plus", menu.Menu.Items[0].Icon);
        Assert.False(menu.Menu.Items[1].IsEnabled);
        Assert.Equal(KlyColor.Error, menu.Menu.Items[1].Color);
        Assert.Equal("Actions", menu.Trigger!.TextView.Text);
        await menu.Choose("new");
        Assert.Equal(["new"], ran);
        menu.Text = "Tools";
        Assert.Equal("Tools", menu.Trigger!.TextView.Text);
        menu.Actions = [];
        Assert.Null(menu.Content);
        _ = list;
    }

    // ---- column panel

    private static async Task<(KlyDataList<Person> List, KlyDataListColumnPanel<Person> Panel)> Columns(Action<KlyDataListColumnPanel<Person>>? configure = null)
    {
        var panel = new KlyDataListColumnPanel<Person>();
        configure?.Invoke(panel);
        var table = new KlyDataListTable<Person>
        {
            Columns =
            {
                new KlyDataListColumn<Person, string> { Property = p => p.Name, Required = true },
                new KlyDataListColumn<Person, string> { Property = p => p.Team },
                new KlyDataListColumn<Person, decimal> { Property = p => p.Salary },
            },
        };
        var list = await TableHarness.Mount(s =>
        {
            s.Children.Add(panel);
            s.Children.Add(table);
        });
        return (list, panel);
    }

    [Fact]
    public async Task The_column_panel_lists_the_columns_that_can_be_hidden_and_hides_one()
    {
        var (list, panel) = await Columns();
        Assert.Equal(2, panel.Boxes.Count);
        Assert.Equal(["Team", "Salary"], panel.Boxes.Select(b => b.Text));
        panel.Boxes[0].IsChecked = false;
        Assert.False(list.Context.ColumnStates.First(c => c.Key == "team").Visible);
        Assert.False(panel.Boxes[0].IsChecked);
        Assert.Equal("Team hidden", panel.Announcement);
        panel.Boxes[0].IsChecked = true;
        Assert.Equal("Team shown", panel.Announcement);
        Assert.Equal("Columns", panel.Trigger!.TextView.Text);
    }

    [Fact]
    public async Task The_last_visible_column_cannot_be_hidden_from_the_panel()
    {
        var (list, panel) = await Columns();
        list.Context.SetColumnVisible("team", false);
        Assert.False(panel.Boxes[1].IsEnabled && !panel.Boxes[1].IsChecked);
        list.Context.SetColumnVisible("salary", false);
        Assert.Equal(["name"], list.Context.TableColumns().Select(c => c.ResolvedKey));
    }

    [Fact]
    public async Task The_panel_moves_pins_shows_hides_and_resets_the_columns()
    {
        var (list, panel) = await Columns();
        panel.RowButtons["salary"].Up!.Activate();
        Assert.Equal(["name", "salary", "team"], list.Context.ColumnStates.Select(c => c.Key));
        Assert.Equal("Salary moved to position 2 of 3", panel.Announcement);
        panel.RowButtons["salary"].Pin!.Activate();
        Assert.Equal(DataListPin.Start, list.Context.ColumnStates.First(c => c.Key == "salary").Pinned);
        Assert.Equal("Salary pinned to the start", panel.Announcement);
        panel.RowButtons["salary"].Pin!.Activate();
        Assert.Equal(DataListPin.End, list.Context.ColumnStates.First(c => c.Key == "salary").Pinned);
        panel.RowButtons["salary"].Pin!.Activate();
        Assert.Equal(DataListPin.None, list.Context.ColumnStates.First(c => c.Key == "salary").Pinned);
        panel.HideAllButton!.Activate();
        Assert.Equal(["name"], list.Context.ColumnStates.Where(c => c.Visible).Select(c => c.Key));
        Assert.Equal("Optional columns hidden", panel.Announcement);
        panel.ShowAllButton!.Activate();
        Assert.Equal(3, list.Context.ColumnStates.Count(c => c.Visible));
        Assert.True(panel.ResetButton!.IsEnabled);
        panel.ResetButton.Activate();
        Assert.Equal(["name", "team", "salary"], list.Context.ColumnStates.Select(c => c.Key));
        Assert.False(panel.ResetButton!.IsEnabled);
        Assert.False(panel.RowButtons["team"].Down is null);
        Assert.False(panel.RowButtons["salary"].Down!.IsEnabled);
    }

    [Fact]
    public async Task The_panel_can_leave_out_the_reorder_and_pin_buttons_and_search_many_columns()
    {
        var (_, panel) = await Columns(p =>
        {
            p.AllowReorder = false;
            p.AllowPinning = false;
            p.SearchThreshold = 2;
        });
        Assert.Null(panel.RowButtons["team"].Up);
        Assert.Null(panel.RowButtons["team"].Pin);
        var search = ((VerticalStackLayout)panel.Popover.PanelContent!).Children.OfType<KlyInput>().Single();
        search.Text = "sal";
        Assert.Equal(["Salary"], panel.Boxes.Select(b => b.Text));
    }

    [Fact]
    public async Task The_column_panel_is_hidden_without_columns()
    {
        var panel = new KlyDataListColumnPanel<Person>();
        await TableHarness.Mount(s => s.Children.Add(panel));
        Assert.False(panel.IsVisible);
    }

    // ---- export

    private static async Task<(KlyDataList<Person> List, KlyDataListExport<Person> Export, List<DataListExportResult> Files)> Export(Action<KlyDataListExport<Person>>? configure = null, Action<KlyDataList<Person>>? list = null)
    {
        var export = new KlyDataListExport<Person> { CsvOptions = new DataListCsvOptions { Culture = CultureInfo.InvariantCulture, Utf8Bom = false, NewLine = DataListCsvNewLine.Lf } };
        configure?.Invoke(export);
        var files = new List<DataListExportResult>();
        export.Exported += (_, file) => files.Add(file);
        var table = new KlyDataListTable<Person> { Columns = { new KlyDataListColumn<Person, string> { Property = p => p.Name }, new KlyDataListColumn<Person, string> { Property = p => p.Team } } };
        var mounted = await TableHarness.Mount(s =>
        {
            s.Children.Add(export);
            s.Children.Add(table);
        }, configure: list);
        return (mounted, export, files);
    }

    [Fact]
    public async Task The_export_of_the_page_writes_a_csv_of_the_visible_columns()
    {
        var (list, export, files) = await Export(l => l.FileName = "staff");
        await export.ExportAsync(DataListExportScope.Page);
        var file = Assert.Single(files);
        Assert.Equal("staff.csv", file.FileName);
        Assert.Equal("Name,Team\nAlice Martin,Backend\nBob Chen,Frontend\n", file.Content);
        Assert.Equal(DataListExportScope.Page, file.Scope);
        Assert.Equal(2, file.RowCount);
        list.Label = "People";
    }

    [Fact]
    public async Task The_file_is_named_after_the_list_when_it_has_no_name()
    {
        var (_, export, files) = await Export(null, l => l.Label = "Staff list");
        await export.ExportAsync(DataListExportScope.Page);
        Assert.StartsWith("Staff list", files[0].FileName, StringComparison.Ordinal);
        Assert.EndsWith(".csv", files[0].FileName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_menu_offers_the_scopes_that_apply_and_a_single_scope_exports_at_once()
    {
        var (list, export, files) = await Export(null, l => l.Selectable = true);
        Assert.NotNull(export.Menu);
        Assert.Equal(["This page", "All 5 results"], export.Menu!.Items.Select(i => i.Label));
        list.Context.SetSelected(list.Context.Rows[1], true);
        Assert.Equal(["This page", "Selected rows (1)", "All 5 results"], export.Menu!.Items.Select(i => i.Label));
        export.Scopes = [DataListExportScope.Selection];
        Assert.Null(export.Menu);
        export.Button!.Activate();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal("Name,Team\nBob Chen,Frontend\n", files.Single().Content);
    }

    [Fact]
    public async Task Every_result_of_a_list_in_memory_is_exported_with_the_search_and_filters_applied()
    {
        var (list, export, files) = await Export();
        list.Context.SetState(s => s with { Search = "backend", Page = 1 });
        await list.Context.WhenLoaded();
        await export.ExportAsync(DataListExportScope.All);
        Assert.Equal("Name,Team\nAlice Martin,Backend\nClaire Petit,Backend\n", files.Single().Content);
    }

    [Fact]
    public async Task Every_result_of_a_provider_list_needs_the_function_of_the_app_which_gets_the_request()
    {
        DataListRequest? asked = null;
        var export = new KlyDataListExport<Person>
        {
            CsvOptions = new DataListCsvOptions { Culture = CultureInfo.InvariantCulture, Utf8Bom = false, NewLine = DataListCsvNewLine.Lf },
            LoadAll = (request, _) =>
            {
                asked = request;
                return Task.FromResult<IReadOnlyList<Person>>(TableHarness.People()[..3]);
            },
        };
        var files = new List<DataListExportResult>();
        export.Exported += (_, f) => files.Add(f);
        var list = new KlyDataList<Person>
        {
            Schema = TableHarness.PersonSchema,
            Provider = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People()),
            Content = new VerticalStackLayout { Children = { export } },
            State = TableHarness.PersonSchema.DefaultState with { PageSize = 2 },
        };
        await list.LoadAsync();
        Assert.False(list.Context.HoldsAllRows);
        Assert.Equal(["This page", "All 5 results"], export.Menu!.Items.Select(i => i.Label));
        await export.ExportAsync(DataListExportScope.All);
        Assert.IsType<DataListPaging.None>(asked!.Paging);
        Assert.Equal(3, files.Single().RowCount);
        export.LoadAll = null;
        Assert.Null(export.Menu);
    }

    [Fact]
    public async Task A_bigger_export_than_the_limit_is_refused_with_a_message_under_the_button()
    {
        var (_, export, files) = await Export(e => e.MaxRows = 3);
        var errors = new List<Exception>();
        export.Error += (_, e) => errors.Add(e);
        await export.ExportAsync(DataListExportScope.All);
        Assert.Empty(files);
        var tooLarge = Assert.IsType<DataListExportTooLargeException>(Assert.Single(errors));
        Assert.Equal((5, 3), (tooLarge.Count, tooLarge.Limit));
        Assert.Equal("Too many rows to export: 5 results, the limit is 3. Narrow the search or the filters.", export.RefusedText);
        await export.ExportAsync(DataListExportScope.Page);
        Assert.Single(files);
        Assert.Null(export.RefusedText);
    }

    [Fact]
    public async Task Without_a_handler_the_file_goes_to_the_share_sheet_and_a_platform_that_has_none_reports_it()
    {
        var export = new KlyDataListExport<Person>();
        var errors = new List<Exception>();
        export.Error += (_, e) => errors.Add(e);
        await TableHarness.Mount(s => s.Children.Add(export));
        await export.ExportAsync(DataListExportScope.Page);
        Assert.Single(errors);
    }

    [Fact]
    public async Task The_file_is_written_as_bytes_in_the_directory_given()
    {
        var directory = Path.Combine(Path.GetTempPath(), "klyroo-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            var result = new DataListExportResult("people.csv", "a,b\n1,é\n", 1, DataListExportScope.Page);
            var path = await TableFileExport.WriteAsync(result, directory);
            Assert.Equal(Path.Combine(directory, "people.csv"), path);
            Assert.Equal(result.ToBytes(), await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task The_export_button_is_disabled_when_there_is_nothing_to_export()
    {
        var (_, export, _) = await Export();
        Assert.True(export.Menu is not null || export.Button!.IsEnabled);
        var empty = new KlyDataListExport<Person>();
        await TableHarness.Mount(s => s.Children.Add(empty), items: []);
        Assert.False(empty.Button!.IsEnabled);
    }
}
