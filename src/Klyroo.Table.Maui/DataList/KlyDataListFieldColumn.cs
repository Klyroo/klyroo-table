using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// A column that shows a field of the schema by its key: the cell shows the text of the field (formatted with <see cref="KlyDataListColumnBase{TItem}.Format"/>)
/// and the header sorts by it when the schema allows. It needs a schema on the list; use <see cref="KlyDataListColumn{TItem, TValue}"/> for a typed value of the row.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class KlyDataListFieldColumn<TItem> : KlyDataListColumnBase<TItem>
{
    /// <summary>Identifies <see cref="Field"/>.</summary>
    public static readonly BindableProperty FieldProperty = BindableProperty.Create(nameof(Field), typeof(string), typeof(KlyDataListFieldColumn<TItem>), "");

    /// <summary>The key of the field of the schema.</summary>
    public string Field { get => (string)GetValue(FieldProperty); set => SetValue(FieldProperty, value); }

    internal override string? ResolvedKey => !string.IsNullOrEmpty(Key) ? Key : Field;

    internal override View? BuildTemplate(TItem item) => null;

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        schema?.FormatField(item, Field, Format, culture);

    internal override bool CanExport(DataListSchema<TItem>? schema) => true;

    internal override DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        new(ReadText(item, schema, culture), schema?.HasStructuredValueFor(item, Field) == true);

    internal override void Validate()
    {
        if (string.IsNullOrEmpty(Field))
        {
            throw new InvalidOperationException($"{nameof(KlyDataListFieldColumn<TItem>)} needs a {nameof(Field)}: the key of a field of the schema.");
        }
    }
}
