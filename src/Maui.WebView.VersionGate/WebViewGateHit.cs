namespace Kebechet.Maui.WebView.VersionGate;

/// <summary>
/// A WebView the gate stopped, passed to <see cref="WebViewVersionGateOptions.OnGateShown"/> so the app can report it.
/// Users stopped here may never reach the app's own UI, so this is often the only signal that they exist.
/// </summary>
public sealed class WebViewGateHit
{
	/// <summary>The engine version from the user agent, the number the gate decided on.</summary>
	public required int ChromiumMajorVersion { get; init; }

	/// <summary>The WebView's default user agent.</summary>
	public required string UserAgent { get; init; }

	/// <summary>The package of the WebView provider, e.g. com.google.android.webview, or com.android.chrome on Android 8-9.</summary>
	public required string ProviderPackageName { get; init; }

	/// <summary>The provider package's own version name, which a non-Google provider may number differently from Chromium.</summary>
	public string? ProviderVersionName { get; init; }

	/// <summary>Whether the dialog was shown with Cancel or with Update only.</summary>
	public required WebViewGateDecision Decision { get; init; }

	/// <summary>The Android API level, e.g. 29 for Android 10.</summary>
	public required int AndroidApiLevel { get; init; }

	/// <summary>The device manufacturer as Android reports it, e.g. samsung.</summary>
	public string? DeviceManufacturer { get; init; }

	/// <summary>The device model as Android reports it, e.g. SM-A536B.</summary>
	public string? DeviceModel { get; init; }
}
