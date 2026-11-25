using Klyroo.UI.Theming;

namespace Klyroo.Table;

/// <summary>Per-instance class overrides of the slots of the data list parts (<see cref="KlyDataList{TItem}"/>, <see cref="KlyDataListTable{TItem}"/>, <see cref="KlyDataListPagination{TItem}"/>...).</summary>
public sealed record DataListUi : ISlotOverrides
{
    /// <summary>Creates an override set that changes no slot.</summary>
    public DataListUi() { }

    /// <summary>The outer element of the list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Root { get; init; }
    /// <summary>The row that holds the search field, the result count and the refresh button in the default layout. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Toolbar { get; init; }
    /// <summary>The search form of <see cref="KlyDataListSearch{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Search { get; init; }
    /// <summary>The wrapper around the search input and its clear button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SearchField { get; init; }
    /// <summary>The search input. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SearchInput { get; init; }
    /// <summary>The button that clears the search text. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SearchClear { get; init; }
    /// <summary>The icon of the clear button of the search field. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SearchClearIcon { get; init; }
    /// <summary>The submit button of the search form, shown in a statically rendered list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SearchSubmit { get; init; }
    /// <summary>The text that gives the number of results. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ResultCount { get; init; }
    /// <summary>The refresh button of <see cref="KlyDataListRefresh{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Refresh { get; init; }
    /// <summary>The icon of the refresh button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RefreshIcon { get; init; }
    /// <summary>The scrolling region around the table. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TableRoot { get; init; }
    /// <summary>The <c>table</c> element. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Table { get; init; }
    /// <summary>The visually hidden caption of the table. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Caption { get; init; }
    /// <summary>The table header section. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Thead { get; init; }
    /// <summary>A table row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Tr { get; init; }
    /// <summary>A header cell. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Th { get; init; }
    /// <summary>The button of a header cell that sorts by its column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortButton { get; init; }
    /// <summary>The sort direction icon of a header cell. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortIcon { get; init; }
    /// <summary>The table body. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Tbody { get; init; }
    /// <summary>A body cell. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Td { get; init; }
    /// <summary>The cell that holds the empty state of the table. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Empty { get; init; }
    /// <summary>A placeholder bar shown in a cell while the first rows load. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Skeleton { get; init; }
    /// <summary>The container of <see cref="KlyDataListPagination{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Pagination { get; init; }
    /// <summary>The page size picker with its label. Classes given here are merged with the theme classes of the slot.</summary>
    public string? PageSize { get; init; }
    /// <summary>The label of the page size picker. Classes given here are merged with the theme classes of the slot.</summary>
    public string? PageSizeLabel { get; init; }
    /// <summary>The select of the page size picker. Classes given here are merged with the theme classes of the slot.</summary>
    public string? PageSizeSelect { get; init; }
    /// <summary>The form around the page size picker in a statically rendered list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? PageSizeForm { get; init; }
    /// <summary>The error state shown when a load failed. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Error { get; init; }
    /// <summary>The container of the filter tags and of the add and clear buttons. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Filters { get; init; }
    /// <summary>The list of filter tags. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTags { get; init; }
    /// <summary>One filter tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTag { get; init; }
    /// <summary>The button of a filter tag that opens its editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagButton { get; init; }
    /// <summary>The name of the filter inside a tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagLabel { get; init; }
    /// <summary>The operator text inside a tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagOperator { get; init; }
    /// <summary>The value text inside a tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagValue { get; init; }
    /// <summary>The button that removes a filter from its tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagRemove { get; init; }
    /// <summary>The icon of the remove button of a tag. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterTagRemoveIcon { get; init; }
    /// <summary>The add filter button of <see cref="KlyDataListAddFilter{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? AddFilter { get; init; }
    /// <summary>The clear filters button of <see cref="KlyDataListClearFilters{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ClearFilters { get; init; }
    /// <summary>The popover that holds a filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPopover { get; init; }
    /// <summary>The root of <see cref="KlyDataListFilterEditor{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterEditor { get; init; }
    /// <summary>The name of the filter above its editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterEditorTitle { get; init; }
    /// <summary>A labelled field inside a filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterField { get; init; }
    /// <summary>The label of a field inside a filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterFieldLabel { get; init; }
    /// <summary>The operator select of a filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOperator { get; init; }
    /// <summary>The value input of a filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterInput { get; init; }
    /// <summary>The list of options of an option filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOptions { get; init; }
    /// <summary>One option of an option filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOption { get; init; }
    /// <summary>The link of an option in a statically rendered editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOptionLink { get; init; }
    /// <summary>The count shown next to an option when its filter has facets. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOptionCount { get; init; }
    /// <summary>The checkbox of an option. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterOptionCheck { get; init; }
    /// <summary>The text shown when an option filter has no options. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterEmpty { get; init; }
    /// <summary>The row of Apply, Cancel and Reset buttons of an editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterActions { get; init; }
    /// <summary>The group of operator choices of an editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterChoices { get; init; }
    /// <summary>The menu that lists the filters in <see cref="KlyDataListAddFilter{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterMenu { get; init; }
    /// <summary>One entry of the add filter menu. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterMenuItem { get; init; }
    /// <summary>The header of the add filter menu. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterMenuHeader { get; init; }
    /// <summary>The button that goes back from an editor to the add filter menu. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterBack { get; init; }
    /// <summary>A section of an editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterSection { get; init; }
    /// <summary>The container of the inputs of a range filter. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterRange { get; init; }
    /// <summary>The slider of a number range filter. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterRangeSlider { get; init; }
    /// <summary>The group of preset buttons of a date filter editor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPresets { get; init; }
    /// <summary>The line that reads back the range of a filter. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterHint { get; init; }
    /// <summary>Added to the popover of an editor that needs more room. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPopoverWide { get; init; }
    /// <summary>The root of <see cref="KlyDataListFilterPanel{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanel { get; init; }
    /// <summary>The header of the filter panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelHeader { get; init; }
    /// <summary>The title of the filter panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelTitle { get; init; }
    /// <summary>The count of active filters in the header of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelCount { get; init; }
    /// <summary>The line under the title of the filter panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelDescription { get; init; }
    /// <summary>One collapsible section of the filter panel, one per filter. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelSection { get; init; }
    /// <summary>The button that opens or closes a section of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelTrigger { get; init; }
    /// <summary>The content of a section of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelContent { get; init; }
    /// <summary>The editor inside a section of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelEditor { get; init; }
    /// <summary>The footer of the panel that holds the Apply and Reset buttons. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelFooter { get; init; }
    /// <summary>The text shown when the schema has no filters. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelEmpty { get; init; }
    /// <summary>A native input of the statically rendered filter forms. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterNativeInput { get; init; }
    /// <summary>The label of a native input of the statically rendered filter forms. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterNativeLabel { get; init; }
    /// <summary>The live region that announces the number of results. Classes given here are merged with the theme classes of the slot.</summary>
    public string? FilterPanelResults { get; init; }
    /// <summary>The cell of the selection checkbox column, header and body. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectCell { get; init; }
    /// <summary>Added to a row while it is ticked. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowSelected { get; init; }
    /// <summary>The root of <see cref="KlyDataListSelectionActions{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionActions { get; init; }
    /// <summary>The bar that shows the selection count and the bulk actions. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionBar { get; init; }
    /// <summary>The text that gives the number of selected rows. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionCount { get; init; }
    /// <summary>The live region that announces the selection to assistive technologies. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionLive { get; init; }
    /// <summary>The buttons that switch between the ticked rows and every result. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionScope { get; init; }
    /// <summary>The group of action buttons in the selection bar. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SelectionGroup { get; init; }
    /// <summary>The trigger button of <see cref="KlyDataListActionsDropdown{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ActionsDropdown { get; init; }
    /// <summary>The cell of the row actions column, header and body. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowActionsCell { get; init; }
    /// <summary>The root of <see cref="KlyDataListRowActions{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowActions { get; init; }
    /// <summary>The inline buttons of the row actions. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowActionsInline { get; init; }
    /// <summary>The menu of the row actions that do not fit inline. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowActionsMenu { get; init; }
    /// <summary>The single button that opens the row actions sheet on a phone. Classes given here are merged with the theme classes of the slot.</summary>
    public string? RowActionsPhone { get; init; }
    /// <summary>The list of actions inside the phone sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetList { get; init; }
    /// <summary>The divider between groups of actions in the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetDivider { get; init; }
    /// <summary>One entry of the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetEntry { get; init; }
    /// <summary>The heading of a submenu in the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetHeading { get; init; }
    /// <summary>The button or link of an action in the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetItem { get; init; }
    /// <summary>The text that says why an action is disabled, in the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetReason { get; init; }
    /// <summary>The icon of an action in the sheet. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SheetIcon { get; init; }
    /// <summary>The root of <see cref="KlyDataListInfiniteLoader{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? InfiniteLoader { get; init; }
    /// <summary>The block shown when loading the next page failed. Classes given here are merged with the theme classes of the slot.</summary>
    public string? LoadMoreError { get; init; }
    /// <summary>The message inside that block. Classes given here are merged with the theme classes of the slot.</summary>
    public string? LoadMoreErrorText { get; init; }
    /// <summary>The button that goes back to the first rows when a cursor in the address matches nothing. Classes given here are merged with the theme classes of the slot.</summary>
    public string? CursorReset { get; init; }
    /// <summary>The text shown after the last page has loaded. Classes given here are merged with the theme classes of the slot.</summary>
    public string? InfiniteEnd { get; init; }
    /// <summary>The live region of the Load more button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? InfiniteStatus { get; init; }
    /// <summary>Added to a header cell pinned to the start edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ThPinnedStart { get; init; }
    /// <summary>Added to a header cell pinned to the end edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ThPinnedEnd { get; init; }
    /// <summary>Added to a body cell pinned to the start edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TdPinnedStart { get; init; }
    /// <summary>Added to a body cell pinned to the end edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TdPinnedEnd { get; init; }
    /// <summary>The resize handle at the end edge of a header cell. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColResize { get; init; }
    /// <summary>The root of <see cref="KlyDataListColumnPanel{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsPanel { get; init; }
    /// <summary>The popover of the columns panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsPopover { get; init; }
    /// <summary>The content of the columns popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsContent { get; init; }
    /// <summary>The header of the columns popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsHeader { get; init; }
    /// <summary>The title of the columns popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsTitle { get; init; }
    /// <summary>The count of visible columns in the header of the popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsCount { get; init; }
    /// <summary>The list of columns. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsList { get; init; }
    /// <summary>One column of the list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsItem { get; init; }
    /// <summary>The label of a column, with its checkbox. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsLabel { get; init; }
    /// <summary>The pin and move controls of a column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsActions { get; init; }
    /// <summary>An icon button of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsIconBtn { get; init; }
    /// <summary>Added to an icon button that is on, such as a pinned column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsIconBtnActive { get; init; }
    /// <summary>Added to the cells of a column that has a fixed width. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColSized { get; init; }
    /// <summary>The row with the show all and reset buttons. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsAll { get; init; }
    /// <summary>The hint that says one column must stay visible. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsHint { get; init; }
    /// <summary>The drag grip of a column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsGrip { get; init; }
    /// <summary>The live region that announces column changes. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsLive { get; init; }
    /// <summary>The group of pin buttons of a column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsPinGroup { get; init; }
    /// <summary>The search field of the panel. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ColumnsSearch { get; init; }
    /// <summary>The trigger button of <see cref="KlyDataListSortMenu{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenu { get; init; }
    /// <summary>The popover of the sort menu. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuPopover { get; init; }
    /// <summary>The content of the sort popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuContent { get; init; }
    /// <summary>The header of the sort popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuHeader { get; init; }
    /// <summary>The title of the sort popover. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuTitle { get; init; }
    /// <summary>The group of the ascending and descending buttons. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuDirections { get; init; }
    /// <summary>One direction button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuDirection { get; init; }
    /// <summary>The list of sortable fields. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuList { get; init; }
    /// <summary>One sortable field of the list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SortMenuItem { get; init; }
    /// <summary>The root of <see cref="KlyDataListLayoutSwitch{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? LayoutSwitch { get; init; }
    /// <summary>One button of the layout switch. Classes given here are merged with the theme classes of the slot.</summary>
    public string? LayoutOption { get; init; }
    /// <summary>The root of <see cref="KlyDataListGrid{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridRoot { get; init; }
    /// <summary>The header row of the grid that holds the select all box. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridHeader { get; init; }
    /// <summary>The container of the cards. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Grid { get; init; }
    /// <summary>One card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCard { get; init; }
    /// <summary>The top of a card: selection box and row actions. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardTop { get; init; }
    /// <summary>The body of a card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardBody { get; init; }
    /// <summary>The list of fields of a card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardFields { get; init; }
    /// <summary>One field of a card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardField { get; init; }
    /// <summary>The label of a field of a card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardFieldLabel { get; init; }
    /// <summary>The value of a field of a card. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridCardFieldValue { get; init; }
    /// <summary>The container of the empty state of the grid. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridEmpty { get; init; }
    /// <summary>The button of <see cref="KlyDataListViewToggle{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ViewToggle { get; init; }
    /// <summary>The header of the summary row list that holds the select all box. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryHeader { get; init; }
    /// <summary>The list of summary rows. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryList { get; init; }
    /// <summary>One summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryRow { get; init; }
    /// <summary>The leading content of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryLeading { get; init; }
    /// <summary>The text block of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryBody { get; init; }
    /// <summary>The title of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryTitle { get; init; }
    /// <summary>The title of a summary row when it is a link. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryTitleLink { get; init; }
    /// <summary>The title of a summary row when it is a button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryTitleButton { get; init; }
    /// <summary>The second line of a summary row: status and details. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryMeta { get; init; }
    /// <summary>The status content of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryStatus { get; init; }
    /// <summary>The details text of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryDetails { get; init; }
    /// <summary>The row actions of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryActions { get; init; }
    /// <summary>The group header row of the table. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupRow { get; init; }
    /// <summary>The cell of a group header row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupCell { get; init; }
    /// <summary>The group header in the summary row list. Classes given here are merged with the theme classes of the slot.</summary>
    public string? SummaryGroupRow { get; init; }
    /// <summary>The container of one group of cards. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridGroup { get; init; }
    /// <summary>The header of one group of cards. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GridGroupHeader { get; init; }
    /// <summary>The root of <see cref="KlyDataListGroupHeader{TItem}"/>. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupHeader { get; init; }
    /// <summary>The fold button of a group header. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupToggle { get; init; }
    /// <summary>The chevron of the fold button. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupChevron { get; init; }
    /// <summary>Added to the chevron while the group is open. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupChevronExpanded { get; init; }
    /// <summary>The label of a group. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupLabel { get; init; }
    /// <summary>The summary badge beside the label of a group. Classes given here are merged with the theme classes of the slot.</summary>
    public string? GroupSummary { get; init; }
    /// <summary>The cell that holds the tree rails, the toggle and the content. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeCell { get; init; }
    /// <summary>A rail segment with no line. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeRailBlank { get; init; }
    /// <summary>A rail segment with a vertical line that continues to the next sibling of an ancestor. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeRailPass { get; init; }
    /// <summary>A rail segment with a branch to a node that has later siblings. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeRailBranch { get; init; }
    /// <summary>A rail segment with a branch to the last node of its level. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeRailLast { get; init; }
    /// <summary>The expand and collapse button of a branch node. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeToggle { get; init; }
    /// <summary>The chevron of the toggle. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeChevron { get; init; }
    /// <summary>Added to the chevron while the node is open. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeChevronExpanded { get; init; }
    /// <summary>The spacer that takes the place of the toggle on a leaf node. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeSpacer { get; init; }
    /// <summary>The cell content next to the toggle. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TreeContent { get; init; }
    /// <summary>The table footer section that holds the summary rows. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Tfoot { get; init; }
    /// <summary>A summary row of the footer. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TfootRow { get; init; }
    /// <summary>A cell of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TfootCell { get; init; }
    /// <summary>Added to a footer cell pinned to the start edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TfootCellPinnedStart { get; init; }
    /// <summary>Added to a footer cell pinned to the end edge. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TfootCellPinnedEnd { get; init; }
    /// <summary>The label of a summary row. Classes given here are merged with the theme classes of the slot.</summary>
    public string? TfootLabel { get; init; }
    /// <summary>The progress indicator drawn at the top of the table while rows reload. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ProgressLine { get; init; }
    /// <summary>The moving bar inside the progress indicator. Classes given here are merged with the theme classes of the slot.</summary>
    public string? ProgressLineBar { get; init; }
    /// <summary>The content wrapper of a header cell. Classes given here are merged with the theme classes of the slot.</summary>
    public string? HeaderCell { get; init; }
    /// <summary>The wrapper of the header menu of a column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? HeaderMenu { get; init; }
    /// <summary>The button that opens the header menu of a column. Classes given here are merged with the theme classes of the slot.</summary>
    public string? HeaderMenuTrigger { get; init; }

