namespace Klyroo.Table.Maui;

/// <summary>
/// A panel with every filter of the schema, one section per filter with its editor (open for the filters that are on), a count of the active ones and a button that clears them. Put it in the body of a
/// <see cref="KlySlideover"/> or a <see cref="KlyModal"/> and set <see cref="DataListPartBase{TItem}.List"/>. With <see cref="DataListApplyMode.Submit"/> the editors stage their changes and the panel applies them all in one state change when the user taps Apply.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListFilterPanel<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="ApplyMode"/>.</summary>
    public static readonly BindableProperty ApplyModeProperty = BindableProperty.Create(nameof(ApplyMode), typeof(DataListApplyMode), typeof(KlyDataListFilterPanel<TItem>), DataListApplyMode.Live, propertyChanged: (b, _, _) => ((KlyDataListFilterPanel<TItem>)b).ApplyModeChanged());
    /// <summary>Identifies <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(KlyDataListFilterPanel<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListFilterPanel<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="CloseOnApply"/>.</summary>
    public static readonly BindableProperty CloseOnApplyProperty = BindableProperty.Create(nameof(CloseOnApply), typeof(bool), typeof(KlyDataListFilterPanel<TItem>), false);

    private readonly FilterDraftScope _scope = new();
    private readonly HashSet<string> _expanded = new(StringComparer.Ordinal);
    private readonly Dictionary<string, KlyDataListFilterEditor<TItem>> _editors = new(StringComparer.Ordinal);
    private HashSet<string> _activeBefore = new(StringComparer.Ordinal);
    private int _token;
    private string _signature = "";

    /// <summary>Creates a filter panel.</summary>
    public KlyDataListFilterPanel()
    {
        _scope.Changed += () => TableSupport.Post(this, UpdateButtons);
        Refresh();
    }

    /// <summary>Whether a change applies at once (<see cref="DataListApplyMode.Live"/>, the default) or waits for Apply, with Reset to drop it (<see cref="DataListApplyMode.Submit"/>).</summary>
    public DataListApplyMode ApplyMode { get => (DataListApplyMode)GetValue(ApplyModeProperty); set => SetValue(ApplyModeProperty, value); }

    /// <summary>A line under the title of the panel. Without it nothing is shown.</summary>
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <summary>Closes the <see cref="KlySlideover"/> or <see cref="KlyModal"/> around the panel when the filters apply (the Submit apply mode).</summary>
    public bool CloseOnApply { get => (bool)GetValue(CloseOnApplyProperty); set => SetValue(CloseOnApplyProperty, value); }

    internal TableButton? ApplyButton { get; private set; }

    internal TableButton? ResetButton { get; private set; }

    internal TableButton? ClearButton { get; private set; }

    internal IReadOnlyDictionary<string, KlyDataListFilterEditor<TItem>> Editors => _editors;

    internal IReadOnlyDictionary<string, TableTap> Headers => _headers;

    private readonly Dictionary<string, TableTap> _headers = new(StringComparer.Ordinal);

    internal bool IsExpanded(string key) => _expanded.Contains(key);

    private bool Submit => ApplyMode == DataListApplyMode.Submit;

    // What the editors staged changes the buttons, not the sections: the editors are left alone, so the field being typed in keeps the focus.
    private void UpdateButtons()
    {
        if (ApplyButton is not null) ApplyButton.IsEnabled = _scope.HasPending;
        if (ResetButton is not null) ResetButton.IsEnabled = _scope.HasPending;
    }

    private void ApplyModeChanged()
    {
        _scope.Deferred = Submit;
        if (!Submit) _scope.Clear();
        Redraw();
    }

    // A section with a filter on is open from the start, and opens when a filter turns on by other means (a tag). The sections the user opens or closes stay as they are.
    private void ExpandActive(DataListContext<TItem> context)
    {
        var active = context.ActiveFilters.Select(r => r.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var key in active)
        {
            if (!_activeBefore.Contains(key)) _expanded.Add(key);
        }
        _activeBefore = active;
    }

    // How many values of a filter are on: the selected options (and the include empty box), else one for the filter itself.
    private int CountOf(DataListContext<TItem> context, string key)
    {
        if (context.RuleFor(key) is not { } rule) return 0;
        if (context.FilterOf(key)?.Kind != FilterKind.Option) return 1;
        var values = rule.Value switch
        {
            FilterValue.List list => list.Values.Count,
            FilterValue.Text => 1,
            _ => 0,
        };
        return Math.Max(1, values + (rule.IncludeEmpty ? 1 : 0));
    }

    /// <summary>Applies what the editors staged, in one state change.</summary>
    internal void ApplyStaged()
    {
        if (Context is not { } context) return;
        var staged = _scope.Take();
        if (staged.Count > 0) context.SetState(state => staged.Aggregate(state, (current, change) => DataListContext<TItem>.WithFilter(current, change.Key, change.Value)));
        _token++;
        Redraw();
        if (CloseOnApply && TableSupport.FindAncestor<KlyOverlayBase>(this) is { } overlay) overlay.Hide();
    }

    /// <summary>Drops what the editors staged.</summary>
    internal void ResetStaged()
    {
        if (!_scope.HasPending) return;
        _scope.Clear();
        _token++;
        Redraw();
    }

    /// <summary>Takes every filter off: at once with Live, staged until Apply with Submit.</summary>
    internal void ClearAll()
    {
        if (Context is not { } context || (context.ActiveFilters.Count == 0 && !_scope.HasPending)) return;
        if (Submit)
        {
            foreach (var key in context.EditableFilterKeys)
            {
                if (context.RuleFor(key) is null) _scope.Unstage(key);
                else _scope.Stage(key, null);
            }
            _token++;
            Redraw();
            return;
        }
        _scope.Clear();
        _token++;
        context.SetState(DataListContext<TItem>.WithoutFilters);
    }

    internal void Toggle(string key)
    {
        if (!_expanded.Remove(key)) _expanded.Add(key);
        Redraw();
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        _scope.Deferred = Submit;
        if (Context is not { Schema: not null } context)
        {
            Content = null;
            return;
        }
        ExpandActive(context);
        var keys = context.EditableFilterKeys;
        // The editors keep the focus while the list changes under them: the sections are only built again when what they show changed.
        var signature = string.Join('|', keys) + "#" + string.Join('|', context.ActiveFilters.Select(r => r.Key + r.Operator)) + "#" + string.Join('|', _expanded.Order()) + "#" + ApplyMode + _token + "#" + Description + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(context.Messages);
        if (Content is not null && signature == _signature)
        {
            return;
        }
        _signature = signature;
        var messages = Messages;
        var root = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(0) };
        var title = TableSupport.NewLabel(16, KlyTheme.TextHighlighted, FontAttributes.Bold);
        title.Text = messages.FilterPanelTitle;
        SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level2);
        var header = new HorizontalStackLayout { Spacing = 8, Children = { title } };
        var count = context.ActiveFilters.Count;
        if (count > 0)
        {
            header.Children.Add(new KlyBadge { Text = TableSupport.Format(messages.FilterPanelActive, count), Color = KlyColor.Primary, Variant = KlyVariant.Soft, Size = KlySize.Sm });
        }
        root.Children.Add(header);
        if (!string.IsNullOrEmpty(Description))
        {
            var description = TableSupport.NewLabel(14, KlyTheme.TextMuted);
            description.Text = Description;
            root.Children.Add(description);
        }
        if (keys.Count == 0)
        {
            var none = TableSupport.NewLabel(14, KlyTheme.TextMuted);
            none.Text = messages.FilterPanelNone;
            root.Children.Add(none);
        }

        foreach (var old in _editors.Values) old.Release();
        _editors.Clear();
        _headers.Clear();
        foreach (var key in keys)
        {
            var current = key;
            var open = _expanded.Contains(key);
            var label = TableSupport.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
            label.Text = context.FilterLabel(key);
            var chevron = new KlyIcon { Name = open ? KlyIconNames.ChevronDown : KlyIconNames.ChevronRight, Size = KlySize.Sm, IconColor = KlyTheme.TextMuted };
            var row = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { chevron, label } };
            if (CountOf(context, key) is > 0 and var on) row.Children.Add(new KlyBadge { Text = on.ToString(context.Culture), Color = KlyColor.Primary, Variant = KlyVariant.Soft, Size = KlySize.Xs });
            var tap = new TableTap(row) { MinimumHeightRequest = 40, AccessibleName = label.Text };
            SemanticProperties.SetHint(tap, open ? "expanded" : "collapsed");
            tap.Tapped += (_, _) => Toggle(current);
            _headers[key] = tap;
            root.Children.Add(tap);
            if (!open) continue;
            var editor = new KlyDataListFilterEditor<TItem> { Key = key, ApplyMode = DataListApplyMode.Live, ShowTitle = false, ResetToken = _token, Scope = _scope, Margin = new Thickness(24, 0, 0, 8) };
            editor.BindExplicit(context);
            _editors[key] = editor;
            root.Children.Add(editor);
        }

        ApplyButton = ResetButton = ClearButton = null;
        var actions = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End };
        var clear = new TableButton(messages.ClearFilters, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm) { Margin = new Thickness(0, 0, 8, 0) };
        clear.Clicked += (_, _) => ClearAll();
        ClearButton = clear;
        actions.Children.Add(clear);
        if (Submit)
        {
            var reset = new TableButton(messages.FilterReset, null, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { Margin = new Thickness(0, 0, 8, 0), IsEnabled = _scope.HasPending };
            reset.Clicked += (_, _) => ResetStaged();
            var apply = new TableButton(messages.FilterApply, null, ButtonVariant.Solid, KlyColor.Primary, KlySize.Sm) { IsEnabled = _scope.HasPending };
            apply.Clicked += (_, _) => ApplyStaged();
            ResetButton = reset;
            ApplyButton = apply;
            actions.Children.Add(reset);
            actions.Children.Add(apply);
        }
        root.Children.Add(actions);
        Content = root;
    }
}
