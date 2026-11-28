namespace Klyroo.Table.Maui;

/// <summary>A button that loads the rows again. The icon turns into a spinner while the rows on screen are loaded again.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListRefresh<TItem> : DataListPartBase<TItem>
{
    private readonly TableButton _button;

    /// <summary>Creates a refresh button.</summary>
    public KlyDataListRefresh()
    {
        _button = new TableButton(null, "refresh-cw", ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm);
        _button.Clicked += (_, _) =>
        {
            if (Context is { } context) _ = context.RefreshAsync();
        };
        Content = _button;
        Refresh();
    }

    internal TableButton Button => _button;

    /// <inheritdoc />
    protected override void Refresh()
    {
        _button.Text = Messages.Refresh;
        _button.IsBusy = Context?.IsRefreshing ?? false;
        _button.IsEnabled = Context is not null;
    }
}
