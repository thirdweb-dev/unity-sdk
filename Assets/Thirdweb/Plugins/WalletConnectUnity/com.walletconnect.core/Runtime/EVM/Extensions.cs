using System;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common.Model.Errors;

namespace WalletConnectUnity.Core.Evm
{
    public static class EvmExtensions
    {
        public static async Task SwitchEthereumChainAsync(this IWalletConnect walletConnect, EthereumChain ethereumChain)
        {
            if (ethereumChain == null)
                throw new ArgumentNullException(nameof(ethereumChain));

            var ciap2ChainId = $"eip155:{ethereumChain.chainIdDecimal}";
            if (!walletConnect.ActiveSession.Namespaces.TryGetValue("eip155", out var @namespace)
                || !@namespace.Chains.Contains(ciap2ChainId))
            {
                var request = new WalletAddEthereumChain(ethereumChain);

                try
                {
                    await walletConnect.RequestAsync<WalletAddEthereumChain, string>(request);
                }
                catch (WalletConnectException)
                {
                    // Wallet can decline if chain has already been added
                }
            }

            var data = new WalletSwitchEthereumChain(ethereumChain.chainIdHex);
            await walletConnect.RequestAsync<WalletSwitchEthereumChain, string>(data);
        }
    }
}