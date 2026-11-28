using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Klyroo.Table.Maui;

/// <summary>Small helpers shared by the controls of the data list. The theme helpers repeat the internal ones of Klyroo.UI.Maui, which are not visible from this assembly.</summary>
internal static class TableSupport
{
    /// <summary>The width below which a table shows cards instead of columns, and a grid shows one column (the <c>sm</c> breakpoint of the web library).</summary>
    public const double PhoneWidth = 640;

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
