using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
            Column(b, ref seq, "team", "Team", (cb, s) => cb.AddComponentParameter(s, nameof(KlyDataListColumn<Person>.Visible), false));
            b.OpenComponent<KlyDataListColumn<Person>>(seq);
            b.AddComponentParameter(seq + 1, nameof(KlyDataListColumn<Person>.Title), "Actions");
            b.AddComponentParameter(seq + 2, nameof(KlyDataListColumn<Person>.Template), (RenderFragment<Person>)(p => x => x.AddContent(0, "x")));
            b.CloseComponent();
        });

        var lines = Lines(_context!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Culture = Fr }));
        Assert.Equal(["Name", "Salary", "Hired"], _context.ExportHeaders());
        Assert.Equal("Name;Salary;Hired", lines[0]);
        // Page size 2, sorted by name: Alice, Bob. The salary has the decimal comma of the culture, the date its short format.
        Assert.Equal(3, lines.Count);
        Assert.StartsWith("Alice Martin;120", lines[1]);
        Assert.EndsWith(",00;15/03/2024", lines[1]);
        Assert.StartsWith("Bob Chen;95", lines[2]);
        Assert.EndsWith(",50;10/01/2025", lines[2]);

        Assert.StartsWith("Name,Salary,Hired\r\nAlice Martin,\"120,000.00\",3/15/2024\r\n", _context.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Culture = En }));
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
        var csv = _context.ExportCsv(DataListExportScope.Selection, new DataListCsvOptions { Culture = En });
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

        var lines = Lines(_context!.ExportCsv(DataListExportScope.Page, new DataListCsvOptions { Culture = CultureInfo.InvariantCulture }));
        Assert.Equal("'=cmd|' /C calc'!A0,'-Backend,-12.5,'@x", lines[1]);
        Assert.Equal("\"Plain, with comma\",Infra,3,\"say \"\"hi\"\"\"", lines[2]);
    }

    [Fact]
    public async Task The_button_downloads_the_file_through_the_browser_module()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadText", _ => true);
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

        var call = Assert.Single(module.Invocations["downloadText"]);
        Assert.Equal("People 2026Q1.csv", call.Arguments[0]);
        Assert.Equal("﻿Name\r\nAlice Martin\r\nBob Chen\r\n", call.Arguments[1]);
        Assert.Equal("text/csv;charset=utf-8", call.Arguments[2]);
    }

    [Fact]
    public async Task OnExport_receives_the_file_and_replaces_the_download()
    {
        Prepare();
        var module = JSInterop.SetupModule(DownloadModule);
        module.SetupVoid("downloadText", _ => true);
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
        Assert.Empty(module.Invocations["downloadText"]);
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
        module.SetupVoid("downloadText", _ => true);
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
        var call = Assert.Single(module.Invocations["downloadText"]);
        Assert.Equal(5, Lines((string)call.Arguments[1]!).Count);
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
}
