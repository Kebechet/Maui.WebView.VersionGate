using Shouldly;
using Xunit;

namespace Kebechet.Maui.WebView.VersionGate.Tests;

/// <summary>
/// The full user agents were read from the WebViews of real phones (DevTools /json/version, 2026-11-24). The built
/// ones carry the bundled WebViews of official emulator images and the reduced user agent's 125.0.0.0.
/// </summary>
public class WebViewUserAgentTests
{
	private const string UserAgentWithoutChromeVersion = "Mozilla/5.0 (Linux; Android 13; sdk_gphone64_x86_64 Build/TE1A.220922.034; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Mobile Safari/537.36";

	[Theory]
	[InlineData("Mozilla/5.0 (Linux; Android 16; Pixel 6 Build/CP1A.260405.005; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/157.0.8103.50 Mobile Safari/537.36", 157)]
	[InlineData("Mozilla/5.0 (Linux; Android 16; SM-A536B Build/BP2A.250605.031.A3; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/157.0.8103.50 Mobile Safari/537.36", 157)]
	[InlineData("Mozilla/5.0 (Linux; Android 11; M2006C3LG Build/RP1A.200720.011; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/153.0.8010.36 Mobile Safari/537.36", 153)]
	public void ParseChromiumMajorVersion_WithRealPhoneUserAgent_ReturnsWebViewMajorVersion(string userAgent, int expectedMajorVersion)
	{
		// Arrange
		// Act
		var majorVersion = WebViewUserAgent.ParseChromiumMajorVersion(userAgent);

		// Assert
		majorVersion.ShouldBe(expectedMajorVersion);
	}

	[Theory]
	[InlineData("74.0.3729.185", 74)]
	[InlineData("95.0.4638.74", 95)]
	[InlineData("101.0.4951.61", 101)]
	[InlineData("125.0.0.0", 125)]
	public void ParseChromiumMajorVersion_WithChromeVersion_ReturnsMajorVersion(string chromeVersion, int expectedMajorVersion)
	{
		// Arrange
		var userAgent = CreateUserAgent(chromeVersion);

		// Act
		var majorVersion = WebViewUserAgent.ParseChromiumMajorVersion(userAgent);

		// Assert
		majorVersion.ShouldBe(expectedMajorVersion);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(UserAgentWithoutChromeVersion)]
	[InlineData("Mozilla/5.0 (Linux; Android 13; wv) Chrome/ Mobile Safari/537.36")]
	public void ParseChromiumMajorVersion_WithoutChromeVersion_ReturnsNull(string? userAgent)
	{
		// Arrange
		// Act
		var majorVersion = WebViewUserAgent.ParseChromiumMajorVersion(userAgent);

		// Assert
		majorVersion.ShouldBeNull();
	}

	private static string CreateUserAgent(string chromeVersion)
	{
		return $"Mozilla/5.0 (Linux; Android 13; sdk_gphone64_x86_64 Build/TE1A.220922.034; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/{chromeVersion} Mobile Safari/537.36";
	}
}
