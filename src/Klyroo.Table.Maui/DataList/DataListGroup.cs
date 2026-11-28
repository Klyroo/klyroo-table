namespace Klyroo.Table.Maui;

/// <summary>One visual group of rows produced by a <see cref="DataListGroupBy{TItem}"/> over the loaded rows of a data list.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListGroup<TItem>
{
    internal DataListGroup(string key, bool isEmptyValue, object? value, string label, string? summary, IReadOnlyList<TItem> items, IReadOnlyList<string> rowKeys, bool defaultCollapsed)
    {
        Key = key;
        IsEmptyValue = isEmptyValue;
        Value = value;
        Label = label;
        Summary = summary;
        Items = items;
        RowKeys = rowKeys;
        DefaultCollapsed = defaultCollapsed;
    }

    /// <summary>Stable identifier of the group (<c>"~empty"</c> when the group value is null or blank).</summary>
    public string Key { get; }

    /// <summary>Whether the group holds rows whose group value is <see langword="null"/> or blank.</summary>
    public bool IsEmptyValue { get; }

    /// <summary>The group value; <see langword="null"/> when <see cref="IsEmptyValue"/> is <see langword="true"/>.</summary>
    public object? Value { get; }

    /// <summary>The display label of the group header.</summary>
    public string Label { get; internal set; }

    /// <summary>The summary text shown beside the group label (the row count by default).</summary>
    public string? Summary { get; internal set; }

    /// <summary>The rows belonging to this group, in the active sort order.</summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>The string keys of <see cref="Items"/> when the schema defines a <c>RowKey</c>; empty otherwise.</summary>
    public IReadOnlyList<string> RowKeys { get; }

    /// <summary>Whether the group starts folded before any user toggle.</summary>
    public bool DefaultCollapsed { get; internal set; }
}
