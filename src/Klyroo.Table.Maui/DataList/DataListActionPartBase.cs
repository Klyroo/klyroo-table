using System.ComponentModel;

namespace Klyroo.Table.Maui;

/// <summary>
/// What the parts that show actions share (<see cref="KlyDataListSelectionActions{TItem}"/>, <see cref="KlyDataListActionsDropdown{TItem}"/>): which actions are visible, disabled or running,
/// the confirmation dialog and the run itself. An action never starts twice at once, and a failure keeps the selection.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DataListActionPartBase<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Actions"/>.</summary>
    public static readonly BindableProperty ActionsProperty = BindableProperty.Create(nameof(Actions), typeof(IReadOnlyList<DataListAction<TItem>>), typeof(DataListActionPartBase<TItem>), Array.Empty<DataListAction<TItem>>(), propertyChanged: (b, _, _) => ((DataListActionPartBase<TItem>)b).Redraw());

    private readonly HashSet<string> _running = new(StringComparer.Ordinal);
    private bool _disposed;

    private protected DataListActionPartBase()
    {
        Unloaded += (_, _) => _disposed = true;
        Loaded += (_, _) => _disposed = false;
    }

    /// <summary>Raised when an action throws; the part keeps the selection, does not refresh the list and does not rethrow.</summary>
    public event EventHandler<DataListActionFailure<TItem>>? Error;

    /// <summary>The actions, in the order they are shown.</summary>
    public IReadOnlyList<DataListAction<TItem>> Actions { get => (IReadOnlyList<DataListAction<TItem>>)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }

    /// <summary>Replaces the confirmation dialog, for the tests: it answers whether the user confirmed.</summary>
    internal Func<TableConfirmRequest, Task<bool>>? Confirmer { get; set; }

    /// <summary>The context the predicates of the actions see: the ticked rows, and the bulk scope.</summary>
    protected DataListActionContext<TItem> Describe() => Context!.CreateActionContext(Context.BulkScope);

    /// <summary>Whether an action is shown for a context.</summary>
    /// <param name="action">The action.</param>
    /// <param name="context">What <see cref="Describe"/> returned.</param>
    protected static bool IsShown(DataListAction<TItem> action, DataListActionContext<TItem> context) => action.Visible?.Invoke(context) ?? true;

    /// <summary>Whether an action is shown but cannot start: it says so, or it is running.</summary>
    /// <param name="action">The action.</param>
    /// <param name="context">What <see cref="Describe"/> returned.</param>
    protected bool IsDisabled(DataListAction<TItem> action, DataListActionContext<TItem> context) => IsRunning(action) || (action.Disabled?.Invoke(context) ?? false);

    /// <summary>Whether an action is running.</summary>
    /// <param name="action">The action.</param>
    protected bool IsRunning(DataListAction<TItem> action) => _running.Contains(action.Key);

    /// <summary>A tap on an action: it runs, or asks first when it has a confirmation.</summary>
    /// <param name="action">The action.</param>
    protected async Task RequestAsync(DataListAction<TItem> action)
    {
        if (Context is null) return;
        var context = Describe();
        if (!IsShown(action, context) || IsDisabled(action, context)) return;
        if (action.Confirm is { } confirm)
        {
            var request = new TableConfirmRequest(confirm.Title, confirm.Description?.Invoke(context), confirm.ConfirmLabel ?? Messages.ActionConfirm, Messages.ActionCancel, confirm.Color ?? action.Color);
            var confirmed = Confirmer is { } custom ? await custom(request) : await TableDialogs.ConfirmAsync(this, request, Context.LifetimeToken);
            if (!confirmed) return;
            // The rows may have changed while the dialog was open: the action must still apply.
            context = Describe();
            if (!IsShown(action, context) || IsDisabled(action, context)) return;
        }
        await RunAsync(action);
    }

    private async Task RunAsync(DataListAction<TItem> action)
    {
        var list = Context;
        if (list is null || !_running.Add(action.Key)) return;
        var failed = false;
        // The rows the action targets are the ones ticked now: rows ticked while it runs are not its business and stay ticked.
        var targeted = list.SelectedKeys;
        try
        {
            // "Every result" of a list in memory is resolved now: the rows the query matches at the moment of the tap.
            var context = await list.CreateActionContextAsync(list.BulkScope, resolveAll: true, list.LifetimeToken);
            if (action.ClearSelection == DataListSelectionClear.Trigger) list.ClearSelection();
            Redraw();
            if (action.OnExecute is { } run) await run(context);
        }
        catch (Exception exception) when (!(exception is OperationCanceledException && list.LifetimeToken.IsCancellationRequested))
        {
            failed = true;
            Error?.Invoke(this, new DataListActionFailure<TItem>(action, exception));
        }
        finally
        {
            _running.Remove(action.Key);
        }
        if (_disposed) return;
        if (!failed)
        {
            if (action.ClearSelection == DataListSelectionClear.Success) list.ClearSelection(targeted);
            if (action.RefreshOnSuccess) await list.RefreshAsync();
        }
        if (!_disposed) Redraw();
    }
}
