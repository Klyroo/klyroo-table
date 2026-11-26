using Microsoft.AspNetCore.Components;

namespace Klyroo.Table;

/// <summary>
/// What a row of <see cref="KlyDataListTable{TItem}"/> reports when the user activates it (<see cref="KlyDataListTable{TItem}.OnRowClick"/>): a click anywhere on the row
/// that did not land on a control inside it, or Enter or Space while the row itself has the focus.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="Item">The row.</param>
/// <param name="Key">The row key (<c>RowKey</c> of the schema) as text; <c>#3</c> (the position) for a list whose schema has none.</param>
/// <param name="Position">The place of the row on the page, from 1.</param>
/// <param name="ShiftKey">Shift was held.</param>
/// <param name="CtrlKey">Ctrl was held.</param>
/// <param name="AltKey">Alt was held.</param>
/// <param name="MetaKey">Meta (Command on a Mac, the Windows key) was held.</param>
/// <param name="FromKeyboard">The row was activated with Enter or Space, not with the pointer.</param>
public sealed record DataListRowClickArgs<TItem>(
    TItem Item,
    string Key,
    int Position,
    bool ShiftKey = false,
    bool CtrlKey = false,
    bool AltKey = false,
    bool MetaKey = false,
    bool FromKeyboard = false);

/// <summary>
/// Payload of the <c>klyrowclick</c> and <c>klyrowkey</c> browser events that a clickable row listens to. The browser side works out whether the event
/// is for the row at all (<see cref="Ignore"/>), because .NET cannot see which element a click landed on.
/// </summary>
public sealed class KlyRowEventArgs : EventArgs
{
    /// <summary>The event is not an activation of the row: a click on a control inside it (a link, a button, a box, a field), a key other than Enter and Space, or a key pressed inside a control.</summary>
    public bool Ignore { get; set; }

    /// <summary>Shift was held.</summary>
    public bool ShiftKey { get; set; }

    /// <summary>Ctrl was held.</summary>
    public bool CtrlKey { get; set; }

    /// <summary>Alt was held.</summary>
    public bool AltKey { get; set; }

    /// <summary>Meta was held.</summary>
    public bool MetaKey { get; set; }

    /// <summary>The key of a keyboard event; <see langword="null"/> for a click.</summary>
    public string? Key { get; set; }
}

/// <summary>Custom events of Klyroo.Table, registered on the browser side by <c>Klyroo.Table.lib.module.js</c>.</summary>
[EventHandler("onklyrowclick", typeof(KlyRowEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onklyrowkey", typeof(KlyRowEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class TableEventHandlers;
