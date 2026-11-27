namespace Klyroo.Table.Maui;

/// <summary>Switches a <see cref="KlyDataListTable{TItem}"/> that declares a <see cref="DataListRowSummary{TItem}"/> between compact summary cards and the full multi-column table. Hidden when the table has no summary or the layout is the grid.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListViewToggle<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListViewToggle<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListViewToggle<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowText"/>.</summary>
    public static readonly BindableProperty ShowTextProperty = BindableProperty.Create(nameof(ShowText), typeof(bool), typeof(KlyDataListViewToggle<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListViewToggle<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(KlyDataListViewToggle<TItem>), "list", propertyChanged: (b, _, _) => ((KlyDataListViewToggle<TItem>)b).Redraw());

    private TableButton? _button;

    /// <summary>Creates a view toggle.</summary>
    public KlyDataListViewToggle()
    {
        Refresh();
    }

    /// <summary>Text and accessible name of the toggle button. Defaults to the localized "Summary view".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>Shows the text next to the icon (<see langword="true"/> by default).</summary>
    public bool ShowText { get => (bool)GetValue(ShowTextProperty); set => SetValue(ShowTextProperty, value); }

    /// <summary>Icon name shown on the toggle button (<c>"list"</c> by default).</summary>
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    internal TableButton? Button => _button;

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { HasRowSummary: true } context || context.EffectiveLayout != DataListLayout.Table)
        {
            Content = null;
            _button = null;
            return;
        }
        var active = context.SummaryViewActive;
        var label = Text ?? Messages.SummaryView;
        _button = new TableButton(ShowText ? label : null, Icon, active ? ButtonVariant.Soft : ButtonVariant.Outline, active ? KlyColor.Primary : KlyColor.Neutral, KlySize.Sm)
        {
            AccessibleName = label,
            AccessibleHint = active ? "selected" : null,
        };
        _button.Clicked += (_, _) => context.SetSummaryView(!context.SummaryViewActive);
        Content = _button;
    }
}
