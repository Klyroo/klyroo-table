namespace Klyroo.Table.Maui;

/// <summary>A row of a virtualized list: the row and its place on the page.</summary>
/// <param name="Item">The row.</param>
/// <param name="Position">Its place on the page, from 1.</param>
internal sealed record TableRowEntry<TItem>(TItem Item, int Position);
