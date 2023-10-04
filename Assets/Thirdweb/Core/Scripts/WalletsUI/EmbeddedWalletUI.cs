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

        [DataContract]
        private class UserAuthDetails
        {
            [DataMember(Name = "email")]
            internal string Email { get; set; }

            [DataMember(Name = "userWalletId")]
            internal string WalletUserId { get; set; }
        }

        [DataContract]
        private class AuthVerifiedTokenReturnType
        {
            [DataMember(Name = "verifiedToken")]
            internal VerifiedTokenType VerifiedToken { get; set; }

            [DataMember(Name = "verifiedTokenJwtString")]
            internal string VerifiedTokenJwtString { get; set; }

            [DataContract]
            internal class VerifiedTokenType
            {
                [DataMember(Name = "authDetails")]
                internal UserAuthDetails AuthDetails { get; set; }

                [DataMember]
                private string authProvider;

                [DataMember]
                private string developerClientId;

                [DataMember(Name = "isNewUser")]
                internal bool IsNewUser { get; set; }

                [DataMember]
                private string rawToken;

                [DataMember]
                private string userId;
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

                // START TEST
                var testVerifiedToken = new AuthVerifiedTokenReturnType
                {
                    VerifiedToken = new AuthVerifiedTokenReturnType.VerifiedTokenType
                    {
                        AuthDetails = new UserAuthDetails { Email = "0xfirekeeper+absolutetest@gmail.com", WalletUserId = null },
                        IsNewUser = true
                    }
                };
                var testVerifiedTokenJson = JsonConvert.SerializeObject(testVerifiedToken);
                var testVerifiedTokenBase64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(testVerifiedTokenJson));
                // END TEST

                var loginUrl = $"http://localhost/?authVerifiedToken={testVerifiedTokenBase64}"; // TODO: replace with real login URL from server
                var crossPlatformBrowser = new CrossPlatformBrowser();
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.LinuxEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.LinuxPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.WindowsPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXEditor, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.OSXPlayer, new StandaloneBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.IPhonePlayer, new ASWebAuthenticationSessionBrowser());
                crossPlatformBrowser.platformBrowsers.Add(RuntimePlatform.Android, new DeepLinkBrowser());

                var res = await crossPlatformBrowser.StartAsync(loginUrl, "http://localhost/");
                var qparams = HttpUtility.ParseQueryString(res.redirectUrl[res.redirectUrl.IndexOf('?')..]);
                var authVerifiedToken = qparams["authVerifiedToken"];
                authVerifiedToken = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(authVerifiedToken));
                // Debug.Log($"authVerifiedToken: {authVerifiedToken}");
                var user = await _embeddedWallet.SignInWithGoogleAsync(authVerifiedToken);
                // Debug.Log($"user: {user.EmailAddress}, {user.Account.Address}");
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
