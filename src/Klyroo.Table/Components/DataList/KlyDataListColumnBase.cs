using System.Globalization;
using System.Linq.Expressions;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// The parameters and behavior every column of <see cref="KlyDataListTable{TItem}"/> shares. A column renders nothing itself: it registers with its list (<see cref="DataListContext{TItem}.Columns"/>).
/// What a cell shows comes from the kind of column: <see cref="KlyDataListColumn{TItem, TValue}"/> reads a typed value of the row (<c>Property</c>),
/// <see cref="KlyDataListFieldColumn{TItem}"/> reads a field of the schema by its key and <see cref="KlyDataListTemplateColumn{TItem}"/> renders a template.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public abstract class KlyDataListColumnBase<TItem> : ComponentBase, IDisposable
{
    internal KlyDataListColumnBase() { }

    // The list: columns register with it, not with a table, so a columns panel can see them.
    [CascadingParameter] private DataListContext<TItem>? Context { get; set; }

    /// <summary>
    /// The key of the column, and of the schema field its header sorts by. A <see cref="KlyDataListColumn{TItem, TValue}"/> takes the member path of its <c>Property</c> in camel case when it has none (<c>name</c>, <c>department.name</c>),
    /// a <see cref="KlyDataListFieldColumn{TItem}"/> its field, and a <see cref="KlyDataListTemplateColumn{TItem}"/> needs one to sort.
    /// </summary>
    [Parameter] public string? Key { get; set; }

    /// <summary>The header. Without it, the key written as words (<c>firstName</c> becomes "First name").</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// The content of the header cell, instead of <see cref="Title"/> and the sort control: a select-all checkbox, or an accessible name that is not shown (an actions column).
    /// It renders inside the list, so a part that derives from <see cref="DataListPartBase{TItem}"/> works in it. A column that sorts but has this template has no sort control.
    /// </summary>
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Where the text of the column sits. Numbers read best at the end.</summary>
    [Parameter] public DataListAlign Align { get; set; }

    /// <summary>A format string for the value of a cell (<c>"C2"</c>, <c>"d"</c>), used with the culture of the page. Ignored by a <see cref="KlyDataListTemplateColumn{TItem}"/> and by a template.</summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// Whether the header sorts. By default the schema decides (a field marked sortable); <see langword="false"/> keeps a sortable field from sorting here. A column cannot sort by what the schema does not allow.
    /// </summary>
    [Parameter] public bool? Sortable { get; set; }

    /// <summary>Classes added to the header and the cells of this column.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Classes added to the cell of this column for a row (a color for an overdue date, a muted style for an archived row): it receives the row. The header is not affected; use <see cref="Class"/> for both. It runs on every render of the row: keep it cheap.</summary>
    [Parameter] public Func<TItem, string?>? CellClass { get; set; }

    /// <summary>
    /// Where the column sits among the others: columns are sorted by this number, then by the order in which they first rendered (the order of the markup for columns that
    /// exist from the start). A column that an <c>@if</c> adds later renders after the others unless it has an <c>Order</c>; <see cref="Visible"/> avoids the question.
    /// </summary>
    [Parameter] public int Order { get; set; }

    /// <summary>
    /// Whether the table shows the column (true by default). A hidden column stays in <see cref="DataListContext{TItem}.Columns"/> and keeps its place in the order, which
    /// is why this is the way to show and hide columns; an <c>@if</c> around a column works too, but the column then registers at its place in the markup only while it exists.
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

    /// <summary>When <see langword="true"/>, the column cannot be hidden: <see cref="KlyDataListColumnPanel{TItem}"/> does not list it and <see cref="DataListContext{TItem}.SetColumnVisible"/> ignores it. It cannot be reordered or pinned from the panel either.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Default horizontal pin edge (<see cref="DataListPin.Start"/> or <see cref="DataListPin.End"/>). Users can override it through the column panel. A pinned column sticks from the <c>sm</c> breakpoint up (below it
    /// pinned columns scroll with the table but keep their place in the order), after the selection column and before the row actions column, and needs a width: without <see cref="Width"/> it gets 150 px.
    /// </summary>
    [Parameter] public DataListPin Pinned { get; set; }

    /// <summary>
    /// Default width in pixels, clamped to [<see cref="MinWidth"/>, <see cref="MaxWidth"/>]. <see langword="null"/> lets the browser size the column to its content. A column with a width keeps it whatever its content
    /// (a longer text is cut with an ellipsis); when every column has one the table is as wide as their sum, otherwise the columns without a width share the room that is left. Rendered on the <c>&lt;col&gt;</c> and on every cell,
    /// so a static render has the width at the first paint. The user's width (the column resize, kept in the preferences) wins.
    /// </summary>
    [Parameter] public int? Width { get; set; }

    /// <summary>Minimum width in pixels when resized (64 by default).</summary>
    [Parameter] public int MinWidth { get; set; } = DataListColumnPreferences.MinAllowedWidth;

    /// <summary>Maximum width in pixels when resized (900 by default).</summary>
    [Parameter] public int MaxWidth { get; set; } = DataListColumnPreferences.MaxAllowedWidth;

    /// <summary>Whether the user can resize the column by dragging its header handle or using the keyboard (true by default). Only when <see cref="KlyDataListTable{TItem}.Resizable"/> is on.</summary>
    [Parameter] public bool Resizable { get; set; } = true;

    /// <summary>Built-in aggregation shown for this column in the table's summary footer (<c>&lt;tfoot&gt;</c>): <see cref="DataListSummaryKind.Sum"/>, <see cref="DataListSummaryKind.Average"/>, <see cref="DataListSummaryKind.Count"/>, <see cref="DataListSummaryKind.Min"/> or <see cref="DataListSummaryKind.Max"/>.</summary>
    [Parameter] public DataListSummaryKind Summary { get; set; }

    /// <summary>Format string applied to the summary footer value of this column. When <see langword="null"/>, falls back to <see cref="Format"/> (or the localized row count for <see cref="DataListSummaryKind.Count"/>).</summary>
    [Parameter] public string? SummaryFormat { get; set; }

    /// <summary>Custom template for rendering the summary footer cell (<c>&lt;tfoot&gt;</c>) of this column.</summary>
    [Parameter] public RenderFragment<DataListSummaryCellContext<TItem>>? SummaryTemplate { get; set; }

    /// <summary>Placeholder shape rendered inside this column's cells while the initial rows load (<see cref="DataListSkeletonKind.Text"/> by default).</summary>
    [Parameter] public DataListSkeletonKind Skeleton { get; set; } = DataListSkeletonKind.Text;

    /// <summary>Overrides <see cref="KlyDataListTable{TItem}.HeaderMenu"/> for this column: <see langword="true"/> shows the header actions menu (sort, pin, hide), <see langword="false"/> omits it.</summary>
    [Parameter] public bool? HeaderMenu { get; set; }

    internal bool HasSummary => Summary != DataListSummaryKind.None || HasCustomSummary || SummaryTemplate is not null;

    /// <summary>The key of the column: <see cref="Key"/>, or what its kind derives it from.</summary>
    public string? EffectiveKey => ResolvedKey;

    /// <summary>The header of the column: <see cref="Title"/>, or its key written as words.</summary>
    public string EffectiveTitle => ResolvedTitle;

    internal abstract string? ResolvedKey { get; }

    internal string ResolvedTitle => Title ?? DataListText.Humanize(ResolvedKey ?? "");

    /// <summary>The content of a cell instead of the text of its value; <see langword="null"/> for a column that shows its value.</summary>
    internal abstract RenderFragment<TItem>? CellTemplate { get; }

    /// <summary>Whether the column declares a summary of its own beyond <see cref="Summary"/> and <see cref="SummaryTemplate"/>.</summary>
    internal virtual bool HasCustomSummary => false;

    /// <summary>Whether the column describes a field of the schema when the list has none (a <c>Property</c>).</summary>
    internal virtual bool DescribesField => false;

    /// <summary>Adds the field this column describes to the implicit schema of the list.</summary>
    internal virtual void AddImplicitField(DataListSchemaBuilder<TItem> schema) { }

    /// <summary>The text of a cell for a row: the format of the column, with the culture of the page. <see langword="null"/> when the column has no value.</summary>
    internal abstract string? ReadText(TItem item, DataListSchema<TItem>? schema, CultureInfo culture);

    /// <summary>Whether the column has a value to export.</summary>
    internal abstract bool CanExport(DataListSchema<TItem>? schema);

    /// <summary>The CSV cell of a row: the text, and whether the value is a number, date, time, GUID or boolean (the type writes its text, so a CSV export does not guard it against a formula). For a column declared <see cref="object"/> the runtime value decides.</summary>
    internal abstract DataListCsvCell ReadCell(TItem item, DataListSchema<TItem>? schema, CultureInfo culture);

    /// <summary>The summary of the column over <paramref name="rows"/>: the custom aggregation, or the <see cref="Summary"/> kind. Empty when there is none.</summary>
    internal virtual DataListSummaryValue ComputeSummary(IReadOnlyList<TItem> rows, DataListSchema<TItem>? schema, CultureInfo culture)
    {
        if (Summary == DataListSummaryKind.None || ResolvedKey is not { } key || schema is null) return default;
        return DataListSummaryCalculator.ComputeField(Summary, rows, schema, key, !string.IsNullOrEmpty(SummaryFormat) ? SummaryFormat : Format, culture);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Context is null)
        {
            throw new InvalidOperationException($"{GetType().Name.Split('`')[0]} must be placed inside a {nameof(KlyDataListTable<TItem>)}.");
        }
        Validate();
        _registeredWith = Context;
        _index ??= Context.NextColumnIndex();
        Context.AddColumn(this);
    }

    /// <summary>Throws when the parameters of the column do not describe a column.</summary>
    private protected abstract void Validate();

    private DataListContext<TItem>? _registeredWith;
    private int? _index;

    internal int RegistrationIndex => _index ?? int.MaxValue;

    // The table reads the values of its columns when it renders, after them: a column never renders by itself.
    /// <inheritdoc />
    protected override bool ShouldRender() => false;

    /// <summary>The text of a typed value: the format of the column, with the culture of the page.</summary>
    internal string? FormatValue<TValue>(TValue value, CultureInfo culture) => value switch
    {
        null => null,
        IFormattable formattable => formattable.ToString(string.IsNullOrEmpty(Format) ? null : Format, culture),
        _ => value.ToString(),
    };

    /// <summary>The text of a summary footer cell for an aggregated value.</summary>
    internal string? FormatSummaryValue(DataListSummaryValue value, CultureInfo culture, KlyToolsMessages messages)
    {
        if (value.IsEmpty) return null;
        if (value.Text is { } text) return text;
        var number = value.Number!.Value;
        if (Summary == DataListSummaryKind.Count)
        {
            // A count is a number of rows, not a value of the column: the Format of the column (a date format, say) does not apply, only a SummaryFormat does.
            if (string.IsNullOrEmpty(SummaryFormat) && number == Math.Truncate(number) && number is >= 0 and <= int.MaxValue)
            {
                var count = (int)number;
                return string.Format(culture, count == 1 ? messages.SummaryRowOne : messages.SummaryRowOther, count);
            }
            return DataListSummaryCalculator.FormatNumber(number, SummaryFormat, culture);
        }
        return DataListSummaryCalculator.FormatNumber(number, !string.IsNullOrEmpty(SummaryFormat) ? SummaryFormat : Format, culture);
    }

    /// <summary>Takes the column out of its table.</summary>
    public void Dispose() => _registeredWith?.RemoveColumn(this);
}

/// <summary>Small text helpers of the data list.</summary>
internal static class DataListText
{
    /// <summary>A key written as words: <c>firstName</c> becomes "First name", <c>department.name</c> becomes "Name" (the last segment).</summary>
    public static string Humanize(string key)
    {
        if (key.Length == 0) return "";
        var segment = key[(key.LastIndexOf('.') + 1)..];
        var words = new System.Text.StringBuilder(segment.Length + 4);
        for (var i = 0; i < segment.Length; i++)
        {
            var c = segment[i];
            if (c is '_' or '-')
            {
                words.Append(' ');
                continue;
            }
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(segment[i - 1]) && segment[i - 1] is not ('_' or '-')) words.Append(' ');
            words.Append(i == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
        }
        return words.ToString();
    }
}
