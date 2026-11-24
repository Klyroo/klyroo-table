namespace Lux.Table;

/// <summary>
/// The message tables of Lux.Table, one per locale of <see cref="LxLocales.All"/> and keyed by the same codes. A component picks the table
/// of the current <see cref="LxLocale.Code"/> with <see cref="LxLocales.ResolveCode"/>, so "pt-PT" gets "pt" and an unknown language gets English.
/// Change a table with <see cref="LuxTableOptions.Messages"/>.
/// </summary>
public static partial class LxToolsLocales
{
    private static IReadOnlyDictionary<string, LxToolsMessages>? _tables;

    /// <summary>Every table by locale code: the codes of <see cref="LxLocales.All"/>.</summary>
    public static IReadOnlyDictionary<string, LxToolsMessages> Tables => _tables ??= new Dictionary<string, LxToolsMessages>(StringComparer.OrdinalIgnoreCase)
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
