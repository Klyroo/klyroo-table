namespace Klyroo.Table.Maui.Tests;

public sealed record Person(int Id, string Name, string Team, decimal Salary, DateOnly Hired, bool Active = true, string? Note = null);

/// <summary>A small schema, data set and list shared by the tests of the data list.</summary>
public static class TableHarness
{
    public static readonly DataListSchema<Person> PersonSchema = DataListSchema.For<Person>(s => s
        .RowKey(p => p.Id)
        .RowLabel(p => p.Name)
        .Search(p => p.Name, p => p.Team)
        .Field(p => p.Name, f => f.Key("name").Sortable().Filter(DataListFilter.Text()))
        .Field(p => p.Team, f => f.Key("team").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(p => p.Salary, f => f.Key("salary").Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(p => p.Hired, f => f.Key("hired").Sortable().Filter(DataListFilter.Date()))
        .Field(p => p.Active, f => f.Key("active").Filter(DataListFilter.Boolean()))
        .Field(p => p.Note, f => f.Key("note"))
        .DefaultSort(p => p.Name)
        .DefaultPageSize(2)
        .MaxPageSize(50));

    public static Person[] People() =>
    [
        new(1, "Alice Martin", "Backend", 120_000m, new(2024, 3, 15), true, "Lead"),
        new(2, "Bob Chen", "Frontend", 95_000.5m, new(2025, 1, 10), false),
        new(3, "Claire Petit", "Backend", 110_000m, new(2023, 6, 1), true, "=HYPERLINK(\"x\")"),
        new(4, "David Kim", "Infra", 88_000m, new(2025, 4, 20), true, "a;b,\"c\""),
        new(5, "Emma Stone", "Frontend", 99_000m, new(2022, 2, 2), false),
    ];

    /// <summary>A list of <see cref="People"/> whose content is <paramref name="parts"/>, loaded.</summary>
    public static async Task<KlyDataList<Person>> Mount(Action<VerticalStackLayout>? parts = null, Action<KlyDataList<Person>>? configure = null, IEnumerable<Person>? items = null, int pageSize = 2)
    {
        var list = new KlyDataList<Person> { Schema = PersonSchema, Items = items ?? People(), State = PersonSchema.DefaultState with { PageSize = pageSize } };
        configure?.Invoke(list);
        var stack = new VerticalStackLayout();
        parts?.Invoke(stack);
        list.Content = stack;
        await list.LoadAsync();
        return list;
    }

    public static T Part<T>(this KlyDataList<Person> list) where T : View => (T)((VerticalStackLayout)list.Content!).Children.OfType<T>().First();

    public static IEnumerable<T> Descendants<T>(this Element root) where T : Element
    {
        if (root is T self) yield return self;
        if (root is IVisualTreeElement tree)
        {
            foreach (var child in tree.GetVisualChildren())
            {
                if (child is Element element)
                {
                    foreach (var found in element.Descendants<T>()) yield return found;
                }
            }
        }
    }
}
