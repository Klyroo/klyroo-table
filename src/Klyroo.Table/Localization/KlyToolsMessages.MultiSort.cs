namespace Klyroo.Table;

public sealed partial record KlyToolsMessages
{
    /// <summary>Added to the accessible name of a sort button when the list sorts by several columns. {0} is the place of the column in the sort, from 1.</summary>
    public string SortPriority { get; init; } = "sort order {0}";

    /// <summary>Tooltip of a sort button: how to add a column to the sort instead of replacing it.</summary>
    public string SortAppendHint { get; init; } = "Shift+click to add to the sort";

    /// <summary>Text of the sort menu trigger when the list sorts by several fields. {0} is how many.</summary>
    public string SortMultiple { get; init; } = "Sort by {0} fields";

    /// <summary>Heading of the list of the active sorts in the sort menu, in the order they apply.</summary>
    public string SortOrderHeading { get; init; } = "Sort order";

    /// <summary>Heading of the list of the fields that can still be added to the sort in the sort menu.</summary>
    public string SortAddHeading { get; init; } = "Add a sort";

    /// <summary>Accessible name of the button that takes a field out of the sort. {0} is the label of the field.</summary>
    public string SortRemove { get; init; } = "Remove the sort by {0}";

    /// <summary>Accessible name of the button that makes a sort apply earlier. {0} is the label of the field.</summary>
    public string SortMoveEarlier { get; init; } = "Move {0} earlier in the sort";

    /// <summary>Accessible name of the button that makes a sort apply later. {0} is the label of the field.</summary>
    public string SortMoveLater { get; init; } = "Move {0} later in the sort";

    /// <summary>Accessible name of the button that reverses the direction of one sort. {0} is the label of the field.</summary>
    public string SortReverse { get; init; } = "Reverse the direction of {0}";
}
