using System.Globalization;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// A column that shows a field of the schema by its key, <c>&lt;KlyDataListFieldColumn Field="status" /&gt;</c>: the cell shows the text of the field (formatted with <see cref="KlyDataListColumnBase{TItem}.Format"/>)
/// and the header sorts by it when the schema allows. It needs a schema on the list; use <see cref="KlyDataListColumn{TItem, TValue}"/> for a typed value of the row.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class KlyDataListFieldColumn<TItem> : KlyDataListColumnBase<TItem>
{
    /// <summary>The key of the field of the schema.</summary>
    [Parameter, EditorRequired] public string Field { get; set; } = "";

    internal override string? ResolvedKey => !string.IsNullOrEmpty(Key) ? Key : Field;

    internal override RenderFragment<TItem>? CellTemplate => null;

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        schema?.FormatField(item, Field, Format, culture);

    internal override bool CanExport(DataListSchema<TItem>? schema) => true;

    internal override DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        new(ReadText(item, schema, culture), schema?.HasStructuredValueFor(item, Field) == true);

    internal override DataListSummaryValue ComputeSummary(IReadOnlyList<TItem> rows, DataListSchema<TItem>? schema, CultureInfo culture)
    {
        if (Summary == DataListSummaryKind.None || schema is null) return default;
        return DataListSummaryCalculator.ComputeField(Summary, rows, schema, Field, !string.IsNullOrEmpty(SummaryFormat) ? SummaryFormat : Format, culture);
    }

    private protected override void Validate()
    {
        if (string.IsNullOrEmpty(Field))
        {
            throw new InvalidOperationException($"{nameof(KlyDataListFieldColumn<TItem>)} needs a {nameof(Field)}: the key of a field of the schema.");
        }
    }
}
