namespace Klyroo.Table.Maui;

/// <summary>
/// A button that opens a panel to show and hide the columns of the table, change their order and pin them to the start or the end of the row. It lists the keyed columns that can be hidden (a required column is not listed),
/// with a search box when there are many, and shows how many are visible. Needs a <see cref="KlyOverlayHost"/> above it.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListColumnPanel<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListColumnPanel<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListColumnPanel<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(KlySize), typeof(KlyDataListColumnPanel<TItem>), KlySize.Sm, propertyChanged: (b, _, _) => ((KlyDataListColumnPanel<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="AllowReorder"/>.</summary>
    public static readonly BindableProperty AllowReorderProperty = BindableProperty.Create(nameof(AllowReorder), typeof(bool), typeof(KlyDataListColumnPanel<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListColumnPanel<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="AllowPinning"/>.</summary>
    public static readonly BindableProperty AllowPinningProperty = BindableProperty.Create(nameof(AllowPinning), typeof(bool), typeof(KlyDataListColumnPanel<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListColumnPanel<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="SearchThreshold"/>.</summary>
    public static readonly BindableProperty SearchThresholdProperty = BindableProperty.Create(nameof(SearchThreshold), typeof(int), typeof(KlyDataListColumnPanel<TItem>), 30, propertyChanged: (b, _, _) => ((KlyDataListColumnPanel<TItem>)b).Redraw());

    private readonly KlyPopover _popover = new() { Placement = KlyPlacement.BottomStart };
    private TableButton? _trigger;
    private DataListContext<TItem>? _subscribed;
    private string _search = "";
    private string _announcement = "";

    /// <summary>Creates a column panel button.</summary>
    public KlyDataListColumnPanel()
    {
        Content = _popover;
        Refresh();
    }

    /// <summary>The label of the button and the heading of the panel. Without it the localized "Columns".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>The size of the button. Defaults to small.</summary>
    public KlySize Size { get => (KlySize)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <summary>Shows the buttons that move a column earlier or later (true by default).</summary>
    public bool AllowReorder { get => (bool)GetValue(AllowReorderProperty); set => SetValue(AllowReorderProperty, value); }

    /// <summary>Shows the buttons that pin a column to the start or the end of the row (true by default).</summary>
    public bool AllowPinning { get => (bool)GetValue(AllowPinningProperty); set => SetValue(AllowPinningProperty, value); }

    /// <summary>The number of columns from which the panel has a search box (30 by default).</summary>
    public int SearchThreshold { get => (int)GetValue(SearchThresholdProperty); set => SetValue(SearchThresholdProperty, value); }

    internal KlyPopover Popover => _popover;

    internal TableButton? Trigger => _trigger;

    internal IReadOnlyList<KlyCheckbox> Boxes { get; private set; } = [];

    internal IReadOnlyDictionary<string, (TableButton? Up, TableButton? Down, TableButton? Pin)> RowButtons { get; private set; } = new Dictionary<string, (TableButton?, TableButton?, TableButton?)>();

    internal TableButton? ShowAllButton { get; private set; }

    internal TableButton? HideAllButton { get; private set; }

    internal TableButton? ResetButton { get; private set; }

    internal string Announcement => _announcement;

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        _subscribed = context;
        context.ColumnsChanged += Redraw;
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.ColumnsChanged -= Redraw;
        _subscribed = null;
    }

    private void Say(string text)
    {
        _announcement = text;
        TableSupport.Announce(text);
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (_subscribed is not { } context || context.Columns.Count(c => c.ResolvedKey is not null) == 0)
        {
            IsVisible = false;
            return;
        }
        IsVisible = true;
        var messages = Messages;
        var label = Text ?? messages.Columns;
        _trigger ??= new TableButton(null, TableIcons.Columns, ButtonVariant.Outline, KlyColor.Neutral, Size);
        _trigger.Text = label;
        _popover.Content = _trigger;
        _popover.AccessibleName = label;
        _popover.PanelContent = BuildPanel(context, messages, label);
    }

    private View BuildPanel(DataListContext<TItem> context, KlyToolsMessages messages, string label)
    {
        var states = context.ColumnStates;
        var panel = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(12), MinimumWidthRequest = 260 };
        var title = TableSupport.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
        title.Text = label;
        SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level3);
        var visibleCount = states.Count(s => s.Visible);
        var count = TableSupport.NewLabel(12, KlyTheme.TextMuted);
        count.Text = TableSupport.Format(messages.ColumnsVisible, visibleCount, states.Count);
        panel.Children.Add(title);
        panel.Children.Add(count);

        if (states.Count >= SearchThreshold)
        {
            var search = new KlyInput { Text = _search, Placeholder = messages.ColumnsSearch, AccessibleName = messages.ColumnsSearch, Size = KlySize.Sm };
            search.TextChanged += (_, e) =>
            {
                _search = e.NewTextValue ?? "";
                _popover.PanelContent = BuildPanel(context, messages, label);
            };
            panel.Children.Add(search);
        }

        var tools = new HorizontalStackLayout { Spacing = 6 };
        var showAll = new TableButton(messages.ColumnsShowAll, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs);
        showAll.Clicked += (_, _) =>
        {
            context.SetAllColumnsVisible(true);
            Say(messages.ColumnsAllShown);
        };
        var hideAll = new TableButton(messages.ColumnsHideAll, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs);
        hideAll.Clicked += (_, _) =>
        {
            context.SetAllColumnsVisible(false);
            Say(messages.ColumnsAllHidden);
        };
        var reset = new TableButton(messages.ColumnsReset, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs) { IsEnabled = context.HasCustomColumnPreferences };
        reset.Clicked += (_, _) =>
        {
            context.ResetColumns();
            Say(messages.ColumnsWereReset);
        };
        (ShowAllButton, HideAllButton, ResetButton) = (showAll, hideAll, reset);
        tools.Children.Add(showAll);
        tools.Children.Add(hideAll);
        tools.Children.Add(reset);
        panel.Children.Add(tools);

        var boxes = new List<KlyCheckbox>();
        var buttons = new Dictionary<string, (TableButton?, TableButton?, TableButton?)>();
        var listed = states.Where(s => !s.Required && (_search.Length == 0 || s.Title.Contains(_search, StringComparison.CurrentCultureIgnoreCase))).ToList();
        foreach (var state in listed)
        {
            var key = state.Key;
            var column = context.Columns.First(c => c.ResolvedKey == key);
            var row = new Grid { ColumnSpacing = 4, ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto)] };
            var box = new KlyCheckbox { Text = state.Title, IsChecked = state.Visible, IsEnabled = !state.Visible || context.CanHideColumn(column) };
            box.CheckedChanged += (_, e) =>
            {
                if (e.Value == context.ColumnStates.First(s => s.Key == key).Visible) return;
                context.SetColumnVisible(key, e.Value);
                var now = context.ColumnStates.First(s => s.Key == key).Visible;
                Say(TableSupport.Format(now ? messages.ColumnShown : messages.ColumnHidden, state.Title));
            };
            boxes.Add(box);
            row.Add(box, 0);
            TableButton? up = null, down = null, pin = null;
            if (AllowReorder)
            {
                var index = states.ToList().FindIndex(s => s.Key == key);
                up = new TableButton(null, KlyIconNames.ArrowUp, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs) { AccessibleName = TableSupport.Format(messages.ColumnMoveUp, state.Title), IsEnabled = index > 0 };
                down = new TableButton(null, KlyIconNames.ArrowDown, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs) { AccessibleName = TableSupport.Format(messages.ColumnMoveDown, state.Title), IsEnabled = index < states.Count - 1 };
                up.Clicked += (_, _) => Move(context, key, state.Title, -1, messages);
                down.Clicked += (_, _) => Move(context, key, state.Title, 1, messages);
                row.Add(up, 1);
                row.Add(down, 2);
            }
            if (AllowPinning)
            {
                var next = state.Pinned switch { DataListPin.None => DataListPin.Start, DataListPin.Start => DataListPin.End, _ => DataListPin.None };
                var name = next switch
                {
                    DataListPin.Start => TableSupport.Format(messages.ColumnPinStart, state.Title),
                    DataListPin.End => TableSupport.Format(messages.ColumnPinEnd, state.Title),
                    _ => TableSupport.Format(messages.ColumnUnpin, state.Title),
                };
                pin = new TableButton(null, state.Pinned == DataListPin.None ? TableIcons.Pin : TableIcons.PinOff, state.Pinned == DataListPin.None ? ButtonVariant.Ghost : ButtonVariant.Soft, KlyColor.Neutral, KlySize.Xs) { AccessibleName = name };
                pin.Clicked += (_, _) =>
                {
                    context.SetColumnPinned(key, next);
                    Say(TableSupport.Format(next switch { DataListPin.Start => messages.ColumnPinnedToStart, DataListPin.End => messages.ColumnPinnedToEnd, _ => messages.ColumnUnpinned }, state.Title));
                };
                row.Add(pin, 3);
            }
            buttons[key] = (up, down, pin);
            panel.Children.Add(row);
        }
        Boxes = boxes;
        RowButtons = buttons;
        return panel;
    }

    private void Move(DataListContext<TItem> context, string key, string title, int delta, KlyToolsMessages messages)
    {
        context.MoveColumn(key, delta);
        var states = context.ColumnStates;
        var position = states.ToList().FindIndex(s => s.Key == key) + 1;
        Say(TableSupport.Format(messages.ColumnMoved, title, position, states.Count));
    }
}
