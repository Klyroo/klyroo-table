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
        b.OpenComponent<KlyDataListColumn<Person>>(seq);
        b.AddComponentParameter(seq + 1, nameof(KlyDataListColumn<Person>.Field), field);
        b.AddComponentParameter(seq + 2, nameof(KlyDataListColumn<Person>.Title), title);
        if (format is not null) b.AddComponentParameter(seq + 3, nameof(KlyDataListColumn<Person>.Format), format);
        extra?.Invoke(b, seq + 4);
        b.CloseComponent();
        seq += 20;
    }

    /// <summary>Renders a list of <see cref="People"/> with a table of the columns Name, Team and Salary, plus whatever the caller adds.</summary>
    protected IRenderedComponent<KlyDataList<Person>> Mount(
        Action<ComponentParameterCollectionBuilder<KlyDataListTable<Person>>>? table = null,
        Action<RenderTreeBuilder>? columns = null,
        Action<ComponentParameterCollectionBuilder<KlyDataList<Person>>>? list = null,
        RenderFragment? parts = null,
        IEnumerable<Person>? items = null)
    {
        return Render<KlyDataList<Person>>(p =>
        {
            p.Add(x => x.Schema, PersonSchema)
             .Add(x => x.Items, items ?? People())
             .Add(x => x.UrlSync, false);
            list?.Invoke(p);
            if (parts is not null) p.Add(x => x.ChildContent, parts);
            else
            {
                p.AddChildContent<KlyDataListTable<Person>>(t =>
                {
                    table?.Invoke(t);
                    t.AddChildContent(b =>
                    {
                        if (columns is not null) { columns(b); return; }
                        var seq = 0;
                        Column(b, ref seq, "name", "Name");
                        Column(b, ref seq, "team", "Team");
                        Column(b, ref seq, "salary", "Salary", format: "N2");
                    });
                });
            }
        });
    }
}
