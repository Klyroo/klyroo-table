namespace Klyroo.Table.Maui;

/// <summary>What the table and the grid draw the same way: the progress line, the placeholders, the empty state and the text of a row as label and value pairs.</summary>
internal static class TableBody
{
    /// <summary>The thin line drawn on top of the rows while they are loaded again.</summary>
    public static BoxView ProgressLine()
    {
        var line = new BoxView { HeightRequest = 2, HorizontalOptions = LayoutOptions.Fill, IsVisible = false };
        line.SetThemeColor(BoxView.ColorProperty, KlyTheme.Resolve(KlyColor.Primary).Text);
        AutomationProperties.SetIsInAccessibleTree(line, false);
        return line;
    }

    /// <summary>The vertical padding of a row for a density.</summary>
    public static double RowPadding(DataListDensity density) => density switch
    {
        DataListDensity.Compact => 4,
        DataListDensity.Comfortable => 14,
        _ => 8,
    };

    /// <summary>A placeholder bar.</summary>
    public static View Bar(double width, double height = 14) => new KlySkeleton { WidthRequest = width, HeightRequest = height, HorizontalOptions = LayoutOptions.Start };

    /// <summary>A line between two rows.</summary>
    public static View Separator()
    {
        var line = new BoxView { HeightRequest = 1, HorizontalOptions = LayoutOptions.Fill };
        line.SetThemeColor(BoxView.ColorProperty, KlyTheme.Border);
        AutomationProperties.SetIsInAccessibleTree(line, false);
        return line;
    }

    /// <summary>The empty state of a list that matched nothing: the filtered one with a button that clears the search or the filters, or the one of an empty source.</summary>
    public static View Empty<TItem>(DataListContext<TItem> context, DataListEmptyState? state, Func<DataListContext<TItem>, View>? custom)
    {
        if (custom is not null) return custom(context);
        var messages = context.Messages;
        var shown = context.LoadedState ?? context.State;
        var search = shown.Search.Trim();
        if (context.IsFilteredFor(shown))
        {
            var onlySearch = shown.Filters.SequenceEqual(context.ResetState(shown).Filters);
            var reset = new TableButton(onlySearch ? messages.ClearSearch : messages.ResetFilters, null, ButtonVariant.Outline, KlyColor.Neutral, KlySize.Sm) { HorizontalOptions = LayoutOptions.Center };
            reset.Clicked += (_, _) => context.Reset();
            return new KlyDataListEmpty
            {
                Title = search.Length == 0 ? messages.NoResultsFiltered : string.Format(context.Culture, messages.NoResultsFor, search),
                Description = search.Length == 0 ? messages.NoResultsDescription : messages.NoResultsForDescription,
                Actions = reset,
            };
        }
        var empty = new KlyDataListEmpty
        {
            Icon = state?.Icon,
            Title = state?.Title,
            Description = state?.HideDescription == true ? "" : state?.Description,
        };
        if (state?.Action is { } action)
        {
            var button = new TableButton(action.Label, action.Icon, action.Variant, action.Color, KlySize.Sm) { HorizontalOptions = LayoutOptions.Center };
            button.Clicked += (_, _) => action.OnClick?.Invoke();
            empty.Actions = button;
        }
        return empty;
    }

    /// <summary>The label and the text of each visible column of the table for a row, or of each field of the schema when no column was declared.</summary>
    public static IReadOnlyList<(string Label, string Text)> Fields<TItem>(DataListContext<TItem> context, TItem item)
    {
        var schema = context.Schema;
        var culture = context.Culture;
        var columns = context.TableColumns();
        if (columns.Count > 0)
        {
            return [.. columns.Select(c => (c.ResolvedTitle, c.ReadText(item, schema, culture) ?? "")).Where(f => f.Item2.Length > 0 || true)];
        }
        if (schema is null) return [];
        return [.. schema.FieldInfos.Where(f => !f.IsCollection).Select(f => (TableValues.Humanize(f.Key), schema.FormatField(item, f.Key, null, culture) ?? ""))];
    }

    /// <summary>The check box of a row or a group, with its accessible name.</summary>
    public static KlyCheckbox Box(string name, bool isChecked, Action<bool> changed)
    {
        var box = new KlyCheckbox { IsChecked = isChecked };
        SemanticProperties.SetDescription(box, name);
        box.CheckedChanged += (_, e) => changed(e.Value);
        return box;
    }
}
