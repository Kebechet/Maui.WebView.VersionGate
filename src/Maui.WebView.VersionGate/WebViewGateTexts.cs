namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// The texts of the update dialog, in the app's language.
/// </summary>
/// <param name="Title">The dialog title, e.g. "Update needed".</param>
/// <param name="Message">Why the app cannot run and what to do, e.g. "Update the system WebView in the store, then return to the app."</param>
/// <param name="UpdateButton">The button that opens the store.</param>
/// <param name="CancelButton">The button that closes the dialog when it may be dismissed.</param>
public sealed record WebViewGateTexts(string Title, string Message, string UpdateButton, string CancelButton);
