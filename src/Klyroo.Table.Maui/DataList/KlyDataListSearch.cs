namespace Klyroo.Table.Maui;

/// <summary>
/// The search field of a data list. The list searches after a pause in the typing (<see cref="Debounce"/>) or when the user presses the return key, and goes back to the first page.
/// The clear button, shown while the field has text, empties the field and the search.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListSearch<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(KlyDataListSearch<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSearch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(KlyDataListSearch<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListSearch<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Debounce"/>.</summary>
    public static readonly BindableProperty DebounceProperty = BindableProperty.Create(nameof(Debounce), typeof(int), typeof(KlyDataListSearch<TItem>), 300);
    /// <summary>Identifies <see cref="MinLength"/>.</summary>
    public static readonly BindableProperty MinLengthProperty = BindableProperty.Create(nameof(MinLength), typeof(int), typeof(KlyDataListSearch<TItem>), 0);
    /// <summary>Identifies <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(KlySize), typeof(KlyDataListSearch<TItem>), KlySize.Md, propertyChanged: (b, _, v) => ((KlyDataListSearch<TItem>)b)._input.Size = (KlySize)v);

    private readonly KlyInput _input = new() { ReturnType = ReturnType.Search, Keyboard = Keyboard.Default };
    private readonly TableTap _clear;
    private CancellationTokenSource? _debounce;
    private string _text = "";
    private bool _pending;
    private bool _syncing;

    /// <summary>Creates a search field.</summary>
    public KlyDataListSearch()
    {
        var close = new KlyIcon { Name = KlyIconNames.Close, Size = KlySize.Sm, IconColor = KlyTheme.TextMuted, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _clear = new TableTap(close) { WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, IsVisible = false };
        _clear.Tapped += (_, _) => ClearText();
        _input.TextChanged += (_, e) => OnText(e.NewTextValue);
        _input.Completed += (_, _) => ApplyNow();
        _input.Margin = new Thickness(0, 0, 0, 0);
        Content = new Grid { Children = { _input, _clear } };
        Redraw();
    }

    /// <summary>The placeholder of the field. Without it, the localized "Search…".</summary>
    public string? Placeholder { get => (string?)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    /// <summary>The accessible name of the field. Without it, "Search label" for a list with a label, else the localized "Search".</summary>
    public string? AccessibleName { get => (string?)GetValue(AccessibleNameProperty); set => SetValue(AccessibleNameProperty, value); }

    /// <summary>Milliseconds of quiet after the last keystroke before the list searches (300 by default). The return key searches at once; zero searches on every keystroke.</summary>
    public int Debounce { get => (int)GetValue(DebounceProperty); set => SetValue(DebounceProperty, value); }

    /// <summary>The fewest characters (once the spaces around the text are trimmed) a search must have: a shorter text searches nothing, so the rows come back as if the field were empty. Zero by default.</summary>
    public int MinLength { get => (int)GetValue(MinLengthProperty); set => SetValue(MinLengthProperty, value); }

    /// <summary>The size of the field. Defaults to medium.</summary>
    public KlySize Size { get => (KlySize)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    internal KlyInput Input => _input;

    internal TableTap ClearButton => _clear;

    /// <summary>Types text as the user does: the list searches after the pause.</summary>
    internal void Type(string text) => _input.Text = text;

    /// <summary>Searches now with the text of the field.</summary>
    internal void ApplyNow()
    {
        _debounce?.Cancel();
        _pending = false;
        var term = Effective(_text);
        Context?.SetState(s => s.Search == term ? s : s with { Search = term, Page = 1, Cursor = null });
    }

    private string Effective(string text)
    {
        var trimmed = text.Trim();
        return trimmed.Length >= MinLength ? trimmed : "";
    }

    private void OnText(string? value)
    {
        _text = value ?? "";
        _clear.IsVisible = _text.Length > 0;
        if (_syncing) return;
        _debounce?.Cancel();
        _pending = true;
        var delay = Math.Max(0, Debounce);
        if (delay == 0)
        {
            ApplyNow();
            return;
        }
        var cts = _debounce = new CancellationTokenSource();
        _ = Task.Delay(delay, cts.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled) TableSupport.Post(this, ApplyNow);
        }, TaskScheduler.Default);
    }

    private void ClearText()
    {
        _input.Text = "";
        ApplyNow();
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        var name = AccessibleName ?? (Context?.Label is { } label ? TableSupport.Format(Messages.SearchFor, label) : Messages.Search);
        _input.Placeholder = Placeholder ?? Messages.SearchPlaceholder;
        _input.AccessibleName = name;
        _clear.AccessibleName = Messages.ClearSearch;
        SemanticProperties.SetDescription(this, name);
        IsEnabled = Context is not null;
        // A search that changed without this field (a reset button, a bound state) is written into the field; what the user is typing stays.
        if (Context is { } context && !_pending && context.State.Search != Effective(_text) && context.State.Search != _input.Text)
        {
            _syncing = true;
            try
            {
                _input.Text = context.State.Search;
            }
            finally
            {
                _syncing = false;
            }
            _text = context.State.Search;
            _clear.IsVisible = _text.Length > 0;
        }
    }
}
