using Klyroo.Query;
using Microsoft.JSInterop;

namespace Klyroo.Table;

// The filters a schema marks with DataListFilter.WithPersist: the browser keeps the last value of each in localStorage, and the first interactive render of a list whose address
// (or starting state) says nothing about such a filter takes that value back. Storage is browser-only, so a static render and a prerender never read or write it.
public partial class KlyDataList<TItem>
{
    internal const string FilterStorageModulePath = "./_content/Klyroo.Table/klyroo-table-storage.js";

    private Task<IJSObjectReference?>? _filterStorage;

    // The stored text of each remembered filter as the list last saw it; a write goes out only when it changes. Null until the first interactive render read the storage.
    private Dictionary<string, string>? _rememberedFilters;

    // The localStorage entry of one remembered filter: scoped by the row type, the PersistKey or UrlKey of the list and the key of the filter, so two lists of a page do not share a value.
    internal string FilterStorageKey(string filterKey) =>
        "klyroo-dl-filter:" + typeof(TItem).FullName + ":" + (PersistKey ?? NormalizedUrlKey ?? "") + ":" + filterKey;

    private List<string> RememberedFilterKeys()
    {
        var schema = _schema;
        if (schema is null) return [];
        return [.. schema.FilterKeys.Where(key => schema.TryGetFilter(key, out var filter) && filter.Persist)];
    }

