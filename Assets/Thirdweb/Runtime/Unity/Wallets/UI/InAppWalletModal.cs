using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity
{
    public class InAppWalletModal : MonoBehaviour
    {
        [field: SerializeField, Header("UI Settings")]
        private Canvas InAppWalletCanvas { get; set; }

        [field: SerializeField]
        private TMP_InputField OTPInputField { get; set; }

        [field: SerializeField]
        private Button SubmitButton { get; set; }

        public static Task<InAppWallet> VerifyOTP(InAppWallet wallet)
        {
            var modal = FindObjectOfType<InAppWalletModal>();
            if (modal == null)
            {
                modal = new GameObject("InAppWalletModal").AddComponent<InAppWalletModal>();
            }

            modal.SubmitButton.onClick.RemoveAllListeners();
            modal.OTPInputField.text = string.Empty;
            modal.InAppWalletCanvas.gameObject.SetActive(true);

            modal.OTPInputField.interactable = true;
            modal.SubmitButton.interactable = true;

            var tcs = new TaskCompletionSource<InAppWallet>();

            modal.SubmitButton.onClick.AddListener(async () =>
            {
                var otp = modal.OTPInputField.text;
                if (string.IsNullOrEmpty(otp))
                {
                    return;
                }

                modal.OTPInputField.interactable = false;
                modal.SubmitButton.interactable = false;
                (var address, var canRetry) = await wallet.SubmitOTP(otp);
                if (address != null)
                {
                    modal.InAppWalletCanvas.enabled = false;
                    tcs.SetResult(wallet);
                }
                else if (!canRetry)
                {
                    modal.InAppWalletCanvas.enabled = false;
                    tcs.SetException(new UnityException("Failed to verify OTP."));
                }
                else
                {
                    modal.OTPInputField.text = string.Empty;
                    modal.OTPInputField.interactable = true;
                    modal.SubmitButton.interactable = true;
                }
            });

            modal.InAppWalletCanvas.gameObject.SetActive(true);

            return tcs.Task;
        }
    }
}
