namespace Klyroo.Table.Maui;

/// <summary>A view that raises <see cref="Tapped"/> when it is tapped: the touch area of a row, an icon or a heading. A disabled one ignores the tap.</summary>
internal class TableTap : ContentView
{
    public TableTap(View content)
    {
        Content = content;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Activate();
        GestureRecognizers.Add(tap);
    }

    /// <summary>Raised when the view is tapped.</summary>
    public event EventHandler? Tapped;

    /// <summary>What a screen reader says for the view.</summary>
    public string? AccessibleName
    {
        get => SemanticProperties.GetDescription(this);
        set => SemanticProperties.SetDescription(this, value);
    }

    /// <summary>What a screen reader says about what the tap does.</summary>
    public string? AccessibleHint
    {
        get => SemanticProperties.GetHint(this);
        set => SemanticProperties.SetHint(this, value);
    }

    /// <summary>Does what a tap does.</summary>
    public void Activate()
    {
        if (IsEnabled) Tapped?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled)) Opacity = IsEnabled ? 1 : 0.6;
    }
}