    string? ISlotOverrides.For(string slot) => slot switch
    {
        "root" => Root,
        "toolbar" => Toolbar,
        "search" => Search,
        "searchField" => SearchField,
        "searchInput" => SearchInput,
        "searchClear" => SearchClear,
        "searchClearIcon" => SearchClearIcon,
        "searchSubmit" => SearchSubmit,
        "resultCount" => ResultCount,
        "refresh" => Refresh,
        "refreshIcon" => RefreshIcon,
        "tableRoot" => TableRoot,
        "table" => Table,
        "caption" => Caption,
        "thead" => Thead,
        "tr" => Tr,
        "th" => Th,
        "sortButton" => SortButton,
        "sortIcon" => SortIcon,
        "tbody" => Tbody,
        "td" => Td,
        "empty" => Empty,
        "skeleton" => Skeleton,
        "pagination" => Pagination,
        "pageSize" => PageSize,
        "pageSizeLabel" => PageSizeLabel,
        "pageSizeSelect" => PageSizeSelect,
        "pageSizeForm" => PageSizeForm,
        "error" => Error,
        "filters" => Filters,
        "filterTags" => FilterTags,
        "filterTag" => FilterTag,
        "filterTagButton" => FilterTagButton,
        "filterTagLabel" => FilterTagLabel,
        "filterTagOperator" => FilterTagOperator,
        "filterTagValue" => FilterTagValue,
        "filterTagRemove" => FilterTagRemove,
        "filterTagRemoveIcon" => FilterTagRemoveIcon,
        "addFilter" => AddFilter,
        "clearFilters" => ClearFilters,
        "filterPopover" => FilterPopover,
        "filterEditor" => FilterEditor,
        "filterEditorTitle" => FilterEditorTitle,
        "filterField" => FilterField,
        "filterFieldLabel" => FilterFieldLabel,
        "filterOperator" => FilterOperator,
        "filterInput" => FilterInput,
        "filterOptions" => FilterOptions,
        "filterOption" => FilterOption,
        "filterOptionLink" => FilterOptionLink,
        "filterOptionCount" => FilterOptionCount,
        "filterOptionCheck" => FilterOptionCheck,
        "filterEmpty" => FilterEmpty,
        "filterActions" => FilterActions,
        "filterChoices" => FilterChoices,
        "filterMenu" => FilterMenu,
        "filterMenuItem" => FilterMenuItem,
        "filterMenuHeader" => FilterMenuHeader,
        "filterBack" => FilterBack,
        "filterSection" => FilterSection,
        "filterRange" => FilterRange,
        "filterRangeSlider" => FilterRangeSlider,
        "filterPresets" => FilterPresets,
        "filterHint" => FilterHint,
        "filterPopoverWide" => FilterPopoverWide,
        "filterPanel" => FilterPanel,
        "filterPanelHeader" => FilterPanelHeader,
        "filterPanelTitle" => FilterPanelTitle,
        "filterPanelCount" => FilterPanelCount,
        "filterPanelDescription" => FilterPanelDescription,
        "filterPanelSection" => FilterPanelSection,
        "filterPanelTrigger" => FilterPanelTrigger,
        "filterPanelContent" => FilterPanelContent,
        "filterPanelEditor" => FilterPanelEditor,
        "filterPanelFooter" => FilterPanelFooter,
        "filterPanelEmpty" => FilterPanelEmpty,
        "filterNativeInput" => FilterNativeInput,
        "filterNativeLabel" => FilterNativeLabel,
        "filterPanelResults" => FilterPanelResults,
        "selectCell" => SelectCell,
        "rowSelected" => RowSelected,
        "selectionActions" => SelectionActions,
        "selectionBar" => SelectionBar,
        "selectionCount" => SelectionCount,
        "selectionLive" => SelectionLive,
        "selectionScope" => SelectionScope,
        "selectionGroup" => SelectionGroup,
        "actionsDropdown" => ActionsDropdown,
        "rowActionsCell" => RowActionsCell,
        "rowActions" => RowActions,
        "rowActionsInline" => RowActionsInline,
        "rowActionsMenu" => RowActionsMenu,
        "rowActionsPhone" => RowActionsPhone,
        "sheetList" => SheetList,
        "sheetDivider" => SheetDivider,
        "sheetEntry" => SheetEntry,
        "sheetHeading" => SheetHeading,
        "sheetItem" => SheetItem,
        "sheetReason" => SheetReason,
        "sheetIcon" => SheetIcon,
        "infiniteLoader" => InfiniteLoader,
        "loadMoreError" => LoadMoreError,
        "loadMoreErrorText" => LoadMoreErrorText,
        "cursorReset" => CursorReset,
        "infiniteEnd" => InfiniteEnd,
        "infiniteStatus" => InfiniteStatus,
        "thPinnedStart" => ThPinnedStart,
        "thPinnedEnd" => ThPinnedEnd,
        "tdPinnedStart" => TdPinnedStart,
        "tdPinnedEnd" => TdPinnedEnd,
        "colResize" => ColResize,
        "columnsPanel" => ColumnsPanel,
        "columnsPopover" => ColumnsPopover,
        "columnsContent" => ColumnsContent,
        "columnsHeader" => ColumnsHeader,
        "columnsTitle" => ColumnsTitle,
        "columnsCount" => ColumnsCount,
        "columnsList" => ColumnsList,
        "columnsItem" => ColumnsItem,
        "columnsLabel" => ColumnsLabel,
        "columnsActions" => ColumnsActions,
        "columnsIconBtn" => ColumnsIconBtn,
        "columnsIconBtnActive" => ColumnsIconBtnActive,
        "colSized" => ColSized,
        "columnsAll" => ColumnsAll,
        "columnsHint" => ColumnsHint,
        "columnsGrip" => ColumnsGrip,
        "columnsLive" => ColumnsLive,
        "columnsPinGroup" => ColumnsPinGroup,
        "columnsSearch" => ColumnsSearch,
        "sortMenu" => SortMenu,
        "sortMenuPopover" => SortMenuPopover,
        "sortMenuContent" => SortMenuContent,
        "sortMenuHeader" => SortMenuHeader,
        "sortMenuTitle" => SortMenuTitle,
        "sortMenuDirections" => SortMenuDirections,
        "sortMenuDirection" => SortMenuDirection,
        "sortMenuList" => SortMenuList,
        "sortMenuItem" => SortMenuItem,
        "layoutSwitch" => LayoutSwitch,
        "layoutOption" => LayoutOption,
        "gridRoot" => GridRoot,
        "gridHeader" => GridHeader,
        "grid" => Grid,
        "gridCard" => GridCard,
        "gridCardTop" => GridCardTop,
        "gridCardBody" => GridCardBody,
        "gridCardFields" => GridCardFields,
        "gridCardField" => GridCardField,
        "gridCardFieldLabel" => GridCardFieldLabel,
        "gridCardFieldValue" => GridCardFieldValue,
        "gridEmpty" => GridEmpty,
        "viewToggle" => ViewToggle,
        "summaryHeader" => SummaryHeader,
        "summaryList" => SummaryList,
        "summaryRow" => SummaryRow,
        "summaryLeading" => SummaryLeading,
        "summaryBody" => SummaryBody,
        "summaryTitle" => SummaryTitle,
        "summaryTitleLink" => SummaryTitleLink,
        "summaryTitleButton" => SummaryTitleButton,
        "summaryMeta" => SummaryMeta,
        "summaryStatus" => SummaryStatus,
        "summaryDetails" => SummaryDetails,
        "summaryActions" => SummaryActions,
        "groupRow" => GroupRow,
        "groupCell" => GroupCell,
        "summaryGroupRow" => SummaryGroupRow,
        "gridGroup" => GridGroup,
        "gridGroupHeader" => GridGroupHeader,
        "groupHeader" => GroupHeader,
        "groupToggle" => GroupToggle,
        "groupChevron" => GroupChevron,
        "groupChevronExpanded" => GroupChevronExpanded,
        "groupLabel" => GroupLabel,
        "groupSummary" => GroupSummary,
        "treeCell" => TreeCell,
        "treeRailBlank" => TreeRailBlank,
        "treeRailPass" => TreeRailPass,
        "treeRailBranch" => TreeRailBranch,
        "treeRailLast" => TreeRailLast,
        "treeToggle" => TreeToggle,
        "treeChevron" => TreeChevron,
        "treeChevronExpanded" => TreeChevronExpanded,
        "treeSpacer" => TreeSpacer,
        "treeContent" => TreeContent,
        "tfoot" => Tfoot,
        "tfootRow" => TfootRow,
        "tfootCell" => TfootCell,
        "tfootCellPinnedStart" => TfootCellPinnedStart,
        "tfootCellPinnedEnd" => TfootCellPinnedEnd,
        "tfootLabel" => TfootLabel,
        "progressLine" => ProgressLine,
        "progressLineBar" => ProgressLineBar,
        "headerCell" => HeaderCell,
        "headerMenu" => HeaderMenu,
        "headerMenuTrigger" => HeaderMenuTrigger,
        _ => null,
    };
}
