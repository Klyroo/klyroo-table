using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// The editor of one filter of the schema, with the controls of its kind: a text field, a list of options (with counts when the list asked for them, a search box, and a remote source when the filter has one),
/// a yes / no / any choice, number fields or date fields (with the presets of the operator). It writes the rule the list filters by: at once with <see cref="DataListApplyMode.Live"/>, or when the user taps Apply
/// with <see cref="DataListApplyMode.Submit"/>. The filter tags and the add filter button show one in a popover; a <see cref="KlyDataListFilterPanel{TItem}"/> shows one per filter.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListFilterEditor<TItem> : DataListPartBase<TItem>
{
    private const int VisibleChoices = 8;

    /// <summary>Identifies <see cref="Key"/>.</summary>
    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(KlyDataListFilterEditor<TItem>), "", propertyChanged: (b, _, _) => ((KlyDataListFilterEditor<TItem>)b).Reload());
    /// <summary>Identifies <see cref="ApplyMode"/>.</summary>
    public static readonly BindableProperty ApplyModeProperty = BindableProperty.Create(nameof(ApplyMode), typeof(DataListApplyMode), typeof(KlyDataListFilterEditor<TItem>), DataListApplyMode.Live, propertyChanged: (b, _, _) => ((KlyDataListFilterEditor<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowTitle"/>.</summary>
    public static readonly BindableProperty ShowTitleProperty = BindableProperty.Create(nameof(ShowTitle), typeof(bool), typeof(KlyDataListFilterEditor<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListFilterEditor<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ResetToken"/>.</summary>
    public static readonly BindableProperty ResetTokenProperty = BindableProperty.Create(nameof(ResetToken), typeof(int), typeof(KlyDataListFilterEditor<TItem>), 0, propertyChanged: (b, _, _) => ((KlyDataListFilterEditor<TItem>)b).Reload());

    private DataListFilter? _filter;
    private string _label = "";
    private FilterRule? _seen;
    private FilterOperator _op;
    private string _text = "";
    private string _search = "";
    private List<string> _selected = [];
    private bool _includeEmpty;
    private bool? _bool;
    private decimal? _from;
    private decimal? _to;
    private DateOnly? _day;
    private DateOnly? _dayTo;
    private TimeOnly? _time;
    private TimeOnly? _timeTo;
    private bool _loaded;
    private bool _showAll;
    private bool _syncing;
    private CancellationTokenSource? _debounce;
    private CancellationTokenSource? _remote;
    private readonly List<DataListFilterOption> _remoteItems = [];
    private bool _remoteMore;
    private string? _remoteCursor;
    private bool _remoteLoading;
    private string? _remoteError;
    private bool _remoteStarted;
    private int _structure;
    private object? _signature;

    /// <summary>Creates an editor. Set <see cref="Key"/>.</summary>
    public KlyDataListFilterEditor()
    {
        Refresh();
    }

    /// <summary>Raised after Apply or Cancel (the Submit apply mode), so that the popover around the editor closes.</summary>
    public event EventHandler? Done;

    /// <summary>The key of the filter to edit: one of the filters of the schema of the list (any kind; a date filter exact to the second has no editor).</summary>
    public string Key { get => (string)GetValue(KeyProperty); set => SetValue(KeyProperty, value); }

    /// <summary>Whether a change applies at once (<see cref="DataListApplyMode.Live"/>, the default) or waits for the Apply button (<see cref="DataListApplyMode.Submit"/>).</summary>
    public DataListApplyMode ApplyMode { get => (DataListApplyMode)GetValue(ApplyModeProperty); set => SetValue(ApplyModeProperty, value); }

    /// <summary>Shows the name of the filter above the editor. True by default.</summary>
    public bool ShowTitle { get => (bool)GetValue(ShowTitleProperty); set => SetValue(ShowTitleProperty, value); }

    /// <summary>Change it to drop what was typed and show the filter as the list has it: the popover around the editor does it each time it closes.</summary>
    public int ResetToken { get => (int)GetValue(ResetTokenProperty); set => SetValue(ResetTokenProperty, value); }

    /// <summary>Set by a filter panel: in the Submit apply mode the changes are staged there instead of applied.</summary>
    internal FilterDraftScope? Scope { get; set; }

    /// <summary>The rule the draft stands for, or <see langword="null"/> when the draft filters nothing.</summary>
    internal FilterRule? Draft => _filter is null ? null : BuildRule();

    internal TableButton? ApplyButton { get; private set; }

    internal TableButton? CancelButton { get; private set; }

    internal KlyInput? TextInput { get; private set; }

    internal KlySelect? OperatorSelect { get; private set; }

    internal KlySelect? BooleanSelect { get; private set; }

    internal KlyInputNumber? FromInput { get; private set; }

    internal KlyInputNumber? ToInput { get; private set; }

    internal KlyInputDate? DayInput { get; private set; }

    internal KlyInputDate? DayToInput { get; private set; }

    internal IReadOnlyList<KlyCheckbox> OptionBoxes { get; private set; } = [];

    internal KlyCheckbox? IncludeEmptyBox { get; private set; }

    internal KlyInput? SearchInput { get; private set; }

    internal IReadOnlyList<TableButton> PresetButtons { get; private set; } = [];

    private bool Live => ApplyMode == DataListApplyMode.Live;

    private FilterRule? Rule => Context?.RuleFor(Key);

    private IReadOnlyList<FilterOperator> Operators => Context!.EditorOperators(Key, Rule?.Operator ?? _op);

    private bool IsRange => _op == FilterOperator.Between;

    private bool Timed => _filter!.Granularity != DateGranularity.Day;

    /// <summary>Drops the draft and loads the rule of the list.</summary>
    private void Reload()
    {
        _loaded = false;
        Redraw();
    }

    // ---- the draft

    // Whether the draft was loaded from the list again (the first time, a reset, or a rule that changed without this editor).
    private bool SyncDraft()
    {
        if (Context is null)
        {
            _filter = null;
            return false;
        }
        _filter = Context.FilterOf(Key);
        _label = _filter is null ? "" : Context.FilterLabel(Key);
        if (_filter is null || !DataListContext<TItem>.HasEditor(_filter)) return false;
        var rule = Rule;
        if (!_loaded)
        {
            _loaded = true;
            // A panel that staged a change for this filter (its Clear filters) shows the staged rule, and goes on staging it.
            if (Scope is not null && Scope.TryGetStaged(Key, out var staged)) Load(staged, rule, unstage: false);
            else Load(rule, rule, unstage: true);
            return true;
        }
        if (Equals(rule, _seen)) return false;
        _seen = rule;
        // What this editor sent has come back: nothing is pending any more.
        if (Equals(rule, BuildRule()))
        {
            Scope?.Unstage(Key);
            return false;
        }
        Load(rule, rule, unstage: true);
        return true;
    }

    // <paramref name="seen"/> is the rule of the list the draft starts from: it differs from the draft when the panel staged a change.
    private void Load(FilterRule? rule, FilterRule? seen, bool unstage)
    {
        if (unstage) Scope?.Unstage(Key);
        _seen = seen;
        _op = rule?.Operator is { } op && _filter!.AllowedOperators.Contains(op) ? op : _filter!.DefaultOperator;
        _text = rule?.Value is FilterValue.Text text && _filter!.Kind == FilterKind.Text ? text.Value : "";
        _selected = rule?.Value switch
        {
            FilterValue.Text t when _filter!.Kind == FilterKind.Option => [t.Value],
            FilterValue.List list => [.. list.Values],
            _ => [],
        };
        _includeEmpty = rule?.IncludeEmpty ?? false;
        _bool = rule?.Value is FilterValue.Boolean b ? (rule.Operator == FilterOperator.IsNot ? !b.Value : b.Value) : null;
        _search = "";
        _showAll = false;
        (_from, _to) = (null, null);
        if (_filter!.Kind == FilterKind.Number)
        {
            switch (rule?.Value)
            {
                case FilterValue.Number n:
                    _from = n.Value;
                    break;
                case FilterValue.Range range:
                    _from = (range.From as FilterValue.Number)?.Value;
                    _to = (range.To as FilterValue.Number)?.Value;
                    break;
            }
        }
        (_day, _dayTo, _time, _timeTo) = (null, null, null, null);
        if (_filter.Kind == FilterKind.Date)
        {
            var zone = Context!.Zone;
            if (rule?.Value is FilterValue.Range range)
            {
                if (DataListDates.Bound(range.From, zone) is { } from) (_day, _time) = (from.Day, from.Time);
                if (DataListDates.Bound(range.To, zone) is { } to) (_dayTo, _timeTo) = (to.Day, to.Time);
            }
            else if (DataListDates.Bound(rule?.Value, zone) is { } one)
            {
                (_day, _time) = (one.Day, one.Time);
            }
        }
    }

    // The rule the draft stands for, or null when the draft filters nothing.
    private FilterRule? BuildRule() => _filter!.Kind switch
    {
        FilterKind.Text => _text.Trim() is { Length: > 0 } text ? new FilterRule(Key, _op, new FilterValue.Text(text)) : null,
        FilterKind.Option => OptionRule(),
        FilterKind.Boolean => _bool is { } value ? new FilterRule(Key, FilterOperator.Is, new FilterValue.Boolean(value)) : null,
        FilterKind.Number => NumberRule(),
        FilterKind.Date => DateRule(),
        _ => null,
    };

    private FilterRule? NumberRule()
    {
        if (!IsRange) return _from is { } one ? new FilterRule(Key, _op, new FilterValue.Number(one)) : null;
        return _from is null && _to is null ? null
            : new FilterRule(Key, _op, new FilterValue.Range(_from is { } from ? new FilterValue.Number(from) : null, _to is { } to ? new FilterValue.Number(to) : null));
    }

    // A bound of a date rule: a day without a time (a whole local day), or the instant of the day and time in the zone of the user.
    private FilterValue? DateBound(DateOnly? day, TimeOnly? time)
    {
        if (day is not { } d) return null;
        if (!Timed || time is not { } t) return new FilterValue.Date(d);
        return new FilterValue.Moment(DataListDates.Moment(d, t, Context!.Zone, _filter!.Granularity));
    }

    private FilterRule? DateRule()
    {
        if (!IsRange) return DateBound(_day, _time) is { } one ? new FilterRule(Key, _op, one) : null;
        var from = DateBound(_day, _time);
        var to = DateBound(_dayTo, _timeTo);
        return from is null && to is null ? null : new FilterRule(Key, _op, new FilterValue.Range(from, to));
    }

    // The shapes the state gives back: one value with <c>is</c> is a text, anything else a list.
    private FilterRule? OptionRule()
    {
        if (_selected.Count == 0 && !_includeEmpty) return null;
        FilterValue value = _op == FilterOperator.Is && _selected.Count == 1 ? new FilterValue.Text(_selected[0]) : new FilterValue.List([.. _selected]);
        return new FilterRule(Key, _op, value, _includeEmpty);
    }

    // ---- what the controls report

    /// <summary>Applies the draft: with the Live mode at each change, with Submit when Apply is tapped.</summary>
    private void Changed(bool text = false)
    {
        if (_syncing || !Live) return;
        if (text)
        {
            _debounce?.Cancel();
            var cts = _debounce = new CancellationTokenSource();
            _ = Task.Delay(300, cts.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled) TableSupport.Post(this, Commit);
            }, TaskScheduler.Default);
            return;
        }
        Commit();
    }

    /// <summary>Writes the draft to the list (or stages it for a panel that applies everything at once).</summary>
    internal void Commit()
    {
        if (Context is not { } context || _filter is null) return;
        var rule = BuildRule();
        if (Scope is { Deferred: true })
        {
            if (Equals(rule, Rule)) Scope.Unstage(Key);
            else Scope.Stage(Key, rule);
            return;
        }
        if (Equals(rule, _seen)) return;
        _seen = rule;
        context.ApplyFilter(Key, rule);
    }

    internal void Apply()
    {
        _debounce?.Cancel();
        Commit();
        Done?.Invoke(this, EventArgs.Empty);
    }

    internal void Cancel()
    {
        _debounce?.Cancel();
        _loaded = false;
        Redraw();
        Done?.Invoke(this, EventArgs.Empty);
    }

    private void SetOperator(FilterOperator op)
    {
        if (op == _op) return;
        var wasRange = IsRange;
        _op = op;
        if (wasRange != IsRange)
        {
            if (wasRange)
            {
                _from ??= _to;
                _day ??= _dayTo;
                _time ??= _timeTo;
            }
            (_to, _dayTo, _timeTo) = (null, null, null);
        }
        if (_filter!.Kind == FilterKind.Option && op == FilterOperator.Is && _selected.Count > 1) _selected = [_selected[0]];
        _structure++;
        Redraw();
        Changed();
    }

    private void Toggle(string value, bool on)
    {
        if (_syncing) return;
        if (_op == FilterOperator.Is) _selected = on ? [value] : [];
        else if (on) { if (!_selected.Contains(value)) _selected.Add(value); }
        else _selected.Remove(value);
        if (_op == FilterOperator.Is)
        {
            _structure++;
            Redraw();
        }
        Changed();
    }

    /// <summary>Ticks or unticks an option, as a tap on its box does.</summary>
    /// <param name="value">The value of the option.</param>
    /// <param name="on">Whether it is ticked.</param>
    internal void SetOption(string value, bool on) => Toggle(value, on);

    internal void SetText(string text)
    {
        _text = text;
        Changed(text: true);
    }

    internal void SetFrom(decimal? value)
    {
        _from = value;
        Changed();
    }

    internal void SetTo(decimal? value)
    {
        _to = value;
        Changed();
    }

    internal void SetBoolean(bool? value)
    {
        _bool = value;
        Changed();
    }

    internal void SetDay(DateOnly? value, bool end = false)
    {
        if (end) _dayTo = value;
        else _day = value;
        Changed();
    }

    internal void SetTime(TimeOnly? value, bool end = false)
    {
        if (end) _timeTo = value;
        else _time = value;
        Changed();
    }

    internal void SetOperatorTo(FilterOperator op) => SetOperator(op);

    internal void ApplyPreset(DatePreset preset)
    {
        var today = Context!.Today;
        if (IsRange)
        {
            (_day, _dayTo) = DataListDates.Range(preset, today);
            (_time, _timeTo) = (null, null);
            if (_filter!.MaxRangeDays is { } max && _dayTo is { } end && _day is { } start && end.DayNumber - start.DayNumber + 1 > max) _day = end.AddDays(1 - max);
        }
        else
        {
            _day = DataListDates.Day(preset, today);
            _time = null;
        }
        _structure++;
        Redraw();
        Changed();
    }

    // ---- the view

    /// <inheritdoc />
    protected override void Refresh()
    {
        var reloaded = SyncDraft();
        if (Context is null || _filter is null || !DataListContext<TItem>.HasEditor(_filter))
        {
            ResetControls();
            Content = null;
            _signature = null;
            return;
        }
        // The list changes at every keystroke of a live editor: the controls are only built again when what they show changed, so that the field the user types in keeps the focus.
        var signature = (Context, Key, ApplyMode, ShowTitle, _filter, _op, _filter.Kind == FilterKind.Option ? Context.ResultVersion : 0, _structure);
        if (!reloaded && Content is not null && Equals(signature, _signature)) return;
        _signature = signature;
        ResetControls();
        _syncing = true;
        try
        {
            Content = Build();
        }
        finally
        {
            _syncing = false;
        }
        if (_filter.RemoteOptions is not null && !_remoteStarted)
        {
            _remoteStarted = true;
            _ = LoadRemoteAsync(reset: true);
        }
    }

    private void ResetControls()
    {
        ApplyButton = CancelButton = null;
        TextInput = null;
        OperatorSelect = BooleanSelect = null;
        FromInput = ToInput = null;
        DayInput = DayToInput = null;
        OptionBoxes = [];
        IncludeEmptyBox = null;
        SearchInput = null;
        PresetButtons = [];
    }

    private View Build()
    {
        var messages = Messages;
        var culture = Context!.Culture;
        var root = new VerticalStackLayout { Spacing = 8, MinimumWidthRequest = 240 };
        if (ShowTitle)
        {
            var title = TableSupport.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
            title.Text = _label;
            SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level3);
            root.Children.Add(title);
        }

        var operators = Operators;
        if (_filter!.Kind != FilterKind.Boolean && operators.Count > 1)
        {
            var select = new KlySelect { Size = KlySize.Sm, AccessibleName = TableSupport.Format(messages.FilterOperatorFor, _label) };
            var items = operators.Select(op => new OperatorItem(op, messages.OperatorLabel(op))).ToList();
            select.ItemsSource = items;
            select.SelectedItem = items.FirstOrDefault(i => i.Op == _op);
            select.SelectedItemChanged += (_, _) =>
            {
                if (!_syncing && select.SelectedItem is OperatorItem item) SetOperator(item.Op);
            };
            OperatorSelect = select;
            root.Children.Add(select);
        }

        switch (_filter.Kind)
        {
            case FilterKind.Text:
                var input = new KlyInput { Text = _text, Placeholder = messages.FilterTextPlaceholder, AccessibleName = TableSupport.Format(messages.FilterValueFor, _label), Size = KlySize.Sm };
                input.TextChanged += (_, e) =>
                {
                    if (!_syncing) SetText(e.NewTextValue ?? "");
                };
                input.Completed += (_, _) => Changed();
                TextInput = input;
                root.Children.Add(input);
                break;
            case FilterKind.Option:
                root.Children.Add(BuildOptions(messages, culture));
                break;
            case FilterKind.Boolean:
                root.Children.Add(BuildBoolean(messages));
                break;
            case FilterKind.Number:
                BuildNumber(root, messages, culture);
                break;
            case FilterKind.Date:
                BuildDate(root, messages);
                break;
        }

        if (!Live)
        {
            var row = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.End };
            var cancel = new TableButton(messages.FilterCancel, null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm);
            cancel.Clicked += (_, _) => Cancel();
            var apply = new TableButton(messages.FilterApply, null, ButtonVariant.Solid, KlyColor.Primary, KlySize.Sm);
            apply.Clicked += (_, _) => Apply();
            (CancelButton, ApplyButton) = (cancel, apply);
            row.Children.Add(cancel);
            row.Children.Add(apply);
            root.Children.Add(row);
        }
        return root;
    }

    private sealed record OperatorItem(FilterOperator Op, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record BooleanItem(bool? Value, string Label)
    {
        public override string ToString() => Label;
    }

    private View BuildBoolean(KlyToolsMessages messages)
    {
        var select = new KlySelect { Size = KlySize.Sm, AccessibleName = TableSupport.Format(messages.FilterValueFor, _label) };
        var items = new List<BooleanItem> { new(null, messages.FilterAny), new(true, messages.FilterYes), new(false, messages.FilterNo) };
        select.ItemsSource = items;
        select.SelectedItem = items.First(i => i.Value == _bool);
        select.SelectedItemChanged += (_, _) =>
        {
            if (!_syncing && select.SelectedItem is BooleanItem item) SetBoolean(item.Value);
        };
        BooleanSelect = select;
        return select;
    }

    private View BuildNumber(VerticalStackLayout root, KlyToolsMessages messages, CultureInfo culture)
    {
        KlyInputNumber Field(decimal? value, string accessible, Action<decimal?> set)
        {
            var number = new KlyInputNumber { Value = value, Min = _filter!.Min, Max = _filter.Max, Step = _filter.Step ?? 1, Size = KlySize.Sm, Culture = culture, AccessibleName = accessible };
            number.ValueChanged += (_, e) =>
            {
                if (!_syncing) set(e.NewValue);
            };
            return number;
        }
        var from = Field(_from, TableSupport.Format(IsRange ? messages.FilterFromFor : messages.FilterValueFor, _label), SetFrom);
        FromInput = from;
        root.Children.Add(from);
        if (IsRange)
        {
            var to = Field(_to, TableSupport.Format(messages.FilterToFor, _label), SetTo);
            ToInput = to;
            root.Children.Add(to);
        }
        return root;
    }

    private void BuildDate(VerticalStackLayout root, KlyToolsMessages messages)
    {
        var presets = new List<TableButton>();
        var presetRow = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        foreach (var preset in DataListDates.PresetsFor(_op))
        {
            var current = preset;
            var button = new TableButton(PresetLabel(messages, preset), null, ButtonVariant.Soft, KlyColor.Neutral, KlySize.Xs) { Margin = new Thickness(0, 0, 6, 6) };
            button.Clicked += (_, _) => ApplyPreset(current);
            presets.Add(button);
            presetRow.Children.Add(button);
        }
        PresetButtons = presets;
        SemanticProperties.SetDescription(presetRow, TableSupport.Format(messages.FilterPresetsFor, _label));
        root.Children.Add(presetRow);

        KlyInputDate Day(DateOnly? value, string accessible, bool end)
        {
            var date = new KlyInputDate { Value = value?.ToDateTime(TimeOnly.MinValue), Size = KlySize.Sm, AccessibleName = accessible };
            date.ValueChanged += (_, e) =>
            {
                if (!_syncing) SetDay(e.NewValue is { } v ? DateOnly.FromDateTime(v) : null, end);
            };
            return date;
        }
        void AddTime(TimeOnly? value, string accessible, bool end)
        {
            if (!Timed) return;
            var time = new KlyInputTime { Value = value, Size = KlySize.Sm, AccessibleName = accessible };
            time.ValueChanged += (_, e) =>
            {
                if (!_syncing) SetTime(e.NewValue, end);
            };
            root.Children.Add(time);
        }

        var from = Day(_day, TableSupport.Format(IsRange ? messages.FilterFromFor : messages.FilterValueFor, _label), end: false);
        DayInput = from;
        root.Children.Add(from);
        AddTime(_time, TableSupport.Format(IsRange ? messages.FilterFromTimeFor : messages.FilterTimeFor, _label), end: false);
        if (IsRange)
        {
            var to = Day(_dayTo, TableSupport.Format(messages.FilterToFor, _label), end: true);
            DayToInput = to;
            root.Children.Add(to);
            AddTime(_timeTo, TableSupport.Format(messages.FilterToTimeFor, _label), end: true);
        }
        if (_filter!.MaxRangeDays is { } max && IsRange)
        {
            var hint = TableSupport.NewLabel(12, KlyTheme.TextMuted);
            hint.Text = TableSupport.Format(messages.FilterMaxRangeDays, max);
            root.Children.Add(hint);
        }
    }

    private static string PresetLabel(KlyToolsMessages messages, DatePreset preset) => preset switch
    {
        DatePreset.Today => messages.DatePresetToday,
        DatePreset.Yesterday => messages.DatePresetYesterday,
        DatePreset.StartOfMonth => messages.DatePresetStartOfMonth,
        DatePreset.SevenDaysAgo => messages.DatePreset7DaysAgo,
        DatePreset.StartOfYear => messages.DatePresetStartOfYear,
        DatePreset.Last7Days => messages.DatePresetLast7Days,
        DatePreset.Last30Days => messages.DatePresetLast30Days,
        DatePreset.ThisMonth => messages.DatePresetThisMonth,
        DatePreset.LastMonth => messages.DatePresetLastMonth,
        DatePreset.YearToDate => messages.DatePresetYearToDate,
        _ => preset.ToString(),
    };

    // ---- the options

    private IReadOnlyList<TableFilterChoice> Choices()
    {
        var context = Context!;
        if (_filter!.RemoteOptions is null) return context.FilterChoices(Key, _selected, context.Culture);
        context.LearnLabels(Key, _remoteItems);
        var choices = new List<TableFilterChoice>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var option in _remoteItems)
        {
            if (seen.Add(option.Value)) choices.Add(new TableFilterChoice(option.Value, option.Label, null));
        }
        foreach (var value in _selected)
        {
            if (seen.Add(value)) choices.Add(new TableFilterChoice(value, context.OptionLabelOf(Key, value), null));
        }
        return choices;
    }

    private View BuildOptions(KlyToolsMessages messages, CultureInfo culture)
    {
        var column = new VerticalStackLayout { Spacing = 4 };
        var choices = Choices();
        var remote = _filter!.RemoteOptions;
        SearchInput = null;
        if (remote is not null || choices.Count > VisibleChoices)
        {
            var search = new KlyInput { Text = _search, Placeholder = messages.FilterOptionsSearch, AccessibleName = messages.FilterOptionsSearch, Size = KlySize.Sm, ReturnType = ReturnType.Search };
            search.TextChanged += (_, e) =>
            {
                if (_syncing) return;
                _search = e.NewTextValue ?? "";
                if (remote is not null) _ = SearchRemoteAsync();
                else RebuildOptions();
            };
            SearchInput = search;
            column.Children.Add(search);
        }
        _optionList = new VerticalStackLayout { Spacing = 4 };
        column.Children.Add(_optionList);
        FillOptions(_optionList, messages, culture, choices);
        return column;
    }

    private VerticalStackLayout? _optionList;

    // The list of options, drawn again when the search or the remote page changes: the search box above it is not touched, so it keeps the focus.
    private void FillOptions(VerticalStackLayout column, KlyToolsMessages messages, CultureInfo culture, IReadOnlyList<TableFilterChoice> choices)
    {
        column.Children.Clear();
        var remote = _filter!.RemoteOptions;
        var visible = remote is not null ? choices : choices.Where(Matches).ToList();
        var shown = remote is null && !_showAll && _search.Length == 0 ? visible.Take(VisibleChoices).ToList() : visible;
        var boxes = new List<KlyCheckbox>();
        foreach (var choice in shown)
        {
            var value = choice.Value;
            var box = new KlyCheckbox
            {
                IsChecked = _selected.Contains(value),
                Text = choice.Count is { } count ? $"{choice.Label} ({count.ToString("N0", culture)})" : choice.Label,
            };
            box.CheckedChanged += (_, e) =>
            {
                if (!_syncing && e.Value != _selected.Contains(value)) Toggle(value, e.Value);
            };
            boxes.Add(box);
            column.Children.Add(box);
        }
        OptionBoxes = boxes;

        if (visible.Count == 0)
        {
            var none = TableSupport.NewLabel(14, KlyTheme.TextMuted);
            none.Text = _search.Length > 0 ? messages.FilterNoOptionsMatch : _remoteLoading ? messages.Loading : messages.FilterNoOptions;
            column.Children.Add(none);
        }
        else if (shown.Count < visible.Count)
        {
            var more = new TableButton(TableSupport.Format(messages.FilterOptionsMore, visible.Count - shown.Count), null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs);
            more.Clicked += (_, _) =>
            {
                _showAll = true;
                RebuildOptions();
            };
            column.Children.Add(more);
        }
        if (remote is not null && _remoteMore)
        {
            var more = new TableButton(_remoteLoading ? messages.Loading : "…", null, ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Xs) { IsBusy = _remoteLoading };
            more.Clicked += (_, _) => _ = LoadRemoteAsync(reset: false);
            column.Children.Add(more);
        }
        if (_remoteError is not null)
        {
            var error = TableSupport.NewLabel(12, KlyTheme.Resolve(KlyColor.Error).Text);
            error.Text = _remoteError;
            var retry = new TableButton(messages.Retry, null, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Xs);
            retry.Clicked += (_, _) => _ = LoadRemoteAsync(reset: true);
            column.Children.Add(error);
            column.Children.Add(retry);
        }

        var missing = Context!.MissingCountOf(Key);
        if (missing is null or > 0 || _includeEmpty)
        {
            var empty = new KlyCheckbox
            {
                IsChecked = _includeEmpty,
                Text = missing is { } m ? $"{messages.FilterIncludeEmpty} ({m.ToString("N0", culture)})" : messages.FilterIncludeEmpty,
            };
            empty.CheckedChanged += (_, e) =>
            {
                if (_syncing) return;
                _includeEmpty = e.Value;
                Changed();
            };
            IncludeEmptyBox = empty;
            column.Children.Add(empty);
        }
    }

    private bool Matches(TableFilterChoice choice) =>
        _search.Length == 0 || Context!.Culture.CompareInfo.IndexOf(choice.Label, _search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private void RebuildOptions()
    {
        if (_optionList is null || Context is null || _filter is null) return;
        _syncing = true;
        try
        {
            FillOptions(_optionList, Messages, Context.Culture, Choices());
        }
        finally
        {
            _syncing = false;
        }
    }

    // ---- the remote options

    private async Task SearchRemoteAsync()
    {
        _remote?.Cancel();
        var cts = _remote = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        await LoadRemoteAsync(reset: true);
    }

    private async Task LoadRemoteAsync(bool reset)
    {
        if (_filter?.RemoteOptions is not { } remote || Context is null) return;
        var term = _search.Trim();
        if (reset)
        {
            _remote?.Cancel();
            _remoteItems.Clear();
            _remoteCursor = null;
            _remoteMore = false;
        }
        if (term.Length > 0 && term.Length < remote.MinLength)
        {
            RebuildOptions();
            return;
        }
        var cts = _remote = new CancellationTokenSource();
        _remoteLoading = true;
        _remoteError = null;
        RebuildOptions();
        try
        {
            var page = await remote.Provider(new DataListOptionsRequest(term, _remoteItems.Count, remote.PageSize, _remoteCursor), cts.Token);
            if (cts.IsCancellationRequested) return;
            var known = _remoteItems.Select(o => o.Value).ToHashSet(StringComparer.Ordinal);
            _remoteItems.AddRange(page.Items.Where(o => known.Add(o.Value)));
            _remoteCursor = page.NextCursor;
            _remoteMore = page.HasMore || page.NextCursor is not null;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            _remoteError = exception.Message;
        }
        _remoteLoading = false;
        RebuildOptions();
    }
}
