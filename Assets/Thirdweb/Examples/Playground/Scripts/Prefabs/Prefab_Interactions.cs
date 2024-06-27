using System.Numerics;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class Prefab_Interactions : MonoBehaviour
    {
        private ThirdwebClient _client;

        private void Start()
        {
            _client = ThirdwebManager.Instance.Client;
        }

        public async void GetWalletBalance()
        {
            var wallet = await PrivateKeyWallet.Generate(_client); // Generate a new wallet
            var chainId = BigInteger.One;
            var result = await wallet.GetBalance(chainId);
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            var address = await wallet.GetAddress();
            Debugger.Instance.Log("Native Wallet Balance", $"Address: {address}\nChain:{chainId}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }

        public async void GetERC20Balance()
        {
            var wallet = await PrivateKeyWallet.Generate(_client);
            var chainId = BigInteger.One;
            var tokenAddress = "0x6b175474e89094c44da98b954eedeac495271d0f"; // DAI token address
            var result = await wallet.GetBalance(chainId, tokenAddress);
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            var address = await wallet.GetAddress();
            Debugger.Instance.Log("Token Wallet Balance", $"Address: {address}\nChain:{chainId}\nToken Address: {tokenAddress}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }

        public async void GetContractBalance()
        {
            var contractAddress = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe"; // Ethereum Foundation
            var chainId = BigInteger.One;
            var contract = await ThirdwebContract.Create(_client, contractAddress, chainId);
            var result = await contract.GetBalance();
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            Debugger.Instance.Log("Contract Balance", $"Contract Address: {contractAddress}\nChain:{chainId}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }
    }
}
