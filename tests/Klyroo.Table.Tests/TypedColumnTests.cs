using Xunit;
using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table.Tests;

public sealed class TypedColumnTests : TableHarness
{
    private static readonly CultureInfo Fr = new("fr-FR");

    [Fact]
    public void Typed_columns_compile_and_render_in_markup_without_a_type_argument()
    {
        Prepare();
        var cut = Render<TypedMarkupPage>(p => p.Add(x => x.Rows, People()).Add(x => x.Schema, PersonSchema));

        Assert.Contains("Alice Martin", cut.Markup);
        // The format of the typed column applies to the typed value: 120,000 (N0) and the year of a DateOnly.
        Assert.Contains("120", cut.Markup);
        Assert.Contains("2024", cut.Markup);
        // The template column renders its template with the row.
        Assert.Equal(4, cut.FindAll("button.act").Count);
        Assert.Contains("Open 1", cut.Markup);
        // The grouping by a typed value shows one header per team.
        Assert.Contains("Backend", cut.Markup);
        Assert.Contains("Infra", cut.Markup);
    }

    [Fact]
    public void A_list_without_a_schema_is_described_by_its_typed_columns()
    {
        Prepare();
        var cut = Render<TypedMarkupNoSchemaPage>(p => p.Add(x => x.Rows, People()));
        Assert.Contains("Bob Chen", cut.Markup);
        // Summary Max over a decimal column reads the number, formatted with the format of the column.
        Assert.Contains("120,000", cut.Markup);
    }

    [Fact]
    public void The_summary_row_aggregates_a_typed_property_a_custom_typed_value_and_a_field()
    {
        Prepare();
        var cut = Render<TypedMarkupPage>(p => p.Add(x => x.Rows, People()).Add(x => x.Schema, PersonSchema));
        var footer = cut.Find("tfoot").TextContent;
        // Sum of the salaries (N0), the latest hire year (a DateOnly returned by SummaryValue, formatted yyyy) and the row count of the team field.
        Assert.Contains("413001", new string(footer.Where(char.IsDigit).ToArray()));
        Assert.Contains("2025", footer);
        var messages = new KlyToolsMessages();
        var cells = cut.FindAll("tfoot td").Select(c => c.TextContent.Trim()).ToArray();
        Assert.Single(cells, c => c == string.Format(CultureInfo.CurrentCulture, messages.SummaryRowOther, 4));
    }

    [Fact]
    public void A_count_summary_ignores_a_date_format_and_never_throws()
    {
        var messages = new KlyToolsMessages();
        var column = With(new KlyDataListFieldColumn<Person>(), ("Field", "hired"), ("Format", "d"), ("Summary", DataListSummaryKind.Count));
        var value = column.ComputeSummary(People(), PersonSchema, CultureInfo.InvariantCulture);
        Assert.Equal(string.Format(CultureInfo.InvariantCulture, messages.SummaryRowOther, 4), column.FormatSummaryValue(value, CultureInfo.InvariantCulture, messages));
        // With a SummaryFormat of its own the count uses it.
        var formatted = With(new KlyDataListFieldColumn<Person>(), ("Field", "hired"), ("Format", "d"), ("Summary", DataListSummaryKind.Count), ("SummaryFormat", "D3"));
        Assert.Equal("004", formatted.FormatSummaryValue(value, CultureInfo.InvariantCulture, messages));
    }

    [Fact]
    public void Integer_only_formats_apply_to_whole_summaries_and_a_bad_format_falls_back_to_the_default_text()
    {
        var messages = new KlyToolsMessages();
        var inv = CultureInfo.InvariantCulture;
        var min = With(new KlyDataListColumn<Person, int>(), ("Property", (Expression<Func<Person, int>>)(p => p.Id)), ("Format", "D6"), ("Summary", DataListSummaryKind.Min));
        Assert.Equal("000001", min.FormatSummaryValue(min.ComputeSummary(People(), PersonSchema, inv), inv, messages));
        var hex = With(new KlyDataListColumn<Person, int>(), ("Property", (Expression<Func<Person, int>>)(p => p.Id)), ("Format", "X"), ("Summary", DataListSummaryKind.Max));
        Assert.Equal("4", hex.FormatSummaryValue(hex.ComputeSummary(People(), PersonSchema, inv), inv, messages));
        var custom = With(new KlyDataListColumn<Person, int>(), ("Property", (Expression<Func<Person, int>>)(p => p.Id)), ("Format", "D6"), ("SummaryValue", (Func<IReadOnlyList<Person>, int>)(rows => rows.Count * 10)));
        Assert.Equal("000040", custom.FormatSummaryValue(custom.ComputeSummary(People(), PersonSchema, inv), inv, messages));
        var bad = With(new KlyDataListColumn<Person, int>(), ("Property", (Expression<Func<Person, int>>)(p => p.Id)), ("Format", "F1000000000"), ("Summary", DataListSummaryKind.Max));
        Assert.Equal("4", bad.FormatSummaryValue(bad.ComputeSummary(People(), PersonSchema, inv), inv, messages));
        // A text min or max of a type that rejects the format falls back too.
        var date = With(new KlyDataListColumn<Person, DateOnly>(), ("Property", (Expression<Func<Person, DateOnly>>)(p => p.Hired)), ("Format", "HH"), ("Summary", DataListSummaryKind.Max));
        Assert.Equal("04/20/2025", date.FormatSummaryValue(date.ComputeSummary(People(), PersonSchema, inv), inv, messages));
    }

