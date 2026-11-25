namespace Klyroo.Table;

/// <summary>
/// Every text the components of Klyroo.Table render: accessible names, labels, hints. One set per locale of Klyroo.UI
/// (<see cref="KlyToolsLocales"/>); build a variant with a <c>with</c> expression and register it with <see cref="KlyrooTableOptions.Messages"/>.
/// </summary>
/// <remarks>
/// Partial: each family of components declares its messages in its own file. A message with placeholders ({0}, {1}) is formatted by the
/// component with <see cref="string.Format(string, object?[])"/>; keep the same placeholders when you translate it.
/// </remarks>
public sealed partial record KlyToolsMessages;
