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
using UnityEngine.Events;
using System.Threading;

namespace Thirdweb.Wallets
{
    public class InAppWalletUI : MonoBehaviour
    {
        #region Variables

        public GameObject InAppWalletCanvas;
        public TMP_InputField OTPInput;
        public TMP_InputField RecoveryInput;
        public Button SubmitButton;
        public GameObject RecoveryCodesCanvas;
        public TMP_Text RecoveryCodesText;
        public Button RecoveryCodesCopy;

        [Tooltip("Invoked when the user submits an invalid OTP and can retry.")]
        public UnityEvent OnOTPVerificationFailed;

        protected EmbeddedWallet _embeddedWallet;
        protected string _email;
        protected string _phone;
        protected User _user;
        protected Exception _exception;
        protected string _callbackUrl;
        protected string _customScheme;
        protected CancellationTokenSource _cancellationTokenSource;
        protected string _clientId;

        #endregion

        #region Initialization

        public static InAppWalletUI Instance { get; private set; }

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

        public virtual async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, string phoneNumber, AuthOptions authOptions, string clientId)
        {
            var config = Resources.Load<ThirdwebConfig>("ThirdwebConfig");
            _customScheme = config != null ? config.customScheme : null;
            if (!string.IsNullOrEmpty(_customScheme))
                _customScheme = _customScheme.EndsWith("://") ? _customScheme : $"{_customScheme}://";
            _embeddedWallet = embeddedWallet;
            _email = email;
            _phone = phoneNumber;
            _clientId = clientId;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            RecoveryInput.text = "";
            RecoveryInput.gameObject.SetActive(false);
            SubmitButton.onClick.RemoveAllListeners();
            RecoveryCodesCopy.onClick.RemoveAllListeners();
            InAppWalletCanvas.SetActive(false);
            RecoveryCodesCanvas.SetActive(false);

            try
            {
                string authProvider = authOptions.authProvider switch
                {
                    AuthProvider.EmailOTP => "EmailOTP",
                    AuthProvider.Google => "Google",
                    AuthProvider.Apple => "Apple",
                    AuthProvider.Facebook => "Facebook",
                    AuthProvider.JWT => "CustomAuth",
                    AuthProvider.PhoneOTP => "PhoneOTP",
                    _ => throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}"),
                };
                return await _embeddedWallet.GetUserAsync(_email, authProvider);
            }
            catch (Exception e)
            {
                ThirdwebDebug.Log($"Could not recreate user automatically, proceeding with auth: {e.Message}");
            }

            try
            {
                switch (authOptions.authProvider)
                {
                    case AuthProvider.EmailOTP:
                        await LoginWithOTP();
                        break;
                    case AuthProvider.Google:
                        await LoginWithOauth("Google");
                        break;
                    case AuthProvider.Apple:
                        await LoginWithOauth("Apple");
                        break;
                    case AuthProvider.Facebook:
                        await LoginWithOauth("Facebook");
                        break;
                    case AuthProvider.JWT:
                        await LoginWithJWT(authOptions.jwtOrPayload, authOptions.encryptionKey);
                        break;
                    case AuthProvider.AuthEndpoint:
                        await LoginWithAuthEndpoint(authOptions.jwtOrPayload, authOptions.encryptionKey);
                        break;
                    case AuthProvider.PhoneOTP:
                        await LoginWithPhoneNumber();
                        break;
                    default:
                        throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}");
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }

