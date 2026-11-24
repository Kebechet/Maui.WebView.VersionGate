[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/kebechet)

# Maui.WebView.VersionGate
[![NuGet Version](https://img.shields.io/nuget/v/Kebechet.Maui.WebView.VersionGate)](https://www.nuget.org/packages/Kebechet.Maui.WebView.VersionGate/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Kebechet.Maui.WebView.VersionGate)](https://www.nuget.org/packages/Kebechet.Maui.WebView.VersionGate/)
[![Build](https://github.com/Kebechet/Maui.WebView.VersionGate/actions/workflows/build.yml/badge.svg)](https://github.com/Kebechet/Maui.WebView.VersionGate/actions/workflows/build.yml)
![Last updated](https://img.shields.io/github/last-commit/Kebechet/Maui.WebView.VersionGate/main?label=last%20updated)
[![Twitter](https://img.shields.io/twitter/url/https/twitter.com/samuel_sidor.svg?style=social&label=Follow%20samuel_sidor)](https://x.com/samuel_sidor)

Holds a .NET MAUI app behind an update dialog while the Android System WebView is too old to run it, and sends the user to the Play Store to update it.

## Why

On Android the WebView is an app of its own (Android System WebView, or Chrome on Android 8-9) that updates through the Play Store, apart from the OS. Plenty of phones run one that is years old. A Blazor Hybrid app on such a WebView may show nothing but its splash screen, or raw unstyled HTML, and the user simply leaves. iOS, Mac and Windows need no gate: their WebView updates with the OS.

## What it does

On every resume of a MAUI activity, the gate reads the Chromium version from the WebView's default user agent (not from the provider package's version name, which a non-Google provider may number its own way) and decides:

| WebView | Dialog |
|---|---|
| `MinimumSupportedMajorVersion` or newer, or the version cannot be read | none |
| from `MinimumUsableMajorVersion` up to the supported one | Update + Cancel: the app still runs there, with flaws |
| older, from Google's WebView or Chrome | Update only |
| older, from a provider the Play Store cannot update (AOSP build, OEM WebView) | Update + Cancel: blocking would leave no way forward |

- **Update** opens the provider's page in the Play Store app only, never a store chooser: on Samsung the Galaxy Store also handles `market://` links and does not carry the WebView. Without the Play Store it falls back to the web page and reveals Cancel.
- The dialog stays up while the user is in the store, and back or an outside tap does nothing. Installing a WebView update kills every process that loaded the old one, so the next app start loads the new WebView; there is no restart logic.
- **Cancel** lets the user in until the process ends; the next start asks again.
- **`OnGateShown`** is called once per process with the Chromium version, user agent, provider package and version, decision, API level and device, for reporting. Users stopped here may never reach the app's UI, so analytics that start in Blazor never see them.

## Installation

```bash
dotnet add package Kebechet.Maui.WebView.VersionGate
```

## Usage

```csharp
builder.UseWebViewVersionGate(options =>
{
    options.MinimumSupportedMajorVersion = 105;
    options.MinimumUsableMajorVersion = 101;
    options.Texts = () => new WebViewGateTexts(
        Title: "Update needed",
        Message: "The system WebView component is outdated, so the app can't display correctly. Please update it in the store, then return to the app.",
        UpdateButton: "Update",
        CancelButton: "Cancel");
    options.OnGateShown = hit => logger.LogWarning("WebView {Version} from {Package} hit the gate", hit.ChromiumMajorVersion, hit.ProviderPackageName);
});
```

`Texts` is read each time the dialog is built, so it follows the app's current language.

### Choosing the versions

Measure them for your app: they depend on the JavaScript and CSS features it uses. Official Android emulator images bundle old WebViews you can test on, for example 74 (API 29 `google_apis`), 91 (API 31 `google_apis`), 95 (API 32 `default`), 101 (API 33 `default`) and 109 (API 33 `google_apis_playstore`). Features that commonly set the floor for a Blazor app:

| Chromium | Feature | Without it |
|---|---|---|
| 94 | class static blocks | `blazor.webview.js` does not parse, Blazor never starts |
| 99 | CSS `@layer` | a Tailwind v4 stylesheet is dropped whole |
| 105 | CSS `:has()` | rules that use it are dropped |
| 111 | `color-mix()`, `oklch()` | Tailwind v4's target; it ships fallbacks for older browsers |
| 112 | CSS nesting | declarations after a nested block are dropped; build Tailwind with `optimize` to flatten it |

## License

[MIT](LICENSE)
