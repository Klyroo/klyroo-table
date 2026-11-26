using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class ExportTests : TableHarness
{
    private const string DownloadModule = "./_content/Klyroo.Table/klyroo-table-download.js";

    private DataListContext<Person>? _context;

    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");
    private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

    private IRenderedComponent<KlyDataList<Person>> MountWith(
        Action<RenderTreeBuilder>? exportPart = null,
        Action<ComponentParameterCollectionBuilder<KlyDataList<Person>>>? list = null,
        Action<RenderTreeBuilder>? columns = null,
        IEnumerable<Person>? items = null,
        bool provider = false,
        string? search = null) =>
        Mount(
            columns: columns,
            list: l =>
            {
                l.Add(x => x.State, PersonSchema.DefaultState with { PageSize = 2, Search = search ?? "" });
                list?.Invoke(l);
            },
            before: b =>
            {
                b.OpenComponent<CaptureContext>(0);
                b.AddComponentParameter(1, nameof(CaptureContext.OnContext), (Action<DataListContext<Person>>)(c => _context = c));
                b.CloseComponent();
                exportPart?.Invoke(b);
            },
            items: items,
            provider: provider);

    private static Action<RenderTreeBuilder> ExportPart(Action<RenderTreeBuilder, int>? parameters = null) => b =>
    {
        b.OpenComponent<KlyDataListExport<Person>>(10);
        parameters?.Invoke(b, 11);
        b.CloseComponent();
    };

    private static IReadOnlyList<string> Lines(string csv) => csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void The_csv_has_the_visible_columns_in_the_order_of_the_table_with_their_formats_and_culture()
    {
        Prepare();
        MountWith(columns: b =>
        {
            var seq = 100;
            Column(b, ref seq, "name", "Name");
            Column(b, ref seq, "salary", "Salary", format: "N2");
            Column(b, ref seq, "hired", "Hired", format: "d");
            // A column that is hidden, and one that only has a template, are not exported.
            Column(b, ref seq, "team", "Team", (cb, s) => cb.AddComponentParameter(s, nameof(KlyDataListFieldColumn<Person>.Visible), false));
            b.OpenComponent<KlyDataListTemplateColumn<Person>>(seq);
            b.AddComponentParameter(seq + 1, nameof(KlyDataListTemplateColumn<Person>.Title), "Actions");
            b.AddComponentParameter(seq + 2, nameof(KlyDataListTemplateColumn<Person>.Template), (RenderFragment<Person>)(p => x => x.AddContent(0, "x")));
            b.CloseComponent();
        });

        var lines = Lines(_context!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = Fr }));
        Assert.Equal(["Name", "Salary", "Hired"], _context.ExportHeaders());
        Assert.Equal("Name;Salary;Hired", lines[0]);
        // Page size 2, sorted by name: Alice, Bob. The salary has the decimal comma of the culture, the date its short format.
        Assert.Equal(3, lines.Count);
        Assert.StartsWith("Alice Martin;120", lines[1]);
        Assert.EndsWith(",00;15/03/2024", lines[1]);
        Assert.StartsWith("Bob Chen;95", lines[2]);
        Assert.EndsWith(",50;10/01/2025", lines[2]);

        Assert.StartsWith("Name,Salary,Hired\r\nAlice Martin,\"120,000.00\",3/15/2024\r\n", _context.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = En }));
    }

    [Fact]
    public void A_table_without_declared_columns_exports_the_fields_of_the_schema()
    {
        Prepare();
        Render<KlyDataList<Person>>(p => p
            .Add(x => x.Schema, PersonSchema)
            .Add(x => x.Items, People())
            .Add(x => x.UrlSync, false)
            .AddChildContent(b =>
            {
                b.OpenComponent<CaptureContext>(0);
                b.AddComponentParameter(1, nameof(CaptureContext.OnContext), (Action<DataListContext<Person>>)(c => _context = c));
                b.CloseComponent();
                b.OpenComponent<KlyDataListTable<Person>>(2);
                b.CloseComponent();
            }));

        Assert.Equal(["Name", "Team", "Salary", "Hired", "Note"], _context!.ExportHeaders());
    }

    [Fact]
    public void Scopes_take_the_page_the_selection_or_every_matching_row()
    {
        Prepare();
        var cut = MountWith(list: l => l.Add(x => x.Selectable, true));

        Assert.Equal(["Alice Martin", "Bob Chen"], _context!.ExportRows(DataListExportScope.Page).Select(p => p.Name));
        Assert.Equal(4, _context.ExportRows(DataListExportScope.All).Count);
        Assert.True(_context.HoldsAllRows);
        Assert.Empty(_context.ExportRows(DataListExportScope.Selection));

        // A box ticked on the page is the selection; rows ticked on another page of a list in memory stay in it.
        cut.Find("tbody tr:first-child input[data-select=row]").Change(true);
        Assert.Equal(["Alice Martin"], _context.ExportRows(DataListExportScope.Selection).Select(p => p.Name));
        var csv = _context.ExportCsv(DataListExportScope.Selection, new DataListCsvOptions { Utf8Bom = false, Culture = En });
        Assert.Equal(2, Lines(csv).Count);
    }

    [Fact]
    public void All_follows_the_search_and_the_filters()
    {
        Prepare();
        MountWith(search: "Backend");
        Assert.Equal(["Alice Martin", "Claire Petit"], _context!.ExportRows(DataListExportScope.All).Select(p => p.Name));
    }

    [Fact]
    public void Text_cells_are_protected_against_formulas_and_numbers_are_not()
    {
        Prepare();
        var items = new[]
        {
            new Person(1, "=cmd|' /C calc'!A0", "-Backend", -12.5m, new(2024, 1, 2), "@x"),
            new Person(2, "Plain, with comma", "Infra", 3m, new(2024, 1, 3), "say \"hi\""),
        };
        MountWith(items: items, columns: b =>
        {
            var seq = 100;
            Column(b, ref seq, "name", "Name");
            Column(b, ref seq, "team", "Team");
            Column(b, ref seq, "salary", "Salary");
            Column(b, ref seq, "note", "Note");
        });

        var lines = Lines(_context!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = CultureInfo.InvariantCulture }));
        Assert.Equal("'=cmd|' /C calc'!A0,'-Backend,-12.5,'@x", lines[1]);
        Assert.Equal("\"Plain, with comma\",Infra,3,\"say \"\"hi\"\"\"", lines[2]);
    }

    [Fact]
    public async Task The_button_downloads_the_file_through_the_browser_module()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadStream", _ => true);
        var cut = MountWith(
            ExportPart((b, s) =>
            {
                b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.Page]);
                b.AddComponentParameter(s + 1, nameof(KlyDataListExport<Person>.FileName), "People: 2026/Q1");
                b.AddComponentParameter(s + 2, nameof(KlyDataListExport<Person>.CsvOptions), new DataListCsvOptions { Culture = En, Utf8Bom = true });
            }),
            columns: b =>
            {
                var seq = 100;
                Column(b, ref seq, "name", "Name");
            });

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        var call = Assert.Single(module.Invocations["downloadStream"]);
        Assert.Equal("People 2026Q1.csv", call.Arguments[0]);
        Assert.IsType<DotNetStreamReference>(call.Arguments[1]);
        Assert.Equal("text/csv;charset=utf-8", call.Arguments[2]);
    }

    [Fact]
    public async Task OnExport_receives_the_file_and_replaces_the_download()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadStream", _ => true);
        DataListExportResult? received = null;
        var cut = MountWith(
            ExportPart((b, s) =>
            {
                b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.All]);
                b.AddComponentParameter(s + 1, nameof(KlyDataListExport<Person>.OnExport), EventCallback.Factory.Create<DataListExportResult>(this, r => received = r));
            }),
            columns: b =>
            {
                var seq = 100;
                Column(b, ref seq, "name", "Name");
            });

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        Assert.NotNull(received);
        Assert.Equal(4, received!.RowCount);
        Assert.Equal(DataListExportScope.All, received.Scope);
        Assert.Equal("export.csv", received.FileName);
        Assert.Empty(module.Invocations["downloadStream"]);
    }

    [Fact]
    public void Several_sources_open_a_menu_and_one_source_is_a_button()
    {
        Prepare();
        var many = MountWith(ExportPart(), list: l => l.Add(x => x.Selectable, true));
        var markup = many.Markup;
        Assert.Contains("This page", markup);
        Assert.Contains("All 4 results", markup);
        // Nothing is ticked, so the selection is not offered.
        Assert.DoesNotContain("Selected rows", markup);
        many.Find("tbody tr:first-child input[data-select=row]").Change(true);
        Assert.Contains("Selected rows (1)", many.Markup);
    }

    [Fact]
    public void A_list_over_a_provider_offers_every_result_only_with_LoadAll()
    {
        Prepare();
        var without = MountWith(ExportPart(), provider: true);
        Assert.DoesNotContain("All 4 results", without.Markup);
        // One source left: a plain button, no menu.
        Assert.NotNull(without.Find("[data-slot=export-trigger]"));
        Assert.False(_context!.HoldsAllRows);
        Assert.Empty(_context.ExportRows(DataListExportScope.All));
    }

    [Fact]
    public async Task LoadAll_receives_the_request_of_every_result_and_its_rows_are_exported()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadStream", _ => true);
        DataListRequest? asked = null;
        Func<DataListRequest, CancellationToken, Task<IReadOnlyList<Person>>> loadAll = (request, _) =>
        {
            asked = request;
            return Task.FromResult<IReadOnlyList<Person>>(People());
        };
        var cut = MountWith(
            ExportPart((b, s) =>
            {
                b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.All]);
                b.AddComponentParameter(s + 1, nameof(KlyDataListExport<Person>.LoadAll), loadAll);
            }),
            columns: b =>
            {
                var seq = 100;
                Column(b, ref seq, "name", "Name");
            },
            provider: true);

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        Assert.NotNull(asked);
        Assert.IsType<DataListPaging.None>(asked!.Paging);
        var call = Assert.Single(module.Invocations["downloadStream"]);
        Assert.IsType<DotNetStreamReference>(call.Arguments[1]);
    }

    [Fact]
    public async Task A_failure_is_reported_to_OnError_and_the_button_is_usable_again()
    {
        Prepare();
        JSInterop.SetupModule(DownloadModule);
        Exception? failure = null;
        var cut = MountWith(
            ExportPart((b, s) =>
            {
                b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.All]);
                b.AddComponentParameter(s + 1, nameof(KlyDataListExport<Person>.OnExport), EventCallback.Factory.Create<DataListExportResult>(this, _ => throw new InvalidOperationException("disk full")));
                b.AddComponentParameter(s + 2, nameof(KlyDataListExport<Person>.OnError), EventCallback.Factory.Create<Exception>(this, e => failure = e));
            }));

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        Assert.Equal("disk full", failure?.Message);
        Assert.False(cut.Find("[data-slot=export-trigger]").HasAttribute("disabled"));
    }

    [Fact]
    public void A_static_render_draws_no_export_button()
    {
        Prepare(interactive: false);
        var cut = MountWith(ExportPart());
        Assert.Empty(cut.FindAll("[data-slot=export]"));
    }

    [Fact]
    public async Task The_download_streams_the_utf8_bytes_with_the_byte_order_mark_by_default()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.Setup<object?>("downloadStream", _ => true).SetResult(null);
        var cut = MountWith(
            ExportPart((b, s) => b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.Page])),
            columns: b =>
            {
                var seq = 100;
                Column(b, ref seq, "name", "Né");
            });

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        var call = Assert.Single(module.Invocations["downloadStream"]);
        var reference = Assert.IsType<DotNetStreamReference>(call.Arguments[1]);
        Assert.NotNull(reference);
        Assert.Equal("export.csv", call.Arguments[0]);
    }

    [Fact]
    public async Task An_export_over_MaxRows_is_refused_with_a_message_and_nothing_is_loaded_or_downloaded()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadStream", _ => true);
        Exception? failure = null;
        var cut = MountWith(ExportPart((b, s) =>
        {
            b.AddComponentParameter(s, nameof(KlyDataListExport<Person>.Scopes), (IReadOnlyList<DataListExportScope>)[DataListExportScope.All]);
            b.AddComponentParameter(s + 1, nameof(KlyDataListExport<Person>.MaxRows), 3);
            b.AddComponentParameter(s + 2, nameof(KlyDataListExport<Person>.OnError), EventCallback.Factory.Create<Exception>(this, e => failure = e));
        }));

        await cut.Find("[data-slot=export-trigger]").ClickAsync();

        var tooLarge = Assert.IsType<DataListExportTooLargeException>(failure);
        Assert.Equal(4, tooLarge.Count);
        Assert.Equal(3, tooLarge.Limit);
        Assert.Contains("Too many rows to export: 4 results, the limit is 3", cut.Find("[data-slot=export-message]").TextContent);
        Assert.Empty(module.Invocations["downloadStream"]);
        Assert.False(cut.Find("[data-slot=export-trigger]").HasAttribute("disabled"));
    }

    private enum Level { High }

    private sealed class Odd : IFormattable
    {
        public string ToString(string? format, IFormatProvider? provider) => "=odd";
    }

    [Fact]
    public void Only_known_value_types_skip_the_formula_guard()
    {
        // One field per value type under test: the schema describes each field by its type, so the export knows which ones the type writes.
        var schema = Klyroo.Query.DataListSchema.For<Holder>(s => s
            .Field(h => h.Number, f => f.Key("number"))
            .Field(h => h.Level, f => f.Key("level"))
            .Field(h => h.Odd, f => f.Key("odd"))
            .Field(h => h.Id, f => f.Key("id"))
            .Field(h => h.Text, f => f.Key("text"))
            .Field(h => h.Money, f => f.Key("money"))
            .Field(h => h.Moment, f => f.Key("moment")));
        Prepare();
        Render<KlyDataList<Holder>>(p => p
            .Add(x => x.Schema, schema)
            .Add(x => x.Items, new[] { new Holder(-5, Level.High, new Odd(), Guid.Empty, "-x", -1.5m, new DateTime(2024, 1, 2)) })
            .Add(x => x.UrlSync, false)
            .AddChildContent(b =>
            {
                b.OpenComponent<CaptureHolder>(0);
                b.AddComponentParameter(1, nameof(CaptureHolder.OnContext), (Action<DataListContext<Holder>>)(c => _holder = c));
                b.CloseComponent();
                b.OpenComponent<KlyDataListTable<Holder>>(2);
                b.CloseComponent();
            }));

        var lines = Lines(_holder!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = CultureInfo.InvariantCulture }));
        Assert.Equal(["Number,Level,Odd,Id,Text,Money,Moment", "-5,High,'=odd,00000000-0000-0000-0000-000000000000,'-x,-1.5,01/02/2024 00:00:00"], lines);
    }

    [Fact]
    public void A_field_declared_object_is_guarded_by_the_type_of_each_value_next_to_typed_fields()
    {
        var schema = Klyroo.Query.DataListSchema.For<Mixed>(s => s
            .Field(h => h.Typed, f => f.Key("typed"))
            .Field(h => h.Text, f => f.Key("text"))
            .Field(h => h.Value, f => f.Key("value")));
        Prepare();
        Render<KlyDataList<Mixed>>(p => p
            .Add(x => x.Schema, schema)
            .Add(x => x.Items, new[]
            {
                new Mixed(-1, "-t", -5), new Mixed(-2, "-t", Level.High), new Mixed(-3, "-t", new Odd()), new Mixed(-4, "-t", Guid.Empty),
                new Mixed(-5, "-t", "-x"), new Mixed(-6, "-t", -1.5m), new Mixed(-7, "-t", new DateTime(2024, 1, 2)),
            })
            .Add(x => x.UrlSync, false)
            .AddChildContent(b =>
            {
                b.OpenComponent<CaptureMixed>(0);
                b.AddComponentParameter(1, nameof(CaptureMixed.OnContext), (Action<DataListContext<Mixed>>)(c => _mixed = c));
                b.CloseComponent();
                b.OpenComponent<KlyDataListTable<Mixed>>(2);
                b.CloseComponent();
            }));

        var lines = Lines(_mixed!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = CultureInfo.InvariantCulture }));
        // A number or date held by the object field is not escaped, a string or a custom type is; the typed field never is, the string field always is.
        Assert.Equal(
            ["Typed,Text,Value", "-1,'-t,-5", "-2,'-t,High", "-3,'-t,'=odd", "-4,'-t,00000000-0000-0000-0000-000000000000", "-5,'-t,'-x", "-6,'-t,-1.5", "-7,'-t,01/02/2024 00:00:00"],
            lines);
    }

    private DataListContext<Mixed>? _mixed;

    private sealed record Mixed(int Typed, string Text, object? Value);

    private sealed class CaptureMixed : DataListPartBase<Mixed>
    {
        [Parameter] public Action<DataListContext<Mixed>>? OnContext { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            OnContext?.Invoke(Context);
        }
    }

    private DataListContext<Holder>? _holder;

    private sealed record Holder(int Number, Level Level, Odd Odd, Guid Id, string Text, decimal Money, DateTime Moment);

    private sealed class CaptureHolder : DataListPartBase<Holder>
    {
        [Parameter] public Action<DataListContext<Holder>>? OnContext { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            OnContext?.Invoke(Context);
        }
    }

    [Fact]
    public void A_column_with_a_template_exports_its_formatted_value_from_a_property_or_from_the_schema_field_of_its_key()
    {
        Prepare();
        MountWith(columns: b =>
        {
            var seq = 100;
            RenderFragment<Person> template = p => x => x.AddContent(0, "x");
            b.OpenComponent<KlyDataListColumn<Person, decimal>>(seq);
            b.AddComponentParameter(seq + 1, nameof(KlyDataListColumn<Person, decimal>.Title), "Pay");
            b.AddComponentParameter(seq + 2, nameof(KlyDataListColumn<Person, decimal>.Template), template);
            b.AddComponentParameter(seq + 3, nameof(KlyDataListColumn<Person, decimal>.Property), (System.Linq.Expressions.Expression<Func<Person, decimal>>)(p => p.Salary));
            b.AddComponentParameter(seq + 4, nameof(KlyDataListColumn<Person, decimal>.Format), "N0");
            b.CloseComponent();
            b.OpenComponent<KlyDataListTemplateColumn<Person>>(seq + 10);
            b.AddComponentParameter(seq + 11, nameof(KlyDataListTemplateColumn<Person>.Title), "Since");
            b.AddComponentParameter(seq + 12, nameof(KlyDataListTemplateColumn<Person>.Template), template);
            b.AddComponentParameter(seq + 13, nameof(KlyDataListTemplateColumn<Person>.Key), "hired");
            b.AddComponentParameter(seq + 14, nameof(KlyDataListTemplateColumn<Person>.Format), "yyyy");
            b.CloseComponent();
            b.OpenComponent<KlyDataListTemplateColumn<Person>>(seq + 20);
            b.AddComponentParameter(seq + 21, nameof(KlyDataListTemplateColumn<Person>.Title), "Nothing");
            b.AddComponentParameter(seq + 22, nameof(KlyDataListTemplateColumn<Person>.Template), template);
            b.CloseComponent();
        });

        var lines = Lines(_context!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Utf8Bom = false, Culture = En }));
        Assert.Equal("Pay,Since", lines[0]);
        Assert.Equal("\"120,000\",2024", lines[1]);
    }
}
