using Bunit;
using Microsoft.AspNetCore.Components.Rendering;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Klyroo.Table.Tests;

public sealed record Person(int Id, string Name, string Team, decimal Salary, DateOnly Hired, string? Note = null);

/// <summary>Shared setup of the tests that render a data list with a table: services, the render mode and a small schema and data set.</summary>
public abstract class TableHarness : BunitContext
{
    protected static readonly DataListSchema<Person> PersonSchema = DataListSchema.For<Person>(s => s
        .RowKey(p => p.Id)
        .RowLabel(p => p.Name)
        .Search(p => p.Name, p => p.Team)
        .Field(p => p.Name, f => f.Key("name").Sortable().Filter(DataListFilter.Text()))
        .Field(p => p.Team, f => f.Key("team").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(p => p.Salary, f => f.Key("salary").Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(p => p.Hired, f => f.Key("hired").Sortable())
        .Field(p => p.Note, f => f.Key("note"))
        .DefaultSort(p => p.Name)
        .DefaultPageSize(10)
        .MaxPageSize(50));

    protected static Person[] People() =>
    [
        new(1, "Alice Martin", "Backend", 120_000m, new(2024, 3, 15), "Lead"),
        new(2, "Bob Chen", "Frontend", 95_000.5m, new(2025, 1, 10)),
        new(3, "Claire Petit", "Backend", 110_000m, new(2023, 6, 1), "=HYPERLINK(\"x\")"),
        new(4, "David Kim", "Infra", 88_000m, new(2025, 4, 20), "a;b,\"c\""),
    ];

    protected void Prepare(bool interactive = true)
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(interactive ? new RendererInfo("Server", true) : new RendererInfo("Static", false));
    }

    protected static void Column(RenderTreeBuilder b, ref int seq, string field, string title, Action<RenderTreeBuilder, int>? extra = null, string? format = null)
    {
        b.OpenComponent<KlyDataListFieldColumn<Person>>(seq);
        b.AddComponentParameter(seq + 1, nameof(KlyDataListFieldColumn<Person>.Field), field);
        b.AddComponentParameter(seq + 2, nameof(KlyDataListFieldColumn<Person>.Title), title);
        if (format is not null) b.AddComponentParameter(seq + 3, nameof(KlyDataListFieldColumn<Person>.Format), format);
        extra?.Invoke(b, seq + 4);
        b.CloseComponent();
        seq += 20;
    }

    /// <summary>
    /// Renders a list of <see cref="People"/> with a table of the columns Name, Team and Salary (or <paramref name="columns"/>), the parameters of <paramref name="table"/>, and
    /// <paramref name="before"/> rendered inside the list ahead of the table (a part that is not the table).
    /// </summary>
    protected IRenderedComponent<KlyDataList<Person>> Mount(
        Action<ComponentParameterCollectionBuilder<KlyDataListTable<Person>>>? table = null,
        Action<RenderTreeBuilder>? columns = null,
        Action<ComponentParameterCollectionBuilder<KlyDataList<Person>>>? list = null,
        Action<RenderTreeBuilder>? before = null,
        IEnumerable<Person>? items = null,
        bool provider = false)
    {
        return Render<KlyDataList<Person>>(p =>
        {
            p.Add(x => x.Schema, PersonSchema).Add(x => x.UrlSync, false);
            if (provider)
            {
                var rows = (items ?? People()).ToList();
                p.Add(x => x.Provider, (DataListRequest request, CancellationToken _) => ValueTask.FromResult(PersonSchema.ExecuteInMemory(rows, request)));
            }
            else p.Add(x => x.Items, items ?? People());
            list?.Invoke(p);
            p.AddChildContent<KlyDataListTable<Person>>(t =>
            {
                table?.Invoke(t);
                // Parts that are not columns render inside the table's content too, still inside the list.
                t.AddChildContent(cb =>
                {
                    before?.Invoke(cb);
                    if (columns is not null) { columns(cb); return; }
                    var c = 100;
                    Column(cb, ref c, "name", "Name");
                    Column(cb, ref c, "team", "Team");
                    Column(cb, ref c, "salary", "Salary", format: "N2");
                });
            });
        });
    }
}

/// <summary>A part that hands the <see cref="DataListContext{TItem}"/> of its list to a test.</summary>
public sealed class CaptureContext : DataListPartBase<Person>
{
    /// <summary>Called with the context once the part initialized.</summary>
    [Parameter] public Action<DataListContext<Person>>? OnContext { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        OnContext?.Invoke(Context);
    }
}
