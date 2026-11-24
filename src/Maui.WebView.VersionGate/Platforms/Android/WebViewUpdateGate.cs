using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Button = Android.Widget.Button;
using Uri = Android.Net.Uri;
using View = Android.Views.View;
using ViewStates = Android.Views.ViewStates;
using AndroidWebView = Android.Webkit.WebView;

namespace Kebechet.Maui.WebView.VersionGate.Platforms.Android;

/// <summary>
/// Holds the app behind a dialog while the Android System WebView is too old to run it. Back and outside taps do
/// nothing, and Update leaves the dialog open, so whatever the user does in the store, they come back to it. It never
/// has to close on its own: installing a WebView update kills every process that loaded the old one
/// ("stop com.google.android.webview due to installPackageLI"), so the next start loads the update.
/// It runs from the activity's resume, which covers the launch and every return from the store, and shows a native
/// dialog rather than a MAUI DisplayAlert: an alert fired before the page has loaded is silently dropped, because it
/// waits for a NavigatedTo that never comes for a page hosted directly in a Window.
/// https://github.com/dotnet/maui/issues/12970
/// https://github.com/dotnet/maui/issues/24832
/// </summary>
internal sealed class WebViewUpdateGate
{
	private static ILogger? _logger => IPlatformApplication.Current?.Services.GetService<ILogger<WebViewUpdateGate>>();

	/// <summary>
	/// The gate runs on every resume, and a user returning from the store is the same hit, so it is reported once per
	/// process.
	/// </summary>
	private static bool _isGateHitReported;

	/// <summary>
	/// Cancel lets the user into the app until the process ends; the next start asks again.
	/// </summary>
	private static bool _isDismissedForProcess;

	private readonly WebViewVersionGateOptions _options;
	private readonly Dictionary<Activity, AlertDialog> _updateAlertDialogsByActivity = [];

	public WebViewUpdateGate(WebViewVersionGateOptions options)
	{
		_options = options;
	}

	public void Check(Activity activity)
	{
		if(_isDismissedForProcess || _updateAlertDialogsByActivity.ContainsKey(activity))
		{
			return;
		}

		try
		{
			//A provider numbers its own package versions, which need not be Chromium versions, but every Chromium
			//provider reports its engine version in the default user agent, so the version is read there. The
			//package still decides where the store link points: Chrome is the provider on Android 8-9.
			var webViewPackage = AndroidWebView.CurrentWebViewPackage;
			var userAgent = WebSettings.GetDefaultUserAgent(activity);
			var chromiumMajorVersion = WebViewUserAgent.ParseChromiumMajorVersion(userAgent);
			var decision = WebViewGatePolicy.Decide(
				chromiumMajorVersion,
				webViewPackage?.PackageName,
				_options.MinimumSupportedMajorVersion,
				_options.MinimumUsableMajorVersion);
			if(decision == WebViewGateDecision.None)
			{
				return;
			}

			ShowUpdateAlertDialog(activity, webViewPackage!.PackageName!, decision);
			ReportGateHit(chromiumMajorVersion!.Value, userAgent!, webViewPackage, decision);
		}
		catch(Exception ex)
		{
			_logger?.LogWarning(ex, $"[WebViewVersionGate] {nameof(Check)} failed");
		}
	}

	public void Dismiss(Activity activity)
	{
		var isRemoved = _updateAlertDialogsByActivity.Remove(activity, out var updateAlertDialog);
		if(!isRemoved)
		{
			return;
		}

		updateAlertDialog!.Dismiss();
	}

	private void ShowUpdateAlertDialog(Activity activity, string providerPackageName, WebViewGateDecision decision)
	{
		var texts = _options.Texts!();
		var updateAlertDialog = new AlertDialog.Builder(activity)
			.SetTitle(texts.Title)!
			.SetMessage(texts.Message)!
			.SetPositiveButton(texts.UpdateButton, (IDialogInterfaceOnClickListener?) null)!
			.SetNegativeButton(texts.CancelButton, new CancelOnClickListener(this, activity))!
			.SetCancelable(false)!
			.Create();
		updateAlertDialog.Show();
		_updateAlertDialogsByActivity[activity] = updateAlertDialog;

		var cancelButton = updateAlertDialog.GetButton((int) DialogButtonType.Negative);
		if(decision == WebViewGateDecision.Blocking && cancelButton is not null)
		{
			cancelButton.Visibility = ViewStates.Gone;
		}

		//A button listener given to the builder dismisses the dialog on tap; replacing it after Show keeps the dialog
		//open while the user is in the store.
		var updateButton = updateAlertDialog.GetButton((int) DialogButtonType.Positive);
		updateButton?.SetOnClickListener(new OpenStoreOnClickListener(activity, providerPackageName, cancelButton));
	}

