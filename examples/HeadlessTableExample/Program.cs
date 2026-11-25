using Klyroo.Table;
using Klyroo.Query;

var tree = new DataListTree<Node> { Children = n => n.Children, DefaultExpanded = true };
var (_, allNodes) = tree.BuildTree([new Node("Root", [new Node("Child")])]);
Console.WriteLine($"Flattened tree nodes: {allNodes.Count} (Child depth={allNodes[1].Depth})");

public sealed record Node(string Name, IReadOnlyList<Node>? Children = null);
