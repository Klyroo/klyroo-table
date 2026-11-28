namespace Klyroo.Table.Maui;

/// <summary>
/// A menu to sort a list from outside its table headers, for the card and grid layouts that have none: the trigger names the active sort, the menu lists the sortable fields with an arrow on the active ones,
/// and choosing one sorts ascending, then descending, then back to the default sort. With <see cref="Multiple"/> a choice adds a field to the sort instead of replacing it. Needs a <see cref="KlyOverlayHost"/> above it.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListSortMenu<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListSortMenu<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSortMenu<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(KlySize), typeof(KlyDataListSortMenu<TItem>), KlySize.Sm, propertyChanged: (b, _, _) => ((KlyDataListSortMenu<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Items"/>.</summary>
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IReadOnlyList<DataListSortOption>), typeof(KlyDataListSortMenu<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSortMenu<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Layouts"/>.</summary>
    public static readonly BindableProperty LayoutsProperty = BindableProperty.Create(nameof(Layouts), typeof(IReadOnlyList<DataListLayout>), typeof(KlyDataListSortMenu<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSortMenu<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Multiple"/>.</summary>
    public static readonly BindableProperty MultipleProperty = BindableProperty.Create(nameof(Multiple), typeof(bool?), typeof(KlyDataListSortMenu<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSortMenu<TItem>)b).Redraw());

    private KlyDropdownMenu? _menu;
    private TableButton? _trigger;

    /// <summary>Creates a sort menu.</summary>
    public KlyDataListSortMenu()
    {
        Refresh();
    }

    /// <summary>Label of the trigger button when no sort is active. Defaults to the localized "Sort".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>Size of the trigger button (small by default).</summary>
    public KlySize Size { get => (KlySize)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <summary>Explicit sortable field options. When <see langword="null"/>, the menu lists the sortable columns in user order followed by any other sortable fields of the schema.</summary>
    public IReadOnlyList<DataListSortOption>? Items { get => (IReadOnlyList<DataListSortOption>?)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

    /// <summary>Layouts in which the sort menu is shown (for example <c>[DataListLayout.Grid]</c> when the table headers already sort). When <see langword="null"/>, shown in every layout, and in the table when it shows summary cards.</summary>
    public IReadOnlyList<DataListLayout>? Layouts { get => (IReadOnlyList<DataListLayout>?)GetValue(LayoutsProperty); set => SetValue(LayoutsProperty, value); }

    /// <summary>Whether a choice adds the field to the sort (<see langword="true"/>) or replaces the sort (<see langword="false"/>). <see langword="null"/>, the default, replaces it until the list sorts by more than one field, then adds.</summary>
    public bool? Multiple { get => (bool?)GetValue(MultipleProperty); set => SetValue(MultipleProperty, value); }

    internal KlyDropdownMenu? Menu => _menu;

    internal TableButton? Trigger => _trigger;

    private bool ShouldShow(DataListContext<TItem> context) =>
        Layouts is null || Layouts.Contains(context.EffectiveLayout) || (context.EffectiveLayout == DataListLayout.Table && context.SummaryViewActive);

    private bool Appending(DataListContext<TItem> context) => Multiple ?? context.EffectiveSort.Count > 1;

    /// <summary>Does what choosing an entry of the menu does: <c>~default</c> clears the sort, any other value is the key of a sortable field.</summary>
    /// <param name="value">The value of the entry.</param>
    internal void Choose(string value)
    {
        if (Context is not { } context) return;
        if (value == DefaultValue) context.SetState(context.ClearSort);
        else context.SetState(state => context.NextSort(state, value, Appending(context)));
    }

    private const string DefaultValue = "~default";

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { Schema: not null } context || !ShouldShow(context))
        {
            Content = null;
            return;
        }
        var options = context.SortOptions(Items);
        if (options.Count == 0)
        {
            Content = null;
            return;
        }
        var messages = Messages;
        var active = context.EffectiveSort.Select(s => options.FirstOrDefault(o => o.Key == s.Key)).Where(o => o is not null).ToList();
        var text = active.Count switch
        {
            0 => Text ?? messages.Sort,
            1 => TableSupport.Format(messages.SortBy, active[0]!.Label ?? TableValues.Humanize(active[0]!.Key)),
            var n => TableSupport.Format(messages.SortMultiple, n),
        };
        _trigger = new TableButton(text, TableIcons.ArrowUpDown, ButtonVariant.Outline, KlyColor.Neutral, Size) { AccessibleName = text };
        var items = new List<KlyMenuItem> { KlyMenuItem.Heading(Text ?? messages.Sort) };
        foreach (var option in options)
        {
            var descending = context.SortDescending(option.Key);
            var arrow = descending is null ? "" : descending == true ? "  ↓" : "  ↑";
            items.Add(new KlyMenuItem
            {
                Label = (option.Label ?? TableValues.Humanize(option.Key)) + arrow,
                Value = option.Key,
                Icon = descending is null ? null : descending == true ? KlyIconNames.ArrowDown : KlyIconNames.ArrowUp,
                Description = descending is null ? null : descending == true ? messages.SortDirectionDescending : messages.SortDirectionAscending,
            });
        }
        items.Add(KlyMenuItem.Separator());
        items.Add(new KlyMenuItem { Label = messages.SortDefault, Value = DefaultValue, IsEnabled = !context.IsDefaultSort(context.State) });
        if (_menu is null)
        {
            _menu = new KlyDropdownMenu { Placement = KlyPlacement.BottomStart };
            _menu.ItemSelected += (_, item) => Choose(item.Value ?? item.Label);
        }
        _menu.Content = _trigger;
        _menu.AccessibleName = Text ?? messages.Sort;
        _menu.Items = items;
        Content = _menu;
    }
}
