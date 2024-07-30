using System.Linq;
using System.Numerics;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class ThirdwebIntegrationTest : MonoBehaviour
    {
        private ThirdwebClient _client;

        private IThirdwebWallet _wallet;

        private void Start()
        {
            _client = ThirdwebManager.Instance.Client;

            Test();
        }

        private async void Test()
        {
            if (_client == null)
            {
                ThirdwebDebug.LogError("ThirdwebClient is not initialized.");
                return;
            }

            var inAppWalletOptions = new InAppWalletOptions(email: "firekeeper@thirdweb.com");
            var smartWalletOptions = new SmartWalletOptions(sponsorGas: true);
            var walletOptions = new WalletOptions(provider: WalletProvider.InAppWallet, chainId: 421614, inAppWalletOptions: inAppWalletOptions, smartWalletOptions: smartWalletOptions);
            _wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);

            ThirdwebDebug.Log($"Connected to Wallet: {_wallet.GetType().Name}");
            ThirdwebDebug.Log($"Address: {await _wallet.GetAddress()}");

            var personalWallet = _wallet is SmartWallet ? await (_wallet as SmartWallet).GetPersonalAccount() : _wallet;
            if (personalWallet is InAppWallet)
            {
                var inAppWallet = personalWallet as InAppWallet;
                await inAppWallet.Disconnect();
            }
        }
    }
}
