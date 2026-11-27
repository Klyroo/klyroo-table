namespace Klyroo.Table.Maui;

/// <summary>
/// First-use action button shown inside the unfiltered empty state of <see cref="KlyDataListTable{TItem}"/> or <see cref="KlyDataListGrid{TItem}"/>
/// when <see cref="DataListEmptyState.Action"/> is configured. Never shown on a filtered empty state.
/// </summary>
public sealed class DataListEmptyAction
{
    /// <summary>Creates an empty-state action. Set <see cref="Label"/> and <see cref="OnClick"/>.</summary>
    public DataListEmptyAction()
    {
    }

    /// <summary>Label of the button.</summary>
    public required string Label { get; init; }

    /// <summary>Optional leading icon name.</summary>
    public string? Icon { get; init; }

    /// <summary>Called when the button is tapped.</summary>
    public Action? OnClick { get; init; }

    /// <summary>Button color (<see cref="KlyColor.Primary"/> by default).</summary>
    public KlyColor Color { get; init; } = KlyColor.Primary;

    /// <summary>Button visual variant (<see cref="ButtonVariant.Solid"/> by default).</summary>
    public ButtonVariant Variant { get; init; } = ButtonVariant.Solid;
}
