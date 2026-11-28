namespace Klyroo.Table.Maui;

/// <summary>The toolbar actions of a list (export, import, a new row...) as a menu behind one button. They do not need a selection; an action that wants one reads it from its context. Needs a <see cref="KlyOverlayHost"/> above it.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public class KlyDataListActionsDropdown<TItem> : DataListActionPartBase<TItem>
{
    /// <summary>Identifies <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(KlyDataListActionsDropdown<TItem>), null, propertyChanged: (b, _, _) => ((KlyDataListActionsDropdown<TItem>)b).Redraw());
    /// <summary>Identifies <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(KlyDataListActionsDropdown<TItem>), KlyIconNames.Ellipsis, propertyChanged: (b, _, _) => ((KlyDataListActionsDropdown<TItem>)b).Redraw());

    private KlyDropdownMenu? _menu;
    private TableButton? _trigger;

    /// <summary>Creates an actions menu.</summary>
    public KlyDataListActionsDropdown()
    {
        Refresh();
    }

    /// <summary>The label of the button that opens the menu. Without it the localized "Actions".</summary>
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>The name of the icon of the button (<c>ellipsis</c> by default).</summary>
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    internal KlyDropdownMenu? Menu => _menu;

    internal TableButton? Trigger => _trigger;

    /// <summary>Does what choosing an entry of the menu does.</summary>
    /// <param name="key">The key of the action.</param>
    internal Task Choose(string key) => Actions.FirstOrDefault(a => a.Key == key) is { } action ? RequestAsync(action) : Task.CompletedTask;

    /// <inheritdoc />
    protected override void Refresh()
    {
        if (Context is null)
        {
            Content = null;
            return;
        }
        var describe = Describe();
        var visible = Actions.Where(a => IsShown(a, describe)).ToList();
        if (visible.Count == 0)
        {
            Content = null;
            _menu = null;
            return;
        }
        _trigger = new TableButton(Text ?? Messages.ActionsMenu, Icon, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { IsBusy = visible.Any(IsRunning) };
        if (_menu is null)
        {
            _menu = new KlyDropdownMenu { Placement = KlyPlacement.BottomEnd };
            _menu.ItemSelected += (_, item) => _ = Choose(item.Value ?? item.Label);
        }
        _menu.Content = _trigger;
        _menu.AccessibleName = Text ?? Messages.ActionsMenu;
        _menu.Items = [.. visible.Select(a => new KlyMenuItem
        {
            Label = a.Label,
            Value = a.Key,
            Icon = a.Icon,
            IsEnabled = !IsDisabled(a, describe),
            Color = a.Color.Equals(KlyColor.Neutral) ? default(KlyColor?) : a.Color,
        })];
        Content = _menu;
    }
}
