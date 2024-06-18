using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading;

namespace Thirdweb.Unity
{
    public class InAppWalletModal : MonoBehaviour
    {
        public GameObject InAppWalletCanvas;
        public TMP_InputField OTPInput;
        public Button SubmitButton;

        [Tooltip("Invoked when the user submits an invalid OTP and can retry.")]
        public UnityEvent OnOTPVerificationFailed;

        protected ThirdwebClient _client;
        protected CancellationTokenSource _cancellationTokenSource;

        public static InAppWalletModal Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public async Task<string> Connect(
            InAppWallet wallet,
            AuthProvider authprovider = AuthProvider.Default,
            string jwt = null,
            string encryptionKey = null,
            string payload = null,
            CancellationToken cancellationToken = default
        )
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken linkedToken = _cancellationTokenSource.Token;

            InAppWalletCanvas.SetActive(false);
            SubmitButton.onClick.RemoveAllListeners();
            OTPInput.text = "";

            if (await wallet.IsConnected())
            {
                return await wallet.GetAddress();
            }

            try
            {
                if (authprovider == AuthProvider.Default)
                {
                    InAppWalletCanvas.SetActive(true);
                    await wallet.SendOTP();
                    ThirdwebDebug.Log("Please submit the OTP sent to your email or phone.");
                    SubmitButton.onClick.AddListener(async () => await SubmitOTP(wallet, linkedToken));
                }
                else if (authprovider == AuthProvider.JWT)
                {
                    await wallet.LoginWithJWT(jwt, encryptionKey, null);
                }
                else if (authprovider == AuthProvider.AuthEndpoint)
                {
                    await wallet.LoginWithAuthEndpoint(payload, encryptionKey, null);
                }
                else
                {
                    await wallet.LoginWithOauth(
                        isMobile: Application.isMobilePlatform,
                        browserOpenAction: (url) => Application.OpenURL(url),
                        mobileRedirectScheme: "mythirdwebgame",
                        browser: new CrossPlatformUnityBrowser(),
                        cancellationToken: linkedToken
                    );
                }

                while (!linkedToken.IsCancellationRequested && !await wallet.IsConnected() && Application.isPlaying)
                {
                    await Task.Delay(250, linkedToken); // Use Task.Delay instead of WaitForSecondsRealtime
                }

                if (linkedToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException("Login was cancelled.");
                }

                InAppWalletCanvas.SetActive(false);
                return await wallet.GetAddress();
            }
            catch (TaskCanceledException)
            {
                ThirdwebDebug.Log("Operation was cancelled.");
                return null;
            }
        }

        private async Task<InAppWallet> SubmitOTP(InAppWallet wallet, CancellationToken cancellationToken)
        {
            OTPInput.interactable = false;
            SubmitButton.interactable = false;

            try
            {
                var otp = OTPInput.text;
                (var inAppWalletAddress, var canRetry) = await wallet.SubmitOTP(otp);
                if (inAppWalletAddress == null && canRetry)
                {
                    ThirdwebDebug.Log("Please submit the OTP again.");
                    OTPInput.text = "";
                    OnOTPVerificationFailed.Invoke();
                }
                return wallet;
            }
            catch (TaskCanceledException)
            {
                ThirdwebDebug.Log("OTP submission was cancelled.");
                return null;
            }
            finally
            {
                OTPInput.interactable = true;
                SubmitButton.interactable = true;
            }
        }

        public void Cancel()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            InAppWalletCanvas.SetActive(false);
        }
    }
}
