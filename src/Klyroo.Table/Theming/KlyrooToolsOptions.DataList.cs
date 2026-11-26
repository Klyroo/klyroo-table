using Klyroo.UI.Theming;

namespace Klyroo.Table;

public sealed partial class KlyrooTableOptions
{
    /// <summary>The empty state of a data list.</summary>
    public ComponentTheme DataListEmpty { get; } = ToolsThemes.DataListEmpty();

    /// <summary>The data list and its parts: toolbar, result count, refresh button, table, pagination and error state. One theme for all of them, so a look is changed in one place.</summary>
    public ComponentTheme DataList { get; } = ToolsThemes.DataList();

    private int _searchDebounce = 300;

    /// <summary>
    /// Milliseconds of quiet after the last keystroke before the search field of a data list searches (300 by default), unless the field sets its own
    /// <c>Debounce</c>. Enter searches at once. Set it in <c>AddKlyrooTable</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int SearchDebounce
    {
        get => _searchDebounce;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _searchDebounce = value;
        }
    }

    private int _persistMaxBytes = 24 * 1024;

    /// <summary>
    /// The most bytes, as sent, one data list hands from its prerender to the interactive render (24 KB by default): its state, loaded state and result with its facets and labels, counted with the
    /// escaping and the two base64 encodings Blazor adds, so the JSON itself is about a third of it. A bigger result is not persisted: the interactive render loads it again once, and a warning names the
    /// list and the size. Every list, and the other Klyroo libraries of the page, also share <see cref="KlyrooOptions.PersistedStateMaxBytes"/> (28 KB as sent by default), because the client sends
    /// everything persisted to the server in the message that starts a Server circuit, which SignalR refuses past 32 KB. Set it in <c>AddKlyrooTable</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int PersistMaxBytes
    {
        get => _persistMaxBytes;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _persistMaxBytes = value;
        }
    }
}
