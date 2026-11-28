namespace Klyroo.Table.Maui;

/// <summary>What a row action receives when it runs: the row, its key and place, and a way to report a failure without throwing.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class DataListRowActionContext<TItem>
{
    internal DataListRowActionContext(DataListContext<TItem> list, TItem item, string key, int position, CancellationToken cancellationToken)
    {
        List = list;
        Item = item;
        Key = key;
        Position = position;
        CancellationToken = cancellationToken;
    }

    /// <summary>The list, to read its state or to load it again (<see cref="DataListContext{TItem}.RefreshAsync"/>).</summary>
    public DataListContext<TItem> List { get; }

    /// <summary>The row.</summary>
    public TItem Item { get; }

    /// <summary>The row key (<c>RowKey</c> of the schema, else the place of the row on its page).</summary>
    public string Key { get; }

    /// <summary>The place of the row on the page, from 1; 0 when the part was placed outside a row.</summary>
    public int Position { get; }

    /// <summary>Cancelled when the list goes away.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>The message given to <see cref="Fail"/>.</summary>
    public string? FailureMessage { get; private set; }

    /// <summary>Whether <see cref="Fail"/> was called.</summary>
    public bool Failed { get; private set; }

    /// <summary>
    /// Reports that the action did not do what it was asked (a refusal of the server, a conflict) without throwing: the list is not refreshed and <c>Error</c> of the part is raised.
    /// Call it, then return.
    /// </summary>
    /// <param name="message">What went wrong, for <c>Error</c>.</param>
    public void Fail(string? message = null)
    {
        Failed = true;
        FailureMessage = message;
    }
}
