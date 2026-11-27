namespace Klyroo.Table.Maui;

/// <summary>The question a row action asks before it runs, in a dialog. The text can name the row.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListRowActionConfirm<TItem>
{
    /// <summary>Creates a confirmation. Set <see cref="Title"/>.</summary>
    public DataListRowActionConfirm()
    {
    }

    /// <summary>The title of the dialog.</summary>
    public required string Title { get; init; }

    /// <summary>The text under the title, built from the context (<c>c =&gt; $"{c.Item.Name} will be removed."</c>).</summary>
    public Func<DataListRowActionContext<TItem>, string?>? Description { get; init; }

    /// <summary>The label of the confirm button. Without it the localized "Confirm".</summary>
    public string? ConfirmLabel { get; init; }

    /// <summary>The color of the confirm button; <see cref="KlyColor.Error"/> for a destructive action. Without it the color of the action.</summary>
    public KlyColor? Color { get; init; }
}
