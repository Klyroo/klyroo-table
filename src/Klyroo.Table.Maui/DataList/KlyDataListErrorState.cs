namespace Klyroo.Table.Maui;

/// <summary>Shown when the newest load failed, with a button that tries again. The rows of the previous result, if any, stay in the table under it.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListErrorState<TItem> : DataListPartBase<TItem>
{
    /// <summary>Identifies <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(KlyDataListErrorState<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListErrorState<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(KlyDataListErrorState<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListErrorState<TItem>)b).Redraw());

    private readonly KlyAlert _alert = new() { Color = KlyColor.Error, Variant = KlyVariant.Soft };
    private readonly TableButton _retry = new(null, null, ButtonVariant.Outline, KlyColor.Error, KlySize.Sm);
    private readonly VerticalStackLayout _stack = new() { Spacing = 8 };

    /// <summary>Creates an error state.</summary>
    public KlyDataListErrorState()
    {
        _retry.Clicked += (_, _) =>
        {
            if (Context is { } context) _ = context.RefreshAsync();
        };
        _stack.Children.Add(_alert);
        _stack.Children.Add(_retry);
        Refresh();
    }

    /// <summary>The title. Without it, the localized "Could not load the data".</summary>
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>The text under the title. Without it, the localized hint to try again.</summary>
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    internal KlyAlert Alert => _alert;

    internal TableButton RetryButton => _retry;

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is not { HasError: true })
        {
            Content = null;
            return;
        }
        _alert.Title = Title ?? Messages.ErrorTitle;
        _alert.Description = Description ?? Messages.ErrorDescription;
        _retry.Text = Messages.Retry;
        SemanticProperties.SetDescription(_alert, _alert.Title);
        Content = _stack;
    }
}
