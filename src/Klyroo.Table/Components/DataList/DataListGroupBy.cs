using System.Globalization;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// One visual group of rows produced by a <see cref="DataListGroupBy{TItem}"/> over the loaded rows of a data list. The group of a
/// <see cref="DataListGroupBy{TItem}.By{TKey}"/> grouping is a <see cref="DataListGroup{TItem, TKey}"/>, which also carries the typed value.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public abstract class DataListGroup<TItem>
{
    internal DataListGroup(
        string key,
        bool isEmptyValue,
        string label,
        string? summary,
        IReadOnlyList<TItem> items,
        IReadOnlyList<string> rowKeys,
        bool defaultCollapsed)
    {
        Key = key;
        IsEmptyValue = isEmptyValue;
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

/// <summary>A group of a grouping by a value of type <typeparamref name="TKey"/>.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <typeparam name="TKey">The type of the group value.</typeparam>
public sealed class DataListGroup<TItem, TKey> : DataListGroup<TItem>
{
    internal DataListGroup(
        string key,
        TKey value,
        bool isEmptyValue,
        string label,
        string? summary,
        IReadOnlyList<TItem> items,
        IReadOnlyList<string> rowKeys,
        bool defaultCollapsed)
        : base(key, isEmptyValue, label, summary, items, rowKeys, defaultCollapsed)
    {
        Value = value;
    }

    /// <summary>The group value; <c>default</c> when <see cref="DataListGroup{TItem}.IsEmptyValue"/> is <see langword="true"/> (a null or blank value).</summary>
    public TKey Value { get; }
}

/// <summary>
/// Visual grouping configuration for <see cref="KlyDataListTable{TItem}"/> and <see cref="KlyDataListGrid{TItem}"/>. Create one with <see cref="By{TKey}"/> (a value read from the row)
/// or <see cref="ByField"/> (a field of the schema).
/// Groups are visual only: filtering, sorting, pagination and actions continue to operate on the flat rows, while rows on screen
/// are partitioned under collapsible <see cref="KlyDataListGroupHeader{TItem}"/> headers with optional per-group selection checkboxes.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public abstract class DataListGroupBy<TItem>
{
    internal DataListGroupBy() { }

    /// <summary>The stable key used for the group of rows whose value is <see langword="null"/> or empty.</summary>
    public const string EmptyGroupKey = "~empty";

    /// <summary>
    /// Whether groups start collapsed (folded) by default (<see langword="false"/> by default). The <c>DefaultCollapsed</c> predicate of the grouping, when set, decides instead.
    /// </summary>
    public bool Collapsed { get; set; }

    /// <summary>
    /// Groups the rows by a value read from each row: <c>DataListGroupBy&lt;Order&gt;.By(o =&gt; o.Status)</c>. A <see langword="null"/> or blank value goes to the empty group, which comes last.
    /// </summary>
    /// <typeparam name="TKey">The type of the group value (an enum, a number, a date, a string, a record).</typeparam>
    /// <param name="key">Reads the group value of a row.</param>
    /// <param name="order">Group values placed first, in this order. The other groups follow in ascending value order.</param>
    /// <param name="comparer">Tells when two values are the same group; the default equality of the type when <see langword="null"/>.</param>
    public static DataListGroupBy<TItem, TKey> By<TKey>(Func<TItem, TKey> key, IReadOnlyList<TKey>? order = null, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new DataListGroupBy<TItem, TKey>(key, null, order, comparer);
    }

    /// <summary>
    /// Groups the rows by a field of the schema: <c>DataListGroupBy&lt;Order&gt;.ByField("status")</c>. The value of a group is the text of the field in the invariant culture; the label is formatted with the culture of the page.
    /// Groups come in the order of the field (numbers and dates by value).
    /// </summary>
    /// <param name="fieldKey">The key of the field in the schema.</param>
    /// <param name="order">Group values (the text of the field) placed first, in this order.</param>
    public static DataListGroupBy<TItem, string> ByField(string fieldKey, IReadOnlyList<string>? order = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        return new DataListGroupBy<TItem, string>(null, fieldKey, order, null);
    }

    internal abstract IReadOnlyList<DataListGroup<TItem>> BuildGroups(
        IReadOnlyList<TItem> rows,
        DataListSchema<TItem>? schema,
        KlyToolsMessages messages,
        CultureInfo culture);

    internal abstract RenderFragment? LabelFragment(DataListGroup<TItem> group);

    internal abstract RenderFragment? SummaryFragment(DataListGroup<TItem> group);
}

/// <summary>A grouping by a value of type <typeparamref name="TKey"/>, from <see cref="DataListGroupBy{TItem}.By{TKey}"/> or <see cref="DataListGroupBy{TItem}.ByField"/>.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <typeparam name="TKey">The type of the group value.</typeparam>
public sealed class DataListGroupBy<TItem, TKey> : DataListGroupBy<TItem>
{
    private static readonly bool IsComparable = DataListNumbers.IsComparable<TKey>();

    private readonly Func<TItem, TKey>? _key;
    private readonly string? _field;

    /// <summary>Creates a grouping by a value read from each row, to set the label, summary and collapsing in an object initializer. <see cref="DataListGroupBy{TItem}.By{TKey}"/> is the shorter form when none is needed.</summary>
    /// <param name="key">Reads the group value of a row.</param>
    /// <param name="order">Group values placed first, in this order.</param>
    /// <param name="comparer">Tells when two values are the same group.</param>
    public DataListGroupBy(Func<TItem, TKey> key, IReadOnlyList<TKey>? order = null, IEqualityComparer<TKey>? comparer = null)
        : this(key ?? throw new ArgumentNullException(nameof(key)), null, order, comparer)
    {
    }

    internal DataListGroupBy(Func<TItem, TKey>? key, string? field, IReadOnlyList<TKey>? order, IEqualityComparer<TKey>? comparer)
    {
        _key = key;
        _field = field;
        Order = order;
        Comparer = comparer;
    }

    /// <summary>Group values placed first, in this order. Remaining non-empty groups follow in ascending value order, and the empty group (<see langword="null"/> or blank) comes last unless a blank value is listed.</summary>
    public IReadOnlyList<TKey>? Order { get; }

    /// <summary>Tells when two values are the same group; <see langword="null"/> for the default equality of the type.</summary>
    public IEqualityComparer<TKey>? Comparer { get; }

    /// <summary>
    /// Formats the heading label of a group. When <see langword="null"/>, formats <see cref="DataListGroup{TItem, TKey}.Value"/> or uses the localized "Other" label (<see cref="KlyToolsMessages.GroupEmpty"/>) for empty values.
    /// </summary>
    public Func<DataListGroup<TItem, TKey>, string?>? Label { get; set; }

    /// <summary>Custom template for the group heading label inside <see cref="KlyDataListGroupHeader{TItem}"/>.</summary>
    public RenderFragment<DataListGroup<TItem, TKey>>? LabelTemplate { get; set; }

    /// <summary>
    /// Formats the summary badge shown beside the group label. When <see langword="null"/>, shows the number of rows in the group. Return an empty string to hide the badge.
    /// </summary>
    public Func<DataListGroup<TItem, TKey>, string?>? Summary { get; set; }

    /// <summary>Custom template for the summary badge shown beside the group label inside <see cref="KlyDataListGroupHeader{TItem}"/>.</summary>
    public RenderFragment<DataListGroup<TItem, TKey>>? SummaryTemplate { get; set; }

    /// <summary>Optional per-group predicate determining whether a group starts collapsed (folded) by default. Wins over <see cref="DataListGroupBy{TItem}.Collapsed"/>.</summary>
    public Func<DataListGroup<TItem, TKey>, bool>? DefaultCollapsed { get; set; }

    internal override RenderFragment? LabelFragment(DataListGroup<TItem> group) =>
        LabelTemplate is not null && group is DataListGroup<TItem, TKey> typed ? LabelTemplate(typed) : null;

    internal override RenderFragment? SummaryFragment(DataListGroup<TItem> group) =>
        SummaryTemplate is not null && group is DataListGroup<TItem, TKey> typed ? SummaryTemplate(typed) : null;

    internal override IReadOnlyList<DataListGroup<TItem>> BuildGroups(
        IReadOnlyList<TItem> rows,
        DataListSchema<TItem>? schema,
        KlyToolsMessages messages,
        CultureInfo culture)
    {
        if (rows.Count == 0) return [];
        if (_key is null && (schema is null || string.IsNullOrWhiteSpace(_field))) return [];

        var buckets = new Dictionary<Boxed, Bucket>(new BoxedComparer(Comparer));
        Bucket? emptyBucket = null;
        var discoveryOrder = 0;

        foreach (var row in rows)
        {
            var (isEmpty, value) = ReadValue(row, schema);
            Bucket? bucket;
            if (isEmpty)
            {
                bucket = emptyBucket ??= new Bucket(EmptyGroupKey, default!, row, true, discoveryOrder++);
            }
            else if (!buckets.TryGetValue(new Boxed(value), out bucket))
            {
                bucket = new Bucket(GroupKeyOf(value), value, row, false, discoveryOrder++);
                buckets[new Boxed(value)] = bucket;
            }
            bucket.Items.Add(row);
            if (schema is { HasRowKey: true })
            {
                bucket.RowKeys.Add(schema.GetRowKey(row));
            }
        }

        var sortedBuckets = new List<Bucket>(buckets.Values);
        if (emptyBucket is not null) sortedBuckets.Add(emptyBucket);
        var ranks = BuildRanks();
        var structured = _field is not null && schema is not null && schema.FieldInfos.FirstOrDefault(f => f.Key == _field)?.HasStructuredValue == true;
        sortedBuckets.Sort((a, b) => CompareBuckets(a, b, ranks, schema, structured, culture));

        var result = new List<DataListGroup<TItem>>(sortedBuckets.Count);
        foreach (var bucket in sortedBuckets)
        {
            var defaultLabel = bucket.IsEmpty ? messages.GroupEmpty : FormatLabel(bucket, schema, culture);
            var defaultSummary = bucket.Items.Count.ToString(culture);
            var group = new DataListGroup<TItem, TKey>(
                bucket.GroupKey,
                bucket.Value,
                bucket.IsEmpty,
                defaultLabel,
                defaultSummary,
                bucket.Items,
                bucket.RowKeys,
                Collapsed);

            if (Label is not null)
            {
                var customLabel = Label(group);
                if (!string.IsNullOrWhiteSpace(customLabel)) group.Label = customLabel;
            }
            if (Summary is not null)
            {
                group.Summary = Summary(group);
            }
            if (DefaultCollapsed is not null)
            {
                group.DefaultCollapsed = DefaultCollapsed(group);
            }
            result.Add(group);
        }

        return result;
    }

    // A null or blank value is the empty group; a string value is trimmed.
    private (bool IsEmpty, TKey Value) ReadValue(TItem row, DataListSchema<TItem>? schema)
    {
        if (_key is null)
        {
            var text = schema!.FormatField(row, _field!, null, CultureInfo.InvariantCulture)?.Trim();
            return string.IsNullOrEmpty(text) ? (true, default!) : (false, (TKey)(object)text);
        }
        var value = _key(row);
        if (value is null) return (true, default!);
        if (value is string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (true, default!) : (false, (TKey)(object)s.Trim());
        }
        return (false, value);
    }

    private static string InvariantText(TKey value) =>
        value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "";

    private static string GroupKeyOf(TKey value)
    {
        var token = InvariantText(value);
        return token == EmptyGroupKey ? "~val:" + token : token;
    }

    private string FormatLabel(Bucket bucket, DataListSchema<TItem>? schema, CultureInfo culture) =>
        _key is null
            ? schema?.FormatField(bucket.First, _field!, null, culture)?.Trim() ?? ""
            : bucket.Value is IFormattable f ? f.ToString(null, culture) : bucket.Value?.ToString() ?? "";

    private (Dictionary<Boxed, int> Values, int? Empty)? BuildRanks()
    {
        if (Order is not { Count: > 0 }) return null;
        var values = new Dictionary<Boxed, int>(new BoxedComparer(Comparer));
        int? empty = null;
        for (var i = 0; i < Order.Count; i++)
        {
            var item = Order[i];
            if (item is null || (item is string blank && string.IsNullOrWhiteSpace(blank)))
            {
                empty ??= i;
                continue;
            }
            values.TryAdd(new Boxed(item is string s ? (TKey)(object)s.Trim() : item), i);
        }
        return (values, empty);
    }

    private int CompareBuckets(
        Bucket a,
        Bucket b,
        (Dictionary<Boxed, int> Values, int? Empty)? ranks,
        DataListSchema<TItem>? schema,
        bool structured,
        CultureInfo culture)
    {
        int? rankA = RankOf(a, ranks);
        int? rankB = RankOf(b, ranks);
        if (rankA is { } ra && rankB is { } rb) return ra.CompareTo(rb);
        if (rankA is not null) return -1;
        if (rankB is not null) return 1;

        // Empty values always come last unless explicitly ordered above.
        if (a.IsEmpty != b.IsEmpty) return a.IsEmpty ? 1 : -1;
        if (a.IsEmpty) return a.DiscoveryIndex.CompareTo(b.DiscoveryIndex);

        if (_key is null)
        {
            if (structured)
            {
                var byField = schema!.CompareField(a.First, b.First, _field!);
                if (byField != 0) return byField;
            }
        }
        else if (IsComparable && typeof(TKey) != typeof(string))
        {
            var cmp = Comparer<TKey>.Default.Compare(a.Value, b.Value);
            if (cmp != 0) return cmp;
        }

        var textA = FormatLabel(a, schema, culture);
        var textB = FormatLabel(b, schema, culture);
        var textCmp = culture.CompareInfo.Compare(textA, textB, CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);
        return textCmp != 0 ? textCmp : a.DiscoveryIndex.CompareTo(b.DiscoveryIndex);
    }

    private static int? RankOf(Bucket bucket, (Dictionary<Boxed, int> Values, int? Empty)? ranks)
    {
        if (ranks is not { } r) return null;
        if (bucket.IsEmpty) return r.Empty;
        return r.Values.TryGetValue(new Boxed(bucket.Value), out var rank) ? rank : null;
    }

    // A wrapper so that a null key (TKey is not constrained to be notnull) is still a valid dictionary key.
    private readonly record struct Boxed(TKey Value);

    private sealed class BoxedComparer(IEqualityComparer<TKey>? comparer) : IEqualityComparer<Boxed>
    {
        private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;

        public bool Equals(Boxed x, Boxed y) => _comparer.Equals(x.Value, y.Value);

        public int GetHashCode(Boxed obj) => obj.Value is null ? 0 : _comparer.GetHashCode(obj.Value);
    }

    private sealed class Bucket(string groupKey, TKey value, TItem first, bool isEmpty, int discoveryIndex)
    {
        public string GroupKey { get; } = groupKey;
        public TKey Value { get; } = value;
        public TItem First { get; } = first;
        public bool IsEmpty { get; } = isEmpty;
        public int DiscoveryIndex { get; } = discoveryIndex;
        public List<TItem> Items { get; } = [];
        public List<string> RowKeys { get; } = [];
    }
}
