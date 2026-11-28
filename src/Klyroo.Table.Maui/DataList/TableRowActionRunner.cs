namespace Klyroo.Table.Maui;

/// <summary>What a row action shows for a row right now.</summary>
internal readonly record struct TableRowActionState(bool Visible, bool Disabled, string? Reason, bool Loading, string? Href);

/// <summary>How a run of a row action ended.</summary>
internal enum TableRowActionOutcome
{
    /// <summary>It did not start: hidden, disabled, loading, running, a link or a submenu.</summary>
    Ignored,
    Succeeded,
    Failed,
}

/// <summary>
/// The run loop of the row actions: which are running (per row and action), what each shows and the run itself. An action that is hidden, disabled, loading or running is ignored;
/// a failure, thrown or reported, does not throw.
/// </summary>
internal sealed class TableRowActionRunner<TItem>
{
    private readonly HashSet<(string Row, string Action)> _running = [];

    public bool IsRunning(string row, string action) => _running.Contains((row, action));

    /// <summary>What an action shows for a row. <paramref name="ancestors"/> are the actions it sits under, outermost first: a hidden ancestor hides it and a disabled one disables it.</summary>
    public TableRowActionState StateOf(string row, DataListRowAction<TItem> action, DataListRowActionContext<TItem> context, IReadOnlyList<DataListRowAction<TItem>>? ancestors = null)
    {
        var inheritedDisabled = false;
        string? inheritedReason = null;
        if (ancestors is not null)
        {
            foreach (var ancestor in ancestors)
            {
                if (ancestor.Visible?.Invoke(context) == false) return default;
                if (inheritedDisabled || ancestor.Disabled?.Invoke(context) != true) continue;
                inheritedDisabled = true;
                inheritedReason = ancestor.DisabledReason?.Invoke(context);
            }
        }
        if (action.Visible?.Invoke(context) == false) return default;
        var children = action.Children;
        if (children is { Count: > 0 })
        {
            var below = ancestors is null ? [action] : (IReadOnlyList<DataListRowAction<TItem>>)[.. ancestors, action];
            var visible = children.Any(c => StateOf(row, c, context, below).Visible);
            var ownDisabled = action.Disabled?.Invoke(context) ?? false;
            var disabled = inheritedDisabled || ownDisabled;
            return new TableRowActionState(visible, disabled, disabled ? inheritedReason ?? action.DisabledReason?.Invoke(context) : null, false, null);
        }
        var href = action.Href?.Invoke(context);
        if (action.Href is not null && href is null) return default;
        var running = IsRunning(row, action.Key);
        var blocked = inheritedDisabled || running || (action.Disabled?.Invoke(context) ?? false);
        var loading = running || (action.Loading?.Invoke(context) ?? false);
        return new TableRowActionState(true, blocked, blocked ? inheritedReason ?? action.DisabledReason?.Invoke(context) : null, loading, href);
    }

    /// <summary>The action with this key among <paramref name="roots"/> and the actions it sits under (outermost first), or <see langword="null"/>.</summary>
    public static (DataListRowAction<TItem> Action, IReadOnlyList<DataListRowAction<TItem>> Ancestors)? Locate(IEnumerable<DataListRowAction<TItem>> roots, string? key)
    {
        if (key is null) return null;
        return Search(roots, key, []);
    }

    private static (DataListRowAction<TItem>, IReadOnlyList<DataListRowAction<TItem>>)? Search(IEnumerable<DataListRowAction<TItem>> actions, string key, List<DataListRowAction<TItem>> path)
    {
        foreach (var action in actions)
        {
            if (action.Key == key) return (action, [.. path]);
            if (action.Children is not { Count: > 0 }) continue;
            path.Add(action);
            var found = Search(action.Children, key, path);
            path.RemoveAt(path.Count - 1);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>Runs an action. <paramref name="changed"/> is called when it starts and when it ends, so that the row shows its spinner.</summary>
    public async Task<TableRowActionOutcome> RunAsync(string row, DataListRowAction<TItem> action, DataListRowActionContext<TItem> context, Action changed, Func<Exception?, string?, Task> onFailure, CancellationToken lifetime,
        IReadOnlyList<DataListRowAction<TItem>>? ancestors = null)
    {
        var state = StateOf(row, action, context, ancestors);
        if (!state.Visible || state.Disabled || state.Loading || state.Href is not null || action.Children is { Count: > 0 }) return TableRowActionOutcome.Ignored;
        if (!_running.Add((row, action.Key))) return TableRowActionOutcome.Ignored;
        Exception? thrown = null;
        try
        {
            changed();
            if (action.OnExecute is { } run) await run(context);
        }
        catch (Exception exception) when (!(exception is OperationCanceledException && lifetime.IsCancellationRequested))
        {
            thrown = exception;
        }
        finally
        {
            _running.Remove((row, action.Key));
            changed();
        }
        if (thrown is not null || context.Failed)
        {
            await onFailure(thrown, context.FailureMessage);
            return TableRowActionOutcome.Failed;
        }
        return TableRowActionOutcome.Succeeded;
    }

    /// <summary>The visible actions grouped for a menu: groups in the order of their first visible action, actions without a group together.</summary>
    public static List<List<DataListRowAction<TItem>>> Group(IEnumerable<DataListRowAction<TItem>> visible)
    {
        var groups = new List<List<DataListRowAction<TItem>>>();
        var index = new Dictionary<string, List<DataListRowAction<TItem>>>(StringComparer.Ordinal);
        List<DataListRowAction<TItem>>? ungrouped = null;
        foreach (var action in visible)
        {
            List<DataListRowAction<TItem>> group;
            if (action.Group is null)
            {
                if (ungrouped is null) groups.Add(ungrouped = []);
                group = ungrouped;
            }
            else if (!index.TryGetValue(action.Group, out group!))
            {
                groups.Add(group = []);
                index[action.Group] = group;
            }
            group.Add(action);
        }
        return groups;
    }
}
