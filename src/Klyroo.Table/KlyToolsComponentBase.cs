using Klyroo.UI.Theming;

namespace Klyroo.Table;

/// <summary>
/// Base of the components of Klyroo.Table: the theme resolution of <see cref="KlyComponentBase"/>, plus the options and the texts of this package.
/// </summary>
public abstract class KlyToolsComponentBase : KlyComponentBase
{
    /// <summary>The options of Klyroo.Table: its themes and message overrides.</summary>
    protected KlyrooTableOptions ToolsOptions => Options.Extension<KlyrooTableOptions>();

    /// <summary>The texts of the locale of this component (the same locale as <c>Messages</c> of Klyroo.UI), with the changes of <see cref="KlyrooTableOptions.Messages"/> applied.</summary>
    protected KlyToolsMessages ToolsMessages => ToolsOptions.ResolveMessages(CurrentLocale.Code);
}
