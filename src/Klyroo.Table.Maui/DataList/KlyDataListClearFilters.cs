namespace Klyroo.Table.Maui;

/// <summary>Takes every filter off (the search, the sort and the page size stay). It shows only while a filter is on, and works alone, outside the filter tags.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListClearFilters<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListClearFilters<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListClearFilters<TItem>)b).Redraw());

    private readonly TableButton _button;

    /// <summary>Creates a clear filters button.</summary>
    public KlyDataListClearFilters()
    {
        _button = new TableButton(null, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm);
        _button.Clicked += (_, _) => Context?.SetState(DataListContext<TItem>.WithoutFilters);
        Refresh();
    }

    /// <summary>The text of the button. Without it, the localized "Clear filters".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    internal TableButton Button => _button;

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { Schema: not null } context || context.ActiveFilters.Count == 0)
        {
            Content = null;
            return;
        }
        _button.Text = Text ?? Messages.ClearFilters;
        Content = _button;
    }
}
