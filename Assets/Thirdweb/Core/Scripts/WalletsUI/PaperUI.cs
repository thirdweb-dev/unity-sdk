using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paper;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

namespace Thirdweb.Wallets
{
    public class PaperUI : MonoBehaviour
    {
        public GameObject PaperCanvas;
        public TMP_InputField OTPInput;
        public TMP_InputField RecoveryInput;
        public Button SubmitButton;

        public static PaperUI Instance { get; private set; }

        private PaperEmbeddedWalletSdk _paper;
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

        public async Task<User> Connect(PaperEmbeddedWalletSdk paper, string email)
        {
            _paper = paper;
            _email = email;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            RecoveryInput.text = "";
            RecoveryInput.interactable = false;
            SubmitButton.onClick.RemoveAllListeners();
            SubmitButton.onClick.AddListener(OnSubmitOTP);

            await OnSendOTP();

            PaperCanvas.SetActive(true);

            await new WaitUntil(() => _user != null || _exception != null);

            PaperCanvas.SetActive(false);

            if (_exception != null)
                throw _exception;

            return _user;
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        public async Task OnSendOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice) = await _paper.SendPaperEmailLoginOtp(_email);
                RecoveryInput.interactable = !isNewUser && isNewDevice;
                Debug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (System.Exception e)
            {
                _exception = e;
            }
        }

        public async void OnSubmitOTP()
        {
            try
            {
                string recoveryCode = string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text;
                string otp = OTPInput.text;
                _user = await _paper.VerifyPaperEmailLoginOtp(_email, otp, recoveryCode);
                Debug.Log($"finished validating OTP:  EmailAddress {_user.EmailAddress}, Address {_user.Account.Address}");
            }
            catch (System.Exception e)
            {
                _exception = e;
            }
        }
    }
}
