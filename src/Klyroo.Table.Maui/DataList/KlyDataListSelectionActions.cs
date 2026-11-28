namespace Klyroo.Table.Maui;

/// <summary>
/// The bar that appears while rows are ticked: how many, a switch between the ticked rows and every result (when the source knows how many there are), the actions as buttons (the first
/// <see cref="MaxVisible"/>, the others in a "More actions" menu) and a button that clears the selection. Having one turns the selection on. Needs a <see cref="KlyOverlayHost"/> above it for the menu and the confirmations.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListSelectionActions<TItem> : DataListActionPartBase<TItem>
{
    /// <summary>Identifies <see cref="MaxVisible"/>.</summary>
    public static readonly BindableProperty MaxVisibleProperty = BindableProperty.Create(nameof(MaxVisible), typeof(int), typeof(KlyDataListSelectionActions<TItem>), 3, propertyChanged: (b, _, _) => ((KlyDataListSelectionActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowScope"/>.</summary>
    public static readonly BindableProperty ShowScopeProperty = BindableProperty.Create(nameof(ShowScope), typeof(bool), typeof(KlyDataListSelectionActions<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListSelectionActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Position"/>.</summary>
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(DataListSelectionBarPosition), typeof(KlyDataListSelectionActions<TItem>), DataListSelectionBarPosition.Sticky, propertyChanged: (b, _, _) => ((KlyDataListSelectionActions<TItem>)b).Redraw());

    private DataListContext<TItem>? _subscribed;

    /// <summary>Creates a selection bar.</summary>
    public KlyDataListSelectionActions()
    {
        Refresh();
    }

    /// <summary>How many actions show as buttons; the others go into a "More actions" menu (3 by default).</summary>
    public int MaxVisible { get => (int)GetValue(MaxVisibleProperty); set => SetValue(MaxVisibleProperty, value); }

    /// <summary>Shows the switch between the ticked rows and every result when the source knows how many results there are (true by default).</summary>
    public bool ShowScope { get => (bool)GetValue(ShowScopeProperty); set => SetValue(ShowScopeProperty, value); }

    /// <summary>Where the bar sits: <see cref="DataListSelectionBarPosition.Sticky"/> stays where you put it in the layout, <see cref="DataListSelectionBarPosition.Fixed"/> aligns itself to the bottom center of its parent.</summary>
    public DataListSelectionBarPosition Position { get => (DataListSelectionBarPosition)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

    internal Border? Bar { get; private set; }

    internal IReadOnlyList<TableButton> ActionButtons { get; private set; } = [];

    internal KlyDropdownMenu? Overflow { get; private set; }

    internal TableButton? ClearButton { get; private set; }

    internal TableButton? ScopeButton { get; private set; }

    internal Label? CountLabel { get; private set; }

    /// <summary>Runs the action with this key as a tap on its button does.</summary>
    /// <param name="key">The key of the action.</param>
    internal Task Choose(string key) => Actions.FirstOrDefault(a => a.Key == key) is { } action ? RequestAsync(action) : Task.CompletedTask;

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        _subscribed = context;
        context.AddSelectionPart();
        context.SelectionChanged += Redraw;
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.SelectionChanged -= Redraw;
        context.RemoveSelectionPart();
        _subscribed = null;
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        var list = _subscribed;
        if (list is null || list.SelectedCount == 0)
        {
            Content = null;
            Bar = null;
            return;
        }
        var messages = Messages;
        var scopeAll = list.BulkScope == DataListActionScope.All;
        var describe = Describe();
        var visible = Actions.Where(a => IsShown(a, describe)).ToList();

        var count = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center };
        count.SetThemeColor(Label.TextColorProperty, KlyTheme.TextHighlighted);
        count.Text = scopeAll && list.MatchingCount is { } total
            ? string.Format(list.Culture, messages.SelectionAllResults, total.ToString("N0", list.Culture))
            : string.Format(list.Culture, messages.SelectionCount, list.SelectedCount.ToString("N0", list.Culture));
        SemanticProperties.SetDescription(count, count.Text);
        CountLabel = count;

        var row = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
        count.Margin = new Thickness(0, 0, 12, 0);
        row.Children.Add(count);

        ScopeButton = null;
        if (ShowScope && (scopeAll || list.CanSelectAllResults) && list.MatchingCount is { } matching)
        {
            var text = scopeAll
                ? string.Format(list.Culture, messages.SelectionOnlySelected, list.SelectedCount.ToString("N0", list.Culture))
                : string.Format(list.Culture, messages.SelectionSelectAllResults, matching.ToString("N0", list.Culture));
            var scope = new TableButton(text, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm) { Margin = new Thickness(0, 0, 8, 0) };
            scope.Clicked += (_, _) => list.SetBulkScope(scopeAll ? DataListActionScope.Selection : DataListActionScope.All);
            ScopeButton = scope;
            row.Children.Add(scope);
        }

        var buttons = new List<TableButton>();
        foreach (var action in visible.Take(Math.Max(0, MaxVisible)))
        {
            var current = action;
            var button = new TableButton(action.Label, action.Icon, action.Variant, action.Color, KlySize.Sm)
            {
                IsEnabled = !IsDisabled(action, describe),
                IsBusy = IsRunning(action),
                Margin = new Thickness(0, 0, 8, 0),
            };
            button.Clicked += (_, _) => _ = RequestAsync(current);
            buttons.Add(button);
            row.Children.Add(button);
        }
        ActionButtons = buttons;

        Overflow = null;
        var rest = visible.Skip(Math.Max(0, MaxVisible)).ToList();
        if (rest.Count > 0)
        {
            var trigger = new TableButton(null, KlyIconNames.Ellipsis, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { AccessibleName = messages.ActionsMore, Margin = new Thickness(0, 0, 8, 0) };
            var menu = new KlyDropdownMenu { Content = trigger, Placement = KlyPlacement.TopStart, AccessibleName = messages.ActionsMore };
            menu.Items = [.. rest.Select(a => new KlyMenuItem { Label = a.Label, Value = a.Key, Icon = a.Icon, IsEnabled = !IsDisabled(a, describe), Color = a.Color.Equals(KlyColor.Neutral) ? default(KlyColor?) : a.Color })];
            menu.ItemSelected += (_, item) => _ = Choose(item.Value ?? item.Label);
            Overflow = menu;
            row.Children.Add(menu);
        }

        var clear = new TableButton(messages.SelectionClear, KlyIconNames.Close, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm);
        clear.Clicked += (_, _) => list.ClearSelection();
        ClearButton = clear;
        row.Children.Add(clear);

        var bar = new Border { Padding = new Thickness(12, 8), Content = row, HorizontalOptions = LayoutOptions.Fill };
        bar.Apply(new TableSurface(KlyTheme.BackgroundElevated, KlyTheme.Border, 1, KlyTheme.Text));
        bar.SetRadius(KlyTheme.RadiusOf(1.5));
        SemanticProperties.SetDescription(bar, messages.SelectionActionsLabel);
        if (Position == DataListSelectionBarPosition.Fixed)
        {
            VerticalOptions = LayoutOptions.End;
            HorizontalOptions = LayoutOptions.Center;
        }
        else
        {
            VerticalOptions = LayoutOptions.Fill;
            HorizontalOptions = LayoutOptions.Fill;
        }
        Bar = bar;
        Content = bar;
    }
}
