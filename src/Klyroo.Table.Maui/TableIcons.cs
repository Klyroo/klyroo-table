namespace Klyroo.Table.Maui;

/// <summary>The Lucide icons of the data list that Klyroo.UI.Maui does not build in, registered once.</summary>
internal static class TableIcons
{
    public const string SearchX = "search-x";
    public const string Columns = "columns-3";
    public const string Pin = "pin";
    public const string PinOff = "pin-off";
    public const string ArrowUpDown = "arrow-up-down";
    public const string LayoutGrid = "layout-grid";
    public const string Table = "table-2";
    public const string Filter = "list-filter";

    private static int _registered;

    /// <summary>Registers the icons the first time it is called. The controls call it when they are created.</summary>
    public static void Ensure()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 1) return;
        KlyIcons.Register(SearchX, "m13.5 8.5l-5 5m0-5l5 5", "M11 3a8 8 0 1 0 0 16a8 8 0 0 0 0-16z", "m21 21l-4.3-4.3");
        KlyIcons.Register(Columns, "M5 3h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2z", "M9 3v18m6-18v18");
        KlyIcons.Register(Pin, "M12 17v5M9 10.76a2 2 0 0 1-1.11 1.79l-1.78.9A2 2 0 0 0 5 15.24V16a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-.76a2 2 0 0 0-1.11-1.79l-1.78-.9A2 2 0 0 1 15 10.76V7a1 1 0 0 1 1-1a2 2 0 0 0 0-4H8a2 2 0 0 0 0 4a1 1 0 0 1 1 1z");
        KlyIcons.Register(PinOff, "M12 17v5m3-12.66V7a1 1 0 0 1 1-1a2 2 0 0 0 0-4H7.89M2 2l20 20M9 9v1.76a2 2 0 0 1-1.11 1.79l-1.78.9A2 2 0 0 0 5 15.24V16a1 1 0 0 0 1 1h11");
        KlyIcons.Register(ArrowUpDown, "m21 16l-4 4l-4-4m4 4V4M3 8l4-4l4 4M7 4v16");
        KlyIcons.Register(LayoutGrid, "M4 3h5a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z", "M15 3h5a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1h-5a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z", "M15 14h5a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1h-5a1 1 0 0 1-1-1v-5a1 1 0 0 1 1-1z", "M4 14h5a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1v-5a1 1 0 0 1 1-1z");
        KlyIcons.Register(Table, "M3 9h18M9 3v18", "M5 3h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2z");
        KlyIcons.Register(Filter, "M3 6h18", "M7 12h10", "M10 18h4");
    }
}
