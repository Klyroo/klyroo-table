namespace Klyroo.Table.Maui;

/// <summary>
/// Compact summary-row presentation for <see cref="KlyDataListTable{TItem}"/>, a card per record: an optional leading view, a primary title, a status view, secondary details joined with <c> · </c>,
/// the selection checkbox and the row actions. Shown automatically below 640 points of width when <see cref="AutoPhone"/> is on, when <see cref="DefaultActive"/> is on, or when toggled with <see cref="KlyDataListViewToggle{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed record DataListRowSummary<TItem>
{
    /// <summary>Creates a row summary with every option at its default: the title and details come from the schema and the columns.</summary>
    public DataListRowSummary()
    {
    }

    /// <summary>Primary one-line title of the row. When <see langword="null"/>, uses the row label of the schema or the first visible column's formatted value.</summary>
    public Func<TItem, string?>? Title { get; init; }

    /// <summary>Optional leading view (a 32 point tile, icon or avatar) shown before the title.</summary>
    public Func<TItem, View>? Leading { get; init; }

    /// <summary>Optional status view (a badge) shown at the start of the second line.</summary>
    public Func<TItem, View>? Status { get; init; }

    /// <summary>Secondary detail texts joined with <c> · </c> on the second line (blank entries are skipped). When <see langword="null"/>, uses the remaining visible columns.</summary>
    public Func<TItem, IEnumerable<string?>>? Details { get; init; }

    /// <summary>Called when the card is tapped.</summary>
    public Action<TItem>? OnSelect { get; init; }

    /// <summary>Optional custom body of the card, replacing <see cref="Title"/>, <see cref="Leading"/>, <see cref="Status"/> and <see cref="Details"/>.</summary>
    public Func<TItem, View>? Template { get; init; }

    /// <summary>Starts in summary view on every screen size until the user toggles <see cref="KlyDataListViewToggle{TItem}"/> (<see langword="false"/> by default).</summary>
    public bool DefaultActive { get; init; }

    /// <summary>Automatically switches to summary view below 640 points of width when the user has not explicitly chosen a view (<see langword="true"/> by default).</summary>
    public bool AutoPhone { get; init; } = true;
}
