using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Klyroo.Table;

/// <summary>
/// A row of the table. It renders its cells again only when its row, or the <c>Revision</c> the table gives it, changes: the table renders for every flag of the
/// list (loading, busy, error) and the rows should not rebuild each time. The revision changes when the table's parent renders (so a template that reads the page's
/// state stays current) and when a new result arrives.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class DataListRowView<TItem> : ComponentBase
{
    private object? _item;
    private int _revision;
    private int? _rowIndex;
    private bool _rendered;

    /// <summary>The row.</summary>
    [Parameter] public TItem Item { get; set; } = default!;

    /// <summary>Changes when the cells must be built again.</summary>
    [Parameter] public int Revision { get; set; }

    /// <summary>1-based row index in a virtualized table (2 for the first data row, after the header); <see langword="null"/> otherwise.</summary>
    [Parameter] public int? RowIndex { get; set; }

    /// <summary>The classes of the row.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Attributes of the row from <see cref="KlyDataListTable{TItem}.RowAttributes"/>. <c>class</c> and <c>data-slot</c> are ignored: the row sets them itself.</summary>
    [Parameter] public IReadOnlyDictionary<string, object>? Attributes { get; set; }

    /// <summary>Makes the row reachable with the keyboard and listen to activations: a click on it that is not on a control inside it, and Enter or Space on the row itself.</summary>
    [Parameter] public bool Clickable { get; set; }

    /// <summary>Raised for an activation of the row, as the browser side decided it.</summary>
    [Parameter] public EventCallback<KlyRowEventArgs> OnActivate { get; set; }

    /// <summary>The cells.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>How many times the row has been built; read by tests.</summary>
    internal int BuildCount { get; private set; }

    // The first render does not ask: what was built is recorded when it is built.
    /// <inheritdoc />
    protected override bool ShouldRender() => !(_rendered && ReferenceEquals(_item, Item) && _revision == Revision && _rowIndex == RowIndex);

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        BuildCount++;
        _rendered = true;
        _item = Item;
        _revision = Revision;
        _rowIndex = RowIndex;
        builder.OpenElement(0, "tr");
        // The attributes of the app come first, so that what the row sets itself wins over a duplicate.
        if (Attributes is { Count: > 0 })
        {
            builder.AddMultipleAttributes(1, Attributes.Where(a => !a.Key.Equals("class", StringComparison.OrdinalIgnoreCase)
                && !a.Key.Equals("data-slot", StringComparison.OrdinalIgnoreCase)
                && !a.Key.Equals("onklyrowactivate", StringComparison.OrdinalIgnoreCase)));
        }
        builder.AddAttribute(2, "class", Class);
        builder.AddAttribute(3, "data-slot", "row");
        if (RowIndex is { } index) builder.AddAttribute(4, "aria-rowindex", index);
        if (Clickable)
        {
            builder.AddAttribute(5, "tabindex", "0");
            builder.AddAttribute(6, "data-clickable", "true");
            builder.AddAttribute(7, "onklyrowactivate", OnActivate);
        }
        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

/// <summary>A key that compares by reference: the identity of a row that has no row key in the schema.</summary>
internal sealed class RowReferenceKey(object row)
{
    public override bool Equals(object? obj) => obj is RowReferenceKey other && ReferenceEquals(row, other.RowObject);

    private object RowObject => row;

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(row);
}
