namespace Klyroo.Table.Maui;

/// <summary>
/// One tag per active filter: "Status is Active, Paused +2". A tap opens the editor of the filter in a popover anchored to the tag, the x removes the filter. The tags are a list whose name says how many filters are on.
/// Needs a <see cref="KlyOverlayHost"/> above it for the popovers.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListFilterTags<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="ShowAdd"/>.</summary>
    public static readonly BindableProperty ShowAddProperty = BindableProperty.Create(nameof(ShowAdd), typeof(bool), typeof(KlyDataListFilterTags<TItem>), false, propertyChanged: (b, _, _) => ((KlyDataListFilterTags<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowClear"/>.</summary>
    public static readonly BindableProperty ShowClearProperty = BindableProperty.Create(nameof(ShowClear), typeof(bool), typeof(KlyDataListFilterTags<TItem>), false, propertyChanged: (b, _, _) => ((KlyDataListFilterTags<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ApplyMode"/>.</summary>
    public static readonly BindableProperty ApplyModeProperty = BindableProperty.Create(nameof(ApplyMode), typeof(DataListApplyMode), typeof(KlyDataListFilterTags<TItem>), DataListApplyMode.Live, propertyChanged: (b, _, _) => ((KlyDataListFilterTags<TItem>)b).ApplyModeChanged());

    private readonly FlexLayout _row = new() { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
    private readonly Dictionary<string, Tag> _tags = new(StringComparer.Ordinal);
    private KlyDataListAddFilter<TItem>? _add;
    private KlyDataListClearFilters<TItem>? _clear;

    /// <summary>Creates the filter tags.</summary>
    public KlyDataListFilterTags()
    {
        Refresh();
    }

    /// <summary>Shows the add filter button (<see cref="KlyDataListAddFilter{TItem}"/>) after the tags. False by default; the default layout of the list shows it.</summary>
    public bool ShowAdd { get => (bool)GetValue(ShowAddProperty); set => SetValue(ShowAddProperty, value); }

    /// <summary>Shows the clear filters button (<see cref="KlyDataListClearFilters{TItem}"/>) after the tags. False by default; the default layout of the list shows it.</summary>
    public bool ShowClear { get => (bool)GetValue(ShowClearProperty); set => SetValue(ShowClearProperty, value); }

    /// <summary>Whether the editors apply each change at once (<see cref="DataListApplyMode.Live"/>) or wait for an Apply button (<see cref="DataListApplyMode.Submit"/>).</summary>
    public DataListApplyMode ApplyMode { get => (DataListApplyMode)GetValue(ApplyModeProperty); set => SetValue(ApplyModeProperty, value); }

    /// <summary>The tags on screen, by filter key, for the tests.</summary>
    internal IReadOnlyDictionary<string, Tag> Tags => _tags;

    internal KlyDataListAddFilter<TItem>? AddButton => _add;

    internal KlyDataListClearFilters<TItem>? ClearButton => _clear;

    /// <summary>One tag: its pill, its popover and the editor in it. It survives the redraws of the list, so that a popover that is open stays open while its filter applies.</summary>
    internal sealed class Tag
    {
        public required Border Root { get; init; }
        public required Label Text { get; init; }
        public required TableTap Remove { get; init; }
        public KlyPopover? Popover { get; init; }
        public KlyDataListFilterEditor<TItem>? Editor { get; init; }
        public int Token { get; set; }
    }

    private void ApplyModeChanged()
    {
        foreach (var tag in _tags.Values)
        {
            if (tag.Editor is { } editor) editor.ApplyMode = ApplyMode;
        }
        _add?.ApplyMode = ApplyMode;
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { Schema: not null } context || (context.ActiveFilters.Count == 0 && !(ShowAdd && context.EditableFilterKeys.Count > 0)))
        {
            Content = null;
            foreach (var tag in _tags.Values) tag.Editor?.Release();
            _tags.Clear();
            _row.Children.Clear();
            return;
        }
        var messages = Messages;
        var active = context.ActiveFilters;
        _ = context.ResolveLabelsAsync(context.LifetimeToken);
        foreach (var stale in _tags.Keys.Where(k => active.All(r => r.Key != k)).ToList())
        {
            _tags[stale].Editor?.Release();
            _tags.Remove(stale);
        }

        var children = new List<IView>();
        foreach (var rule in active)
        {
            var key = rule.Key;
            if (!_tags.TryGetValue(key, out var tag)) _tags[key] = tag = NewTag(context, key);
            var label = context.FilterLabel(key);
            var (op, value) = context.Summarize(rule);
            tag.Text.FormattedText = TagText(label, op, value);
            SemanticProperties.SetDescription(tag.Root, TableSupport.Format(messages.EditFilter, label));
            SemanticProperties.SetDescription(tag.Remove, TableSupport.Format(messages.RemoveFilter, label));
            if (tag.Editor is { } editor) editor.ApplyMode = ApplyMode;
            children.Add(tag.Root);
        }
        if (ShowAdd)
        {
            _add ??= new KlyDataListAddFilter<TItem> { Margin = new Thickness(0, 0, 8, 8) };
            _add.ApplyMode = ApplyMode;
            children.Add(_add);
        }
        if (ShowClear)
        {
            _clear ??= new KlyDataListClearFilters<TItem> { Margin = new Thickness(0, 0, 8, 8) };
            children.Add(_clear);
        }
        // The children are only touched when they changed: a popover that is open must stay in the tree.
        if (!_row.Children.SequenceEqual(children))
        {
            _row.Children.Clear();
            foreach (var child in children) _row.Children.Add(child);
        }
        SemanticProperties.SetDescription(_row, TableSupport.Format(messages.FilterTagsLabel, active.Count.ToString(context.Culture)));
        Content = _row;
    }

    private static FormattedString TagText(string label, string op, string value)
    {
        var text = new FormattedString();
        text.Spans.Add(new Span { Text = label, FontAttributes = FontAttributes.Bold });
        text.Spans.Add(new Span { Text = " " + op + (value.Length > 0 ? " " : "") });
        if (value.Length > 0) text.Spans.Add(new Span { Text = value, FontAttributes = FontAttributes.Bold });
        return text;
    }

    private Tag NewTag(DataListContext<TItem> context, string key)
    {
        var text = new Label { FontSize = 12, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
        text.SetThemeColor(Label.TextColorProperty, KlyTheme.Text);
        var close = new KlyIcon { Name = KlyIconNames.Close, Size = KlySize.Xs, IconColor = KlyTheme.TextMuted, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var remove = new TableTap(close) { WidthRequest = 32, HeightRequest = 32, VerticalOptions = LayoutOptions.Center };
        remove.Tapped += (_, _) => context.ApplyFilter(key, null);

        KlyPopover? popover = null;
        KlyDataListFilterEditor<TItem>? editor = null;
        View label = text;
        var filter = context.FilterOf(key);
        var tag = (Tag?)null;
        if (filter is not null && DataListContext<TItem>.HasEditor(filter))
        {
            var trigger = new TableTap(new ContentView { Content = text, Padding = new Thickness(10, 0, 4, 0) }) { MinimumHeightRequest = 32, VerticalOptions = LayoutOptions.Center };
            editor = new KlyDataListFilterEditor<TItem> { Key = key, ApplyMode = ApplyMode, ShowTitle = false, Margin = new Thickness(12) };
            editor.BindExplicit(context);
            popover = new KlyPopover { Content = trigger, PanelContent = editor, Placement = KlyPlacement.BottomStart };
            popover.Closed += (_, _) =>
            {
                if (_tags.TryGetValue(key, out var current) && current.Editor is { } e) e.ResetToken = ++current.Token;
            };
            editor.Done += (_, _) => popover.IsOpen = false;
            label = popover;
        }
        else
        {
            text.Margin = new Thickness(10, 0, 4, 0);
        }
        var row = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { label, remove } };
        var root = new Border { Content = row, Margin = new Thickness(0, 0, 8, 8), HorizontalOptions = LayoutOptions.Start };
        root.Apply(new TableSurface(KlyTheme.BackgroundElevated, KlyTheme.Border, 1, KlyTheme.Text));
        root.SetRadius(KlyTheme.RadiusOf(1));
        tag = new Tag { Root = root, Text = text, Remove = remove, Popover = popover, Editor = editor };
        return tag;
    }
}
