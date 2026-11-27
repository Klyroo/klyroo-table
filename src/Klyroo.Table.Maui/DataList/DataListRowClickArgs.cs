namespace Klyroo.Table.Maui;

/// <summary>What a row reports when the user taps it (<see cref="KlyDataListTable{TItem}.RowClick"/>): a tap anywhere on the row that did not land on a control inside it.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="Item">The row.</param>
/// <param name="Key">The row key (<c>RowKey</c> of the schema) as text; <c>#3</c> (the position) for a list whose schema has none.</param>
/// <param name="Position">The place of the row on the page, from 1.</param>
public sealed record DataListRowClickArgs<TItem>(TItem Item, string Key, int Position);
