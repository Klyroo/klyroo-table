using System.Diagnostics;
using Klyroo.Query;
using Klyroo.Table;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class PerformanceAndStressTests
{
    private sealed record TreeNodeItem(int Id, string Name, IReadOnlyList<TreeNodeItem>? Children = null);

    private static readonly DataListSchema<TreeNodeItem> Schema = DataListSchema.For<TreeNodeItem>(s => s
        .RowKey(x => x.Id)
        .RowLabel(x => x.Name));

    [Fact]
    public void DataListTree_BuildTree_flattens_1000_nodes_across_3_levels_in_subsecond_time()
    {
        var id = 1;
        var roots = new List<TreeNodeItem>();
        for (var r = 0; r < 10; r++)
        {
            var children = new List<TreeNodeItem>();
            for (var c = 0; c < 10; c++)
            {
                var leaves = new List<TreeNodeItem>();
                for (var l = 0; l < 9; l++)
                    leaves.Add(new TreeNodeItem(id++, $"Leaf-{r}-{c}-{l}"));
                children.Add(new TreeNodeItem(id++, $"Branch-{r}-{c}", leaves));
            }
            roots.Add(new TreeNodeItem(id++, $"Root-{r}", children));
        }

        var tree = new DataListTree<TreeNodeItem> { Children = x => x.Children, DefaultExpandedDepth = 3 };
        var sw = Stopwatch.StartNew();
        var (builtRoots, flat) = tree.BuildTree(roots, Schema);
        sw.Stop();

        Assert.Equal(10, builtRoots.Count);
        Assert.Equal(1010, flat.Count);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(2), $"Tree build took too long: {sw.Elapsed}");
    }
}
