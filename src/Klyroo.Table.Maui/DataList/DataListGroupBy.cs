using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// Visual grouping configuration for <see cref="KlyDataListTable{TItem}"/> and <see cref="KlyDataListGrid{TItem}"/>. Create one with <see cref="By{TKey}"/> (a value read from the row)
/// or <see cref="ByField"/> (a field of the schema). Groups are visual only: filtering, sorting, paging and actions operate on the flat rows, while the rows on screen are partitioned under
/// collapsible <see cref="KlyDataListGroupHeader{TItem}"/> headers with per-group selection checkboxes. Rows are not virtualized while a grouping is on.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListGroupBy<TItem>
{
    /// <summary>The key of the group of rows whose value is <see langword="null"/> or empty.</summary>
    public const string EmptyGroupKey = "~empty";

    private readonly Func<TItem, object?>? _key;
    private readonly string? _field;
    private readonly IReadOnlyList<string>? _order;

    private DataListGroupBy(Func<TItem, object?>? key, string? field, IReadOnlyList<string>? order)
    {
        _key = key;
        _field = field;
        _order = order;
    }

    /// <summary>Whether groups start collapsed (folded) by default. The <see cref="DefaultCollapsed"/> predicate, when set, decides instead.</summary>
    public bool Collapsed { get; set; }

    /// <summary>Formats the heading label of a group. When <see langword="null"/>, formats the value or uses the localized "Other" label for empty values.</summary>
    public Func<DataListGroup<TItem>, string?>? Label { get; set; }

    /// <summary>Formats the summary shown beside the group label. When <see langword="null"/>, shows the number of rows in the group. Return an empty string to hide it.</summary>
    public Func<DataListGroup<TItem>, string?>? Summary { get; set; }

    /// <summary>Per-group predicate telling whether a group starts collapsed. Wins over <see cref="Collapsed"/>.</summary>
    public Func<DataListGroup<TItem>, bool>? DefaultCollapsed { get; set; }

    /// <summary>Groups the rows by a value read from each row: <c>DataListGroupBy&lt;Order&gt;.By(o =&gt; o.Status)</c>. A <see langword="null"/> or blank value goes to the empty group, which comes last.</summary>
    /// <typeparam name="TKey">The type of the group value (an enum, a number, a date, a string).</typeparam>
    /// <param name="key">Reads the group value of a row.</param>
    /// <param name="order">Group values placed first, in this order (compared by their text in the invariant culture). The other groups follow in ascending order.</param>
    public static DataListGroupBy<TItem> By<TKey>(Func<TItem, TKey> key, IReadOnlyList<TKey>? order = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new DataListGroupBy<TItem>(item => key(item), null, order?.Select(o => Text(o)).ToArray());
    }

    /// <summary>Groups the rows by a field of the schema: <c>DataListGroupBy&lt;Order&gt;.ByField("status")</c>. The value of a group is the text of the field in the invariant culture.</summary>
    /// <param name="fieldKey">The key of the field in the schema.</param>
    /// <param name="order">Group values (the text of the field) placed first, in this order.</param>
    public static DataListGroupBy<TItem> ByField(string fieldKey, IReadOnlyList<string>? order = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        return new DataListGroupBy<TItem>(null, fieldKey, order);
    }

    internal IReadOnlyList<DataListGroup<TItem>> BuildGroups(IReadOnlyList<TItem> rows, DataListSchema<TItem>? schema, KlyToolsMessages messages, CultureInfo culture)
    {
        if (rows.Count == 0) return [];
        if (_key is null && (schema is null || string.IsNullOrWhiteSpace(_field))) return [];

        var buckets = new Dictionary<string, Bucket>(StringComparer.Ordinal);
        var all = new List<Bucket>();
        Bucket? empty = null;
        foreach (var row in rows)
        {
            var value = ReadValue(row, schema);
            Bucket bucket;
            if (value is null)
            {
                bucket = empty ??= NewBucket(EmptyGroupKey, null, row, all);
            }
            else
            {
                var text = Text(value);
                if (!buckets.TryGetValue(text, out bucket!))
                {
                    var key = text == EmptyGroupKey ? "~val:" + text : text;
                    buckets[text] = bucket = NewBucket(key, value, row, all);
                }
            }
            bucket.Items.Add(row);
            if (schema is { HasRowKey: true }) bucket.RowKeys.Add(schema.GetRowKey(row));
        }

        all.Sort((a, b) => Compare(a, b, culture));
        var groups = new List<DataListGroup<TItem>>(all.Count);
        foreach (var bucket in all)
        {
            var label = bucket.Value is null ? messages.GroupEmpty : bucket.Value is IFormattable f ? f.ToString(null, culture) : bucket.Value.ToString() ?? "";
            var group = new DataListGroup<TItem>(bucket.Key, bucket.Value is null, bucket.Value, label, bucket.Items.Count.ToString(culture), bucket.Items, bucket.RowKeys, Collapsed);
            if (Label?.Invoke(group) is { } custom && !string.IsNullOrWhiteSpace(custom)) group.Label = custom;
            if (Summary is not null) group.Summary = Summary(group);
            if (DefaultCollapsed is not null) group.DefaultCollapsed = DefaultCollapsed(group);
            groups.Add(group);
        }
        return groups;
    }

    private static Bucket NewBucket(string key, object? value, TItem first, List<Bucket> all)
    {
        var bucket = new Bucket(key, value, first, all.Count);
        all.Add(bucket);
        return bucket;
    }

    // A null or blank value is the empty group; a string is trimmed.
    private object? ReadValue(TItem row, DataListSchema<TItem>? schema)
    {
        object? value = _key is null ? schema!.FormatField(row, _field!, null, CultureInfo.InvariantCulture) : _key(row);
        if (value is string s) return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        return value;
    }

    private static string Text(object? value) => value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "";

    private int Rank(Bucket bucket)
    {
        if (_order is null) return -1;
        if (bucket.Value is null) return _order.ToList().FindIndex(string.IsNullOrWhiteSpace);
        var text = Text(bucket.Value);
        for (var i = 0; i < _order.Count; i++)
        {
            if (_order[i] == text) return i;
        }
        return -1;
    }

    private int Compare(Bucket a, Bucket b, CultureInfo culture)
    {
        int rankA = Rank(a), rankB = Rank(b);
        if (rankA >= 0 && rankB >= 0) return rankA.CompareTo(rankB);
        if (rankA >= 0) return -1;
        if (rankB >= 0) return 1;
        var aEmpty = a.Value is null;
        if (aEmpty != (b.Value is null)) return aEmpty ? 1 : -1;
        if (aEmpty) return a.Discovery.CompareTo(b.Discovery);
        if (a.Value is IComparable comparable && a.Value.GetType() == b.Value!.GetType() && a.Value is not string)
        {
            var byValue = comparable.CompareTo(b.Value);
            if (byValue != 0) return byValue;
        }
        var byText = culture.CompareInfo.Compare(Text(a.Value), Text(b.Value), CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);
        return byText != 0 ? byText : a.Discovery.CompareTo(b.Discovery);
    }

    private sealed class Bucket(string key, object? value, TItem first, int discovery)
    {
        public string Key { get; } = key;
        public object? Value { get; } = value;
        public TItem First { get; } = first;
        public int Discovery { get; } = discovery;
        public List<TItem> Items { get; } = [];
        public List<string> RowKeys { get; } = [];
    }
}
