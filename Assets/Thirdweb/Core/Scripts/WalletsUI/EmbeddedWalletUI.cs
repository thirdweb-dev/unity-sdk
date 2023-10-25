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
using System;
using System.Linq;
using System.Collections.Generic;

namespace Thirdweb.Wallets
{
    public class EmbeddedWalletUI : MonoBehaviour
    {
        #region Variables

        public GameObject EmbeddedWalletCanvas;
        public TMP_InputField OTPInput;
        public TMP_InputField RecoveryInput;
        public Button SubmitButton;
        public GameObject RecoveryCodesCanvas;
        public TMP_Text RecoveryCodesText;
        public Button RecoveryCodesCopy;

        private EmbeddedWallet _embeddedWallet;
        private string _email;
        private User _user;
        private Exception _exception;
        private string _callbackUrl;
        private string _customScheme;

        #endregion

        #region Initialization

        public static EmbeddedWalletUI Instance { get; private set; }

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

        #endregion

        #region Connection Flow

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
            RecoveryCodesCanvas.SetActive(false);

            switch (authOptions.authProvider)
            {
                case AuthProvider.EmailOTP:
                    return await LoginWithOTP();
                case AuthProvider.Google:
                    return await LoginWithGoogle();
                case AuthProvider.CustomAuth:
                    return await LoginWithCustomJwt(authOptions.authToken);
                default:
                    throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}");
            }
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        #endregion

        #region Email OTP Flow

        private async Task<User> LoginWithOTP()
        {
            if (_email == null)
                throw new UnityException("Email is required for OTP login");

            try
            {
                _user = await _embeddedWallet.GetUserAsync(_email);
            }
            catch (Exception e)
            {
                ThirdwebDebug.Log($"Could not recreate user automatically, proceeding with auth: {e}");
            }

            if (_user != null)
            {
                ThirdwebDebug.Log($"Logged In Existing User - Email: {_user.EmailAddress}, User Address: {_user.Account.Address}");
                return _user;
            }

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
                (bool isNewUser, bool isNewDevice, bool needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                RecoveryInput.gameObject.SetActive(needsRecoveryCode && !isNewUser && isNewDevice);
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
            RecoveryInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
                _user = res.User;
                if (res.MainRecoveryCode != null)
                {
                    List<string> recoveryCodes = new() { res.MainRecoveryCode };
                    if (res.BackupRecoveryCodes != null)
                        recoveryCodes.AddRange(res.BackupRecoveryCodes);
                    ShowRecoveryCodes(recoveryCodes);
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                OTPInput.interactable = true;
                RecoveryInput.interactable = true;
                SubmitButton.interactable = true;
            }
        }

        private void ShowRecoveryCodes(List<string> recoveryCodes)
        {
            string recoveryCodesString = string.Join("\n", recoveryCodes.Select((code, i) => $"{i + 1}. {code}"));
            string message = $"Please save the following recovery codes in a safe place:\n\n{recoveryCodesString}";
            ThirdwebDebug.Log(message);
            RecoveryCodesText.text = message;
            string messageToSave = JsonConvert.SerializeObject(recoveryCodes);
            RecoveryCodesCopy.onClick.RemoveAllListeners();
            RecoveryCodesCopy.onClick.AddListener(() => GUIUtility.systemCopyBuffer = messageToSave);
            RecoveryCodesCanvas.SetActive(true);
        }

        #endregion

        #region OAuth2 Flow

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
            return user;
        }

        private async Task<string> GetLoginLink(string authProvider = "Google")
        {
            string loginUrl = await _embeddedWallet.FetchHeadlessOauthLoginLinkAsync(authProvider);
            string platform = "unity";
            string redirectUrl = UnityWebRequest.EscapeURL(Application.isMobilePlatform ? _customScheme : "http://localhost:8789/");
            string developerClientId = UnityWebRequest.EscapeURL(ThirdwebManager.Instance.SDK.session.Options.clientId);
            return $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={developerClientId}";
        }

        #endregion

        #region Custom JWT Flow

        private async Task<User> LoginWithCustomJwt(string jwtToken)
        {
            return await _embeddedWallet.SignInWithJwtAuthAsync(jwtToken);
        }

        #endregion
    }
}
