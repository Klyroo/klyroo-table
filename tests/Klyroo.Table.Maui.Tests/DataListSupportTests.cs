namespace Klyroo.Table.Maui.Tests;

/// <summary>The helper controls, the way parts find their list and let go of it.</summary>
public class DataListSupportTests
{
    [Fact]
    public void A_tap_area_ignores_the_tap_while_it_is_disabled()
    {
        var taps = 0;
        var area = new TableTap(new Label()) { AccessibleName = "Open", AccessibleHint = "Opens it" };
        area.Tapped += (_, _) => taps++;
        area.Activate();
        area.IsEnabled = false;
        area.Activate();
        Assert.Equal(1, taps);
        Assert.Equal(0.6, area.Opacity);
        Assert.Equal("Open", SemanticProperties.GetDescription(area));
        Assert.Equal("Opens it", SemanticProperties.GetHint(area));
    }

    [Fact]
    public void A_button_ignores_the_tap_while_it_is_busy_or_disabled_and_names_itself_after_its_text()
    {
        var clicks = 0;
        var button = new TableButton("Save", "check");
        button.Clicked += (_, _) => clicks++;
        button.Activate();
        button.IsBusy = true;
        button.Activate();
        Assert.Equal(KlyIconNames.Loading, button.IconView.Name);
        button.IsBusy = false;
        button.IsEnabled = false;
        button.Activate();
        Assert.Equal(1, clicks);
        Assert.Equal("Save", SemanticProperties.GetDescription(button));
        Assert.Equal("check", button.IconView.Name);
        Assert.True(button.MinimumHeightRequest >= 36);
        button.Text = null;
        Assert.False(button.TextView.IsVisible);
    }

    [Theory]
    [InlineData(ButtonVariant.Solid, 1)]
    [InlineData(ButtonVariant.Outline, 1)]
    [InlineData(ButtonVariant.Soft, 0)]
    [InlineData(ButtonVariant.Ghost, 0)]
    public void A_button_variant_draws_the_border_of_the_web_variant(ButtonVariant variant, double thickness)
    {
        Assert.Equal(thickness, new TableButton("x", null, variant).StrokeThickness);
    }

    [Fact]
    public void The_theme_tables_match_the_ones_of_the_button()
    {
        var set = KlyTheme.Resolve(KlyColor.Primary);
        Assert.Equal(set.Fill, TableSurface.For(ButtonVariant.Solid, set).Background);
        Assert.Equal(KlyThemeColor.Transparent, TableSurface.For(ButtonVariant.Outline, set).Background);
        Assert.Equal(set.Fill.WithAlpha(0.10f), TableSurface.For(ButtonVariant.Soft, set).Background);
        Assert.Equal(KlyTheme.Radius, TableSupport.RadiusOf(1));
    }

    [Fact]
    public async Task A_part_added_later_to_the_tree_finds_the_list()
    {
        var list = await TableHarness.Mount();
        var count = new KlyDataListResultCount<Person>();
        var holder = new VerticalStackLayout { Children = { new VerticalStackLayout { Children = { count } } } };
        Assert.Null(count.Context);
        ((VerticalStackLayout)list.Content!).Children.Add(holder);
        Assert.Same(list.Context, count.Context);
        Assert.Equal("1–2 of 5", count.Text);
    }

