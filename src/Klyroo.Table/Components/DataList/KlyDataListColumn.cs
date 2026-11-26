using System.Globalization;
using System.Linq.Expressions;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// A column that shows a typed value of the row, <c>&lt;KlyDataListColumn Property="@(o =&gt; o.Total)" Format="C2" /&gt;</c>. The row type comes from the list and the value type from the expression, so neither is written.
/// Without a schema on the list the column describes the schema: its key is the member path in camel case (<c>name</c>, <c>department.name</c>) unless <see cref="KlyDataListColumnBase{TItem}.Key"/> says otherwise,
/// and it sorts (give <c>Sortable="false"</c> to turn that off). With a schema the key must be one of its fields to sort.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <typeparam name="TValue">The type of the value of the column.</typeparam>
public sealed class KlyDataListColumn<TItem, TValue> : KlyDataListColumnBase<TItem>
{
    private Expression<Func<TItem, TValue>>? _compiledFrom;
    private Func<TItem, TValue>? _compiled;

    /// <summary>The value of a cell as an expression, <c>@(e =&gt; e.Name)</c>: the cell shows it, formatted with <see cref="KlyDataListColumnBase{TItem}.Format"/>.</summary>
    [Parameter, EditorRequired] public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

    /// <summary>The content of a cell, instead of the formatted value (which still sorts, summarizes and exports).</summary>
    [Parameter] public RenderFragment<TItem>? Template { get; set; }

    /// <summary>Custom synchronous aggregation calculator for this column in the table's summary footer (<c>&lt;tfoot&gt;</c>), receiving the rows of the active <see cref="DataListSummaryScope"/>. A numeric result stays a number (formatted with <see cref="KlyDataListColumnBase{TItem}.SummaryFormat"/>), anything else is shown as text.</summary>
    [Parameter] public Func<IReadOnlyList<TItem>, TValue>? SummaryValue { get; set; }

    internal override string? ResolvedKey => !string.IsNullOrEmpty(Key) ? Key : Property is null ? null : DataListSchema.DefaultFieldKey(Property);

    internal override RenderFragment<TItem>? CellTemplate => Template;

    internal override bool HasCustomSummary => SummaryValue is not null;

    internal override bool DescribesField => Property is not null;

    internal override void AddImplicitField(DataListSchemaBuilder<TItem> schema) =>
        schema.Field(Property, f =>
        {
            if (Key is not null) f.Key(Key);
            f.Sortable(Sortable ?? true);
        });

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        TryRead(item, out var value) ? FormatValue(value, culture) : null;

    internal override bool CanExport(DataListSchema<TItem>? schema) => true;

    internal override DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        TryRead(item, out var value) ? new DataListCsvCell(FormatValue(value, culture), DataListNumbers.IsStructured(value)) : new DataListCsvCell(null, DataListNumbers.IsStructured<TValue>(default!));

    internal override DataListSummaryValue ComputeSummary(IReadOnlyList<TItem> rows, DataListSchema<TItem>? schema, CultureInfo culture)
    {
        var format = !string.IsNullOrEmpty(SummaryFormat) ? SummaryFormat : Format;
        if (SummaryValue is not null) return DataListSummaryCalculator.FromTyped(SummaryValue(rows), format, culture);
        return DataListSummaryCalculator.Compute<TItem, TValue>(Summary, rows, TryRead, format, culture);
    }

    // A null along the path (e.Department.Name with no department) is an empty cell, like the in-memory executor reads it.
    private bool TryRead(TItem item, out TValue value)
    {
        if (!ReferenceEquals(_compiledFrom, Property))
        {
            _compiled = Property.Compile();
            _compiledFrom = Property;
        }
        try
        {
            value = _compiled!(item);
            return true;
        }
        catch (NullReferenceException)
        {
            value = default!;
            return false;
        }
    }

    private protected override void Validate()
    {
        if (Property is null)
        {
            throw new InvalidOperationException($"{nameof(KlyDataListColumn<TItem, TValue>)} needs a {nameof(Property)}: use {nameof(KlyDataListFieldColumn<TItem>)} for a field of the schema or {nameof(KlyDataListTemplateColumn<TItem>)} for a template.");
        }
    }
}
