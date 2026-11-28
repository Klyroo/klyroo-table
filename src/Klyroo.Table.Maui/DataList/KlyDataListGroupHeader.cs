namespace Klyroo.Table.Maui;

/// <summary>
/// The collapsible header bar of a visual row group inside <see cref="KlyDataListTable{TItem}"/> or <see cref="KlyDataListGrid{TItem}"/>: an optional tri-state checkbox that ticks or unticks every row of the group,
/// and a button with a chevron, the group label and the summary (the row count by default) that folds or unfolds the group.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListGroupHeader<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Group"/>.</summary>
    public static readonly BindableProperty GroupProperty = BindableProperty.Create(nameof(Group), typeof(DataListGroup<TItem>), typeof(KlyDataListGroupHeader<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGroupHeader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Collapsed"/>.</summary>
    public static readonly BindableProperty CollapsedProperty = BindableProperty.Create(nameof(Collapsed), typeof(bool?), typeof(KlyDataListGroupHeader<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGroupHeader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Selectable"/>.</summary>
    public static readonly BindableProperty SelectableProperty = BindableProperty.Create(nameof(Selectable), typeof(bool?), typeof(KlyDataListGroupHeader<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGroupHeader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ToggleLabel"/>.</summary>
    public static readonly BindableProperty ToggleLabelProperty = BindableProperty.Create(nameof(ToggleLabel), typeof(string), typeof(KlyDataListGroupHeader<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListGroupHeader<TItem>)b).Redraw());

    private DataListContext<TItem>? _subscribed;

    /// <summary>Creates a group header. Set <see cref="Group"/>.</summary>
    public KlyDataListGroupHeader()
    {
        Refresh();
    }

    /// <summary>Raised with the new state when the fold button is tapped. Without a handler the header folds the group on the list.</summary>
    public event EventHandler<bool>? Toggled;

    /// <summary>The group drawn by this header.</summary>
    public DataListGroup<TItem>? Group { get => (DataListGroup<TItem>?)GetValue(GroupProperty); set => SetValue(GroupProperty, value); }

    /// <summary>Overrides whether the group is currently collapsed. When <see langword="null"/>, reads <see cref="DataListContext{TItem}.IsGroupCollapsed"/>.</summary>
    public bool? Collapsed { get => (bool?)GetValue(CollapsedProperty); set => SetValue(CollapsedProperty, value); }

    /// <summary>Overrides whether the header shows a select-all-in-group checkbox. Defaults to the selection being on for the list.</summary>
    public bool? Selectable { get => (bool?)GetValue(SelectableProperty); set => SetValue(SelectableProperty, value); }

    /// <summary>Accessible name of the fold button. Defaults to the localized "Expand or collapse label".</summary>
    public string? ToggleLabel { get => (string?)GetValue(ToggleLabelProperty); set => SetValue(ToggleLabelProperty, value); }

    internal KlyCheckbox? Checkbox { get; private set; }

    internal TableTap? Toggle { get; private set; }

    internal Label? LabelView { get; private set; }

    internal Label? SummaryView { get; private set; }

    /// <summary>Does what a tap on the fold button does.</summary>
    internal void Fold()
    {
        if (Group is not { } group || Context is not { } context) return;
        var next = !(Collapsed ?? context.IsGroupCollapsed(group));
        if (Toggled is { } handler) handler(this, next);
        else context.ToggleGroup(group);
    }

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        _subscribed = context;
        context.SelectionChanged += Redraw;
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.SelectionChanged -= Redraw;
        _subscribed = null;
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        Checkbox = null;
        Toggle = null;
        if (Group is not { } group || _subscribed is not { } context)
        {
            Content = null;
            return;
        }
        var messages = Messages;
        var collapsed = Collapsed ?? context.IsGroupCollapsed(group);
        var selectable = Selectable ?? context.SelectionEnabled;
        var toggleLabel = !string.IsNullOrWhiteSpace(ToggleLabel) ? ToggleLabel : TableSupport.Format(messages.GroupToggle, group.Label);

        var row = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        if (selectable)
        {
            var state = context.GetGroupSelection(group);
            var box = new KlyCheckbox { IsChecked = state == true, IsEnabled = group.Items.Count > 0 };
            SemanticProperties.SetDescription(box, TableSupport.Format(messages.GroupSelect, group.Label));
            box.CheckedChanged += (_, e) =>
            {
                if (e.Value != (context.GetGroupSelection(group) == true)) context.SetGroupSelected(group, e.Value);
            };
            Checkbox = box;
            row.Children.Add(box);
        }
        var chevron = new KlyIcon { Name = collapsed ? KlyIconNames.ChevronRight : KlyIconNames.ChevronDown, Size = KlySize.Sm, IconColor = KlyTheme.TextMuted };
        var label = TableSupport.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
        label.Text = group.Label;
        LabelView = label;
        var inner = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { chevron, label } };
        SummaryView = null;
        if (!string.IsNullOrEmpty(group.Summary))
        {
            var summary = TableSupport.NewLabel(12, KlyTheme.TextMuted);
            summary.Text = group.Summary;
            SummaryView = summary;
            inner.Children.Add(summary);
        }
        var toggle = new TableTap(inner) { AccessibleName = toggleLabel, MinimumHeightRequest = 40 };
        SemanticProperties.SetHint(toggle, collapsed ? "collapsed" : "expanded");
        toggle.Tapped += (_, _) => Fold();
        Toggle = toggle;
        row.Children.Add(toggle);
        var bar = new Border { Content = row, Padding = new Thickness(8, 2), HorizontalOptions = LayoutOptions.Fill };
        bar.Apply(new TableSurface(KlyTheme.BackgroundMuted, KlyThemeColor.Transparent, 0, KlyTheme.Text));
        bar.SetRadius(KlyTheme.RadiusOf(0.5));
        SemanticProperties.SetHeadingLevel(bar, SemanticHeadingLevel.Level3);
        Content = bar;
    }
}
