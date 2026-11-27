namespace Klyroo.Table.Maui;

/// <summary>The view of a row in a virtualized list: it draws the row its binding context names, and draws it again when a recycled view is given another one.</summary>
internal sealed class TableRowHost<TItem> : ContentView
{
    private readonly Func<TItem, int, View> _build;

    public TableRowHost(Func<TItem, int, View> build)
    {
        _build = build;
        BindingContextChanged += (_, _) => Content = BindingContext is TableRowEntry<TItem> entry ? _build(entry.Item, entry.Position) : null;
    }
}
