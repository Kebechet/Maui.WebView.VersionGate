namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// What the gate does for a given WebView.
/// </summary>
public enum WebViewGateDecision
{
	/// <summary>The WebView is new enough, or its version cannot be read: no dialog.</summary>
	None,

	/// <summary>The dialog shows with Update and Cancel: the app still runs, or the WebView cannot be updated from the Play Store.</summary>
	Dismissible,

	/// <summary>The dialog shows with Update only. Cancel appears only if the Play Store cannot be opened.</summary>
	Blocking,
}
