using System.Globalization;
using System.Linq.Expressions;

namespace Klyroo.Table.Maui;

/// <summary>
/// A column that shows a typed value of the row, <c>new KlyDataListColumn&lt;Order, decimal&gt; { Property = o =&gt; o.Total, Format = "C2" }</c>. The row type comes from the list and the value type from the expression.
/// Without a schema on the list the column describes the schema: its key is the member path in camel case (<c>name</c>, <c>department.name</c>) unless <see cref="KlyDataListColumnBase{TItem}.Key"/> says otherwise,
/// and it sorts (set <c>Sortable = false</c> to turn that off). With a schema the key must be one of its fields to sort.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <typeparam name="TValue">The type of the value of the column.</typeparam>
public sealed class KlyDataListColumn<TItem, TValue> : KlyDataListColumnBase<TItem>
{
    /// <summary>Identifies <see cref="Property"/>.</summary>
    public static readonly BindableProperty PropertyProperty = BindableProperty.Create(nameof(Property), typeof(Expression<Func<TItem, TValue>>), typeof(KlyDataListColumn<TItem, TValue>), null);
    /// <summary>Identifies <see cref="Template"/>.</summary>
    public static readonly BindableProperty TemplateProperty = BindableProperty.Create(nameof(Template), typeof(Func<TItem, View>), typeof(KlyDataListColumn<TItem, TValue>), null);

    private Expression<Func<TItem, TValue>>? _compiledFrom;
    private Func<TItem, TValue>? _compiled;

    /// <summary>The value of a cell as an expression, <c>o =&gt; o.Name</c>: the cell shows it, formatted with <see cref="KlyDataListColumnBase{TItem}.Format"/>.</summary>
    public Expression<Func<TItem, TValue>>? Property { get => (Expression<Func<TItem, TValue>>?)GetValue(PropertyProperty); set => SetValue(PropertyProperty, value); }

    /// <summary>Builds the cell from the row, instead of the formatted value (which still sorts and exports).</summary>
    public Func<TItem, View>? Template { get => (Func<TItem, View>?)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }

    internal override string? ResolvedKey => !string.IsNullOrEmpty(Key) ? Key : Property is null ? null : DataListSchema.DefaultFieldKey(Property);

    internal override bool DescribesField => Property is not null;

    internal override void AddImplicitField(DataListSchemaBuilder<TItem> schema) =>
        schema.Field(Property!, f =>
        {
            if (Key is not null) f.Key(Key);
            f.Sortable(Sortable ?? true);
        });

    internal override View? BuildTemplate(TItem item) => Template?.Invoke(item);

    internal override string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        TryRead(item, out var value) ? TableValues.FormatValue(value, Format, culture) : null;

    internal override bool CanExport(DataListSchema<TItem>? schema) => Property is not null;

    internal override DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture) =>
        TryRead(item, out var value)
            ? new DataListCsvCell(TableValues.FormatValue(value, Format, culture), TableValues.IsStructured(value, typeof(TValue)))
            : new DataListCsvCell(null, TableValues.IsStructured(typeof(TValue)));

    internal override void Validate()
    {
        if (Property is null)
        {
            throw new InvalidOperationException($"{nameof(KlyDataListColumn<TItem, TValue>)} needs a {nameof(Property)}: use {nameof(KlyDataListFieldColumn<TItem>)} for a field of the schema or {nameof(KlyDataListTemplateColumn<TItem>)} for a template.");
        }
    }

    // A null along the path (o.Department.Name with no department) is an empty cell, like the in-memory executor reads it.
    private bool TryRead(TItem item, out TValue value)
    {
        var property = Property;
        if (property is null)
        {
            value = default!;
            return false;
        }
        if (!ReferenceEquals(_compiledFrom, property))
        {
            _compiled = property.Compile();
            _compiledFrom = property;
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
}
