using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Klyroo.Table.Maui;

/// <summary>Small helpers shared by the controls of the data list. The theme helpers repeat the internal ones of Klyroo.UI.Maui, which are not visible from this assembly.</summary>
internal static class TableSupport
{
    /// <summary>The width below which a table shows cards instead of columns, and a grid shows one column (the <c>sm</c> breakpoint of the web library).</summary>
    public const double PhoneWidth = 640;

    /// <summary>Corner radius of a fraction of <see cref="KlyTheme.Radius"/>: <c>0.5</c> is the web <c>rounded-sm</c>, <c>1</c> <c>lg</c>.</summary>
    public static double RadiusOf(double fraction) => KlyTheme.Radius * fraction;

    /// <summary>Binds a color property to the light and dark values: it follows the app theme.</summary>
    public static void SetThemeColor(this BindableObject target, BindableProperty property, KlyThemeColor color) =>
        target.SetAppThemeColor(property, color.Light, color.Dark);

    /// <summary>Binds the border color to the light and dark values.</summary>
    public static void SetStroke(this Border border, KlyThemeColor color) =>
        border.SetAppTheme<Brush>(Border.StrokeProperty, new SolidColorBrush(color.Light), new SolidColorBrush(color.Dark));

    /// <summary>A rounded rectangle border shape.</summary>
    public static void SetRadius(this Border border, double radius) =>
        border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) };

    /// <summary>Applies a surface (fill, border) to a border.</summary>
    public static void Apply(this Border border, TableSurface surface)
    {
        border.SetThemeColor(VisualElement.BackgroundColorProperty, surface.Background);
        border.SetStroke(surface.Stroke);
        border.StrokeThickness = surface.StrokeThickness;
    }

    /// <summary>A label with a size and a theme color.</summary>
    public static Label NewLabel(double size, KlyThemeColor color, FontAttributes attributes = FontAttributes.None)
    {
        var label = new Label { FontSize = size, FontAttributes = attributes, VerticalTextAlignment = TextAlignment.Center };
        label.SetThemeColor(Label.TextColorProperty, color);
        return label;
    }

    /// <summary>Formats a message that has placeholders with the current culture.</summary>
    public static string Format(string template, params object?[] args) => string.Format(CultureInfo.CurrentCulture, template, args);

    /// <summary>Runs <paramref name="action"/> on the UI thread of <paramref name="owner"/>, or at once when it has no dispatcher (a unit test).</summary>
    public static void Post(Element owner, Action action)
    {
        IDispatcher? dispatcher;
        try
        {
            dispatcher = owner.Dispatcher;
        }
        catch (InvalidOperationException)
        {
            dispatcher = null;
        }
        if (dispatcher is { IsDispatchRequired: true }) dispatcher.Dispatch(action);
        else action();
    }

    /// <summary>Tells a screen reader a message. Does nothing where there is no screen reader service.</summary>
    public static void Announce(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        try
        {
            SemanticScreenReader.Default.Announce(text);
        }
        catch (NotImplementedException)
        {
        }
        catch (FeatureNotSupportedException)
        {
        }
    }

    /// <summary>The nearest ancestor of a given type, or <see langword="null"/>.</summary>
    public static T? FindAncestor<T>(Element? from) where T : class
    {
        for (var element = from; element is not null; element = element.Parent)
        {
            if (element is T found) return found;
        }
        return null;
    }
}
