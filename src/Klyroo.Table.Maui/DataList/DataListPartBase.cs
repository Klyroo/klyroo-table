namespace Klyroo.Table.Maui;

/// <summary>
/// Base of the parts of a <see cref="KlyDataList{TItem}"/>: a view that finds the list above it, draws what the list holds (<see cref="Context"/>) and draws again when it changes.
/// Derive from it to write a part of your own: build the view in <see cref="Refresh"/>.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public abstract class DataListPartBase<TItem> : ContentView, IDataListPart<TItem>
{
    /// <summary>Identifies <see cref="List"/>.</summary>
    public static readonly BindableProperty ListProperty = BindableProperty.Create(nameof(List), typeof(KlyDataList<TItem>), typeof(DataListPartBase<TItem>), null, propertyChanged: (b, _, v) => ((DataListPartBase<TItem>)b).OnListSet((KlyDataList<TItem>?)v));

    private DataListContext<TItem>? _context;
    private bool _explicit;

    /// <summary>Creates a part. It binds itself to the list it is put in.</summary>
    protected DataListPartBase()
    {
        TableIcons.Ensure();
        ParentChanged += (_, _) => ResolveFromTree();
        Loaded += (_, _) => ResolveFromTree();
    }

    /// <summary>
    /// The list this part belongs to when it is not below it in the visual tree: a part shown in the body of a <see cref="KlyModal"/> or a <see cref="KlySlideover"/> lives in the overlay host, not in the list.
    /// A part below its list finds it by itself and needs no value here.
    /// </summary>
    public KlyDataList<TItem>? List { get => (KlyDataList<TItem>?)GetValue(ListProperty); set => SetValue(ListProperty, value); }

    /// <summary>The state of the list this part belongs to, or <see langword="null"/> while it is not in one.</summary>
    public DataListContext<TItem>? Context => _context;

    private void OnListSet(KlyDataList<TItem>? list)
    {
        if (list is null)
        {
            _explicit = false;
            ResolveFromTree();
            return;
        }
        BindExplicit(list.Context);
    }

    /// <summary>The texts of the list, in the language of the user.</summary>
    protected KlyToolsMessages Messages => _context?.Messages ?? TableMessages.For(System.Globalization.CultureInfo.CurrentUICulture);

    void IDataListPart<TItem>.Bind(DataListContext<TItem>? context) => Bind(context);

    /// <summary>Hands the part the context of a list by code, for a part that is shown outside the tree of its list (in an overlay).</summary>
    /// <param name="context">The context of the list.</param>
    internal void BindExplicit(DataListContext<TItem> context)
    {
        _explicit = true;
        Bind(context);
    }

    /// <summary>Lets go of the list: a part that a part built for itself and then discards must stop listening to it, or it stays alive as long as the list does.</summary>
    internal void Release()
    {
        _explicit = false;
        Bind(null);
    }

    internal void Bind(DataListContext<TItem>? context)
    {
        if (_explicit && context is null) return;
        if (ReferenceEquals(_context, context)) return;
        if (_context is not null)
        {
            if (RefreshOnListChange) _context.Changed -= OnContextChanged;
            OnContextUnbound(_context);
        }
        _context = context;
        if (context is not null)
        {
            if (RefreshOnListChange) context.Changed += OnContextChanged;
            OnContextBound(context);
        }
        Refresh();
    }

    /// <summary>Whether <see cref="Refresh"/> runs each time the list changes (the default). A part with many instances that does not read the list, such as the actions of a row, turns it off.</summary>
    protected virtual bool RefreshOnListChange => true;

    /// <summary>Called when the part got its list, before the first <see cref="Refresh"/>. Subscribe to what the part listens to.</summary>
    /// <param name="context">The list.</param>
    protected virtual void OnContextBound(DataListContext<TItem> context)
    {
    }

    /// <summary>Called when the part lost its list. Unsubscribe from what <see cref="OnContextBound"/> subscribed to.</summary>
    /// <param name="context">The list it had.</param>
    protected virtual void OnContextUnbound(DataListContext<TItem> context)
    {
    }

    /// <summary>Builds the view of the part from <see cref="Context"/>. It is called when the part gets its list and each time the list changes; <see cref="Context"/> is <see langword="null"/> when the part has none.</summary>
    protected abstract void Refresh();

    /// <summary>Draws the part again.</summary>
    protected void Redraw() => Refresh();

    private void OnContextChanged() => Refresh();

    private void ResolveFromTree()
    {
        if (_explicit) return;
        var list = TableSupport.FindAncestor<KlyDataList<TItem>>(this);
        Bind(list?.Context);
    }
}
