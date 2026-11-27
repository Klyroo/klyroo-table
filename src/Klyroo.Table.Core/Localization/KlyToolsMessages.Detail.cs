namespace Klyroo.Table;

public sealed partial record KlyToolsMessages
{
    /// <summary>Accessible name of the button that opens the detail row of a row. {0} is the label of the row (<c>RowLabel</c> of the schema), or its position (<c>#3</c>) when it has none.</summary>
    public string DetailExpand { get; init; } = "Show details for {0}";

    /// <summary>Accessible name of the button that closes the detail row of a row. {0} is the label of the row, or its position (<c>#3</c>) when it has none.</summary>
    public string DetailCollapse { get; init; } = "Hide details for {0}";

    /// <summary>Header, read by assistive technology only, of the column of the buttons that open the detail rows.</summary>
    public string DetailColumn { get; init; } = "Details";
}
