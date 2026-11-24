namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// Configures the gate in <see cref="MauiAppBuilderExtensions.UseWebViewVersionGate"/>.
/// </summary>
public sealed class WebViewVersionGateOptions
{
	/// <summary>
	/// The oldest WebView major version the app supports. Below it the update dialog shows. Measure it for your app:
	/// it depends on the JavaScript and CSS features the app uses.
	/// </summary>
	public int MinimumSupportedMajorVersion { get; set; }

	/// <summary>
	/// The oldest WebView major version on which the app still runs, with flaws. From it up to
	/// <see cref="MinimumSupportedMajorVersion"/> the dialog has Cancel. Null when every version below the supported
	/// one should block (unless the WebView cannot be updated from the Play Store).
	/// </summary>
	public int? MinimumUsableMajorVersion { get; set; }

	/// <summary>
	/// The dialog texts, read each time the dialog is built so they follow the app's current language.
	/// </summary>
	public Func<WebViewGateTexts>? Texts { get; set; }

	/// <summary>
	/// Called once per process when the dialog first shows, for reporting. Runs on the UI thread.
	/// </summary>
	public Action<WebViewGateHit>? OnGateShown { get; set; }

	/// <exception cref="ArgumentException">The options are incomplete or contradict each other.</exception>
	internal void Validate()
	{
		if(MinimumSupportedMajorVersion <= 0)
		{
			throw new ArgumentException($"{nameof(MinimumSupportedMajorVersion)} must be a WebView major version, such as 105.", nameof(MinimumSupportedMajorVersion));
		}

		if(MinimumUsableMajorVersion > MinimumSupportedMajorVersion)
		{
			throw new ArgumentException($"{nameof(MinimumUsableMajorVersion)} {MinimumUsableMajorVersion} is above {nameof(MinimumSupportedMajorVersion)} {MinimumSupportedMajorVersion}.", nameof(MinimumUsableMajorVersion));
		}

		if(Texts is null)
		{
			throw new ArgumentException($"{nameof(Texts)} must provide the dialog texts.", nameof(Texts));
		}
	}
}
