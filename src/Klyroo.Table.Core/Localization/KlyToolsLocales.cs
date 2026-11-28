namespace Klyroo.Table;

/// <summary>
/// The message tables of Klyroo.Table, one per locale of <c>KlyLocales.All</c> and keyed by the same codes. A component picks the table
/// of the current <c>KlyLocale.Code</c> with <c>KlyLocales.ResolveCode</c>, so "pt-PT" gets "pt" and an unknown language gets English.
/// Change a table with <c>KlyrooTableOptions.Messages</c>.
/// </summary>
public static partial class KlyToolsLocales
{
    private static IReadOnlyDictionary<string, KlyToolsMessages>? _tables;

    /// <summary>Every table by locale code: the codes of <c>KlyLocales.All</c>.</summary>
    public static IReadOnlyDictionary<string, KlyToolsMessages> Tables => _tables ??= new Dictionary<string, KlyToolsMessages>(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = En,
        ["fr"] = Fr,
        ["de"] = De,
        ["es"] = Es,
        ["it"] = It,
        ["pt"] = Pt,
        ["pt-BR"] = PtBr,
        ["nl"] = Nl,
        ["ja"] = Ja,
        ["zh-Hans"] = ZhCn,
        ["ar"] = Ar,
        ["ru"] = Ru,
    };

    /// <summary>The codes whose texts were reviewed by a native speaker. The other tables are drafts waiting for review.</summary>
    public static IReadOnlyList<string> ReviewedCodes { get; } = ["en", "fr"];
}
