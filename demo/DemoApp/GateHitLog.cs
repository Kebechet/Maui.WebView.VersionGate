using Kebechet.Maui.WebView.VersionGate;

namespace DemoApp;

/// <summary>
/// Keeps the hit the gate reported, so the home page can show what it read. A real app sends it to its error or
/// analytics service instead.
/// </summary>
public static class GateHitLog
{
	public static WebViewGateHit? LastGateHit { get; private set; }

	public static void Record(WebViewGateHit gateHit)
	{
		LastGateHit = gateHit;
	}
}