	private void OnCancelled(Activity activity)
	{
		_isDismissedForProcess = true;
		_updateAlertDialogsByActivity.Remove(activity);
	}

	private void ReportGateHit(int chromiumMajorVersion, string userAgent, PackageInfo webViewPackage, WebViewGateDecision decision)
	{
		if(_isGateHitReported || _options.OnGateShown is null)
		{
			return;
		}

		_isGateHitReported = true;
		_options.OnGateShown(new WebViewGateHit
		{
			ChromiumMajorVersion = chromiumMajorVersion,
			UserAgent = userAgent,
			ProviderPackageName = webViewPackage.PackageName!,
			ProviderVersionName = webViewPackage.VersionName,
			Decision = decision,
			AndroidApiLevel = (int) Build.VERSION.SdkInt,
			DeviceManufacturer = Build.Manufacturer,
			DeviceModel = Build.Model,
		});
	}

	private sealed class CancelOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
	{
		private readonly WebViewUpdateGate _updateGate;
		private readonly Activity _activity;

		public CancelOnClickListener(WebViewUpdateGate updateGate, Activity activity)
		{
			_updateGate = updateGate;
			_activity = activity;
		}

		public void OnClick(IDialogInterface? dialog, int which)
		{
			_updateGate.OnCancelled(_activity);
		}
	}

	private sealed class OpenStoreOnClickListener : Java.Lang.Object, View.IOnClickListener
	{
		private const string StoreAppDetailsUrlPrefix = "market://details?id=";
		private const string StoreWebPageDetailsUrlPrefix = "https://play.google.com/store/apps/details?id=";

		/// <summary>
		/// The market:// link is sent to the Play Store only. On Samsung the Galaxy Store handles it too, so Android
		/// asks which store to open, and the Galaxy Store does not carry the WebView; the chooser counts as opened,
		/// so the https fallback would never run.
		/// </summary>
		private const string PlayStorePackageName = "com.android.vending";

		private readonly Activity _activity;
		private readonly string _providerPackageName;
		private readonly Button? _cancelButton;

		public OpenStoreOnClickListener(Activity activity, string providerPackageName, Button? cancelButton)
		{
			_activity = activity;
			_providerPackageName = providerPackageName;
			_cancelButton = cancelButton;
		}

		public void OnClick(View? view)
		{
			//An exception escaping a Java callback ends the process, and the user would hit it again on every tap.
			try
			{
				var isStoreAppOpened = TryOpenUrl($"{StoreAppDetailsUrlPrefix}{_providerPackageName}", PlayStorePackageName);
				if(isStoreAppOpened)
				{
					return;
				}

				//Without the Play Store nothing on the phone can update the WebView, so the user may continue.
				if(_cancelButton is not null)
				{
					_cancelButton.Visibility = ViewStates.Visible;
				}

				var isStoreWebPageOpened = TryOpenUrl($"{StoreWebPageDetailsUrlPrefix}{_providerPackageName}", targetPackageName: null);
				if(!isStoreWebPageOpened)
				{
					_logger?.LogWarning($"[WebViewVersionGate] {nameof(OnClick)}: no app opens the store page of {_providerPackageName}");
				}
			}
			catch(Exception ex)
			{
				_logger?.LogWarning(ex, $"[WebViewVersionGate] {nameof(OnClick)} failed");
			}
		}

		private bool TryOpenUrl(string url, string? targetPackageName)
		{
			var viewIntent = new Intent(Intent.ActionView, Uri.Parse(url));
			viewIntent.AddFlags(ActivityFlags.NewTask);
			if(targetPackageName is not null)
			{
				viewIntent.SetPackage(targetPackageName);
			}

			try
			{
				_activity.StartActivity(viewIntent);
				return true;
			}
			catch(ActivityNotFoundException)
			{
				return false;
			}
		}
	}
}
