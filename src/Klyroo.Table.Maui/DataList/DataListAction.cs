namespace Klyroo.Table.Maui;

/// <summary>
/// An action of a data list: a button of the <see cref="KlyDataListSelectionActions{TItem}"/> bar (it acts on the selection) or an entry of the <see cref="KlyDataListActionsDropdown{TItem}"/> menu.
/// While it runs its button shows a spinner and the action cannot start twice.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListAction<TItem>
{
    /// <summary>Creates an action. Set <see cref="Key"/>, <see cref="Label"/> and <see cref="OnExecute"/>.</summary>
    public DataListAction()
    {
    }

    /// <summary>Identifies the action: tests and the menu use it.</summary>
    public required string Key { get; init; }

    /// <summary>The text of the button.</summary>
    public required string Label { get; init; }

    /// <summary>The name of an icon shown before the label (<c>trash</c>), one of <see cref="KlyIconNames"/>.</summary>
    public string? Icon { get; init; }

    /// <summary>The color of the button; <see cref="KlyColor.Error"/> for a destructive action.</summary>
    public KlyColor Color { get; init; } = KlyColor.Neutral;

    /// <summary>The variant of the button in the selection bar.</summary>
    public ButtonVariant Variant { get; init; } = ButtonVariant.Soft;

    /// <summary>Hides the action when it returns <see langword="false"/> (an action that does not apply to the selected rows).</summary>
    public Func<DataListActionContext<TItem>, bool>? Visible { get; init; }

    /// <summary>Disables the action when it returns <see langword="true"/>.</summary>
    public Func<DataListActionContext<TItem>, bool>? Disabled { get; init; }

    /// <summary>Asks before the action runs. Without it the action runs on the tap.</summary>
    public DataListActionConfirm<TItem>? Confirm { get; init; }

    /// <summary>When the ticked rows are unticked: never by default.</summary>
    public DataListSelectionClear ClearSelection { get; init; }

    /// <summary>Loads the list again once the action succeeded (the rows it changed or removed). Off by default; an action can also call <see cref="DataListContext{TItem}.RefreshAsync"/> itself.</summary>
    public bool RefreshOnSuccess { get; init; }

    /// <summary>What the action does. An exception marks the run as failed: the selection stays, the list is not refreshed and <c>Error</c> of the part is raised.</summary>
    public Func<DataListActionContext<TItem>, Task>? OnExecute { get; init; }
}
