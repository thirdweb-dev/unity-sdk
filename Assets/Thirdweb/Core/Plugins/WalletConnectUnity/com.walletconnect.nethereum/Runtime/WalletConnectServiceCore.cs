using System.Linq;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.HostWallet;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Evm;
using EthSendTransaction = WalletConnectUnity.Core.Evm.EthSendTransaction;
using Transaction = WalletConnectUnity.Core.Evm.Transaction;
using WalletAddEthereumChain = WalletConnectUnity.Core.Evm.WalletAddEthereumChain;
using WalletSwitchEthereumChain = WalletConnectUnity.Core.Evm.WalletSwitchEthereumChain;

namespace WalletConnectUnity.Nethereum
{
    public class WalletConnectServiceCore : WalletConnectService
    {
        public override bool IsWalletConnected
        {
            get => !string.IsNullOrWhiteSpace(_signClient.AddressProvider.DefaultSession.Topic);
        }

        private readonly ISignClient _signClient;

        public WalletConnectServiceCore(ISignClient signClient)
        {
            _signClient = signClient;
        }

        private string GetDefaultAddress()
        {
            var addressProvider = _signClient.AddressProvider;
            var defaultChainId = addressProvider.DefaultChainId;
            return addressProvider.DefaultSession.CurrentAddress(defaultChainId).Address;
        }

        protected override bool IsMethodSupportedCore(string method)
        {
            var addressProvider = _signClient.AddressProvider;
            var defaultNamespace = addressProvider.DefaultNamespace;
            return addressProvider.DefaultSession.Namespaces[defaultNamespace].Methods.Contains(method);
        }

        protected override async Task<object> SendTransactionAsyncCore(TransactionInput transaction)
        {
            var fromAddress = GetDefaultAddress();
            var txData = new Transaction
            {
                from = fromAddress,
                to = transaction.To,
                value = transaction.Value?.HexValue,
                gas = transaction.Gas?.HexValue,
                gasPrice = transaction.GasPrice?.HexValue,
                data = transaction.Data
            };
            var sendTransactionRequest = new EthSendTransaction(txData);
            return await _signClient.Request<EthSendTransaction, string>(sendTransactionRequest);
        }

        protected override async Task<object> PersonalSignAsyncCore(string message)
        {
            var address = GetDefaultAddress();
            var signDataRequest = new PersonalSign(message, address);
            return await _signClient.Request<PersonalSign, string>(signDataRequest);
        }

        protected override async Task<object> EthSignTypedDataV4AsyncCore(string data)
        {
            var address = GetDefaultAddress();
            var signDataRequest = new EthSignTypedDataV4(address, data);
            return await _signClient.Request<EthSignTypedDataV4, string>(signDataRequest);
        }

        protected override async Task<object> WalletSwitchEthereumChainAsyncCore(SwitchEthereumChainParameter chainId)
        {
            var switchChainRequest = new WalletSwitchEthereumChain(chainId.ChainId.HexValue);
            return await _signClient.Request<WalletSwitchEthereumChain, string>(switchChainRequest);
        }

        protected override async Task<object> WalletAddEthereumChainAsyncCore(AddEthereumChainParameter chain)
        {
            var nativeCurrency = new Currency(chain.NativeCurrency.Name, chain.NativeCurrency.Symbol, (int)chain.NativeCurrency.Decimals);
            var ethereumChain = new EthereumChain(chain.ChainId.HexValue, chain.ChainName, nativeCurrency, chain.RpcUrls.ToArray(), chain.BlockExplorerUrls.ToArray());
            var addEthereumChainRequest = new WalletAddEthereumChain(ethereumChain);
            return await _signClient.Request<WalletAddEthereumChain, string>(addEthereumChainRequest);
        }
    }
}