namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// Reads the engine version out of a WebView's default user agent.
/// </summary>
public static class WebViewUserAgent
{
	/// <summary>
	/// The user agent token every Chromium-based WebView puts its engine version after, as in "Chrome/101.0.4951.61".
	/// </summary>
	private const string ChromeProductToken = "Chrome/";

	/// <returns>
	/// The major version after the Chrome token, such as 157 for "Chrome/157.0.8103.50" or 125 for the reduced
	/// "Chrome/125.0.0.0", or null when the user agent has none.
	/// </returns>
	public static int? ParseChromiumMajorVersion(string? userAgent)
	{
		if(string.IsNullOrWhiteSpace(userAgent))
		{
			return null;
		}

		var chromeProductTokenIndex = userAgent.IndexOf(ChromeProductToken, StringComparison.Ordinal);
		if(chromeProductTokenIndex < 0)
		{
			return null;
		}

		var textAfterChromeProductToken = userAgent[(chromeProductTokenIndex + ChromeProductToken.Length)..];
		var majorVersionDigits = string.Concat(textAfterChromeProductToken.TakeWhile(char.IsAsciiDigit));
		if(!int.TryParse(majorVersionDigits, out var majorVersion))
		{
			return null;
		}

		return majorVersion;
	}
}
