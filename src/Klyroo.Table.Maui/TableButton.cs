namespace Klyroo.Table.Maui;

/// <summary>
/// A small button with an optional icon and text, drawn with the variants of <see cref="KlyButton"/>. <see cref="KlyButton"/> shows no icon,
/// and the parts of the data list need one beside their label.
/// </summary>
internal sealed class TableButton : Border
{
    private readonly Label _label = new() { LineBreakMode = LineBreakMode.TailTruncation, VerticalTextAlignment = TextAlignment.Center };
    private readonly KlyIcon _icon = new() { Size = KlySize.Sm };
    private readonly HorizontalStackLayout _row = new() { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    private readonly ButtonVariant _variant;
    private readonly KlyColor _color;
    private readonly KlySize _size;
    private string? _text;
    private bool _busy;

    public TableButton(string? text, string? icon = null, ButtonVariant variant = ButtonVariant.Outline, KlyColor? color = null, KlySize size = KlySize.Sm)
    {
        TableIcons.Ensure();
        _variant = variant;
        _color = color ?? KlyColor.Neutral;
        _size = size;
        _text = text;
        _row.Children.Add(_icon);
        _row.Children.Add(_label);
        Content = _row;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Activate();
        GestureRecognizers.Add(tap);
        IconName = icon;
        ApplyStyle();
        Refresh();
    }

    /// <summary>Raised when the button is tapped, unless it is disabled or busy.</summary>
    public event EventHandler? Clicked;

    /// <summary>The text of the button; without it the button is only an icon.</summary>
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            Refresh();
        }
    }

    /// <summary>The name of the icon before the text.</summary>
    public string? IconName { get; set; }

    /// <summary>What a screen reader says; without it the text.</summary>
    public string? AccessibleName
    {
        get => SemanticProperties.GetDescription(this);
        set => SemanticProperties.SetDescription(this, value);
    }

    /// <summary>What a screen reader says about the effect of the button, such as why it is disabled.</summary>
    public string? AccessibleHint
    {
        get => SemanticProperties.GetHint(this);
        set => SemanticProperties.SetHint(this, value);
    }

    /// <summary>A spinner replaces the icon and the button ignores taps while the work runs.</summary>
    public bool IsBusy
    {
        get => _busy;
        set
        {
            if (_busy == value) return;
            _busy = value;
            Refresh();
        }
    }

    internal KlyIcon IconView => _icon;

    internal Label TextView => _label;

    /// <summary>Does what a tap does.</summary>
    public void Activate()
    {
        if (IsEnabled && !_busy) Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled)) Opacity = IsEnabled ? 1 : 0.6;
    }

    /// <summary>Shows the icon and the text again after <see cref="IconName"/> changed.</summary>
    public void Refresh()
    {
        _icon.Name = _busy ? KlyIconNames.Loading : IconName ?? "";
        _icon.IsVisible = _busy || !string.IsNullOrEmpty(IconName);
        _label.Text = _text;
        _label.IsVisible = !string.IsNullOrEmpty(_text);
        if (string.IsNullOrEmpty(AccessibleName) && !string.IsNullOrEmpty(_text)) SemanticProperties.SetDescription(this, _text);
    }

    private void ApplyStyle()
    {
        var surface = TableSurface.For(_variant, KlyTheme.Resolve(_color));
        this.Apply(surface);
        _label.SetThemeColor(Label.TextColorProperty, surface.Text);
        _icon.IconColor = surface.Text;
        this.SetRadius(KlyTheme.RadiusOf(1));
        (Padding, _label.FontSize, MinimumHeightRequest) = _size switch
        {
            KlySize.Xs => (new Thickness(8, 4), 12d, 28d),
            KlySize.Sm => (new Thickness(10, 6), 12d, 36d),
            KlySize.Md => (new Thickness(10, 6), 14d, 40d),
            _ => (new Thickness(12, 8), 14d, 44d),
        };
        MinimumWidthRequest = string.IsNullOrEmpty(_text) ? MinimumHeightRequest : 0;
    }
}
