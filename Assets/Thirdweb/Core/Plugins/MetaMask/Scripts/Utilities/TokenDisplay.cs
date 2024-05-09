using MetaMask.Unity;
using MetaMask.Unity.Contracts;
using MetaMask.Unity.Utils;
using TMPro;
using UnityEngine;

public class TokenDisplay : BindableMonoBehavior
{
    private IMetaMaskSDK _metaMask => MetaMaskSDK.SDKInstance;
    
    private TextMeshProUGUI _balanceText;

    public ScriptableERC20 contract;
    
    // Start is called before the first frame update
    void Start()
    {
        _balanceText = GetComponent<TextMeshProUGUI>();

        if (_metaMask.Wallet.IsConnected)
        {
            DisplayBalance();
        }

        _metaMask.Wallet.Events.AccountChanged += (_, __) => DisplayBalance();
    }

    private async void DisplayBalance()
    {
        var address = _metaMask.Wallet.SelectedAddress;

        var tokenSymbol = await contract.Symbol();
        var tokenBalance = await contract.BalanceOf(address);

        _balanceText.text = $"{tokenSymbol}: {tokenBalance}";
    }
}