    /// <summary>
    /// The text a remembered filter is stored as: the URL keys and values of its rules in <paramref name="state"/> (<c>f.status=Paid%2CDraft</c>), encoded the way the address writes them,
    /// or an empty text when the state holds no rule for it or holds the default rules of the schema.
    /// </summary>
    internal static string EncodeRememberedFilter(DataListSchema schema, DataListState state, string key)
    {
        var rules = state.Filters.Where(r => r is not null && r.Key == key).ToList();
        if (rules.Count == 0) return "";
        // Only this filter differs from the defaults, so the URL form holds its keys and nothing else.
        var defaults = schema.DefaultState;
        var own = defaults with { Filters = [.. defaults.Filters.Where(r => r.Key != key), .. rules] };
        return string.Join('&', DataListStateQuery.ToQuery(own, schema).Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
    }

    /// <summary>
    /// The rules of a remembered filter read back from its stored text, with the reading of the address: an entry the schema no longer accepts (another kind, a removed option, an operator
    /// it no longer allows) is dropped, and so is anything that is not a key of this filter.
    /// </summary>
    internal static IReadOnlyList<FilterRule> DecodeRememberedFilter(DataListSchema schema, string key, string? stored, string? timeZone)
    {
        if (string.IsNullOrEmpty(stored)) return [];
        var pairs = new List<KeyValuePair<string, string?>>();
        foreach (var part in stored.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var equals = part.IndexOf('=');
            if (equals <= 0) continue;
            var name = Uri.UnescapeDataString(part[..equals]);
            if (!name.StartsWith("f.", StringComparison.Ordinal)) continue;
            pairs.Add(new(name, Uri.UnescapeDataString(part[(equals + 1)..])));
        }
        if (pairs.Count == 0) return [];
        var read = DataListStateQuery.FromQuery(pairs, schema, null, timeZone);
        var rules = read.Filters.Where(r => r.Key == key).ToList();
        // Nothing parsed: the defaults of the schema came back, which is not a remembered value.
        return rules.SequenceEqual(schema.DefaultState.Filters.Where(r => r.Key == key)) ? [] : rules;
    }

    // Whether the list starts with a value of its own for a filter: a key of it in the address when the state came from there, else rules other than the defaults in the state the parent gave.
    private bool StartsWithFilterValue(string key, DataListState state, bool fromUrl)
    {
        if (fromUrl)
        {
            var prefix = NormalizedUrlKey;
            foreach (var (name, _) in Query.Snapshot(prefix ?? ""))
            {
                if (!IsOwned(name)) continue;
                var own = prefix is null ? name : name[(prefix.Length + 1)..];
                if (!own.StartsWith("f.", StringComparison.Ordinal)) continue;
                var rest = own[2..];
                if (rest == key) return true;
                // f.<key>.<operator>, f.<key>.from and f.<key>.to; a filter whose own key has dots (team.lead) is matched whole first, as the address is read.
                var dot = rest.LastIndexOf('.');
                if (dot > 0 && rest[..dot] == key && !_schema!.TryGetFilter(rest, out _)) return true;
            }
            return false;
        }
        return !state.Filters.Where(r => r.Key == key).SequenceEqual(_schema!.DefaultState.Filters.Where(r => r.Key == key));
    }

    /// <summary>
    /// The state the list starts from, on its first interactive render or after a navigation to its page, once the remembered filters are applied: each one the list does not start with a
    /// value for takes the stored value (on page 1). <paramref name="fromUrl"/> says the state was read from the address. Returns <paramref name="state"/> itself when nothing was restored.
    /// </summary>
    private async Task<DataListState> RestoreRememberedFiltersAsync(DataListState state, bool fromUrl)
    {
        var keys = RememberedFilterKeys();
        if (keys.Count == 0 || !RendererInfo.IsInteractive || _schema is null) return state;
        var next = state;
        var open = keys.Where(k => !StartsWithFilterValue(k, state, fromUrl)).ToList();
        if (open.Count > 0)
        {
            var stored = await ReadRememberedFiltersAsync([.. open.Select(FilterStorageKey)]);
            var restored = false;
            for (var i = 0; i < open.Count; i++)
            {
                var rules = DecodeRememberedFilter(_schema, open[i], i < stored.Length ? stored[i] : null, _timeZone);
                if (rules.Count == 0) continue;
                next = next with { Filters = [.. next.Filters.Where(r => r.Key != open[i]), .. rules] };
                restored = true;
            }
            // Other filters, other rows: the page of the address is for the ones it described.
            if (restored) next = next with { Page = 1, Cursor = null };
        }
        return next;
    }

    // From now on a change of a remembered filter is written to the browser; the values the list starts with are not (a shared link does not replace what the user chose).
    private void StartRememberingFilters(DataListState state)
    {
        if (!RendererInfo.IsInteractive || _schema is not { } schema) return;
        var keys = RememberedFilterKeys();
        if (keys.Count == 0) return;
        _rememberedFilters = keys.ToDictionary(k => k, k => EncodeRememberedFilter(schema, state, k), StringComparer.Ordinal);
    }

    // Every load passes here: a remembered filter whose value changed is stored, and one that was cleared (or set back to its default) is removed.
    private void SaveRememberedFilters(DataListState state)
    {
        if (_rememberedFilters is null || _schema is not { } schema || _disposed) return;
        foreach (var key in _rememberedFilters.Keys.ToList())
        {
            var encoded = EncodeRememberedFilter(schema, state, key);
            if (encoded == _rememberedFilters[key]) continue;
            _rememberedFilters[key] = encoded;
            _ = WriteRememberedFilterAsync(FilterStorageKey(key), encoded);
        }
    }

    private Task<IJSObjectReference?> FilterStorageAsync()
    {
        // An import that found no browser side (a circuit reconnecting) is tried again on the next call.
        if (_filterStorage is { IsCompletedSuccessfully: true, Result: null }) _filterStorage = null;
        return _filterStorage ??= ImportFilterStorageAsync();
    }

    private async Task<IJSObjectReference?> ImportFilterStorageAsync()
    {
        try
        {
            return await JS.InvokeAsync<IJSObjectReference>("import", FilterStorageModulePath);
        }
        catch (Exception e) when (IsStorageFailure(e))
        {
            // No browser side (a test, a circuit that ended): nothing is remembered.
            return null;
        }
    }

    private async Task<string?[]> ReadRememberedFiltersAsync(string[] keys)
    {
        try
        {
            if (await FilterStorageAsync() is not { } module) return [];
            return await module.InvokeAsync<string?[]>("readAll", new object?[] { keys }) ?? [];
        }
        catch (Exception e) when (IsStorageFailure(e))
        {
            return [];
        }
    }

    private async Task WriteRememberedFilterAsync(string key, string value)
    {
        try
        {
            if (await FilterStorageAsync() is not { } module) return;
            await module.InvokeVoidAsync("write", key, value);
        }
        catch (Exception e) when (IsStorageFailure(e))
        {
            // The browser keeps its previous value; the list on screen is not affected.
        }
    }

    private static bool IsStorageFailure(Exception e) =>
        e is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException or ObjectDisposedException;

    private void DisposeFilterStorage()
    {
        // An import still running is disposed when it ends, so the reference does not outlive the list.
        if (_filterStorage is { } loading) _ = DisposeModuleAsync(loading);

        static async Task DisposeModuleAsync(Task<IJSObjectReference?> loading)
        {
            try
            {
                if (await loading is { } module) await module.DisposeAsync();
            }
            catch (Exception e) when (IsStorageFailure(e))
            {
                // The circuit is gone with the page.
            }
        }
    }
}
