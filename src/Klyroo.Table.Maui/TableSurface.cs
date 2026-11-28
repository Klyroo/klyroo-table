namespace Klyroo.Table.Maui;

/// <summary>What a variant of a colored control draws: a fill, a border and a text color. The same table as the one of Klyroo.UI.Maui, which is internal there.</summary>
internal readonly record struct TableSurface(KlyThemeColor Background, KlyThemeColor Stroke, double StrokeThickness, KlyThemeColor Text)
{
    private const float Tint = 0.10f;
    private const float Ring = 0.75f;

    public static TableSurface For(ButtonVariant variant, KlyColorSet c) => variant switch
    {
        ButtonVariant.Solid => new(c.Fill, c.Text, 1, c.Contrast),
        ButtonVariant.Outline => new(KlyThemeColor.Transparent, c.Text.WithAlpha(Ring), 1, c.Text),
        ButtonVariant.Soft => new(c.Fill.WithAlpha(Tint), KlyThemeColor.Transparent, 0, c.Text),
        ButtonVariant.Subtle => new(c.Fill.WithAlpha(Tint), c.Text.WithAlpha(Ring), 1, c.Text),
        ButtonVariant.Ghost or ButtonVariant.Link => new(KlyThemeColor.Transparent, KlyThemeColor.Transparent, 0, c.Text),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
    };
}
