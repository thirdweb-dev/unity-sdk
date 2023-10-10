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
using System.Runtime.InteropServices;

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
        private string _redirectUrl;
        private string _customScheme;

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

            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        public async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, bool useGoogle)
        {
            var config = Resources.Load<ThirdwebConfig>("ThirdwebConfig");
            _customScheme = config != null ? config.customScheme : null;
            if (!string.IsNullOrEmpty(_customScheme))
                _customScheme = _customScheme.EndsWith("://") ? _customScheme : $"{_customScheme}://";
            _embeddedWallet = embeddedWallet;
            _email = email;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            SubmitButton.onClick.RemoveAllListeners();

            if (useGoogle)
            {
                EmbeddedWalletCanvas.SetActive(false);

                return await LoginWithGoogle();
            }
            else
            {
                return await LoginWithOTP();
            }
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

        private async Task<User> LoginWithGoogle()
        {
            if (Application.isMobilePlatform && string.IsNullOrEmpty(_customScheme))
                throw new UnityException("No custom scheme provided for mobile deeplinks, please set one in your ThirdwebConfig (found in ThirdwebManager)");

            try
            {
                string loginUrl = await GetLoginLink();
                OpenURL(loginUrl);
            }
            catch (System.Exception e)
            {
                _exception = e;
            }

            await new WaitUntil(() => _redirectUrl != null || _exception != null);
            if (_exception != null)
                throw _exception;

            string decodedUrl = HttpUtility.UrlDecode(_redirectUrl);
            Uri uri = new(decodedUrl);
            string queryString = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(queryString);
            string authResultJson = queryDict["authResult"];
            var user = await _embeddedWallet.SignInWithGoogleAsync(authResultJson);
            ThirdwebDebug.Log($"User Email: {user.EmailAddress}, User Address: {user.Account.Address}");
            return user;
        }

#if UNITY_IOS
        // [DllImport("__Internal")]
        // private static extern void _OpenURL(string url);

        public void OpenURL(string url)
        {
            // _OpenURL(url);
            // // TODO: Implement callback
            throw new UnityException("Embedded Wallets are not supported on iOS yet!");
        }
#elif UNITY_ANDROID
        public void OpenURL(string url)
        {
            AndroidJavaClass thirdwebActivityClass = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject thirdwebActivity = thirdwebActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
            thirdwebActivity.Call("OpenCustomTab", url);
        }
#else
        public async void OpenURL(string url)
        {
            var standaloneBrowser = new StandaloneBrowser();
            var res = await standaloneBrowser.StartAsync(url, "http://localhost:3000/");
            _redirectUrl = res.redirectUrl;
        }
#endif

        public void OnDeepLinkActivated(string url)
        {
            ThirdwebDebug.Log($"Received Link {url}");
            _redirectUrl = url;
        }

        private async Task<string> GetLoginLink()
        {
            string platform = UnityWebRequest.EscapeURL("unity");
            string authProvider = UnityWebRequest.EscapeURL("google");
            string baseUrl = UnityWebRequest.EscapeURL("https://ews.thirdweb.com");
            string url = $"https://ews.thirdweb.com/api/2022-08-12/embedded-wallet/headless-login-link?platform={platform}&authProvider={authProvider}&baseUrl={baseUrl}";

            using UnityWebRequest req = UnityWebRequest.Get(url);
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                throw new UnityException("Failed to get login link");

            string loginUrl = JsonConvert.DeserializeObject<JObject>(req.downloadHandler.text)["platformLoginLink"].ToString();
            Debug.Log($"Login URL: {loginUrl}");
            string redirectUrl = UnityWebRequest.EscapeURL(Application.isMobilePlatform ? _customScheme : "http://localhost:3000/");
            string developerClientId = UnityWebRequest.EscapeURL(ThirdwebManager.Instance.SDK.session.Options.clientId);
            return $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={developerClientId}";
        }

        private void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }
    }
}