            await new WaitUntil(() => _user != null || _exception != null);
            InAppWalletCanvas.SetActive(false);
            if (_exception != null)
                throw _exception;
            return _user;
        }

        [ContextMenu("Cancel")]
        public virtual void Cancel()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _exception = new UnityException("User cancelled");
        }

        #endregion

        #region Email OTP Flow

        public virtual async Task LoginWithOTP()
        {
            if (_email == null)
                throw new UnityException("Email is required for OTP login");

            SubmitButton.onClick.AddListener(OnSubmitOTP);
            await OnSendOTP();
            InAppWalletCanvas.SetActive(true);
        }

        public virtual async Task OnSendOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice, bool needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                if (needsRecoveryCode && !isNewUser && isNewDevice)
                    DisplayRecoveryInput(false);
                ThirdwebDebug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public virtual async void OnSubmitOTP()
        {
            OTPInput.interactable = false;
            RecoveryInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
                if (res.User == null)
                {
                    if (res.CanRetry && OnOTPVerificationFailed.GetPersistentEventCount() > 0)
                    {
                        OnOTPVerificationFailed.Invoke();
                        return;
                    }
                    _exception = new UnityException("User OTP Verification Failed.");
                    return;
                }
                _user = res.User;
                ShowRecoveryCodes(res);
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

        #endregion

        #region Phone Number Flow

        public virtual async Task LoginWithPhoneNumber()
        {
            if (_phone == null)
                throw new UnityException("Phone number is required!");

            SubmitButton.onClick.AddListener(OnSubmitPhoneOTP);
            await OnSendPhoneOTP();
            InAppWalletCanvas.SetActive(true);
        }

        public virtual async Task OnSendPhoneOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice, bool needsRecoveryCode) = await _embeddedWallet.SendOtpPhoneAsync(_phone);
                if (needsRecoveryCode && !isNewUser && isNewDevice)
                    DisplayRecoveryInput(false);
                ThirdwebDebug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public virtual async void OnSubmitPhoneOTP()
        {
            OTPInput.interactable = false;
            RecoveryInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyPhoneOtpAsync(_phone, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
                if (res.User == null)
                {
                    if (res.CanRetry && OnOTPVerificationFailed.GetPersistentEventCount() > 0)
                    {
                        OnOTPVerificationFailed.Invoke();
                        return;
                    }
                    _exception = new UnityException("User OTP Verification Failed.");
                    return;
                }
                _user = res.User;
                ShowRecoveryCodes(res);
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

        #endregion

        #region OAuth2 Flow

        public virtual async Task LoginWithOauth(string authProviderStr)
        {
            if (Application.isMobilePlatform && string.IsNullOrEmpty(_customScheme))
                throw new UnityException("No custom scheme provided for mobile deeplinks, please set one in your ThirdwebConfig (found in ThirdwebManager)");

            string loginUrl = await GetLoginLink(authProviderStr);

            string redirectUrl = Application.isMobilePlatform ? _customScheme : "http://localhost:8789/";
            CrossPlatformBrowser browser = new();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var browserResult = await browser.Login(loginUrl, redirectUrl, _cancellationTokenSource.Token);

            if (browserResult.status == BrowserStatus.UserCanceled)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                ThirdwebDebug.LogWarning("User cancelled login");
                return;
            }
            else if (browserResult.status != BrowserStatus.Success)
            {
                _exception = new UnityException($"Failed to login with {authProviderStr}: {browserResult.status} | {browserResult.error}");
                return;
            }
            else
            {
                _callbackUrl = browserResult.callbackUrl;
            }

            await new WaitUntil(() => _callbackUrl != null);

            string decodedUrl = HttpUtility.UrlDecode(_callbackUrl);
            Uri uri = new(decodedUrl);
            string queryString = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(queryString);
            string authResultJson = queryDict["authResult"];

            bool needsRecoveryCode = await _embeddedWallet.IsRecoveryCodeNeededAsync(authResultJson);

            if (needsRecoveryCode)
            {
                SubmitButton.onClick.AddListener(() => OnSubmitRecoveryOauth(authProviderStr, authResultJson));
                DisplayRecoveryInput(true);
            }
            else
            {
                try
                {
                    var res = await _embeddedWallet.SignInWithOauthAsync(authProviderStr, authResultJson, null);
                    _user = res.User;
                }
                catch (Exception e)
                {
                    _exception = e;
                }
            }
        }

        public virtual async void OnSubmitRecoveryOauth(string authProviderStr, string authResult)
        {
            try
            {
                string recoveryCode = RecoveryInput.text;
                var res = await _embeddedWallet.SignInWithOauthAsync(authProviderStr, authResult, recoveryCode);
                _user = res.User;
                ShowRecoveryCodes(res);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public virtual async Task<string> GetLoginLink(string authProvider)
        {
            string loginUrl = await _embeddedWallet.FetchHeadlessOauthLoginLinkAsync(authProvider);
            string platform = "unity";
            string redirectUrl = UnityWebRequest.EscapeURL(Application.isMobilePlatform ? _customScheme : "http://localhost:8789/");
            string developerClientId = UnityWebRequest.EscapeURL(_clientId);
            return $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={developerClientId}&authOption={authProvider}";
        }

        #endregion

        #region JWT Flow

        public virtual async Task LoginWithJWT(string jwtToken, string encryptionKey, string recoveryCode = null)
        {
            if (string.IsNullOrEmpty(jwtToken))
                throw new UnityException("JWT token is required for JWT login!");
            if (string.IsNullOrEmpty(encryptionKey))
                throw new UnityException("Encryption key is required for JWT login!");

            var res = await _embeddedWallet.SignInWithJwtAsync(jwtToken, encryptionKey, recoveryCode);
            _user = res.User;
            ShowRecoveryCodes(res);
        }

        #endregion

        #region Auth Endpoint Flow

        public virtual async Task LoginWithAuthEndpoint(string payload, string encryptionKey, string recoveryCode = null)
        {
            if (string.IsNullOrEmpty(payload))
                throw new UnityException("Auth payload is required for Auth Endpoint login!");
            if (string.IsNullOrEmpty(encryptionKey))
                throw new UnityException("Encryption key is required for Auth Endpoint login!");

            var res = await _embeddedWallet.SignInWithAuthEndpointAsync(payload, encryptionKey, recoveryCode);
            _user = res.User;
            ShowRecoveryCodes(res);
        }

        #endregion

        #region Common

        public virtual void DisplayRecoveryInput(bool hideOtpInput)
        {
            if (hideOtpInput)
                OTPInput.gameObject.SetActive(false);
            RecoveryInput.gameObject.SetActive(true);
            InAppWalletCanvas.SetActive(true);
        }

        public virtual void ShowRecoveryCodes(EmbeddedWallet.VerifyResult res)
        {
            if (res.MainRecoveryCode != null && res.WasEmailed.HasValue && res.WasEmailed.Value == false)
            {
                List<string> recoveryCodes = new() { res.MainRecoveryCode };
                if (res.BackupRecoveryCodes != null)
                    recoveryCodes.AddRange(res.BackupRecoveryCodes);
                string recoveryCodesString = string.Join("\n", recoveryCodes.Select((code, i) => $"{i + 1}. {code}"));
                string message = $"Please save the following recovery codes in a safe place:\n\n{recoveryCodesString}";
                ThirdwebDebug.Log(message);
                RecoveryCodesText.text = message;
                string messageToSave = JsonConvert.SerializeObject(recoveryCodes);
                RecoveryCodesCopy.onClick.AddListener(() => GUIUtility.systemCopyBuffer = messageToSave);
                RecoveryCodesCanvas.SetActive(true);
            }
        }

        #endregion
    }
}
