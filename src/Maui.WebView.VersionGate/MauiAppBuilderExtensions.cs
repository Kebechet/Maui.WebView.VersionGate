#if ANDROID
using Kebechet.Maui.WebView.VersionGate.Platforms.Android;
using Microsoft.Maui.LifecycleEvents;
#endif

namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// Registers the WebView version gate on a MAUI app.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// On Android, shows the update dialog on every resume of a MAUI activity while the system WebView is older than
	/// <see cref="WebViewVersionGateOptions.MinimumSupportedMajorVersion"/>. On other platforms it registers nothing:
	/// their WebView updates with the OS.
	/// </summary>
	/// <exception cref="ArgumentException">The options are incomplete or contradict each other.</exception>
	public static MauiAppBuilder UseWebViewVersionGate(this MauiAppBuilder builder, Action<WebViewVersionGateOptions> configure)
	{
		var options = new WebViewVersionGateOptions();
		configure(options);
		options.Validate();

#if ANDROID
		var updateGate = new WebViewUpdateGate(options);
		builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android
			.OnResume(updateGate.Check)
			.OnDestroy(updateGate.Dismiss)));
#endif

		return builder;
	}
}
