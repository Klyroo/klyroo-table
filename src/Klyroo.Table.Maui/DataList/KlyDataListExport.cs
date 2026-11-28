using System.Globalization;

namespace Klyroo.Table.Maui;

/// <summary>
/// Exports the rows of the list as a CSV file. Without a handler for <see cref="Exported"/> the file is written to the cache directory of the app and handed to the system share sheet
/// (<see cref="Microsoft.Maui.ApplicationModel.DataTransfer.Share"/>), which saves, mails or copies it. The menu offers this page, the selection and every result, each only while it applies;
/// with one source left the button exports at once. Needs a <see cref="KlyOverlayHost"/> above it when it shows the menu.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListExport<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListExport<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListExport<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(KlyDataListExport<TItem>), "download", propertyChanged: (b, _, _) => ((KlyDataListExport<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Scopes"/>.</summary>
    public static readonly BindableProperty ScopesProperty = BindableProperty.Create(nameof(Scopes), typeof(IReadOnlyList<DataListExportScope>), typeof(KlyDataListExport<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListExport<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="MaxRows"/>.</summary>
    public static readonly BindableProperty MaxRowsProperty = BindableProperty.Create(nameof(MaxRows), typeof(int), typeof(KlyDataListExport<TItem>), 100_000);
    /// <summary>Identifies <see cref="FileName"/>.</summary>
    public static readonly BindableProperty FileNameProperty = BindableProperty.Create(nameof(FileName), typeof(string), typeof(KlyDataListExport<TItem>), null);
    /// <summary>Identifies <see cref="CsvOptions"/>.</summary>
    public static readonly BindableProperty CsvOptionsProperty = BindableProperty.Create(nameof(CsvOptions), typeof(DataListCsvOptions), typeof(KlyDataListExport<TItem>), null);
    /// <summary>Identifies <see cref="LoadAll"/>.</summary>
    public static readonly BindableProperty LoadAllProperty = BindableProperty.Create(nameof(LoadAll), typeof(Func<DataListRequest, CancellationToken, Task<IReadOnlyList<TItem>>>), typeof(KlyDataListExport<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListExport<TItem>)b).Redraw());

    private readonly Label _refused = TableSupport.NewLabel(12, KlyTheme.Resolve(KlyColor.Error).Text);
    private KlyDropdownMenu? _menu;
    private TableButton? _button;
    private bool _busy;

    /// <summary>Creates an export button.</summary>
    public KlyDataListExport()
    {
        Refresh();
    }

    /// <summary>Raised with the finished file instead of the share sheet: save it yourself (a file picker, an upload, a copy to the clipboard).</summary>
    public event EventHandler<DataListExportResult>? Exported;

    /// <summary>Raised when loading the rows or saving the file threw.</summary>
    public event EventHandler<Exception>? Error;

    /// <summary>The label of the button. Without it the localized "Export".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>The name of the icon of the button (<c>download</c> by default).</summary>
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>
    /// The sources of rows the user can pick from, in the order of the menu. By default this page, the selection and every result; each is offered only while it applies (the selection needs a ticked row,
    /// every result needs a list of rows in memory or <see cref="LoadAll"/>). With one source left the button exports at once.
    /// </summary>
    public IReadOnlyList<DataListExportScope>? Scopes { get => (IReadOnlyList<DataListExportScope>?)GetValue(ScopesProperty); set => SetValue(ScopesProperty, value); }

    /// <summary>The most rows one export may hold, whatever the scope (100,000 by default). A bigger export is refused before the rows are loaded when the source knows how many there are, with a message under the button and <see cref="Error"/> receives a <see cref="DataListExportTooLargeException"/>.</summary>
    public int MaxRows { get => (int)GetValue(MaxRowsProperty); set => SetValue(MaxRowsProperty, value); }

    /// <summary>The name of the file, with or without <c>.csv</c>. Without it the label of the list, else <c>export</c>.</summary>
    public string? FileName { get => (string?)GetValue(FileNameProperty); set => SetValue(FileNameProperty, value); }

    /// <summary>How the file is written: delimiter, byte order mark, header, protection against formulas. Its culture defaults to the current culture, and the delimiter follows it (<c>;</c> where the decimal separator is a comma).</summary>
    public DataListCsvOptions? CsvOptions { get => (DataListCsvOptions?)GetValue(CsvOptionsProperty); set => SetValue(CsvOptionsProperty, value); }

    /// <summary>Loads every row a request matches, for <see cref="DataListExportScope.All"/> on a list over a provider or a queryable (the list never holds those rows). It receives the request without paging and facets, as the list sent it.</summary>
    public Func<DataListRequest, CancellationToken, Task<IReadOnlyList<TItem>>>? LoadAll { get => (Func<DataListRequest, CancellationToken, Task<IReadOnlyList<TItem>>>?)GetValue(LoadAllProperty); set => SetValue(LoadAllProperty, value); }

    /// <inheritdoc />
    protected override void OnContextBound(DataListContext<TItem> context) => context.SelectionChanged += Redraw;

    /// <inheritdoc />
    protected override void OnContextUnbound(DataListContext<TItem> context) => context.SelectionChanged -= Redraw;

    internal TableButton? Button => _button;

    internal KlyDropdownMenu? Menu => _menu;

    internal string? RefusedText => _refused.IsVisible ? _refused.Text : null;

    /// <summary>The scopes that apply now.</summary>
    internal List<DataListExportScope> AvailableScopes()
    {
        var wanted = Scopes ?? [DataListExportScope.Page, DataListExportScope.Selection, DataListExportScope.All];
        var result = new List<DataListExportScope>();
        if (Context is not { } context) return result;
        foreach (var scope in wanted.Distinct())
        {
            var applies = scope switch
            {
                DataListExportScope.Selection => context.SelectionEnabled && context.SelectedCount > 0,
                DataListExportScope.All => (context.HoldsAllRows || LoadAll is not null) && context.Rows.Count > 0,
                _ => context.Rows.Count > 0,
            };
            if (applies) result.Add(scope);
        }
        return result;
    }

    private string ScopeText(DataListExportScope scope)
    {
        var context = Context!;
        var culture = context.Culture;
        return scope switch
        {
            DataListExportScope.Selection => string.Format(culture, Messages.ExportSelection, context.SelectedCount.ToString("N0", culture)),
            DataListExportScope.All => context.MatchingCount is { } total ? string.Format(culture, Messages.ExportAll, total.ToString("N0", culture)) : Messages.ExportAllUnknown,
            _ => Messages.ExportPage,
        };
    }

    /// <summary>Exports the rows of a scope: loads them, writes the CSV and hands the file to <see cref="Exported"/> or to the share sheet.</summary>
    /// <param name="scope">Which rows.</param>
    public async Task ExportAsync(DataListExportScope scope)
    {
        if (_busy || Context is not { } context) return;
        _busy = true;
        _refused.IsVisible = false;
        Redraw();
        try
        {
            if (Expected(context, scope) is { } expected && expected > MaxRows) throw new DataListExportTooLargeException(expected, MaxRows);
            var rows = await RowsAsync(context, scope);
            if (rows.Count > MaxRows) throw new DataListExportTooLargeException(rows.Count, MaxRows);
            var options = (CsvOptions ?? new DataListCsvOptions()) with { Culture = CsvOptions?.Culture ?? context.Culture };
            var content = context.ToCsv(rows, options);
            var name = DataListCsv.FileName(FileName ?? context.Label);
            var result = new DataListExportResult(name, content, rows.Count, scope);
            if (Exported is { } handler) handler(this, result);
            else await TableFileExport.ShareAsync(result);
        }
        catch (DataListExportTooLargeException tooLarge)
        {
            _refused.Text = string.Format(context.Culture, Messages.ExportTooLarge, tooLarge.Count.ToString("N0", context.Culture), tooLarge.Limit.ToString("N0", context.Culture));
            _refused.IsVisible = true;
            Error?.Invoke(this, tooLarge);
        }
        catch (Exception exception) when (!(exception is OperationCanceledException && context.LifetimeToken.IsCancellationRequested))
        {
            Error?.Invoke(this, exception);
        }
        finally
        {
            _busy = false;
            Redraw();
        }
    }

    // How many rows a scope holds when that is known without loading them.
    private static long? Expected(DataListContext<TItem> context, DataListExportScope scope) => scope switch
    {
        DataListExportScope.Page => context.Rows.Count,
        DataListExportScope.Selection => context.SelectedCount,
        _ => context.MatchingCount,
    };

    // A list of rows in memory resolves "every result" itself; any other needs the function of the app.
    private async Task<IReadOnlyList<TItem>> RowsAsync(DataListContext<TItem> context, DataListExportScope scope)
    {
        if (scope == DataListExportScope.All && !context.HoldsAllRows)
        {
            var request = context.AllResultsRequest ?? throw new InvalidOperationException("The list has not loaded yet: there is no request to export.");
            return await LoadAll!(request, context.LifetimeToken);
        }
        return await context.ExportRowsAsync(scope, context.LifetimeToken);
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is null)
        {
            Content = null;
            return;
        }
        var scopes = AvailableScopes();
        _button = new TableButton(Text ?? Messages.ExportLabel, Icon, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { IsBusy = _busy, IsEnabled = scopes.Count > 0 && !_busy };
        var stack = new VerticalStackLayout { Spacing = 4 };
        if (scopes.Count > 1)
        {
            _menu ??= new KlyDropdownMenu { Placement = KlyPlacement.BottomStart };
            if (!_menuWired)
            {
                _menu.ItemSelected += (_, item) =>
                {
                    if (Enum.TryParse<DataListExportScope>(item.Value, out var scope)) _ = ExportAsync(scope);
                };
                _menuWired = true;
            }
            _menu.Content = _button;
            _menu.AccessibleName = Text ?? Messages.ExportLabel;
            _menu.Items = [.. scopes.Select(scope => new KlyMenuItem { Label = ScopeText(scope), Value = scope.ToString(), IsEnabled = !_busy })];
            stack.Children.Add(_menu);
        }
        else
        {
            _menu = null;
            _menuWired = false;
            var only = scopes.Count == 1 ? scopes[0] : DataListExportScope.Page;
            _button.Clicked += (_, _) => _ = ExportAsync(only);
            stack.Children.Add(_button);
        }
        stack.Children.Add(_refused);
        Content = stack;
    }

    private bool _menuWired;
}
