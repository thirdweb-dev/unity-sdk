using UnityEngine;
using Paper;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using Thirdweb.Redcode.Awaiting;
using Cdm.Authentication.Browser;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;

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
        private System.Exception _exception;

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

                string loginUrl =
                    $"https://ews.thirdweb.com/sdk/2022-08-12/embedded-wallet/auth/headless-google-login-managed-unity?developerClientId={ThirdwebManager.Instance.SDK.session.Options.clientId}";
                var crossPlatformBrowser = new CrossPlatformBrowser();
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.LinuxEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.LinuxPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.IPhonePlayer, new ASWebAuthenticationSessionBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.Android, new DeepLinkBrowser());

                var res = await crossPlatformBrowser.StartAsync(loginUrl, "http://localhost:3000/");
                string decodedUrl = HttpUtility.UrlDecode(res.redirectUrl);
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
                SubmitButton.onClick.AddListener(OnSubmitOTP);
                await OnSendOTP();
                EmbeddedWalletCanvas.SetActive(true);
                await new WaitUntil(() => _user != null || _exception != null);
                EmbeddedWalletCanvas.SetActive(false);
                if (_exception != null)
                    throw _exception;
                return _user;
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

        private void WebViewCallback(string msg)
        {
            ThirdwebDebug.Log($"WebViewCallback: {msg}");
        }
    }
}
