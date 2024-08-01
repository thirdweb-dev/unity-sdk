using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity.Examples
{
    public class WebGLMetaMaskExample : MonoBehaviour
    {
        [field: SerializeField, Header("Chain Settings")]
        private ulong ChainId = 421614;

        [field: SerializeField, Header("UI Settings")]
        private TMP_Text LogText;

        [field: SerializeField]
        private Button SignButton;

        [field: SerializeField]
        private Button GetAddressButton;

        [field: SerializeField]
        private Button GetBalanceButton;

        private bool _connected;

        private void Awake()
        {
            LogText.text = string.Empty;
            SignButton.onClick.AddListener(Sign);
            GetAddressButton.onClick.AddListener(GetAddress);
            GetBalanceButton.onClick.AddListener(GetBalance);
        }

        private async void Start()
        {
            _connected = await TryConnect();
        }

        private async Task<bool> TryConnect()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Log("This example is only for WebGL builds. Use WalletConnect for native platforms.");
                return false;
            }

            if (_connected)
            {
                return true;
            }

            try
            {
                var options = new WalletOptions(provider: WalletProvider.MetaMaskWallet, chainId: ChainId);
                _ = await ThirdwebManager.Instance.ConnectWallet(options);
                Log($"Connected.");
                return true;
            }
            catch (System.Exception e)
            {
                Log($"Error connecting to wallet: {e.Message}");
                return false;
            }
        }

        private async void Sign()
        {
            if (await TryConnect())
            {
                try
                {
                    var wallet = ThirdwebManager.Instance.GetActiveWallet();
                    var message = "Hello, World!";
                    var signature = await wallet.PersonalSign(message);
                    Log($"Signature: {signature}");
                }
                catch (System.Exception e)
                {
                    Log($"Error signing message: {e.Message}");
                }
            }
        }

        private async void GetAddress()
        {
            if (await TryConnect())
            {
                try
                {
                    var wallet = ThirdwebManager.Instance.GetActiveWallet();
                    var address = await wallet.GetAddress();
                    Log($"Address: {address}");
                }
                catch (System.Exception e)
                {
                    Log($"Error getting address: {e.Message}");
                }
            }
        }

        private async void GetBalance()
        {
            if (await TryConnect())
            {
                try
                {
                    var wallet = ThirdwebManager.Instance.GetActiveWallet();
                    var balance = await wallet.GetBalance(chainId: ChainId);
                    var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 2, addCommas: true);
                    var chainData = await Utils.FetchThirdwebChainDataAsync(client: ThirdwebManager.Instance.Client, chainId: ChainId);
                    Log($"Balance: {balanceEth} {chainData.NativeCurrency.Symbol}");
                }
                catch (System.Exception e)
                {
                    Log($"Error getting balance: {e.Message}");
                }
            }
        }

        private void Log(string message)
        {
            LogText.text = message;
            ThirdwebDebug.Log(message);
        }
    }
}
