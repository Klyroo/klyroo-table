using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// Compact summary-row presentation for <c>KlyDataListTable{TItem}</c>: one scannable row per record with an optional leading visual,
/// a primary title (optionally a link or button), a status badge/pill, secondary details joined with <c> · </c>, selection checkbox and row actions.
/// Shown automatically below the <c>md</c> breakpoint when <c>AutoPhone</c> is <see langword="true"/>, when <c>DefaultActive</c> is <see langword="true"/>,
/// or when toggled with <c>KlyDataListViewToggle{TItem}</c>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed record DataListRowSummary<TItem>
{
    /// <summary>Primary one-line title of the row. When <see langword="null"/>, uses <c>Schema.GetRowLabel(item)</c> or the first visible column's formatted value.</summary>
    public Func<TItem, string?>? Title { get; init; }

    /// <summary>Optional leading content (a 32 px tile, icon or avatar) shown before the title.</summary>
    public RenderFragment<TItem>? Leading { get; init; }

    /// <summary>Optional status content (a badge or pill) shown at the start of the second line.</summary>
    public RenderFragment<TItem>? Status { get; init; }

    /// <summary>Secondary detail texts joined with <c> · </c> on the second line (blank entries are skipped). When <see langword="null"/>, uses the remaining visible columns.</summary>
    public Func<TItem, IEnumerable<string?>>? Details { get; init; }

    /// <summary>Optional link destination (<c>href</c>) for the title of the row.</summary>
    public Func<TItem, string?>? Href { get; init; }

    /// <summary>Optional click callback when the summary row's title button is activated (interactive only, when <c>Href</c> is <see langword="null"/>).</summary>
    public EventCallback<TItem> OnSelect { get; init; }

    /// <summary>Optional custom template for the body of the summary row, replacing <c>Title</c>, <c>Leading</c>, <c>Status</c> and <c>Details</c>.</summary>
    public RenderFragment<TItem>? Template { get; init; }

    /// <summary>Starts in summary-row view on every screen size until the user toggles <c>KlyDataListViewToggle{TItem}</c> (<see langword="false"/> by default).</summary>
    public bool DefaultActive { get; init; }

    /// <summary>Automatically switches to summary-row view below the <c>md</c> breakpoint (768 px) when the user has not explicitly chosen a view (<see langword="true"/> by default).</summary>
    public bool AutoPhone { get; init; } = true;
}
