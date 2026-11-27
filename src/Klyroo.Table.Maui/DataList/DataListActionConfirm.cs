namespace Klyroo.Table.Maui;

/// <summary>The question an action asks before it runs, in a dialog. The text can say how many rows are affected.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListActionConfirm<TItem>
{
    /// <summary>Creates a confirmation. Set <see cref="Title"/>.</summary>
    public DataListActionConfirm()
    {
    }

    /// <summary>The title of the dialog.</summary>
    public required string Title { get; init; }

    /// <summary>The text under the title, built from the context so that it can count the rows (<c>c =&gt; $"{c.TargetCount} customers will be removed."</c>).</summary>
    public Func<DataListActionContext<TItem>, string?>? Description { get; init; }

    /// <summary>The label of the confirm button. Without it the localized "Confirm".</summary>
    public string? ConfirmLabel { get; init; }

    /// <summary>The color of the confirm button; <see cref="KlyColor.Error"/> for a destructive action. Without it the color of the action.</summary>
    public KlyColor? Color { get; init; }
}
