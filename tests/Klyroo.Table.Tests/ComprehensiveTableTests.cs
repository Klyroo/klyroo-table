using Bunit;
using Klyroo.Query;
using Klyroo.Table;
using Klyroo.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class ComprehensiveTableTests : BunitContext
{
    private sealed record OrgMember(int Id, string Name, string Dept, decimal Salary, IReadOnlyList<OrgMember>? Reports = null);

    private static readonly DataListSchema<OrgMember> Schema = DataListSchema.For<OrgMember>(s => s
        .RowKey(m => m.Id)
        .RowLabel(m => m.Name)
        .Field(m => m.Name, f => f.Key("name").Sortable())
        .Field(m => m.Dept, f => f.Key("dept").Sortable())
        .Field(m => m.Salary, f => f.Key("salary").Sortable()));

    [Fact]
    public void DataListTree_BuildTree_tracks_depth_continuations_and_guards_duplicate_keys()
    {
        var leaf = new OrgMember(3, "Alan", "Eng", 140_000m);
        var dupLeaf = new OrgMember(3, "Duplicate Alan", "Eng", 140_000m);
        var manager = new OrgMember(2, "Grace", "Eng", 160_000m, [leaf, dupLeaf]);
        var cto = new OrgMember(1, "Ada", "Exec", 200_000m, [manager]);

        var tree = new DataListTree<OrgMember>
        {
            Children = m => m.Reports,
            DefaultExpandedDepth = 2,
            Selectable = DataListTreeSelectionMode.Roots
        };

        var (roots, all) = tree.BuildTree([cto], Schema);

        Assert.Single(roots);
        Assert.Equal(3, all.Count); // Duplicate key 3 skipped safely
        Assert.Equal(0, all[0].Depth);
        Assert.True(all[0].HasChildren);
        Assert.Equal(1, all[1].Depth);
        Assert.Equal(2, all[2].Depth);
        Assert.False(all[2].HasChildren);
    }

    [Fact]
    public void KlyDataList_renders_grouped_rows_and_footer_summary_in_bUnit()
    {
        Services.AddKlyrooTable();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Server", true));

        OrgMember[] rows =
        [
            new(1, "Alice", "Engineering", 120_000m),
            new(2, "Bob", "Engineering", 90_000m),
            new(3, "Claire", "Product", 110_000m)
        ];

        var cut = Render(builder =>
        {
            builder.OpenComponent<KlyDataList<OrgMember>>(0);
            builder.AddAttribute(1, nameof(KlyDataList<OrgMember>.Schema), Schema);
            builder.AddAttribute(2, nameof(KlyDataList<OrgMember>.Items), rows);
            builder.AddAttribute(3, nameof(KlyDataList<OrgMember>.UrlSync), false);
            builder.AddAttribute(4, nameof(KlyDataList<OrgMember>.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<KlyDataListTable<OrgMember>>(0);
                b.AddAttribute(1, nameof(KlyDataListTable<OrgMember>.HeaderMenu), true);
                b.AddAttribute(2, nameof(KlyDataListTable<OrgMember>.GroupBy), new DataListGroupBy<OrgMember> { Key = m => m.Dept });
                b.AddAttribute(3, nameof(KlyDataListTable<OrgMember>.Summaries), new DataListSummaries<OrgMember>());
                b.AddAttribute(4, nameof(KlyDataListTable<OrgMember>.ChildContent), (RenderFragment)(cb =>
                {
                    cb.OpenComponent<KlyDataListColumn<OrgMember>>(0);
                    cb.AddAttribute(1, nameof(KlyDataListColumn<OrgMember>.Field), "name");
                    cb.AddAttribute(2, nameof(KlyDataListColumn<OrgMember>.Title), "Name");
                    cb.CloseComponent();
                    cb.OpenComponent<KlyDataListColumn<OrgMember>>(3);
                    cb.AddAttribute(4, nameof(KlyDataListColumn<OrgMember>.Field), "salary");
                    cb.AddAttribute(5, nameof(KlyDataListColumn<OrgMember>.Title), "Salary");
                    cb.AddAttribute(6, nameof(KlyDataListColumn<OrgMember>.Summary), DataListSummaryKind.Sum);
                    cb.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains("Alice", cut.Markup);
        Assert.Contains("Engineering", cut.Markup);
        Assert.Contains("Product", cut.Markup);
        Assert.NotNull(cut.Find("tfoot"));
    }
}
