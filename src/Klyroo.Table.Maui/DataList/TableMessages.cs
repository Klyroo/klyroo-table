using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>Picks the table of messages of a culture: the exact code ("pt-BR"), then the language ("pt"), Simplified Chinese for any other Chinese, else English.</summary>
internal static class TableMessages
{
    public static KlyToolsMessages For(CultureInfo culture)
    {
        var tables = KlyToolsLocales.Tables;
        var name = culture.Name;
        if (tables.TryGetValue(name, out var exact)) return exact;
        var language = culture.TwoLetterISOLanguageName;
        if (language == "zh") return tables["zh-Hans"];
        return tables.TryGetValue(language, out var byLanguage) ? byLanguage : KlyToolsLocales.En;
    }
}
