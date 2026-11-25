using Klyroo.UI.Theming;

namespace Klyroo.Table;

/// <summary>Per-instance class overrides of <see cref="KlyDataListEmpty"/> slots.</summary>
public sealed record DataListEmptyUi : ISlotOverrides
{
    /// <summary>Creates an override set that changes no slot.</summary>
    public DataListEmptyUi() { }

    /// <summary>The outer element of the empty state. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Root { get; init; }
    /// <summary>The icon of the empty state. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Icon { get; init; }
    /// <summary>The title of the empty state. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Title { get; init; }
    /// <summary>The text under the title of the empty state. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Description { get; init; }
    /// <summary>The container of the actions of the empty state. Classes given here are merged with the theme classes of the slot.</summary>
    public string? Actions { get; init; }

    string? ISlotOverrides.For(string slot) => slot switch
    {
        "root" => Root,
        "icon" => Icon,
        "title" => Title,
        "description" => Description,
        "actions" => Actions,
        _ => null,
    };
}
