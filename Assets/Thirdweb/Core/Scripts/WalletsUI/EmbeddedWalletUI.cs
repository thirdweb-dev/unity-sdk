using UnityEngine;
using Thirdweb.EWS;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using Thirdweb.Redcode.Awaiting;
using Thirdweb.Browser;
using System.Web;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;

namespace Thirdweb.Wallets
{
    public class EmbeddedWalletUI : MonoBehaviour
    {
        public GameObject EmbeddedWalletCanvas;
        public TMP_InputField OTPInput;
        public TMP_InputField RecoveryInput;
        public Button SubmitButton;

        public static EmbeddedWalletUI Instance { get; private set; }

        private EmbeddedWallet _embeddedWallet;
        private string _email;
        private User _user;
        private Exception _exception;
        private string _callbackUrl;
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
        }

        public async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, AuthOptions authOptions)
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
            RecoveryInput.text = "";
            RecoveryInput.gameObject.SetActive(false);
            SubmitButton.onClick.RemoveAllListeners();
            EmbeddedWalletCanvas.SetActive(false);

            if (authOptions?.authProvider == AuthProvider.EmailOTP)
            {
                return await LoginWithOTP();
            }
            else if (authOptions?.authProvider == AuthProvider.Google)
            {
                return await LoginWithGoogle();
            }
            else if (authOptions?.authProvider == AuthProvider.CustomJwt)
            {
                return await LoginWithCustomJwt(authOptions.jwtToken, authOptions.encryptionKey);
            }
            else
            {
                throw new UnityException($"Unsupported auth provider: {authOptions?.authProvider}");
            }
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        // Default flow

        private async Task<User> LoginWithOTP()
        {
            if (_email == null)
                throw new UnityException("Email is required for OTP login");

            SubmitButton.onClick.AddListener(OnSubmitOTP);
            await OnSendOTP();
            EmbeddedWalletCanvas.SetActive(true);
            await new WaitUntil(() => _user != null || _exception != null);
            EmbeddedWalletCanvas.SetActive(false);
            if (_exception != null)
                throw _exception;
            return _user;
        }

        private async Task OnSendOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice) = await _embeddedWallet.SendOtpEmailAsync(_email);
                RecoveryInput.gameObject.SetActive(!_embeddedWallet.IsManagedRecovery && !isNewUser && isNewDevice);
                ThirdwebDebug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (System.Exception e)
            {
                _exception = e;
            }
        }

        private async void OnSubmitOTP()
        {
            OTPInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
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

        // Google flow

        private async Task<User> LoginWithGoogle()
        {
            if (Application.isMobilePlatform && string.IsNullOrEmpty(_customScheme))
                throw new UnityException("No custom scheme provided for mobile deeplinks, please set one in your ThirdwebConfig (found in ThirdwebManager)");

            try
            {
                string loginUrl = await GetLoginLink();
                string redirectUrl = Application.isMobilePlatform ? _customScheme : "http://localhost:8789/";
                CrossPlatformBrowser browser = new();
                var browserResult = await browser.Login(loginUrl, redirectUrl);
                if (browserResult.status != BrowserStatus.Success)
                    _exception = new UnityException($"Failed to login with Google: {browserResult.status} | {browserResult.error}");
                else
                    _callbackUrl = browserResult.callbackUrl;
            }
            catch (Exception e)
            {
                _exception = e;
            }

            await new WaitUntil(() => _callbackUrl != null || _exception != null);
            if (_exception != null)
                throw _exception;

            string decodedUrl = HttpUtility.UrlDecode(_callbackUrl);
            Uri uri = new(decodedUrl);
            string queryString = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(queryString);
            string authResultJson = queryDict["authResult"];
            var user = await _embeddedWallet.SignInWithGoogleAsync(authResultJson);
            ThirdwebDebug.Log($"User Email: {user.EmailAddress}, User Address: {user.Account.Address}");
            return user;
        }

        private async Task<string> GetLoginLink()
        {
            string platform = UnityWebRequest.EscapeURL("unity");
            string authProvider = UnityWebRequest.EscapeURL("Google");
            string baseUrl = UnityWebRequest.EscapeURL("https://embedded-wallet.thirdweb.com");
            string url = $"https://embedded-wallet.thirdweb.com/api/2023-10-20/embedded-wallet/headless-oauth-login-link?platform={platform}&authProvider={authProvider}&baseUrl={baseUrl}";

            using UnityWebRequest req = UnityWebRequest.Get(url);
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                throw new UnityException("Failed to get login link");

            string loginUrl = JsonConvert.DeserializeObject<JObject>(req.downloadHandler.text)["platformLoginLink"].ToString();
            Debug.Log($"Login URL: {loginUrl}");
            string redirectUrl = UnityWebRequest.EscapeURL(Application.isMobilePlatform ? _customScheme : "http://localhost:8789/");
            string developerClientId = UnityWebRequest.EscapeURL(ThirdwebManager.Instance.SDK.session.Options.clientId);
            return $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={developerClientId}";
        }

        // Custom auth flow

        private async Task<User> LoginWithCustomJwt(string jwtToken, string encryptionKey)
        {
            return await _embeddedWallet.SignInWithJwtAuthAsync(jwtToken, encryptionKey);
        }
    }
}
