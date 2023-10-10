# Authentication Package for Unity
- OAuth2
  - Authorization code flow (https://www.rfc-editor.org/rfc/rfc6749#section-4.1)
  - Authorization code flow with PKCE (Proof Key for Code Exchange) (https://www.rfc-editor.org/rfc/rfc7636)
- Built-in clients
  - GitHub
  - Google
  - Facebook
- Built-in browser support
  - Deep link browser (iOS, Android, Universal Windows Platform and macOS) (https://docs.unity3d.com/Manual/deep-linking.html)
  - Standalone as local server (Windows, macOS and Linux)
  - ASWebAuthenticationSession (iOS) (https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession)
  - WKWebView (iOS) (https://developer.apple.com/documentation/webkit/wkwebview)

## Install via Unity Package Manager:
* Add `"com.cdm.authentication": "https://github.com/cdmvision/authentication-unity.git#1.2.0"` to your project's package manifest file in dependencies section.
* Or, `Package Manager > Add package from git URL...` and paste this URL: `https://github.com/cdmvision/authentication-unity.git#1.2.0`

## Example usage

You should create your client auth configuration:
```csharp
// Also you can use your own client configuration.
var auth = new GoogleAuth()
{
  clientId = "...",
  redirectUrl = "...",
  scope = "openid email profile"
};
```

Authentication session is created with auth configuration and a browser:
```csharp
using var authenticationSession = new AuthenticationSession(auth, new StandaloneBrowser());
```

Also you can use different browsers for each platform by using cross platform browser:
```csharp
var crossPlatformBrowser = new CrossPlatformBrowser();
var crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsEditor, new StandaloneBrowser());
var crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsPlayer, new StandaloneBrowser());
var crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXEditor, new StandaloneBrowser());
var crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXPlayer, new StandaloneBrowser());
var crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.IPhonePlayer, new ASWebAuthenticationSessionBrowser());

using var authenticationSession = new AuthenticationSession(auth, crossPlatformBrowser);

// Opens a browser to log user in
AccessTokenResponse accessTokenResponse = await authenticationSession.AuthenticateAsync();

// Authentication header can be used to make authorized http calls.
AuthenticationHeaderValue authenticationHeader = accessTokenResponse.GetAuthenticationHeader();

// Gets the current acccess token, or refreshes if it is expired.
accessTokenResponse = await authenticationSession.GetOrRefreshTokenAsync();

// Gets new access token by using the refresh token.
AccessTokenResponse newAccessTokenResponse = await authenticationSession.RefreshTokenAsync();

// Or you can get new access token with specified refresh token (i.e. stored on the local disk to prevent multiple sign-in for each app launch)
newAccessTokenResponse = await authenticationSession.RefreshTokenAsync("my_refresh_token");
```
