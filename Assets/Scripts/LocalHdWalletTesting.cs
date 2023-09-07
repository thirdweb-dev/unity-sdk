using NBitcoin;
using Thirdweb;
using UnityEngine;

public class LocalHdWalletTesting : MonoBehaviour
{
    public async void ShouldErrorWhenConnectingWithInvalidMnemonic()
    {
        var walletConnection = new WalletConnection(
            WalletProvider.LocalHdWallet,
            ThirdwebManager.Instance.SDK.session.ChainId,
            mnemonic: "test test test test test test test test test test test junk unit"
        );

        var wallet = await ThirdwebManager.Instance.SDK.wallet.Connect(walletConnection);
        Debug.Log(wallet);
    }

    public async void ShouldConnectWithValidMnemonic()
    {
        var walletConnection = new WalletConnection(
            WalletProvider.LocalHdWallet,
            ThirdwebManager.Instance.SDK.session.ChainId,
            mnemonic: new Mnemonic(Wordlist.English, WordCount.Twelve).ToString()
        );

        var wallet = await ThirdwebManager.Instance.SDK.wallet.Connect(walletConnection);
        Debug.Log(wallet);
    }
}