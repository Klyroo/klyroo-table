using Lux.UI.Theming;

namespace Lux.Table;

/// <summary>
/// Base of the components of Lux.Table: the theme resolution of <see cref="BxComponentBase"/>, plus the options and the texts of this package.
/// </summary>
public abstract class BxToolsComponentBase : BxComponentBase
{
    /// <summary>The options of Lux.Table: its themes and message overrides.</summary>
    protected LuxTableOptions ToolsOptions => Options.Extension<LuxTableOptions>();

    /// <summary>The texts of the locale of this component (the same locale as <c>Messages</c> of Lux.UI), with the changes of <see cref="LuxTableOptions.Messages"/> applied.</summary>
    protected BxToolsMessages ToolsMessages => ToolsOptions.ResolveMessages(CurrentLocale.Code);
}
