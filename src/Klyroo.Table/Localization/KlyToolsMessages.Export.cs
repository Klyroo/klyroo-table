namespace Klyroo.Table;

public sealed partial record KlyToolsMessages
{
    /// <summary>Label of the button that exports a data list as a CSV file.</summary>
    public string ExportLabel { get; init; } = "Export";

    /// <summary>Menu entry that exports the rows of the current page.</summary>
    public string ExportPage { get; init; } = "This page";

    /// <summary>Menu entry that exports the selected rows. {0} is how many are selected.</summary>
    public string ExportSelection { get; init; } = "Selected rows ({0})";

    /// <summary>Menu entry that exports every result of the search and the filters. {0} is how many results match.</summary>
    public string ExportAll { get; init; } = "All {0} results";

    /// <summary>Menu entry that exports every result when the source does not say how many there are.</summary>
    public string ExportAllUnknown { get; init; } = "All results";

    /// <summary>Shown under the export button when the export holds more rows than <c>MaxRows</c> allows. {0} is how many rows it would hold, {1} the limit.</summary>
    public string ExportTooLarge { get; init; } = "Too many rows to export: {0} results, the limit is {1}. Narrow the search or the filters.";
}
