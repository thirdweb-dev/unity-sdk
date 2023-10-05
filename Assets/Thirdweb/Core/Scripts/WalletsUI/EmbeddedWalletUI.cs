using UnityEngine;
using Paper;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using Thirdweb.Redcode.Awaiting;
using Cdm.Authentication.Browser;
using System.Web;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;
using Thirdweb.WebView;

namespace Thirdweb.Wallets
{
    public class EmbeddedWalletUI : MonoBehaviour
    {
        public GameObject EmbeddedWalletCanvas;
        public TMP_InputField OTPInput;
        public Button SubmitButton;

        public static EmbeddedWalletUI Instance { get; private set; }

        private EmbeddedWallet _embeddedWallet;
        private string _email;
        private User _user;
        private Exception _exception;
        private WebViewObject _webView;
        private string _redirectUrl;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }

        public async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, bool useGoogle)
        {
            _embeddedWallet = embeddedWallet;
            _email = email;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            SubmitButton.onClick.RemoveAllListeners();

            if (useGoogle)
            {
                EmbeddedWalletCanvas.SetActive(false);

                if (Application.isMobilePlatform)
                {
                    await LoginWithWebView();
                }
                else
                {
                    await LoginWithBrowser();
                }

                string decodedUrl = HttpUtility.UrlDecode(_redirectUrl);
                Uri uri = new(decodedUrl);
                string queryString = uri.Query;
                var queryDict = HttpUtility.ParseQueryString(queryString);
                string authResultJson = queryDict["authResult"];
                var user = await _embeddedWallet.SignInWithGoogleAsync(authResultJson);
                ThirdwebDebug.Log($"User Email: {user.EmailAddress}, User Address: {user.Account.Address}");
                return user;
            }
            else
            {
                return await LoginWithOTP();
            }
        }

        public void OnRedirectDetected(string url)
        {
            _redirectUrl = url;
            _webView.SetVisibility(false);
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        public async Task OnSendOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice) = await _embeddedWallet.SendOtpEmailAsync(_email);
                ThirdwebDebug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (System.Exception e)
            {
                _exception = e;
            }
        }

        public async void OnSubmitOTP()
        {
            OTPInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, null);
                _user = res.User;
                ThirdwebDebug.Log($"finished validating OTP:  EmailAddress {_user.EmailAddress}, Address {_user.Account.Address}");
            }
            catch (System.Exception e)
            {
                _exception = e;
            }
            finally
            {
                OTPInput.interactable = true;
                SubmitButton.interactable = true;
            }
        }

        private void WebViewCallback(string msg)
        {
            ThirdwebDebug.Log($"WebViewCallback: {msg}");
        }

        private async Task LoginWithBrowser()
        {
            string loginUrl =
                $"https://ews.thirdweb.com/sdk/2022-08-12/embedded-wallet/auth/headless-google-login-managed-unity?developerClientId={ThirdwebManager.Instance.SDK.session.Options.clientId}";
            var standaloneBrowser = new StandaloneBrowser();
            var res = await standaloneBrowser.StartAsync(loginUrl, "http://localhost:3000/");
            _redirectUrl = res.redirectUrl;
        }

        private async Task<User> LoginWithOTP()
        {
            SubmitButton.onClick.AddListener(OnSubmitOTP);
            await OnSendOTP();
            EmbeddedWalletCanvas.SetActive(true);
            await new WaitUntil(() => _user != null || _exception != null);
            EmbeddedWalletCanvas.SetActive(false);
            if (_exception != null)
                throw _exception;
            return _user;
        }

        private async Task LoginWithWebView()
        {
            string platform = UnityWebRequest.EscapeURL("mobile");
            string authProvider = UnityWebRequest.EscapeURL("google");
            string baseUrl = UnityWebRequest.EscapeURL("https://ews.thirdweb.com");

            string loginUrl;
            string getUrl = $"https://ews.thirdweb.com/api/2022-08-12/embedded-wallet/headless-login-link?platform={platform}&authProvider={authProvider}&baseUrl={baseUrl}";

            using (UnityWebRequest req = UnityWebRequest.Get(getUrl))
            {
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                    throw new UnityException("Failed to get login link!");

                loginUrl = JsonConvert.DeserializeObject<JObject>(req.downloadHandler.text)["googleLoginLink"].ToString();
            }

            if (!_webView)
            {
                _webView = gameObject.AddComponent<WebViewObject>();
            }

            _webView.Init(
                cb: (msg) =>
                {
                    // Handle messages sent from the WebView
                    ThirdwebDebug.Log(string.Format("CallFromJS[{0}]", msg));
                },
                ua: "Mozilla/5.0 (Linux; Android 11; Pixel 4a) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Mobile Safari/537.36"
            );

            _webView.EvaluateJS(
                @"
                    window.addEventListener('userLoginSuccess', function(event) {
                        unityObject.SendMessage('EmbeddedWalletUI', 'OnLoginSuccess', event.detail);
                    });

                    window.addEventListener('userLoginFailed', function(event) {
                        unityObject.SendMessage('EmbeddedWalletUI', 'OnLoginFailed', event.detail);
                    });

                    window.addEventListener('message', function(event) {
                        if (event.data && event.data.eventType === 'injectDeveloperClientId') {
                            event.source.postMessage({
                                eventType: 'injectDeveloperClientIdResult',
                                developerClientId: '"
                    + ThirdwebManager.Instance.SDK.session.Options.clientId
                    + @"',
                                redirectUrl: 'myapp://'
                            }, event.origin);
                        }
                    });
                "
            );

            _webView.LoadURL(loginUrl);
            _webView.SetVisibility(true);
        }

        // WebView Callback, handle successful authentication
        public void OnLoginSuccess(string authResult)
        {
            _webView.SetVisibility(false);
            ThirdwebDebug.Log($"Authentication Success: {authResult}");
        }

        // WebView Callback, handle failed authentication
        public void OnLoginFailed(string error)
        {
            _webView.SetVisibility(false);
            ThirdwebDebug.LogError($"Authentication Failed: {error}");
        }
    }
}
