using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// A column whose cell is a view built from the row. It has no value of its own: give it a <see cref="KlyDataListColumnBase{TItem}.Key"/>
/// naming a field of the schema and the header sorts by that field, and a CSV export reads it too.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class KlyDataListTemplateColumn<TItem> : KlyDataListColumnBase<TItem>
{
    /// <summary>Identifies <see cref="Template"/>.</summary>
    public static readonly BindableProperty TemplateProperty = BindableProperty.Create(nameof(Template), typeof(Func<TItem, View>), typeof(KlyDataListTemplateColumn<TItem>), null);

    /// <summary>Builds the cell of a row.</summary>
    public Func<TItem, View>? Template { get => (Func<TItem, View>?)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }

    internal override string? ResolvedKey => string.IsNullOrEmpty(Key) ? null : Key;

    internal override View? BuildTemplate(TItem item) => Template?.Invoke(item);

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        ResolvedKey is { } key ? schema?.FormatField(item, key, Format, culture) : null;

    internal override bool CanExport(DataListSchema<TItem>? schema) =>
        ResolvedKey is { } key && schema?.FieldInfos.Any(f => f.Key == key && !f.IsCollection) == true;

    internal override DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        new(ReadText(item, schema, culture), ResolvedKey is { } key && schema?.HasStructuredValueFor(item, key) == true);

    internal override void Validate()
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
