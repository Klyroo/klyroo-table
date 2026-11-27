namespace Klyroo.Table.Maui;

/// <summary>A view that takes its state from the <see cref="DataListContext{TItem}"/> of the list above it.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
internal interface IDataListPart<TItem>
{
    /// <summary>Gives the part the context of its list, or takes it away (<see langword="null"/>).</summary>
    void Bind(DataListContext<TItem>? context);
}