    [Fact]
    public async Task A_part_that_leaves_the_tree_lets_go_of_the_list()
    {
        var count = new KlyDataListResultCount<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(count));
        Assert.NotNull(count.Context);
        ((VerticalStackLayout)list.Content!).Children.Remove(count);
        Assert.Null(count.Context);
        Assert.Equal("", count.Text);
    }

    [Fact]
    public async Task A_part_with_its_own_list_keeps_it_when_it_is_moved_out_of_the_tree()
    {
        var list = await TableHarness.Mount();
        var count = new KlyDataListResultCount<Person> { List = list };
        Assert.Same(list.Context, count.Context);
        count.List = null;
        Assert.Null(count.Context);
    }

    [Fact]
    public async Task Two_lists_on_a_page_do_not_share_their_parts()
    {
        var first = new KlyDataListResultCount<Person>();
        var second = new KlyDataListResultCount<Person>();
        var a = await TableHarness.Mount(s => s.Children.Add(first), pageSize: 2);
        var b = await TableHarness.Mount(s => s.Children.Add(second), pageSize: 4);
        Assert.Same(a.Context, first.Context);
        Assert.Same(b.Context, second.Context);
        Assert.Equal("1–2 of 5", first.Text);
        Assert.Equal("1–4 of 5", second.Text);
    }

    [Fact]
    public async Task Rows_drawn_again_let_go_of_the_headers_and_the_actions_they_built()
    {
        var table = new KlyDataListTable<Person>
        {
            GroupBy = DataListGroupBy<Person>.By(p => p.Team),
            RowActions = [new DataListRowAction<Person> { Key = "a", Label = "A" }],
        };
        var list = await TableHarness.Mount(s => s.Children.Add(table), configure: l => l.Selectable = true, pageSize: 10);
        var headers = table.RowStack!.Descendants<KlyDataListGroupHeader<Person>>().ToList();
        Assert.All(headers, h => Assert.NotNull(h.Context));
        list.Context.ToggleGroup(list.Context.Groups()[0]);
        Assert.All(headers, h => Assert.Null(h.Context));
        var current = table.RowStack!.Descendants<KlyDataListGroupHeader<Person>>().ToList();
        Assert.All(current, h => Assert.NotNull(h.Context));
        list.Context.SetSelected(list.Context.Rows[0], true);
        Assert.NotNull(current[0].Checkbox);
    }

    [Fact]
    public async Task The_add_button_and_the_tags_let_go_of_the_editors_they_discard()
    {
        var add = new KlyDataListAddFilter<Person>();
        var list = await TableHarness.Mount(s => s.Children.Add(add));
        add.Pick("salary");
        var first = add.Editor!;
        add.Pick("name");
        Assert.Null(first.Context);
        Assert.NotNull(add.Editor!.Context);
        _ = list;
    }

    [Fact]
    public async Task A_list_that_is_stopped_cancels_its_load()
    {
        var gate = new TaskCompletionSource();
        var inner = TableHarness.PersonSchema.InMemoryProvider(TableHarness.People());
        var list = new KlyDataList<Person> { Schema = TableHarness.PersonSchema, Provider = async (r, t) => { await gate.Task; t.ThrowIfCancellationRequested(); return await inner(r, t); } };
        var loading = list.LoadAsync();
        list.Context.Stop();
        gate.SetResult();
        await loading;
        Assert.True(list.Context.IsInitial);
    }

    [Fact]
    public void A_group_without_a_schema_or_rows_has_no_groups()
    {
        var grouping = DataListGroupBy<Person>.ByField("team");
        Assert.Empty(grouping.BuildGroups([], TableHarness.PersonSchema, KlyToolsLocales.En, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Empty(grouping.BuildGroups(TableHarness.People(), null, KlyToolsLocales.En, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Throws<ArgumentException>(() => DataListGroupBy<Person>.ByField(" "));
        Assert.Throws<ArgumentNullException>(() => DataListGroupBy<Person>.By<string>(null!));
    }

    [Fact]
    public void The_key_of_a_column_is_its_member_path_or_what_it_says()
    {
        Assert.Equal("name", new KlyDataListColumn<Person, string> { Property = p => p.Name }.EffectiveKey);
        Assert.Equal("custom", new KlyDataListColumn<Person, string> { Property = p => p.Name, Key = "custom" }.EffectiveKey);
        Assert.Equal("Custom key", new KlyDataListColumn<Person, string> { Property = p => p.Name, Key = "customKey" }.EffectiveTitle);
        Assert.Equal("team", new KlyDataListFieldColumn<Person> { Field = "team" }.EffectiveKey);
        Assert.Null(new KlyDataListTemplateColumn<Person> { Template = _ => new Label() }.EffectiveKey);
        Assert.Equal("First name", TableValues.Humanize("firstName"));
        Assert.Equal("Name", TableValues.Humanize("department.name"));
        Assert.Equal("Created at", TableValues.Humanize("created_at"));
    }

    [Fact]
    public void A_column_reads_a_null_along_its_path_as_an_empty_cell()
    {
        var column = new KlyDataListColumn<Dept, string?> { Property = d => d.Lead!.Name };
        Assert.Null(column.ReadText(new Dept(null), null, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal("Ann", column.ReadText(new Dept(new Lead("Ann")), null, System.Globalization.CultureInfo.InvariantCulture));
        Assert.True(column.CanExport(null));
        Assert.Equal("lead.name", column.EffectiveKey);
    }

    private sealed record Lead(string Name);

    private sealed record Dept(Lead? Lead);

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(decimal?), true)]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(DateOnly), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(object), false)]
    public void Only_the_types_that_write_their_own_text_skip_the_formula_guard(Type type, bool structured)
    {
        Assert.Equal(structured, TableValues.IsStructured(type));
    }

    [Fact]
    public void A_value_declared_object_is_judged_by_its_runtime_type()
    {
        Assert.True(TableValues.IsStructured(5, typeof(object)));
        Assert.False(TableValues.IsStructured("=x", typeof(object)));
        Assert.False(TableValues.IsStructured(null, typeof(object)));
        Assert.Null(TableValues.FormatValue(null, null, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal("1.50", TableValues.FormatValue(1.5m, "F2", System.Globalization.CultureInfo.InvariantCulture));
    }
}
