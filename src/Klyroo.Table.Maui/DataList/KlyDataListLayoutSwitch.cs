namespace Klyroo.Table.Maui;

/// <summary>Toggles the data list between its table view and its grid view. The options form a group; the active one is pressed.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListLayoutSwitch<TItem> : DataListPartBase<TItem>
{
    private static readonly DataListLayout[] DefaultOrder = [DataListLayout.Table, DataListLayout.Grid];

    /// <summary>Identifies <see cref="ShowLabels"/>.</summary>
    public static readonly BindableProperty ShowLabelsProperty = BindableProperty.Create(nameof(ShowLabels), typeof(bool), typeof(KlyDataListLayoutSwitch<TItem>), false, propertyChanged: (b, _, _) => ((KlyDataListLayoutSwitch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Order"/>.</summary>
    public static readonly BindableProperty OrderProperty = BindableProperty.Create(nameof(Order), typeof(IReadOnlyList<DataListLayout>), typeof(KlyDataListLayoutSwitch<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListLayoutSwitch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="TableIcon"/>.</summary>
    public static readonly BindableProperty TableIconProperty = BindableProperty.Create(nameof(TableIcon), typeof(string), typeof(KlyDataListLayoutSwitch<TItem>), TableIcons.Table, propertyChanged: (b, _, _) => ((KlyDataListLayoutSwitch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="GridIcon"/>.</summary>
    public static readonly BindableProperty GridIconProperty = BindableProperty.Create(nameof(GridIcon), typeof(string), typeof(KlyDataListLayoutSwitch<TItem>), TableIcons.LayoutGrid, propertyChanged: (b, _, _) => ((KlyDataListLayoutSwitch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(KlyDataListLayoutSwitch<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListLayoutSwitch<TItem>)b).Redraw());

    private readonly HorizontalStackLayout _row = new() { Spacing = 4 };
    private readonly Dictionary<DataListLayout, TableButton> _buttons = [];

    /// <summary>Creates a layout switch.</summary>
    public KlyDataListLayoutSwitch()
    {
        Content = _row;
        Refresh();
    }

    /// <summary>Shows the localized text labels ("Table view", "Grid view") next to the icons (<see langword="false"/> by default).</summary>
    public bool ShowLabels { get => (bool)GetValue(ShowLabelsProperty); set => SetValue(ShowLabelsProperty, value); }

    /// <summary>Order of the layout options (<c>[Table, Grid]</c> by default).</summary>
    public IReadOnlyList<DataListLayout>? Order { get => (IReadOnlyList<DataListLayout>?)GetValue(OrderProperty); set => SetValue(OrderProperty, value); }

    /// <summary>Icon name for the table layout (<c>"table-2"</c> by default).</summary>
    public string TableIcon { get => (string)GetValue(TableIconProperty); set => SetValue(TableIconProperty, value); }

    /// <summary>Icon name for the grid layout (<c>"layout-grid"</c> by default).</summary>
    public string GridIcon { get => (string)GetValue(GridIconProperty); set => SetValue(GridIconProperty, value); }

    /// <summary>What a screen reader says for the group. Defaults to the localized "View layout".</summary>
    public string? AccessibleName { get => (string?)GetValue(AccessibleNameProperty); set => SetValue(AccessibleNameProperty, value); }

    internal IReadOnlyDictionary<DataListLayout, TableButton> Buttons => _buttons;

    private IReadOnlyList<DataListLayout> EffectiveOrder => Order is { Count: > 0 } ? Order.Distinct().ToArray() : DefaultOrder;

    /// <inheritdoc />
    protected override void Refresh()
    {
        _row.Children.Clear();
        _buttons.Clear();
        SemanticProperties.SetDescription(_row, AccessibleName ?? Messages.LayoutSwitch);
        foreach (var layout in EffectiveOrder)
        {
            var item = layout;
            var active = Context?.EffectiveLayout == item;
            var label = item == DataListLayout.Grid ? Messages.GridView : Messages.TableView;
            var button = new TableButton(ShowLabels ? label : null, item == DataListLayout.Grid ? GridIcon : TableIcon, active ? ButtonVariant.Soft : ButtonVariant.Ghost, active ? KlyColor.Primary : KlyColor.Neutral, KlySize.Sm)
            {
                AccessibleName = label,
                AccessibleHint = active ? "selected" : null,
                IsEnabled = Context is not null,
            };
            button.Clicked += (_, _) => Context?.SetLayout(item);
            _buttons[item] = button;
            _row.Children.Add(button);
        }
    }
}
