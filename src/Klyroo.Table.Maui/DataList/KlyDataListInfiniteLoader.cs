namespace Klyroo.Table.Maui;

/// <summary>
/// The loader of a cursor-paged list: it appends the next page when the user scrolls near the end of the rows (<see cref="Auto"/>, the default) or when they tap the Load more button, shows the
/// error of a page that failed with a Retry button, and offers a way back to the first page for a list that was entered mid-way. Put it after the table, in the same scroll view; a virtualized table asks the list for the next page by itself.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListInfiniteLoader<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Auto"/>.</summary>
    public static readonly BindableProperty AutoProperty = BindableProperty.Create(nameof(Auto), typeof(bool), typeof(KlyDataListInfiniteLoader<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListInfiniteLoader<TItem>)b).AutoChanged());
    /// <summary>Identifies <see cref="Threshold"/>.</summary>
    public static readonly BindableProperty ThresholdProperty = BindableProperty.Create(nameof(Threshold), typeof(double), typeof(KlyDataListInfiniteLoader<TItem>), 320d);
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListInfiniteLoader<TItem>), "Load more", propertyChanged: (b, _, _) => ((KlyDataListInfiniteLoader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowEnd"/>.</summary>
    public static readonly BindableProperty ShowEndProperty = BindableProperty.Create(nameof(ShowEnd), typeof(bool), typeof(KlyDataListInfiniteLoader<TItem>), false, propertyChanged: (b, _, _) => ((KlyDataListInfiniteLoader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="EndText"/>.</summary>
    public static readonly BindableProperty EndTextProperty = BindableProperty.Create(nameof(EndText), typeof(string), typeof(KlyDataListInfiniteLoader<TItem>), "No more items", propertyChanged: (b, _, _) => ((KlyDataListInfiniteLoader<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="LoadingText"/>.</summary>
    public static readonly BindableProperty LoadingTextProperty = BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(KlyDataListInfiniteLoader<TItem>), "Loading more…", propertyChanged: (b, _, _) => ((KlyDataListInfiniteLoader<TItem>)b).Redraw());

    private ScrollView? _scroll;
    private DataListContext<TItem>? _watching;

    /// <summary>Creates a loader.</summary>
    public KlyDataListInfiniteLoader()
    {
        SizeChanged += (_, _) => CheckNearEnd();
        Refresh();
    }

    /// <summary>Loads the next page by itself when the end of the rows comes within <see cref="Threshold"/> of the viewport (<see langword="true"/> by default). Set to <see langword="false"/> for a Load more button only.</summary>
    public bool Auto { get => (bool)GetValue(AutoProperty); set => SetValue(AutoProperty, value); }

    /// <summary>How far, in points, from the bottom of the viewport the loader asks for the next page when <see cref="Auto"/> is on (320 by default).</summary>
    public double Threshold { get => (double)GetValue(ThresholdProperty); set => SetValue(ThresholdProperty, value); }

    /// <summary>Text of the Load more button.</summary>
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>Shows <see cref="EndText"/> once the last page has been loaded. It is announced to assistive technologies either way.</summary>
    public bool ShowEnd { get => (bool)GetValue(ShowEndProperty); set => SetValue(ShowEndProperty, value); }

    /// <summary>The text shown at the end of the rows.</summary>
    public string EndText { get => (string)GetValue(EndTextProperty); set => SetValue(EndTextProperty, value); }

    /// <summary>The text shown while the next page loads.</summary>
    public string LoadingText { get => (string)GetValue(LoadingTextProperty); set => SetValue(LoadingTextProperty, value); }

    internal TableButton? LoadMoreButton { get; private set; }

    internal TableButton? RetryButton { get; private set; }

    internal TableButton? BackButton { get; private set; }

    internal View? Body => Content;

    /// <summary>Loads the next page if the loader is near the end of the viewport.</summary>
    internal void CheckNearEnd()
    {
        if (!Auto || Context is not { HasMore: true, IsLoadingMore: false, HasLoadMoreError: false, PagingMode: DataListPagingMode.Cursor, IsLoading: false } context || context.Rows.Count == 0) return;
        if (_scroll is null || Height < 0 || !IsNear(_scroll)) return;
        _ = context.LoadMoreAsync();
    }

    private bool IsNear(ScrollView scroll)
    {
        double y = 0;
        Element? current = this;
        while (current is not null && !ReferenceEquals(current, scroll))
        {
            if (current is VisualElement element) y += element.Y;
            current = current.Parent;
        }
        return y - scroll.ScrollY - scroll.Height <= Threshold;
    }

    private void AutoChanged()
    {
        if (Context is { } context) context.AutoLoadMore = Auto;
        Redraw();
    }

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context)
    {
        _watching = context;
        context.AutoLoadMore = Auto;
        ParentChanged += OnParentChanged;
        WatchScroll();
    }

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context)
    {
        context.AutoLoadMore = false;
        ParentChanged -= OnParentChanged;
        StopWatchingScroll();
        _watching = null;
    }

    private void OnParentChanged(object? sender, EventArgs e) => WatchScroll();

    private void WatchScroll()
    {
        StopWatchingScroll();
        _scroll = TableSupport.FindAncestor<ScrollView>(this);
        if (_scroll is not null) _scroll.Scrolled += OnScrolled;
    }

    private void StopWatchingScroll()
    {
        if (_scroll is not null) _scroll.Scrolled -= OnScrolled;
        _scroll = null;
    }

    private void OnScrolled(object? sender, ScrolledEventArgs e) => CheckNearEnd();

    /// <inheritdoc />
    protected override void Refresh()
    {
        LoadMoreButton = RetryButton = BackButton = null;
        if (Context is not { PagingMode: DataListPagingMode.Cursor, Result: not null } context)
        {
            Content = null;
            return;
        }
        var messages = Messages;
        var enteredMidWay = !string.IsNullOrEmpty(context.State.Cursor);
        var stack = new VerticalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center };
        if (context.HasLoadMoreError)
        {
            var error = TableSupport.NewLabel(14, KlyTheme.Resolve(KlyColor.Error).Text);
            error.Text = messages.LoadMoreError;
            error.HorizontalTextAlignment = TextAlignment.Center;
            SemanticProperties.SetDescription(error, error.Text);
            var retry = new TableButton(messages.Retry, null, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { HorizontalOptions = LayoutOptions.Center };
            retry.Clicked += (_, _) => _ = context.LoadMoreAsync();
            RetryButton = retry;
            stack.Children.Add(error);
            stack.Children.Add(retry);
        }
        else if (context.Rows.Count > 0)
        {
            if (context.IsLoadingMore)
            {
                var loading = TableSupport.NewLabel(14, KlyTheme.TextMuted);
                loading.Text = LoadingText;
                loading.HorizontalTextAlignment = TextAlignment.Center;
                SemanticProperties.SetDescription(loading, LoadingText);
                stack.Children.Add(loading);
            }
            else if (context.HasMore && !Auto)
            {
                var more = new TableButton(Text, null, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { HorizontalOptions = LayoutOptions.Center };
                more.Clicked += (_, _) => _ = context.LoadMoreAsync();
                LoadMoreButton = more;
                stack.Children.Add(more);
            }
            else if (!context.HasMore && ShowEnd)
            {
                var end = TableSupport.NewLabel(12, KlyTheme.TextDimmed);
                end.Text = EndText;
                end.HorizontalTextAlignment = TextAlignment.Center;
                stack.Children.Add(end);
            }
        }
        if (enteredMidWay)
        {
            var back = new TableButton(messages.BackToStart, null, context.Rows.Count == 0 ? ButtonVariant.Outline : ButtonVariant.Ghost, KlyColor.Neutral, KlySize.Sm) { HorizontalOptions = LayoutOptions.Center };
            back.Clicked += (_, _) => context.SetState(s => s with { Cursor = null, Page = 1 });
            BackButton = back;
            stack.Children.Add(back);
        }
        Content = stack.Children.Count == 0 ? null : stack;
        // A page that did not fill the viewport leaves the end in sight: ask for the next one.
        if (_watching is not null) CheckNearEnd();
    }
}
