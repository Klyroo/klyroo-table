namespace Klyroo.Table;

// Razor components cannot carry an XML doc comment on their generated class. Each declaration below is the other half of the partial class of the
// component with the same name, so the documentation reaches the compiled XML file and the API reference. They hold no code.

/// <summary>
/// The confirmation dialog of an action: a title, an optional description, a Cancel button and a confirm button. <see cref="KlyDataListSelectionActions{TItem}"/> and
/// <see cref="KlyDataListActionsDropdown{TItem}"/> open it for an action that has a <see cref="DataListAction{TItem}.Confirm"/>.
/// </summary>
public partial class DataListActionDialog;

/// <summary>The selection checkbox of one row. It listens to the selection only, so ticking a box does not render the other rows.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class DataListRowSelect<TItem>;

/// <summary>The select-all checkbox in the header of the table. It ticks the rows on screen and is mixed while only some of them are ticked.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class DataListSelectAll<TItem>;

/// <summary>The toolbar actions of a list (export, import, a new row) as a menu behind one button. They do not need a selection; an action that wants one reads it from its context.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListActionsDropdown<TItem>;

/// <summary>
/// The add filter button: a menu that lists the filters of the schema that are not on, and opens the editor of the one you pick. With many filters the menu gets a search field
/// (<see cref="SearchFrom"/>).
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListAddFilter<TItem>;

/// <summary>A button that takes every filter off and goes back to the first page; the search, the sort and the page size stay. It shows only while a filter is on.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListClearFilters<TItem>;

/// <summary>
/// A button that opens a popover to show and hide columns, reorder them, pin them to either edge and restore the defaults. Changes are kept in the column preferences
/// of the list (<see cref="KlyDataList{TItem}.PreferencesKey"/>).
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListColumnPanel<TItem>;

/// <summary>
/// A button that exports the list as a CSV file: the visible columns in the order the user sees them, formatted like the table, with the culture of the page and a delimiter that fits it.
/// With several sources of rows (this page, the selection, every result) it opens a menu. The file is downloaded by the browser, or handed to <see cref="OnExport"/> when the app saves it itself.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListExport<TItem>;

/// <summary>The empty state of a data list: an icon, a title, a description and optional actions. The table and the grid draw it when there are no rows.</summary>
public partial class KlyDataListEmpty;

/// <summary>The error state, shown when the newest load failed. The rows of the previous result, if any, stay in the table under it, and a Retry button loads the request again.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListErrorState<TItem>;

/// <summary>
/// Every filter of the schema as a collapsible section, with the same editors as the filter tags and the add filter menu. Place it anywhere inside a list: next to the table as a sidebar,
/// or inside a <c>KlySlideover</c> on a small screen. A section that has a filter on starts expanded.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListFilterPanel<TItem>;

/// <summary>One tag per active filter ("Status is Active, Paused +2"). A click opens the editor of the filter in a popover, and the remove button takes the filter off.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListFilterTags<TItem>;

/// <summary>
/// The rows of the list as a grid of cards, the alternative to <see cref="KlyDataListTable{TItem}"/>. Place both inside one list with a <see cref="KlyDataListLayoutSwitch{TItem}"/>
/// to let the user choose; each card shows the visible columns as label and value pairs, or an <see cref="ItemTemplate"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListGrid<TItem>;

/// <summary>The header of a group of rows in a table, a grid or a summary list: an optional checkbox that ticks the group, and a button with the label and the summary that folds it.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListGroupHeader<TItem>;

/// <summary>
/// Cursor paging: loads the next page when the user scrolls near the end of the list (<see cref="Auto"/>) or clicks Load more. Statically rendered, the button is a link
/// to the next cursor. Use it with <see cref="DataListPagingMode.Cursor"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListInfiniteLoader<TItem>;

/// <summary>A button group that switches the list between the table and the grid layout. The choice is part of the state of the list, so it goes in the URL.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListLayoutSwitch<TItem>;

/// <summary>Numbered pagination for a list with <see cref="DataListPagingMode.Offset"/> paging: page links or buttons, optional first and last, and a page size picker.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListPagination<TItem>;

/// <summary>A button that loads the rows again. Without interactivity it is a link to the current address, which the server answers by loading the list again.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListRefresh<TItem>;

/// <summary>The range of rows on screen as a live region, with the total when the source knows it, and "No results" (naming the search text, if any) when nothing matches.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListResultCount<TItem>;

/// <summary>
/// The actions of one row: buttons up to <see cref="MaxVisible"/>, then a menu, and below the <c>md</c> breakpoint one button that opens a sheet. The table and the grid place it
/// for you when they have <c>RowActions</c>; use it directly in a template of your own.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListRowActions<TItem>;

/// <summary>The search field of the list. It searches after a pause in the typing (<see cref="Debounce"/>) or on Enter; statically rendered it is a GET form with a search button.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListSearch<TItem>;

/// <summary>
/// The bar of bulk actions on the selected rows: the number of rows ticked, a switch to act on every result, and the buttons of the actions. It appears when rows are ticked
/// and its presence turns the selection on.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListSelectionActions<TItem>;

/// <summary>A menu that picks the sort field and its direction, or restores the default sort. Useful in the grid layout, which has no column headers, and on small screens.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListSortMenu<TItem>;

/// <summary>
/// The table of the list: <see cref="KlyDataListColumn{TItem}"/> children define the columns, and the table adds sorting, selection, row actions, pinned and resizable columns,
/// grouping, a tree, summary footers, virtualization and the compact summary rows of a phone. Without columns it shows one per field of the schema.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListTable<TItem>;

/// <summary>A toggle that switches the table between its columns and the compact summary rows configured by <c>RowSummary</c>. It renders nothing when the table has no row summary.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class KlyDataListViewToggle<TItem>;
