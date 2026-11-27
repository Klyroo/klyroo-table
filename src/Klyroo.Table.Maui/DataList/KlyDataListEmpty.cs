namespace Klyroo.Table.Maui;

/// <summary>The empty state of a data list: an icon, a title, a description and buttons. Without a title it says "No results" in the language of the user.</summary>
public class KlyDataListEmpty : ContentView
{
    /// <summary>Identifies <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(KlyDataListEmpty), null, propertyChanged: (b, _, _) => ((KlyDataListEmpty)b).Refresh());
    /// <summary>Identifies <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(KlyDataListEmpty), null, propertyChanged: (b, _, _) => ((KlyDataListEmpty)b).Refresh());
    /// <summary>Identifies <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(KlyDataListEmpty), null, propertyChanged: (b, _, _) => ((KlyDataListEmpty)b).Refresh());
    /// <summary>Identifies <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(nameof(Body), typeof(View), typeof(KlyDataListEmpty), null, propertyChanged: (b, _, v) => ((KlyDataListEmpty)b)._empty.Body = (View?)v);
    /// <summary>Identifies <see cref="Actions"/>.</summary>
    public static readonly BindableProperty ActionsProperty = BindableProperty.Create(nameof(Actions), typeof(View), typeof(KlyDataListEmpty), null, propertyChanged: (b, _, v) => ((KlyDataListEmpty)b)._empty.Actions = (View?)v);

    private readonly KlyEmpty _empty = new() { Variant = EmptyVariant.Outline };

    /// <summary>Creates an empty state.</summary>
    public KlyDataListEmpty()
    {
        TableIcons.Ensure();
        Content = _empty;
        Refresh();
    }

    /// <summary>The title. Without it, the localized "No results".</summary>
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>The text under the title. Without it, the localized hint to change the search or the filters; an empty string shows none.</summary>
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <summary>The icon above the title, by name (any <see cref="KlyIcon"/> name). Defaults to <c>search-x</c>; an empty string shows none.</summary>
    public string? Icon { get => (string?)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>Extra content between the description and the actions.</summary>
    public View? Body { get => (View?)GetValue(BodyProperty); set => SetValue(BodyProperty, value); }

    /// <summary>Buttons shown under the text, for example one that clears the filters.</summary>
    public View? Actions { get => (View?)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }

    private void Refresh()
    {
        var messages = TableMessages.For(System.Globalization.CultureInfo.CurrentUICulture);
        _empty.Title = Title ?? messages.NoResults;
        _empty.Description = Description ?? messages.NoResultsDescription;
        var icon = Icon ?? TableIcons.SearchX;
        _empty.Icon = icon.Length == 0 ? null : new KlyIcon { Name = icon, Size = KlySize.Xl, IconColor = KlyTheme.TextMuted };
    }
}
