using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class EcosystemWalletManager : MonoBehaviour
    {
        public TMP_InputField EcosystemWalletIdInputField;
        public TMP_InputField EcosystemWalletPartnerIdInputField;
        public TMP_InputField ClientIdInputField;
        public TMP_InputField BundleIdInputField;
        public TMP_InputField EmailInputField;
        public TMP_InputField PhoneInputField;

        public TMP_Text LogText;

        private ThirdwebClient _client;
        private IThirdwebWallet _ecosystemWallet;

        private void Awake()
        {
            EcosystemWalletIdInputField.text = "ecosystem.bonfire-development";
            EcosystemWalletPartnerIdInputField.text = "02077ab1-59be-4b62-a768-6c6609da7864";
            ClientIdInputField.text = "3cbee06cf6521a47257c1a3d1ff860ad";
            BundleIdInputField.text = "";
        }

        public async void InitializeEcosystemWallet()
        {
            var clientId = ClientIdInputField.text;
            var bundleId = BundleIdInputField.text;

            if (string.IsNullOrEmpty(clientId))
            {
                Log("Please enter Client Id");
                return;
            }

            bundleId = string.IsNullOrEmpty(bundleId) ? null : bundleId;

            var email = string.IsNullOrEmpty(EmailInputField.text) ? null : EmailInputField.text;
            var phone = string.IsNullOrEmpty(PhoneInputField.text) ? null : PhoneInputField.text;

            if (email == null && phone == null)
            {
                Log("Please enter Email or Phone");
                return;
            }

            var ecosystemWalletId = EcosystemWalletIdInputField.text;
            var ecosystemWalletPartnerId = EcosystemWalletPartnerIdInputField.text;

            if (string.IsNullOrEmpty(ecosystemWalletId))
            {
                Log("Please enter Ecosystem Wallet Id");
                return;
            }

            ecosystemWalletPartnerId = string.IsNullOrEmpty(ecosystemWalletPartnerId) ? null : ecosystemWalletPartnerId;

            _client = ThirdwebClient.Create(
                clientId: clientId,
                bundleId: bundleId,
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer ? new Helpers.UnityThirdwebHttpClient() : new ThirdwebHttpClient(),
                headers: new Dictionary<string, string>
                {
                    { "x-sdk-name", Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK" },
                    { "x-sdk-os", Application.platform.ToString() },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-version", ThirdwebManager.THIRDWEB_UNITY_SDK_VERSION },
                    { "x-client-id", clientId },
                    { "x-bundle-id", clientId }
                }
            );

            _ecosystemWallet = await EcosystemWallet.Create(
                client: _client,
                ecosystemId: ecosystemWalletId,
                ecosystemPartnerId: ecosystemWalletPartnerId,
                email: email,
                phoneNumber: phone,
                storageDirectoryPath: Path.Combine(Application.persistentDataPath, "Thirdweb", "EcosystemWallet")
            );

            Log($"Ecosystem Wallet Initialized.");
        }

        public async void Login()
        {
            if (_ecosystemWallet == null)
            {
                Log("Please initialize Ecosystem Wallet first.");
                return;
            }

            string address;
            if (!await _ecosystemWallet.IsConnected() && _ecosystemWallet is EcosystemWallet)
            {
                _ = await (_ecosystemWallet as EcosystemWallet).SendOTP();
                _ecosystemWallet = await EcosystemWalletModal.LoginWithOtp(_ecosystemWallet as EcosystemWallet);
            }

            if (_ecosystemWallet is not SmartWallet)
            {
                _ecosystemWallet = await SmartWallet.Create(_ecosystemWallet, 421614, gasless: true);
            }

            address = await _ecosystemWallet.GetAddress();
            Log($"Connected with address: {address}");
        }

        public async void PersonalSign()
        {
            if (_ecosystemWallet == null)
            {
                Log("Please initialize Ecosystem Wallet first.");
                return;
            }

            var message = "Hello World!";
            var signature = await _ecosystemWallet.PersonalSign(message);
            Log($"Personal Sign: {signature}");
        }

        public async void SignTransaction()
        {
            if (_ecosystemWallet == null)
            {
                Log("Please initialize Ecosystem Wallet first.");
                return;
            }

            var address = await _ecosystemWallet.GetAddress();
            var contract = await ThirdwebContract.Create(_client, "0xEBB8a39D865465F289fa349A67B3391d8f910da9", 421614);
            var receipt = await contract.DropERC20_Claim(_ecosystemWallet, address, "1");
            Log($"Hash: {receipt.TransactionHash}");
        }

        private void Log(string message)
        {
            LogText.text = message;
            Debug.Log(message);
        }
    }
}
