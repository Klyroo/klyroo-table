using Microsoft.Maui.ApplicationModel;

namespace Klyroo.Table.Maui;

/// <summary>
/// The actions of one row: the first <see cref="MaxVisible"/> as buttons and the others, and every action with children, in a menu behind a "more" button. An action that is hidden for the row is not drawn, one that is disabled shows
/// dimmed with its reason as accessible hint, one that runs shows a spinner and cannot start twice. An action can ask first (<see cref="DataListRowAction{TItem}.Confirm"/>, in a dialog of the <see cref="KlyOverlayHost"/>), refresh the list when it succeeded,
/// and report a failure through <see cref="Error"/> instead of throwing. Put it in a template column of the table (a <see cref="KlyDataListTable{TItem}"/> with <c>RowActions</c> does it for you).
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListRowActions<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Item"/>.</summary>
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(nameof(Item), typeof(TItem), typeof(KlyDataListRowActions<TItem>), default(TItem), propertyChanged: (b, _, _) => ((KlyDataListRowActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Position"/>.</summary>
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(KlyDataListRowActions<TItem>), 0, propertyChanged: (b, _, _) => ((KlyDataListRowActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Actions"/>.</summary>
    public static readonly BindableProperty ActionsProperty = BindableProperty.Create(nameof(Actions), typeof(IReadOnlyList<DataListRowAction<TItem>>), typeof(KlyDataListRowActions<TItem>), Array.Empty<DataListRowAction<TItem>>(), propertyChanged: (b, _, _) => ((KlyDataListRowActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="MaxVisible"/>.</summary>
    public static readonly BindableProperty MaxVisibleProperty = BindableProperty.Create(nameof(MaxVisible), typeof(int), typeof(KlyDataListRowActions<TItem>), 3, propertyChanged: (b, _, _) => ((KlyDataListRowActions<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(KlySize), typeof(KlyDataListRowActions<TItem>), KlySize.Sm, propertyChanged: (b, _, _) => ((KlyDataListRowActions<TItem>)b).Redraw());

    private readonly TableRowActionRunner<TItem> _runner = new();
    private bool _unloaded;

    /// <summary>Creates the actions of a row. Set <see cref="Item"/> and <see cref="Actions"/>.</summary>
    public KlyDataListRowActions()
    {
        Unloaded += (_, _) => _unloaded = true;
        Loaded += (_, _) => _unloaded = false;
        Refresh();
    }

    /// <summary>Raised when a row action threw or reported a failure: the list is not refreshed and nothing is rethrown.</summary>
    public event EventHandler<DataListRowActionFailure<TItem>>? Error;

    /// <summary>Raised with the address of an action that is a link (<see cref="DataListRowAction{TItem}.Href"/>): route it in the app. A web, mail or phone address is also opened by the system.</summary>
    public event EventHandler<string>? LinkActivated;

    /// <summary>The row.</summary>
    public TItem? Item { get => (TItem?)GetValue(ItemProperty); set => SetValue(ItemProperty, value); }

    /// <summary>The place of the row on the page, from 1; 0 when the part is outside a table row.</summary>
    public int Position { get => (int)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

    /// <summary>The actions, in the order they are shown.</summary>
    public IReadOnlyList<DataListRowAction<TItem>> Actions { get => (IReadOnlyList<DataListRowAction<TItem>>)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }

    /// <summary>How many actions show as buttons; the others go into a menu (3 by default, 0 keeps one menu button per row).</summary>
    public int MaxVisible { get => (int)GetValue(MaxVisibleProperty); set => SetValue(MaxVisibleProperty, value); }

    /// <summary>The size of the buttons. Defaults to small.</summary>
    public KlySize Size { get => (KlySize)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <inheritdoc />
    protected override bool RefreshOnListChange => false;

    /// <summary>Replaces the confirmation dialog, for the tests: it answers whether the user confirmed.</summary>
    internal Func<TableConfirmRequest, Task<bool>>? Confirmer { get; set; }

    internal IReadOnlyDictionary<string, TableButton> Buttons { get; private set; } = new Dictionary<string, TableButton>();

    internal KlyDropdownMenu? Menu { get; private set; }

    private DataListRowActionContext<TItem>? Describe() => Context is { } context && Item is { } item ? context.CreateRowActionContext(item, Position) : null;

    /// <summary>Runs the action with this key as a tap on its button does: it asks first when it has a confirmation.</summary>
    /// <param name="key">The key of the action (children included).</param>
    internal Task Choose(string key)
    {
        var found = TableRowActionRunner<TItem>.Locate(Actions, key);
        return found is { } located ? RequestAsync(located.Action, located.Ancestors) : Task.CompletedTask;
    }

    private async Task RequestAsync(DataListRowAction<TItem> action, IReadOnlyList<DataListRowAction<TItem>> ancestors)
    {
        if (Describe() is not { } context) return;
        var state = _runner.StateOf(context.Key, action, context, ancestors);
        if (!state.Visible || state.Disabled || state.Loading) return;
        if (state.Href is { } href)
        {
            LinkActivated?.Invoke(this, href);
            if (Uri.TryCreate(href, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" or "mailto" or "tel" or "sms")
            {
                try
                {
                    await Launcher.Default.OpenAsync(uri);
                }
                catch (Exception e) when (e is NotImplementedException or FeatureNotSupportedException or PlatformNotSupportedException)
                {
                    // No system browser on this platform: the app routes the link through LinkActivated.
                }
            }
            return;
        }
        if (action.Children is { Count: > 0 }) return;
        if (action.Confirm is { } confirm)
        {
            var request = new TableConfirmRequest(confirm.Title, confirm.Description?.Invoke(context), confirm.ConfirmLabel ?? Messages.ActionConfirm, Messages.ActionCancel, confirm.Color ?? action.Color);
            var confirmed = Confirmer is { } custom ? await custom(request) : await TableDialogs.ConfirmAsync(this, request, context.CancellationToken);
            if (!confirmed) return;
        }
        var list = Context!;
        var outcome = await _runner.RunAsync(context.Key, action, context, () => { if (!_unloaded) Redraw(); }, (exception, message) =>
        {
            Error?.Invoke(this, new DataListRowActionFailure<TItem>(action, context.Item, exception, message));
            return Task.CompletedTask;
        }, context.CancellationToken, ancestors);
        if (outcome == TableRowActionOutcome.Succeeded && action.RefreshOnSuccess) await list.RefreshAsync();
    }

    private static List<KlyMenuItem> MenuItems(IEnumerable<DataListRowAction<TItem>> actions, TableRowActionRunner<TItem> runner, DataListRowActionContext<TItem> context, IReadOnlyList<DataListRowAction<TItem>> ancestors)
    {
        var items = new List<KlyMenuItem>();
        var groups = TableRowActionRunner<TItem>.Group(actions.Where(a => runner.StateOf(context.Key, a, context, ancestors).Visible));
        for (var i = 0; i < groups.Count; i++)
        {
            if (i > 0) items.Add(KlyMenuItem.Separator());
            foreach (var action in groups[i])
            {
                var state = runner.StateOf(context.Key, action, context, ancestors);
                IReadOnlyList<KlyMenuItem>? children = action.Children is { Count: > 0 } ? MenuItems(action.Children, runner, context, [.. ancestors, action]) : null;
                items.Add(new KlyMenuItem
                {
                    Label = action.Label,
                    Value = action.Key,
                    Icon = action.Icon,
                    Href = state.Href,
                    IsEnabled = !state.Disabled && !state.Loading,
                    Description = state.Reason,
                    Color = action.Color.Equals(KlyColor.Neutral) ? default(KlyColor?) : action.Color,
                    Children = children,
                });
            }
        }
        return items;
    }

    /// <inheritdoc />
    protected override void Refresh()
    {
        Buttons = new Dictionary<string, TableButton>();
        Menu = null;
        if (Describe() is not { } context)
        {
            Content = null;
            return;
        }
        var messages = Messages;
        var visible = Actions.Where(a => _runner.StateOf(context.Key, a, context).Visible).ToList();
        if (visible.Count == 0)
        {
            Content = null;
            return;
        }
        var inline = visible.Where(a => a.Children is not { Count: > 0 }).Take(Math.Max(0, MaxVisible)).ToList();
        var rest = visible.Except(inline).ToList();
        var row = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var buttons = new Dictionary<string, TableButton>();
        foreach (var action in inline)
        {
            var current = action;
            var state = _runner.StateOf(context.Key, action, context);
            var button = new TableButton(action.Label, action.Icon, ButtonVariant.Ghost, action.Color, Size)
            {
                IsEnabled = !state.Disabled,
                IsBusy = state.Loading,
                AccessibleHint = state.Reason,
            };
            button.Clicked += (_, _) => _ = RequestAsync(current, []);
            buttons[action.Key] = button;
            row.Children.Add(button);
        }
        Buttons = buttons;
        if (rest.Count > 0)
        {
            var label = Position > 0 ? TableSupport.Format(messages.RowActionsLabelAt, Position) : TableSupport.Format(messages.RowActionsLabel, Item?.ToString() ?? "");
            var trigger = new TableButton(null, KlyIconNames.Ellipsis, ButtonVariant.Ghost, KlyColor.Neutral, Size) { AccessibleName = label };
            var menu = new KlyDropdownMenu { Content = trigger, Placement = KlyPlacement.BottomEnd, AccessibleName = label, Items = MenuItems(rest, _runner, context, []) };
            menu.ItemSelected += (_, item) => _ = Choose(item.Value ?? item.Label);
            Menu = menu;
            row.Children.Add(menu);
        }
        SemanticProperties.SetDescription(row, Position > 0 ? TableSupport.Format(messages.RowActionsLabelAt, Position) : TableSupport.Format(messages.RowActionsLabel, Item?.ToString() ?? ""));
        Content = row;
    }
}
