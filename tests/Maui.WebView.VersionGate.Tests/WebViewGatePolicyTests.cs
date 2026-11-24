using Shouldly;
using Xunit;

namespace Kebechet.Maui.WebView.VersionGate.Tests;

public class WebViewGatePolicyTests
{
	private const string GoogleWebViewPackageName = "com.google.android.webview";
	private const int MinimumSupportedMajorVersion = 105;
	private const int MinimumUsableMajorVersion = 101;

	[Theory]
	[InlineData(105)]
	[InlineData(157)]
	public void Decide_WithSupportedVersion_ReturnsNone(int chromiumMajorVersion)
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(chromiumMajorVersion, GoogleWebViewPackageName, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.None);
	}

	[Fact]
	public void Decide_WithUnreadableVersion_ReturnsNone()
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(null, GoogleWebViewPackageName, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.None);
	}

	[Fact]
	public void Decide_WithoutProvider_ReturnsNone()
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(95, null, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.None);
	}

	[Theory]
	[InlineData(101)]
	[InlineData(104)]
	public void Decide_WithUsableVersionBelowSupported_ReturnsDismissible(int chromiumMajorVersion)
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(chromiumMajorVersion, GoogleWebViewPackageName, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.Dismissible);
	}

	[Theory]
	[InlineData(74, "com.google.android.webview")]
	[InlineData(100, "com.google.android.webview")]
	[InlineData(95, "com.google.android.webview.beta")]
	[InlineData(90, "com.android.chrome")]
	public void Decide_WithUnusableVersionFromPlayStoreProvider_ReturnsBlocking(int chromiumMajorVersion, string providerPackageName)
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(chromiumMajorVersion, providerPackageName, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.Blocking);
	}

	[Theory]
	[InlineData("com.android.webview")]
	[InlineData("com.huawei.webview")]
	public void Decide_WithUnusableVersionFromProviderOutsidePlayStore_ReturnsDismissible(string providerPackageName)
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(95, providerPackageName, MinimumSupportedMajorVersion, MinimumUsableMajorVersion);

		// Assert
		decision.ShouldBe(WebViewGateDecision.Dismissible);
	}

	[Fact]
	public void Decide_WithoutUsableVersion_ReturnsBlockingJustBelowSupported()
	{
		// Arrange
		// Act
		var decision = WebViewGatePolicy.Decide(104, GoogleWebViewPackageName, MinimumSupportedMajorVersion, null);

		// Assert
		decision.ShouldBe(WebViewGateDecision.Blocking);
	}
}
