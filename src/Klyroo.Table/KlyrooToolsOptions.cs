using System.Collections.Concurrent;
using Klyroo.UI.Theming;

namespace Klyroo.Table;

/// <summary>
/// Configuration of Klyroo.Table: its component themes and its message tables. It is an extension of <see cref="KlyrooOptions"/>
/// (<see cref="KlyrooOptions.Extension{T}"/>), so its themes freeze with the ones of Klyroo.UI and the Tailwind class extractor and the docs
/// bundle find them. Configure it with <c>services.AddKlyrooTable(t =&gt; ...)</c>, in code shared by the server and the WebAssembly client.
/// </summary>
/// <remarks>Partial: each family of components declares its themes in its own file.</remarks>
public sealed partial class KlyrooTableOptions
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<Func<KlyToolsMessages, KlyToolsMessages>>> _messageChanges = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, KlyToolsMessages> _resolved = new(StringComparer.OrdinalIgnoreCase);
    private volatile bool _frozen;

    /// <summary>
    /// Changes the texts of a locale, like <see cref="KlyLocales.Extend"/> does for Klyroo.UI: <paramref name="change"/> receives the table the
    /// locale resolves to and returns the one to use, usually <c>m =&gt; m with { ... }</c>. <paramref name="localeCode"/> is a code of
    /// <see cref="KlyLocales.All"/> ("fr"), or the code of a locale you added ("en-AU", "tlh"): a locale without a table of its own
    /// starts from the table <see cref="KlyLocales.ResolveCode"/> picks for it, then its changes apply. Like the themes, the messages become
    /// read-only the first time a component resolves a text.
    /// </summary>
    /// <exception cref="InvalidOperationException">A component has already resolved a text.</exception>
    public KlyrooTableOptions Messages(string localeCode, Func<KlyToolsMessages, KlyToolsMessages> change)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        ArgumentNullException.ThrowIfNull(change);
        lock (_gate)
        {
            if (_frozen)
            {
                throw new InvalidOperationException($"{nameof(KlyrooTableOptions)}.{nameof(Messages)} cannot change once a component has resolved a text: the result of each locale is cached. Set it in AddKlyrooTable, before the app starts rendering.");
            }
            if (!_messageChanges.TryGetValue(localeCode, out var list))
            {
                _messageChanges[localeCode] = list = [];
            }
            list.Add(change);
        }
        return this;
    }

    /// <summary>The texts for a locale code (<see cref="KlyLocale.Code"/>), cached per code. The first call makes <see cref="Messages"/> read-only.</summary>
    internal KlyToolsMessages ResolveMessages(string localeCode)
    {
        // Read on every render: take the lock only the first time.
        if (!_frozen)
        {
            lock (_gate) _frozen = true;
        }
        return _resolved.GetOrAdd(localeCode, static (code, self) => self.Compute(code), this);
    }

    private KlyToolsMessages Compute(string localeCode)
    {
        var tables = KlyToolsLocales.Tables;
        var tableCode = KlyLocales.ResolveCode(localeCode, tables.Keys);
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
