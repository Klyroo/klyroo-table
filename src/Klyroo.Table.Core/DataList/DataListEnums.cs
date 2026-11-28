namespace Klyroo.Table;

/// <summary>How a data list pages its rows.</summary>
public enum DataListPagingMode
{
    /// <summary>Numbered pages (<c>p</c>, <c>ps</c>) with <c>KlyDataListPagination</c>.</summary>
    Offset,

    /// <summary>Keyset cursor paging (<c>c</c>): pages append in place with <c>KlyDataListInfiniteLoader</c>, and degrade to a link in static rendering.</summary>
    Cursor,

    /// <summary>No paging: the source returns every matching row at once. Pairs with <c>KlyDataListTable.Virtualize</c>.</summary>
    None,
}

/// <summary>How tight the rows of a data list are.</summary>
public enum DataListDensity
{
    /// <summary>Normal rows.</summary>
    Default,

    /// <summary>Little padding, for long lists.</summary>
    Compact,

    /// <summary>Generous padding.</summary>
    Comfortable,
}

/// <summary>Horizontal alignment of a column.</summary>
public enum DataListAlign
{
    /// <summary>At the start of the line: left in a left-to-right language.</summary>
    Start,

    /// <summary>Centered.</summary>
    Center,

    /// <summary>At the end of the line, for numbers.</summary>
    End,
}

/// <summary>How a filter editor applies what the user changes.</summary>
public enum DataListApplyMode
{
    /// <summary>Each change applies at once (a text filter after a pause in the typing). The history gets an entry per change.</summary>
    Live,

    /// <summary>Changes wait in the editor until the Apply button; Cancel, Escape or a click outside drop them.</summary>
    Submit,
}

/// <summary>Which rows a bulk or toolbar action targets.</summary>
public enum DataListActionScope
{
    /// <summary>The rows that are ticked.</summary>
    Selection,

    /// <summary>Every row the search and the filters match, on every page. A list over a provider or a query does not hold them: the action gets the <c>DataListActionContext.Request</c> and applies it where the data lives.</summary>
    All,
}

/// <summary>Where the rows of a selection survive: across pages, or only on the page they were ticked on.</summary>
public enum DataListSelectionScope
{
    /// <summary>The list decides: a list of rows in memory (<c>Items</c>) keeps the selection across pages, a list over a provider or a queryable keeps it for the rows of the page.</summary>
    Auto,

    /// <summary>Rows that leave the page (a new page, a new search or filter, another sort) are no longer selected.</summary>
    Page,

    /// <summary>Selected rows stay selected on other pages while the search and the filters still match them. Only a list of rows in memory (<c>Items</c>) can do it: the others behave as <see cref="Page"/>.</summary>
    All,
}

/// <summary>When running an action unticks the rows.</summary>
public enum DataListSelectionClear
{
    /// <summary>The selection is kept (the default).</summary>
    Never,

    /// <summary>The rows are unticked as the action starts.</summary>
    Trigger,

    /// <summary>The rows are unticked once the action finished without failing; a failure keeps them so that the user can try again.</summary>
    Success,
}

/// <summary>Where the selection bar sits.</summary>
public enum DataListSelectionBarPosition
{
    /// <summary>At the bottom of the window while the list is on screen, and no lower than the end of the list (the default).</summary>
    Sticky,

    /// <summary>Fixed to the bottom of the window, centered.</summary>
    Fixed,
}

/// <summary>Which horizontal edge a column of <c>KlyDataListTable</c> is pinned to.</summary>
public enum DataListPin
{
    /// <summary>Scrolls horizontally with the table.</summary>
    None,

    /// <summary>Sticks to the start edge (left in left-to-right languages).</summary>
    Start,

    /// <summary>Sticks to the end edge (right in left-to-right languages).</summary>
    End,
}

/// <summary>An option offered by a sort menu.</summary>
/// <param name="Key">The key of the sortable field in the schema.</param>
/// <param name="Label">The display label; when <see langword="null"/>, uses the title of the matching column or the humanized key.</param>
public sealed record DataListSortOption(string Key, string? Label = null);
