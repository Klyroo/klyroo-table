namespace Klyroo.Table.Maui;

/// <summary>Numbered pages for a list that pages by offset, with a picker of the number of rows per page. Built on <see cref="KlyPagination"/>.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListPagination<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="PageSizes"/>.</summary>
    public static readonly BindableProperty PageSizesProperty = BindableProperty.Create(nameof(PageSizes), typeof(int[]), typeof(KlyDataListPagination<TItem>), new[] { 10, 25, 50, 100 }, propertyChanged: (b, _, _) => ((KlyDataListPagination<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="ShowPageSize"/>.</summary>
    public static readonly BindableProperty ShowPageSizeProperty = BindableProperty.Create(nameof(ShowPageSize), typeof(bool), typeof(KlyDataListPagination<TItem>), true, propertyChanged: (b, _, _) => ((KlyDataListPagination<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="SiblingCount"/>.</summary>
    public static readonly BindableProperty SiblingCountProperty = BindableProperty.Create(nameof(SiblingCount), typeof(int), typeof(KlyDataListPagination<TItem>), 1, propertyChanged: (b, _, v) => ((KlyDataListPagination<TItem>)b)._pagination.SiblingCount = (int)v);
    /// <summary>Identifies <see cref="ShowFirstLast"/>.</summary>
    public static readonly BindableProperty ShowFirstLastProperty = BindableProperty.Create(nameof(ShowFirstLast), typeof(bool), typeof(KlyDataListPagination<TItem>), false, propertyChanged: (b, _, v) => ((KlyDataListPagination<TItem>)b)._pagination.ShowFirstLast = (bool)v);
    /// <summary>Identifies <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(KlyDataListPagination<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListPagination<TItem>)b).Redraw());

    private readonly KlyPagination _pagination = new() { Size = KlySize.Sm };
    private readonly KlySelect _sizes = new() { Size = KlySize.Sm, WidthRequest = 96 };
    private readonly Label _sizeLabel = TableSupport.NewLabel(14, KlyTheme.TextMuted);
    private readonly HorizontalStackLayout _sizeRow = new() { Spacing = 8, VerticalOptions = LayoutOptions.Center };
    private bool _syncing;

    /// <summary>Creates a pagination.</summary>
    public KlyDataListPagination()
    {
        _pagination.PageChanged += (_, page) =>
        {
            if (!_syncing) Context?.SetState(s => s with { Page = page });
        };
        _sizes.SelectedItemChanged += (_, _) =>
        {
            if (!_syncing && _sizes.SelectedItem is int size && size > 0) Context?.SetState(s => s with { PageSize = size, Page = 1, Cursor = null });
        };
        _sizeRow.Children.Add(_sizeLabel);
        _sizeRow.Children.Add(_sizes);
        var root = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, JustifyContent = Microsoft.Maui.Layouts.FlexJustify.SpaceBetween, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
        root.Children.Add(_pagination);
        root.Children.Add(_sizeRow);
        Content = root;
        Refresh();
    }

    /// <summary>The page sizes of the picker. The current size and sizes above the schema's maximum are handled for you.</summary>
    public int[] PageSizes { get => (int[])GetValue(PageSizesProperty); set => SetValue(PageSizesProperty, value); }

    /// <summary>Shows the page size picker.</summary>
    public bool ShowPageSize { get => (bool)GetValue(ShowPageSizeProperty); set => SetValue(ShowPageSizeProperty, value); }

    /// <summary>Pages shown on each side of the current one.</summary>
    public int SiblingCount { get => (int)GetValue(SiblingCountProperty); set => SetValue(SiblingCountProperty, value); }

    /// <summary>Buttons that jump to the first and the last page.</summary>
    public bool ShowFirstLast { get => (bool)GetValue(ShowFirstLastProperty); set => SetValue(ShowFirstLastProperty, value); }

    /// <summary>The accessible name of the pagination. Give each list of a page its own. Without it the name is "Pagination, label" for a list with a label, else the localized "Pagination".</summary>
    public string? AccessibleName { get => (string?)GetValue(AccessibleNameProperty); set => SetValue(AccessibleNameProperty, value); }

    internal KlyPagination Pagination => _pagination;

    internal KlySelect SizePicker => _sizes;

    // Without a count from the provider, the pages known so far: the next one exists when this one is full.
    private int Total
    {
        get
        {
            if (Context?.Result is not { } result) return 0;
            if (result.TotalCount is { } total) return total;
            var size = Math.Max(1, Context.State.PageSize);
            return (Context.State.Page - 1) * size + result.Items.Count + (result.Items.Count >= size ? 1 : 0);
        }
    }

    private IEnumerable<int> Sizes
    {
        get
        {
            var current = Context?.State.PageSize ?? 0;
            var max = Context?.Schema?.MaxPageSize ?? int.MaxValue;
            return PageSizes.Append(current).Where(s => s > 0 && (s <= max || s == current)).Distinct().Order();
        }
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        _syncing = true;
        try
        {
            var label = Context?.Label;
            _pagination.AccessibleName = AccessibleName ?? (label is null ? "Pagination" : TableSupport.Format(Messages.PaginationFor, label));
            _sizeLabel.Text = Messages.PageSize;
            _sizeRow.IsVisible = ShowPageSize;
            IsEnabled = Context is not null;
            if (Context is not { } context) return;
            var size = Math.Max(1, context.State.PageSize);
            _pagination.PageSize = size;
            _pagination.Total = Total;
            _pagination.Page = context.State.Page;
            var sizes = Sizes.ToList();
            if (_sizes.ItemsSource is not List<int> shown || !shown.SequenceEqual(sizes)) _sizes.ItemsSource = sizes;
            _sizes.SelectedItem = context.State.PageSize;
            _sizes.AccessibleName = Messages.PageSize;
        }
        finally
        {
            _syncing = false;
        }
    }
}
