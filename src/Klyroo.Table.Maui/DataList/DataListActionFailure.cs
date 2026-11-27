namespace Klyroo.Table.Maui;

/// <summary>An action that threw.</summary>
/// <param name="Action">The action.</param>
/// <param name="Exception">What it threw.</param>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed record DataListActionFailure<TItem>(DataListAction<TItem> Action, Exception Exception);
