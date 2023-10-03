using UnityEngine;
using Paper;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using Thirdweb.Redcode.Awaiting;
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
        private System.Exception _exception;
        private WebViewObject _webViewObject;
        private bool _webViewLoaded;

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
            if (useGoogle)
            {
                EmbeddedWalletCanvas.SetActive(false);

                _webViewLoaded = false;

                string state = System.Guid.NewGuid().ToString();
                string authorizationUrl = GoogleAuthenticator.GetAuthorizationUrlAsync(state);

                _webViewObject ??= new GameObject("WebViewObject").AddComponent<WebViewObject>();
                _webViewObject.Init(
                    cb: WebViewCallback,
                    ld: (msg) =>
                    {
                        ThirdwebDebug.Log($"WebView Loaded [{msg}]");
                        _webViewLoaded = true;
                    },
                    httpErr: (msg) =>
                    {
                        ThirdwebDebug.LogError($"WebView HTTP Error [{msg}]");
                    },
                    err: (msg) =>
                    {
                        ThirdwebDebug.LogError($"WebView Error [{msg}]");
                    }
                );

                await new WaitUntil(() => _webViewLoaded);

                _webViewObject.LoadURL(authorizationUrl);
                _webViewObject.SetVisibility(true);
                throw new UnityException("Google authentication not yet implemented");
            }
            else
            {
                _embeddedWallet = embeddedWallet;
                _email = email;
                _user = null;
                _exception = null;
                OTPInput.text = "";
                SubmitButton.onClick.RemoveAllListeners();
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