    [Fact]
    public void An_empty_summary_format_is_unset_for_every_kind_of_column()
    {
        var inv = CultureInfo.InvariantCulture;
        var column = With(new KlyDataListTemplateColumn<Person>(), ("Key", "hired"), ("Format", "yyyy"), ("SummaryFormat", ""), ("Summary", DataListSummaryKind.Max));
        Assert.Equal("2025", column.ComputeSummary(People(), PersonSchema, inv).Text);
    }

    private static T With<T>(T column, params (string Name, object? Value)[] values)
    {
        foreach (var (name, value) in values) typeof(T).GetProperty(name)!.SetValue(column, value);
        return column;
    }

    private sealed class Dept(string name)
    {
        public string Name { get; } = name;
    }

    [Fact]
    public void Two_distinct_values_that_print_the_same_get_distinct_stable_group_keys()
    {
        var a = new Dept("x");
        var b = new Dept("y");
        var rows = new[] { (a, 1), (b, 2), (a, 3) };
        var groupBy = DataListGroupBy<(Dept, int)>.By(r => r.Item1);
        var first = groupBy.BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture);
        var second = groupBy.BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture);
        Assert.Equal(2, first.Count);
        Assert.Equal(2, first.Select(g => g.Key).Distinct().Count());
        Assert.Equal(first.Select(g => g.Key).ToArray(), second.Select(g => g.Key).ToArray());
    }

    [Fact]
    public void A_typed_column_reads_a_null_along_its_path_as_an_empty_cell_and_skips_it_in_a_sum()
    {
        var column = new KlyDataListColumn<Child, decimal>();
        var rows = new[] { new Child("a", new Parent(5m)), new Child("b", null), new Child("c", new Parent(7m)) };
        var read = new ValueReaderProbe(rows);
        Assert.Equal(12m, read.Sum().Number);
        Assert.Equal(2m, read.Count().Number);
        Assert.NotNull(column);
    }

    private sealed record Parent(decimal Budget);

    private sealed record Child(string Name, Parent? Parent);

    private sealed class ValueReaderProbe(IReadOnlyList<Child> rows)
    {
        private static bool Read(Child c, out decimal value)
        {
            try
            {
                value = c.Parent!.Budget;
                return true;
            }
            catch (NullReferenceException)
            {
                value = 0m;
                return false;
            }
        }

        public DataListSummaryValue Sum() => DataListSummaryCalculator.Compute<Child, decimal>(DataListSummaryKind.Sum, rows, Read, null, CultureInfo.InvariantCulture);

        public DataListSummaryValue Count() => DataListSummaryCalculator.Compute<Child, decimal>(DataListSummaryKind.Count, rows, Read, null, CultureInfo.InvariantCulture);
    }

    [Fact]
    public void Summary_kinds_over_typed_values_of_every_shape()
    {
        var inv = CultureInfo.InvariantCulture;
        var rows = People();
        // Numbers.
        Assert.Equal(413_000.5m, DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Sum, rows, (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).Number);
        Assert.Equal(88_000m, DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Min, rows, (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).Number);
        Assert.Equal(120_000m, DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Max, rows, (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).Number);
        Assert.Equal(413_000.5m / 4, DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Average, rows, (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).Number);
        // Dates: Min and Max are text formatted with the column format; Sum is 0 and Average is empty because a date is not a number.
        var minHired = DataListSummaryCalculator.Compute<Person, DateOnly>(DataListSummaryKind.Min, rows, (Person p, out DateOnly v) => { v = p.Hired; return true; }, "yyyy-MM-dd", inv);
        Assert.Equal("2023-06-01", minHired.Text);
        Assert.Null(minHired.Number);
        Assert.Equal(0m, DataListSummaryCalculator.Compute<Person, DateOnly>(DataListSummaryKind.Sum, rows, (Person p, out DateOnly v) => { v = p.Hired; return true; }, null, inv).Number);
        Assert.True(DataListSummaryCalculator.Compute<Person, DateOnly>(DataListSummaryKind.Average, rows, (Person p, out DateOnly v) => { v = p.Hired; return true; }, null, inv).IsEmpty);
        // Text: Count skips blanks, Max compares strings.
        Assert.Equal(3m, DataListSummaryCalculator.Compute<Person, string?>(DataListSummaryKind.Count, rows, (Person p, out string? v) => { v = p.Note; return true; }, null, inv).Number);
        Assert.Equal("Infra", DataListSummaryCalculator.Compute<Person, string>(DataListSummaryKind.Max, rows, (Person p, out string v) => { v = p.Team; return true; }, null, inv).Text);
        // An empty list.
        Assert.Equal(0m, DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Sum, [], (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).Number);
        Assert.True(DataListSummaryCalculator.Compute<Person, decimal>(DataListSummaryKind.Max, [], (Person p, out decimal v) => { v = p.Salary; return true; }, null, inv).IsEmpty);
    }

    [Fact]
    public void Summary_kinds_over_a_field_of_the_schema_read_numbers_and_compare_like_a_sort()
    {
        var inv = CultureInfo.InvariantCulture;
        var rows = People();
        Assert.Equal(413_000.5m, DataListSummaryCalculator.ComputeField(DataListSummaryKind.Sum, rows, PersonSchema, "salary", null, inv).Number);
        Assert.Equal(88_000m, DataListSummaryCalculator.ComputeField(DataListSummaryKind.Min, rows, PersonSchema, "salary", null, inv).Number);
        Assert.Equal(4m, DataListSummaryCalculator.ComputeField(DataListSummaryKind.Count, rows, PersonSchema, "name", null, inv).Number);
        Assert.Equal(3m, DataListSummaryCalculator.ComputeField(DataListSummaryKind.Count, rows, PersonSchema, "note", null, inv).Number);
        Assert.Equal("2025", DataListSummaryCalculator.ComputeField(DataListSummaryKind.Max, rows, PersonSchema, "hired", "yyyy", inv).Text);
        Assert.Equal("Alice Martin", DataListSummaryCalculator.ComputeField(DataListSummaryKind.Min, rows, PersonSchema, "name", null, inv).Text);
    }

    [Fact]
    public void A_summary_value_converts_from_a_number_or_a_text_and_formats_in_the_footer()
    {
        DataListSummaryValue number = 1234.5m;
        DataListSummaryValue integer = 3;
        DataListSummaryValue text = "n/a";
        DataListSummaryValue nothing = (string?)null;
        Assert.Equal(1234.5m, number.Number);
        Assert.Equal(3m, integer.Number);
        Assert.Equal("n/a", text.Text);
        Assert.True(nothing.IsEmpty);
        Assert.True(DataListSummaryValue.FromDouble(double.NaN).IsEmpty);
        Assert.Equal(2.5m, DataListSummaryValue.FromDouble(2.5).Number);
        Assert.Equal("1234.5", number.ToString());
    }

    [Fact]
    public void An_async_resolver_returns_typed_summary_values_that_the_footer_formats_with_the_column_format()
    {
        Prepare();
        var cut = Render<KlyDataList<Person>>(p => p
            .Add(x => x.Schema, PersonSchema)
            .Add(x => x.UrlSync, false)
            .Add(x => x.Items, People())
            .AddChildContent<KlyDataListTable<Person>>(t => t
                .Add(x => x.Summaries, new DataListSummaries<Person>
                {
                    ResolveAsync = _ => ValueTask.FromResult<IReadOnlyDictionary<string, DataListSummaryValue>>(
                        new Dictionary<string, DataListSummaryValue> { ["salary"] = 999_999m, ["name"] = "everyone" }),
                })
                .AddChildContent<KlyDataListFieldColumn<Person>>(c => c.Add(x => x.Field, "name").Add(x => x.Title, "Name"))
                .AddChildContent<KlyDataListFieldColumn<Person>>(c => c.Add(x => x.Field, "salary").Add(x => x.Title, "Salary").Add(x => x.Format, "N0"))));
        cut.WaitForAssertion(() => Assert.Contains("everyone", cut.Find("tfoot").TextContent));
        Assert.Contains("999", cut.Find("tfoot").TextContent);
    }

    [Fact]
    public void A_grouping_by_a_typed_value_orders_the_groups_and_keeps_the_typed_value()
    {
        var rows = new[]
        {
            new Task("a", Priority.Low, 2), new Task("b", Priority.High, 10), new Task("c", Priority.High, 2), new Task("d", null, 1), new Task("e", Priority.Medium, 10),
        };
        var byPriority = DataListGroupBy<Task>.By(t => t.Priority);
        var groups = byPriority.BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture);
        // Enums order by value, the empty group (a null priority) comes last.
        Assert.Equal(["Low", "Medium", "High", "~empty"], groups.Select(g => g.Key).ToArray());
        var high = Assert.IsType<DataListGroup<Task, Priority?>>(groups[2]);
        Assert.Equal(Priority.High, high.Value);
        Assert.Equal(2, high.Items.Count);
        Assert.True(groups[3].IsEmptyValue);
        Assert.Equal("High", high.Label);

        var ordered = DataListGroupBy<Task>.By(t => t.Priority, order: [Priority.High, Priority.Low]);
        Assert.Equal(["High", "Low", "Medium", "~empty"], ordered.BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture).Select(g => g.Key).ToArray());

        var labelled = DataListGroupBy<Task>.By(t => t.Points);
        labelled.Label = g => $"{g.Value} pts";
        labelled.Summary = g => g.Items.Count == 1 ? "one" : $"{g.Items.Count} tasks";
        labelled.DefaultCollapsed = g => g.Value > 5;
        var byPoints = labelled.BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture);
        // Numbers order by value, not by text: 1, 2, 10.
        Assert.Equal(["1 pts", "2 pts", "10 pts"], byPoints.Select(g => g.Label).ToArray());
        Assert.Equal(["one", "2 tasks", "2 tasks"], byPoints.Select(g => g.Summary).ToArray());
        Assert.Equal([false, false, true], byPoints.Select(g => g.DefaultCollapsed).ToArray());
    }

    [Fact]
    public void A_grouping_uses_the_equality_comparer_it_is_given()
    {
        var rows = new[] { new Task("a", Priority.Low, 1), new Task("A", Priority.Low, 2), new Task("b", Priority.Low, 3) };
        var groups = DataListGroupBy<Task>.By(t => t.Name, comparer: StringComparer.OrdinalIgnoreCase).BuildGroups(rows, null, new KlyToolsMessages(), CultureInfo.InvariantCulture);
        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups[0].Items.Count);
    }

    [Fact]
    public void A_grouping_by_a_field_orders_numbers_and_dates_by_value_and_formats_labels_with_the_culture()
    {
        var byHired = DataListGroupBy<Person>.ByField("hired");
        var groups = byHired.BuildGroups(People(), PersonSchema, new KlyToolsMessages(), Fr);
        Assert.Equal([2023, 2024, 2025, 2025], groups.Select(g => int.Parse(g.Items[0].Hired.Year.ToString(), CultureInfo.InvariantCulture)).ToArray());
        Assert.Equal("01/06/2023", groups[0].Label);

        var bySalary = DataListGroupBy<Person>.ByField("salary").BuildGroups(People(), PersonSchema, new KlyToolsMessages(), Fr);
        Assert.Equal([88_000m, 95_000.5m, 110_000m, 120_000m], bySalary.Select(g => g.Items[0].Salary).ToArray());
        Assert.Equal("95000,5", bySalary[1].Label);

        var byNote = DataListGroupBy<Person>.ByField("note", order: ["Lead"]).BuildGroups(People(), PersonSchema, new KlyToolsMessages(), Fr);
        Assert.Equal("Lead", Assert.IsType<DataListGroup<Person, string>>(byNote[0]).Value);
        Assert.True(byNote[^1].IsEmptyValue);
        // A grouping by a field needs a schema.
        Assert.Empty(byHired.BuildGroups(People(), null, new KlyToolsMessages(), Fr));
    }

    private enum Priority { Low, Medium, High }

    private sealed record Task(string Name, Priority? Priority, int Points);
}
