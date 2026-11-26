using System.Globalization;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// A column whose cell is a template, <c>&lt;KlyDataListTemplateColumn Title="Actions"&gt;...&lt;/KlyDataListTemplateColumn&gt;</c>. It has no value of its own: give it a <see cref="KlyDataListColumnBase{TItem}.Key"/>
/// naming a field of the schema and the header sorts by that field, and a CSV export and a summary read it too.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class KlyDataListTemplateColumn<TItem> : KlyDataListColumnBase<TItem>
{
    /// <summary>The content of a cell.</summary>
    [Parameter, EditorRequired] public RenderFragment<TItem> Template { get; set; } = default!;

    internal override string? ResolvedKey => string.IsNullOrEmpty(Key) ? null : Key;

    internal override RenderFragment<TItem>? CellTemplate => Template;

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        ResolvedKey is { } key ? schema?.FormatField(item, key, Format, culture) : null;

    internal override bool CanExport(DataListSchema<TItem>? schema) =>
        ResolvedKey is { } key && schema?.FieldInfos.Any(f => f.Key == key && !f.IsCollection) == true;

    internal override bool HasStructuredValue(DataListSchema<TItem>? schema) =>
        ResolvedKey is { } key && schema?.FieldInfos.FirstOrDefault(f => f.Key == key)?.HasStructuredValue == true;

    private protected override void Validate()
    {
        if (Template is null)
        {
            throw new InvalidOperationException($"{nameof(KlyDataListTemplateColumn<TItem>)} needs a {nameof(Template)}.");
        }
        if (ResolvedKey is null && (Sortable ?? false))
        {
            throw new InvalidOperationException($"{nameof(KlyDataListTemplateColumn<TItem>)} '{Title}' sorts but has no {nameof(Key)}: a template has no value to sort by.");
        }
    }
}
