using System.Collections.Concurrent;
using Lux.UI.Theming;

namespace Lux.Table;

/// <summary>
/// Configuration of Lux.Table: its component themes and its message tables. It is an extension of <see cref="LuxOptions"/>
/// (<see cref="LuxOptions.Extension{T}"/>), so its themes freeze with the ones of Lux.UI and the Tailwind class extractor and the docs
/// bundle find them. Configure it with <c>services.AddLuxTable(t =&gt; ...)</c>, in code shared by the server and the WebAssembly client.
/// </summary>
/// <remarks>Partial: each family of components declares its themes in its own file.</remarks>
public sealed partial class LuxTableOptions
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<Func<LxToolsMessages, LxToolsMessages>>> _messageChanges = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, LxToolsMessages> _resolved = new(StringComparer.OrdinalIgnoreCase);
    private volatile bool _frozen;

    /// <summary>
    /// Changes the texts of a locale, like <see cref="LxLocales.Extend"/> does for Lux.UI: <paramref name="change"/> receives the table the
    /// locale resolves to and returns the one to use, usually <c>m =&gt; m with { ... }</c>. <paramref name="localeCode"/> is a code of
    /// <see cref="LxLocales.All"/> ("fr"), or the code of a locale you added ("en-AU", "tlh"): a locale without a table of its own
    /// starts from the table <see cref="LxLocales.ResolveCode"/> picks for it, then its changes apply. Like the themes, the messages become
    /// read-only the first time a component resolves a text.
    /// </summary>
    /// <exception cref="InvalidOperationException">A component has already resolved a text.</exception>
    public LuxTableOptions Messages(string localeCode, Func<LxToolsMessages, LxToolsMessages> change)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        ArgumentNullException.ThrowIfNull(change);
        lock (_gate)
        {
            if (_frozen)
            {
                throw new InvalidOperationException($"{nameof(LuxTableOptions)}.{nameof(Messages)} cannot change once a component has resolved a text: the result of each locale is cached. Set it in AddLuxTable, before the app starts rendering.");
            }
            if (!_messageChanges.TryGetValue(localeCode, out var list))
            {
                _messageChanges[localeCode] = list = [];
            }
            list.Add(change);
        }
        return this;
    }

    /// <summary>The texts for a locale code (<see cref="LxLocale.Code"/>), cached per code. The first call makes <see cref="Messages"/> read-only.</summary>
    internal LxToolsMessages ResolveMessages(string localeCode)
    {
        // Read on every render: take the lock only the first time.
        if (!_frozen)
        {
            lock (_gate) _frozen = true;
        }
        return _resolved.GetOrAdd(localeCode, static (code, self) => self.Compute(code), this);
    }

    private LxToolsMessages Compute(string localeCode)
    {
        var tables = LxToolsLocales.Tables;
        var tableCode = LxLocales.ResolveCode(localeCode, tables.Keys);
        var messages = tables[tableCode];
        foreach (var key in new[] { tableCode, localeCode }.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (_messageChanges.TryGetValue(key, out var changes))
            {
                foreach (var change in changes) messages = change(messages);
            }
        }
        return messages;
    }
}
