namespace Klyroo.Table.Maui;

/// <summary>
/// A button that adds a filter: it opens a popover that lists the filters of the schema that are not on (with a search box when there are many), and shows the editor of the one the user picks, with a way back to the list.
/// Needs a <see cref="KlyOverlayHost"/> above it.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListAddFilter<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="ApplyMode"/>.</summary>
    public static readonly BindableProperty ApplyModeProperty = BindableProperty.Create(nameof(ApplyMode), typeof(DataListApplyMode), typeof(KlyDataListAddFilter<TItem>), DataListApplyMode.Live, propertyChanged: (b, _, _) => ((KlyDataListAddFilter<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(KlySize), typeof(KlyDataListAddFilter<TItem>), KlySize.Sm, propertyChanged: (b, _, _) => ((KlyDataListAddFilter<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="SearchFrom"/>.</summary>
    public static readonly BindableProperty SearchFromProperty = BindableProperty.Create(nameof(SearchFrom), typeof(int), typeof(KlyDataListAddFilter<TItem>), 8, propertyChanged: (b, _, _) => ((KlyDataListAddFilter<TItem>)b).Redraw());

    private readonly KlyPopover _popover = new() { Placement = KlyPlacement.BottomStart };
    private TableButton? _trigger;
    private string? _picked;
    private string _search = "";
    private int _token;

    /// <summary>Creates an add filter button.</summary>
    public KlyDataListAddFilter()
    {
        _popover.Closed += (_, _) =>
        {
            _picked = null;
            _search = "";
            _token++;
            Redraw();
        };
        Content = _popover;
        Refresh();
    }

    /// <summary>Whether the editor applies each change at once (<see cref="DataListApplyMode.Live"/>) or waits for an Apply button (<see cref="DataListApplyMode.Submit"/>).</summary>
    public DataListApplyMode ApplyMode { get => (DataListApplyMode)GetValue(ApplyModeProperty); set => SetValue(ApplyModeProperty, value); }

    /// <summary>The size of the button. Defaults to small.</summary>
    public KlySize Size { get => (KlySize)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <summary>The number of filters from which the list has a search box (8 by default).</summary>
    public int SearchFrom { get => (int)GetValue(SearchFromProperty); set => SetValue(SearchFromProperty, value); }

    internal KlyPopover Popover => _popover;

    internal TableButton? Trigger => _trigger;

    internal IReadOnlyList<TableTap> Choices { get; private set; } = [];

    internal KlyDataListFilterEditor<TItem>? Editor { get; private set; }

    internal string? Picked => _picked;

    /// <summary>The filters that are not on and have an editor.</summary>
    internal IReadOnlyList<string> Available => Context is null ? [] : [.. Context.EditableFilterKeys.Where(k => Context.RuleFor(k) is null)];

    /// <summary>Picks a filter, as a tap on its name does.</summary>
    /// <param name="key">The key of the filter.</param>
    internal void Pick(string key)
    {
        _picked = key;
        Redraw();
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { Schema: not null } context || context.EditableFilterKeys.Count == 0)
        {
            Visible(false);
            return;
        }
        Visible(true);
        var messages = Messages;
        _trigger ??= new TableButton(null, KlyIconNames.Plus, ButtonVariant.Outline, KlyColor.Neutral, Size);
        _trigger.Text = messages.AddFilter;
        _popover.Content = _trigger;
        _popover.AccessibleName = messages.AddFilter;
        _popover.PanelContent = BuildPanel(context, messages);
    }

    private void Visible(bool visible)
    {
        IsVisible = visible;
        if (!visible) _popover.PanelContent = null;
    }

    private View BuildPanel(DataListContext<TItem> context, KlyToolsMessages messages)
    {
        Choices = [];
        Editor?.Release();
        Editor = null;
        var panel = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(12), MinimumWidthRequest = 240 };
        if (_picked is { } key && context.FilterOf(key) is not null)
        {
            var back = new TableButton(messages.AddFilterBack, KlyIconNames.ChevronLeft, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs);
            back.Clicked += (_, _) =>
            {
                _picked = null;
                Redraw();
            };
            var editor = new KlyDataListFilterEditor<TItem> { Key = key, ApplyMode = ApplyMode, ResetToken = _token };
            editor.BindExplicit(context);
            editor.Done += (_, _) => _popover.IsOpen = false;
            Editor = editor;
            panel.Children.Add(back);
            panel.Children.Add(editor);
            return panel;
        }
        var available = Available;
        if (available.Count == 0)
        {
            var none = TableSupport.NewLabel(14, KlyTheme.TextMuted);
            none.Text = messages.AddFilterNone;
            panel.Children.Add(none);
            return panel;
        }
        if (available.Count >= SearchFrom)
        {
            var search = new KlyInput { Text = _search, Placeholder = messages.AddFilterSearch, AccessibleName = messages.AddFilterSearch, Size = KlySize.Sm };
            search.TextChanged += (_, e) =>
            {
                _search = e.NewTextValue ?? "";
                _popover.PanelContent = BuildPanel(context, messages);
            };
            panel.Children.Add(search);
        }
        var taps = new List<TableTap>();
        foreach (var candidate in available.Where(k => _search.Length == 0 || context.FilterLabel(k).Contains(_search, StringComparison.CurrentCultureIgnoreCase)))
        {
            var current = candidate;
            var label = TableSupport.NewLabel(14, KlyTheme.Text);
            label.Text = context.FilterLabel(candidate);
            var tap = new TableTap(label) { MinimumHeightRequest = 40, AccessibleName = label.Text };
            tap.Tapped += (_, _) => Pick(current);
            taps.Add(tap);
            panel.Children.Add(tap);
        }
        Choices = taps;
        return panel;
    }
}
