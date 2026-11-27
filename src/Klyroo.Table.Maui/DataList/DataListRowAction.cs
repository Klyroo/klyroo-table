namespace Klyroo.Table.Maui;

/// <summary>
/// An action on one row of a data list (<see cref="KlyDataListRowActions{TItem}"/>): a button of the row or an entry of its menu. Actions with the same
/// <see cref="Group"/> are kept together, separated from the others by a divider; an action with <see cref="Children"/> opens a submenu.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListRowAction<TItem>
{
    /// <summary>Creates a row action. Set <see cref="Key"/> and <see cref="Label"/>, then <see cref="OnExecute"/> or <see cref="Href"/>.</summary>
    public DataListRowAction()
    {
    }

    /// <summary>Identifies the action; unique among the actions of the part, children included.</summary>
    public required string Key { get; init; }

    /// <summary>The text of the button or of the menu entry.</summary>
    public required string Label { get; init; }

    /// <summary>The name of an icon shown before the label (<c>copy</c>), one of <see cref="KlyIconNames"/>.</summary>
    public string? Icon { get; init; }

    /// <summary>The color; <see cref="KlyColor.Error"/> for a destructive action.</summary>
    public KlyColor Color { get; init; } = KlyColor.Neutral;

    /// <summary>The group the action belongs to. Groups are shown in the order of their first visible action, with a divider between them; actions without a group form one of their own.</summary>
    public string? Group { get; init; }

    /// <summary>The entries of a submenu. An action with children opens them instead of running, and only shows when one of them does. It always sits in the menu, never in a button of the row.</summary>
    public IReadOnlyList<DataListRowAction<TItem>>? Children { get; init; }

    /// <summary>Makes the action a link to the address it returns (<see langword="null"/>: no link for this row). The app routes it: <see cref="OnExecute"/> is not called for it.</summary>
    public Func<DataListRowActionContext<TItem>, string?>? Href { get; init; }

    /// <summary>Hides the action when it returns <see langword="false"/> (an action that does not apply to this row); the children of a hidden action are hidden with it.</summary>
    public Func<DataListRowActionContext<TItem>, bool>? Visible { get; init; }

    /// <summary>Disables the action when it returns <see langword="true"/>; the children of a disabled action cannot run either.</summary>
    public Func<DataListRowActionContext<TItem>, bool>? Disabled { get; init; }

    /// <summary>Why the action is disabled: it is the accessible hint of its button or menu entry.</summary>
    public Func<DataListRowActionContext<TItem>, string?>? DisabledReason { get; init; }

    /// <summary>Shows a spinner and blocks the action while it returns <see langword="true"/> (work started elsewhere). The action also shows one while it runs.</summary>
    public Func<DataListRowActionContext<TItem>, bool>? Loading { get; init; }

    /// <summary>Asks before the action runs. Without it the action runs on the tap.</summary>
    public DataListRowActionConfirm<TItem>? Confirm { get; init; }

    /// <summary>Loads the list again once the action succeeded (the row it changed or removed). Off by default; an action can also call <see cref="DataListContext{TItem}.RefreshAsync"/> itself.</summary>
    public bool RefreshOnSuccess { get; init; }

    /// <summary>What the action does. An exception, or a call to <see cref="DataListRowActionContext{TItem}.Fail"/>, marks the run as failed: the list is not refreshed and <c>Error</c> of the part is raised.</summary>
    public Func<DataListRowActionContext<TItem>, Task>? OnExecute { get; init; }
}
