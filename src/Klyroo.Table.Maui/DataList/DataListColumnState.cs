namespace Klyroo.Table.Maui;

/// <summary>The effective state of one column of a data list: visibility, pinning, width and limits.</summary>
/// <param name="Key">The column key (<see cref="KlyDataListColumnBase{TItem}.EffectiveKey"/>).</param>
/// <param name="Title">The display title of the column.</param>
/// <param name="Visible">Whether the column is currently shown.</param>
/// <param name="Required">Whether the column cannot be hidden.</param>
/// <param name="Pinned">The active pin edge: a pinned column keeps its place at the start or the end of the row.</param>
/// <param name="Width">The active width in points, or <see langword="null"/> when automatic.</param>
public sealed record DataListColumnState(string Key, string Title, bool Visible, bool Required, DataListPin Pinned, double? Width);
