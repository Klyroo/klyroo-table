using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// The properties and behavior every column of <see cref="KlyDataListTable{TItem}"/> shares. A column draws nothing itself: the table asks it for the cell of each row.
/// What a cell shows comes from the kind of column: <see cref="KlyDataListColumn{TItem, TValue}"/> reads a typed value of the row (<c>Property</c>),
/// <see cref="KlyDataListFieldColumn{TItem}"/> reads a field of the schema by its key and <see cref="KlyDataListTemplateColumn{TItem}"/> builds a view.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public abstract class KlyDataListColumnBase<TItem> : BindableObject
{
    /// <summary>Identifies <see cref="Key"/>.</summary>
    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(KlyDataListColumnBase<TItem>), null);
    /// <summary>Identifies <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(KlyDataListColumnBase<TItem>), null);
    /// <summary>Identifies <see cref="HeaderTemplate"/>.</summary>
    public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(nameof(HeaderTemplate), typeof(View), typeof(KlyDataListColumnBase<TItem>), null);
    /// <summary>Identifies <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(nameof(Align), typeof(DataListAlign), typeof(KlyDataListColumnBase<TItem>), DataListAlign.Start);
    /// <summary>Identifies <see cref="Format"/>.</summary>
    public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(KlyDataListColumnBase<TItem>), null);
    /// <summary>Identifies <see cref="Sortable"/>.</summary>
    public static readonly BindableProperty SortableProperty = BindableProperty.Create(nameof(Sortable), typeof(bool?), typeof(KlyDataListColumnBase<TItem>), null);
    /// <summary>Identifies <see cref="Order"/>.</summary>
    public static readonly BindableProperty OrderProperty = BindableProperty.Create(nameof(Order), typeof(int), typeof(KlyDataListColumnBase<TItem>), 0);
    /// <summary>Identifies <see cref="Visible"/>.</summary>
    public static readonly BindableProperty VisibleProperty = BindableProperty.Create(nameof(Visible), typeof(bool), typeof(KlyDataListColumnBase<TItem>), true);
    /// <summary>Identifies <see cref="Required"/>.</summary>
    public static readonly BindableProperty RequiredProperty = BindableProperty.Create(nameof(Required), typeof(bool), typeof(KlyDataListColumnBase<TItem>), false);
    /// <summary>Identifies <see cref="Pinned"/>.</summary>
    public static readonly BindableProperty PinnedProperty = BindableProperty.Create(nameof(Pinned), typeof(DataListPin), typeof(KlyDataListColumnBase<TItem>), DataListPin.None);
    /// <summary>Identifies <see cref="Width"/>.</summary>
    public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double?), typeof(KlyDataListColumnBase<TItem>), null);

    /// <summary>The width of a column that has none, in points.</summary>
    public const double DefaultWidth = 160;

    private static int _registrations;

    internal KlyDataListColumnBase()
    {
        RegistrationIndex = Interlocked.Increment(ref _registrations);
    }

    /// <summary>Raised when a property of the column changes, so the table draws it again.</summary>
    internal event EventHandler? Changed;

    /// <summary>
    /// The key of the column, and of the schema field its header sorts by. A <see cref="KlyDataListColumn{TItem, TValue}"/> takes the member path of its <c>Property</c> in camel case when it has none (<c>name</c>, <c>department.name</c>),
    /// a <see cref="KlyDataListFieldColumn{TItem}"/> its field, and a <see cref="KlyDataListTemplateColumn{TItem}"/> needs one to sort.
    /// </summary>
    public string? Key { get => (string?)GetValue(KeyProperty); set => SetValue(KeyProperty, value); }

    /// <summary>The header. Without it, the key written as words (<c>firstName</c> becomes "First name").</summary>
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>The content of the header cell, instead of <see cref="Title"/> and the sort control. A column that sorts but has this view has no sort control.</summary>
    public View? HeaderTemplate { get => (View?)GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }

    /// <summary>Where the text of the column sits. Numbers read best at the end.</summary>
    public DataListAlign Align { get => (DataListAlign)GetValue(AlignProperty); set => SetValue(AlignProperty, value); }

    /// <summary>A format string for the value of a cell (<c>"C2"</c>, <c>"d"</c>), used with the current culture. Ignored by a <see cref="KlyDataListTemplateColumn{TItem}"/> and by a template.</summary>
    public string? Format { get => (string?)GetValue(FormatProperty); set => SetValue(FormatProperty, value); }

    /// <summary>Whether the header sorts. By default the schema decides (a field marked sortable); <see langword="false"/> keeps a sortable field from sorting here. A column cannot sort by what the schema does not allow.</summary>
    public bool? Sortable { get => (bool?)GetValue(SortableProperty); set => SetValue(SortableProperty, value); }

    /// <summary>Where the column sits among the others: columns are sorted by this number, then by the order in which they were declared.</summary>
    public int Order { get => (int)GetValue(OrderProperty); set => SetValue(OrderProperty, value); }

    /// <summary>Whether the table shows the column (true by default). A hidden column stays in the column panel and keeps its place in the order.</summary>
    public bool Visible { get => (bool)GetValue(VisibleProperty); set => SetValue(VisibleProperty, value); }

    /// <summary>When <see langword="true"/>, the column cannot be hidden: <see cref="KlyDataListColumnPanel{TItem}"/> does not list it and <see cref="DataListContext{TItem}.SetColumnVisible"/> ignores it.</summary>
    public bool Required { get => (bool)GetValue(RequiredProperty); set => SetValue(RequiredProperty, value); }

    /// <summary>Default pin edge (<see cref="DataListPin.Start"/> or <see cref="DataListPin.End"/>): a pinned column keeps its place at the start or the end of the row, before or after the others. Users can override it through the column panel.</summary>
    public DataListPin Pinned { get => (DataListPin)GetValue(PinnedProperty); set => SetValue(PinnedProperty, value); }

    /// <summary>Default width in points, or <see langword="null"/> for <see cref="DefaultWidth"/>. The user's width, when set with <see cref="DataListContext{TItem}.SetColumnWidth"/>, wins.</summary>
    public double? Width { get => (double?)GetValue(WidthProperty); set => SetValue(WidthProperty, value); }

    /// <summary>The key of the column: <see cref="Key"/>, or what its kind derives it from.</summary>
    public string? EffectiveKey => ResolvedKey;

    /// <summary>The header of the column: <see cref="Title"/>, or its key written as words.</summary>
    public string EffectiveTitle => ResolvedTitle;

    internal int RegistrationIndex { get; }

    internal abstract string? ResolvedKey { get; }

    internal string ResolvedTitle => Title ?? TableValues.Humanize(ResolvedKey ?? "");

    /// <summary>Whether the column describes a field of the schema when the list has none (a <c>Property</c>).</summary>
    internal virtual bool DescribesField => false;

    /// <summary>Adds the field this column describes to the implicit schema of the list.</summary>
    internal virtual void AddImplicitField(DataListSchemaBuilder<TItem> schema)
    {
    }

    /// <summary>The view of a cell instead of the text of its value; <see langword="null"/> for a column that shows its value.</summary>
    internal abstract View? BuildTemplate(TItem item);

    /// <summary>The text of a cell for a row: the format of the column, with the culture of the user. <see langword="null"/> when the column has no value.</summary>
    internal abstract string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture);

    /// <summary>Whether the column has a value to export.</summary>
    internal abstract bool CanExport(DataListSchema<TItem>? schema);

    /// <summary>The CSV cell of a row: the text, and whether the value writes its own text (a number, date, time, GUID or boolean).</summary>
    internal abstract DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture);

    /// <summary>Throws when the properties of the column do not describe a column.</summary>
    internal abstract void Validate();

    /// <summary>The cell of a row: the template, or the text of the value set in the font and the alignment of the column.</summary>
    internal View CreateCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture)
    {
        if (BuildTemplate(item) is { } template) return template;
        var label = TableSupport.NewLabel(14, KlyTheme.Text);
        label.Text = ReadText(item, schema, culture) ?? "";
        label.LineBreakMode = LineBreakMode.TailTruncation;
        label.HorizontalTextAlignment = TextAlignmentOf(Align);
        return label;
    }

    internal static TextAlignment TextAlignmentOf(DataListAlign align) => align switch
    {
        DataListAlign.Center => TextAlignment.Center,
        DataListAlign.End => TextAlignment.End,
        _ => TextAlignment.Start,
    };

    /// <inheritdoc />
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
