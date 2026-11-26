namespace Klyroo.Table;

public sealed partial record KlyToolsMessages
{
    /// <summary>Accessible name of the link that covers a row of a table with <c>RowHref</c>. {0} is the label of the row (<c>RowLabel</c> of the schema).</summary>
    public string RowOpen { get; init; } = "Open {0}";

    /// <summary>Accessible name of the link that covers a row that has no label. {0} is the position of the row on the page, from 1.</summary>
    public string RowOpenAt { get; init; } = "Open row {0}";
}
