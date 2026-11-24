using Kebechet.Maui.WebView.VersionGate;
using Microsoft.Extensions.Logging;

namespace DemoApp;

public static class MauiProgram
{
	/// <summary>
	/// Far above any real WebView, so the dialog shows on every phone.
	/// </summary>
	private const int DemoMinimumSupportedMajorVersion = 999;

	/// <summary>
	/// Every real WebView counts as usable, so the dialog offers Cancel and the demo stays reachable.
	/// </summary>
	private const int DemoMinimumUsableMajorVersion = 1;

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseWebViewVersionGate(options =>
			{
				options.MinimumSupportedMajorVersion = DemoMinimumSupportedMajorVersion;
				options.MinimumUsableMajorVersion = DemoMinimumUsableMajorVersion;
				options.Texts = () => new WebViewGateTexts(
					Title: "Update needed",
					Message: "The system WebView component is outdated, so the app can't display correctly. Please update it in the store, then return to the app.",
					UpdateButton: "Update",
					CancelButton: "Cancel");
				options.OnGateShown = GateHitLog.Record;
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
