namespace Klyroo.Table.Maui;

/// <summary>A question a part asks before it acts.</summary>
/// <param name="Title">The title of the dialog.</param>
/// <param name="Message">The text under the title.</param>
/// <param name="ConfirmLabel">The label of the confirm button.</param>
/// <param name="CancelLabel">The label of the cancel button.</param>
/// <param name="Color">The color of the confirm button.</param>
internal sealed record TableConfirmRequest(string Title, string? Message, string ConfirmLabel, string CancelLabel, KlyColor Color);

/// <summary>Opens the dialogs of the data list in the <see cref="KlyOverlayHost"/> above a part.</summary>
internal static class TableDialogs
{
    /// <summary>Asks a yes or no question; true when the user confirms.</summary>
    public static Task<bool> ConfirmAsync(Element from, TableConfirmRequest request, CancellationToken cancellationToken)
    {
        if (!KlyOverlayHost.TryFind(from, out var host))
        {
            throw new InvalidOperationException("An action that asks for a confirmation needs a KlyOverlayHost above the data list: wrap the page content in KlyOverlayHost.");
        }
        return host.ConfirmAsync(request.Title, request.Message, request.ConfirmLabel, request.CancelLabel, request.Color, cancellationToken);
    }
}
