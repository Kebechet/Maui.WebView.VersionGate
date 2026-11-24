using Shouldly;
using Xunit;

namespace Kebechet.Maui.WebView.VersionGate.Tests;

public class WebViewVersionGateOptionsTests
{
	[Fact]
	public void Validate_WithCompleteOptions_DoesNotThrow()
	{
		// Arrange
		var options = CreateOptions();

		// Act
		// Assert
		Should.NotThrow(options.Validate);
	}

	[Fact]
	public void Validate_WithoutSupportedVersion_ThrowsNamingIt()
	{
		// Arrange
		var options = CreateOptions();
		options.MinimumSupportedMajorVersion = 0;

		// Act
		var exception = Should.Throw<ArgumentException>(options.Validate);

		// Assert
		exception.ParamName.ShouldBe(nameof(WebViewVersionGateOptions.MinimumSupportedMajorVersion));
	}

	[Fact]
	public void Validate_WithUsableVersionAboveSupported_ThrowsNamingIt()
	{
		// Arrange
		var options = CreateOptions();
		options.MinimumUsableMajorVersion = 110;

		// Act
		var exception = Should.Throw<ArgumentException>(options.Validate);

		// Assert
		exception.ParamName.ShouldBe(nameof(WebViewVersionGateOptions.MinimumUsableMajorVersion));
	}

	[Fact]
	public void Validate_WithoutTexts_ThrowsNamingThem()
	{
		// Arrange
		var options = CreateOptions();
		options.Texts = null;

		// Act
		var exception = Should.Throw<ArgumentException>(options.Validate);

		// Assert
		exception.ParamName.ShouldBe(nameof(WebViewVersionGateOptions.Texts));
	}

	private static WebViewVersionGateOptions CreateOptions()
	{
		return new WebViewVersionGateOptions
		{
			MinimumSupportedMajorVersion = 105,
			MinimumUsableMajorVersion = 101,
			Texts = () => new WebViewGateTexts("Update needed", "Update the WebView.", "Update", "Cancel"),
		};
	}
}
