namespace Klyroo.Table.Maui;

/// <summary>The count of the rows on screen: "1–25 of 214", "No results for “x”", "Loading…". A screen reader is told when it changes.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListResultCount<TItem> : DataListPartBase<TItem>
{
    private readonly Label _label = TableSupport.NewLabel(14, KlyTheme.TextMuted);
    private string _announced = "";

    /// <summary>Creates a result count.</summary>
    public KlyDataListResultCount()
    {
        _label.VerticalOptions = LayoutOptions.Center;
        Content = _label;
        Refresh();
    }

    /// <summary>The text shown now.</summary>
    public string Text => _label.Text ?? "";

    /// <inheritdoc />
    protected override void Refresh()
    {
        var text = Compose();
        _label.Text = text;
        SemanticProperties.SetDescription(this, text);
        if (text.Length > 0 && text != _announced && Context is { IsInitial: false })
        {
            _announced = text;
            TableSupport.Announce(text);
        }
    }

    // The rows on screen, not the ones being loaded: the text changes when they do.
    private string Compose()
    {
        if (Context is not { } context) return "";
        var messages = Messages;
        if (context.IsInitial) return messages.Loading;
        if (context.Result is not { } result) return "";
        var culture = context.Culture;
        if (result.Items.Count == 0)
        {
            var search = (context.LoadedState ?? context.State).Search.Trim();
            return search.Length > 0 ? string.Format(culture, messages.NoResultsFor, search) : messages.NoResults;
        }
        var loaded = context.LoadedState ?? context.State;
        string Number(long value) => value.ToString("N0", culture);
        if (context.PagingMode == DataListPagingMode.Cursor && !string.IsNullOrEmpty(loaded.Cursor))
        {
            return result.TotalCount is { } cursorTotal
                ? string.Format(culture, messages.ResultCountLoaded, Number(result.Items.Count), Number(cursorTotal))
                : string.Format(culture, messages.ResultCountShown, Number(result.Items.Count));
        }
        var first = context.PagingMode == DataListPagingMode.Offset ? (long)(loaded.Page - 1) * loaded.PageSize + 1 : 1;
        var last = first + result.Items.Count - 1;
        return result.TotalCount is { } total
            ? string.Format(culture, messages.ResultCount, Number(first), Number(last), Number(total))
            : string.Format(culture, messages.ResultRange, Number(first), Number(last));
    }
}
