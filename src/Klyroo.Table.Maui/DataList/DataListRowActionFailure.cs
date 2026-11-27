namespace Klyroo.Table.Maui;

/// <summary>A row action that failed: it threw, or called <see cref="DataListRowActionContext{TItem}.Fail"/>.</summary>
/// <param name="Action">The action.</param>
/// <param name="Item">The row.</param>
/// <param name="Exception">What it threw; <see langword="null"/> when it reported the failure itself.</param>
/// <param name="Message">What <see cref="DataListRowActionContext{TItem}.Fail"/> was given.</param>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed record DataListRowActionFailure<TItem>(DataListRowAction<TItem> Action, TItem Item, Exception? Exception, string? Message);
