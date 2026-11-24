namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// Decides whether a WebView gets the update dialog and whether the user may dismiss it.
/// </summary>
public static class WebViewGatePolicy
{
	/// <summary>
	/// WebView providers Google ships through the Play Store: Android System WebView and its channels, and Chrome,
	/// which is the provider on Android 8-9. Any other provider (an AOSP build on a custom ROM, an OEM WebView)
	/// cannot be updated from the Play Store, so blocking its user would leave them no way forward.
	/// </summary>
	private static readonly HashSet<string> _playStoreProviderPackageNames =
	[
		"com.google.android.webview",
		"com.google.android.webview.beta",
		"com.google.android.webview.dev",
		"com.google.android.webview.canary",
		"com.android.chrome",
		"com.chrome.beta",
		"com.chrome.dev",
		"com.chrome.canary",
	];

	/// <param name="chromiumMajorVersion">The engine version from the WebView's user agent, or null when it could not be read.</param>
	/// <param name="providerPackageName">The package of the WebView provider in use, or null when the system reports none.</param>
	/// <param name="minimumSupportedMajorVersion">Below this version the dialog shows.</param>
	/// <param name="minimumUsableMajorVersion">From this version the dialog can be dismissed, because the app still runs there; null when no version below the supported one can be dismissed.</param>
	/// <returns>
	/// <see cref="WebViewGateDecision.None"/> when the version or the provider cannot be read: a WebView the gate
	/// cannot read is never blocked.
	/// </returns>
	public static WebViewGateDecision Decide(
		int? chromiumMajorVersion,
		string? providerPackageName,
		int minimumSupportedMajorVersion,
		int? minimumUsableMajorVersion)
	{
		if(chromiumMajorVersion is null ||
			providerPackageName is null ||
			chromiumMajorVersion.Value >= minimumSupportedMajorVersion)
		{
			return WebViewGateDecision.None;
		}

		if(minimumUsableMajorVersion is not null &&
			chromiumMajorVersion.Value >= minimumUsableMajorVersion.Value)
		{
			return WebViewGateDecision.Dismissible;
		}

		if(!IsPlayStoreProvider(providerPackageName))
		{
			return WebViewGateDecision.Dismissible;
		}

		return WebViewGateDecision.Blocking;
	}

	/// <returns>True when the Play Store can update this WebView provider.</returns>
	public static bool IsPlayStoreProvider(string providerPackageName)
	{
		return _playStoreProviderPackageNames.Contains(providerPackageName);
	}
}
