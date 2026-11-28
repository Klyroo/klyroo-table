namespace Klyroo.Table.Maui;

/// <summary>
/// Declarative configuration for the unfiltered empty state of <see cref="KlyDataListTable{TItem}"/> or <see cref="KlyDataListGrid{TItem}"/>.
/// When search or filters are active and match nothing, the filtered empty state (with Clear search / Reset filters) is still shown instead,
/// and <see cref="Action"/> is omitted so a first-use creation action is never offered while results are merely filtered out.
/// </summary>
public sealed class DataListEmptyState
{
    /// <summary>Creates an empty-state configuration that changes nothing: the default icon, title and description.</summary>
    public DataListEmptyState()
    {
    }

    /// <summary>Optional icon name overriding the default empty-state icon (or an empty string to hide the icon).</summary>
    public string? Icon { get; init; }

    /// <summary>Optional title overriding the localized <see cref="KlyToolsMessages.NoResults"/>.</summary>
    public string? Title { get; init; }

    /// <summary>Optional description overriding the localized <see cref="KlyToolsMessages.NoResultsDescription"/>.</summary>
    public string? Description { get; init; }

    /// <summary>When <see langword="true"/>, hides the description line on the unfiltered empty state.</summary>
    public bool HideDescription { get; init; }

    /// <summary>Optional first-use call-to-action shown only when the list is empty without active search or filters.</summary>
    public DataListEmptyAction? Action { get; init; }
}
