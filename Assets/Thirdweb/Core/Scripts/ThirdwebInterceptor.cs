using System;
using System.Threading.Tasks;
using MetaMask.Unity;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Thirdweb.AccountAbstraction;
using WalletConnectSharp.Unity;

namespace Thirdweb
{
    public class ThirdwebInterceptor : RequestInterceptor
    {
        private readonly WalletProvider _walletProvider;
        private readonly Account _localAccount;
        private readonly SmartWallet _smartWallet;

        public ThirdwebInterceptor(ThirdwebSession thirdwebSession)
        {
            _walletProvider = thirdwebSession.WalletProvider;
            _localAccount = thirdwebSession.LocalAccount;
            _smartWallet = thirdwebSession.SmartWallet;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            if (request.Method == "eth_accounts")
            {
                string address = "";
                switch (_walletProvider)
                {
                    case WalletProvider.LocalWallet:
                        if (_localAccount == null)
                            throw new Exception("No Account Connected!");
                        address = _localAccount.Address;
                        break;
                    case WalletProvider.WalletConnectV1:
                        address = WalletConnect.Instance.Session.Accounts[0];
                        break;
                    case WalletProvider.MagicLink:
                        address = await MagicUnity.Instance.GetAddress();
                        break;
                    case WalletProvider.Metamask:
                        address = MetaMaskUnity.Instance.Wallet.SelectedAddress;
                        break;
                    case WalletProvider.SmartWallet:
                        address = _smartWallet.Accounts[0];
                        break;
                    default:
                        throw new Exception("No Account Connected!");
                }
                return new string[] { Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address) };
            }
            else if (request.Method == "personal_sign")
            {
                var message = request.RawParameters[0].ToString();
                var address = request.RawParameters[1].ToString();

                if (_walletProvider == WalletProvider.LocalWallet)
                    return new EthereumMessageSigner().EncodeUTF8AndSign(message, new EthECKey(_localAccount.PrivateKey));
                else if (_walletProvider == WalletProvider.SmartWallet)
                    return await _smartWallet.PersonalWeb3.Client.SendRequestAsync<T>("personal_sign", null, message, address);
            }
            else if (request.Method == "eth_signTypedData_v4")
            {
                // Should only happen with non Local Wallet personal wallet
                if (_walletProvider == WalletProvider.SmartWallet)
                {
                    return await _smartWallet.PersonalWeb3.Client.SendRequestAsync<T>("eth_signTypedData_v4", null, request.RawParameters[0], request.RawParameters[1]);
                }
            }

            return await interceptedSendRequestAsync(request, route);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList
        )
        {
            if (method == "eth_accounts")
            {
                string address = "";
                switch (_walletProvider)
                {
                    case WalletProvider.LocalWallet:
                        if (_localAccount == null)
                            throw new Exception("No Account Connected!");
                        address = _localAccount.Address;
                        break;
                    case WalletProvider.WalletConnectV1:
                        address = WalletConnect.Instance.Session.Accounts[0];
                        break;
                    case WalletProvider.MagicLink:
                        address = await MagicUnity.Instance.GetAddress();
                        break;
                    case WalletProvider.Metamask:
                        address = MetaMaskUnity.Instance.Wallet.SelectedAddress;
                        break;
                    case WalletProvider.SmartWallet:
                        address = _smartWallet.Accounts[0];
                        break;
                    default:
                        throw new Exception("No Account Connected!");
                }
                return new string[] { Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address) };
            }
            else if (method == "personal_sign")
            {
                var message = paramList[0].ToString();
                var address = paramList[1].ToString();

                if (_walletProvider == WalletProvider.LocalWallet)
                    return new EthereumMessageSigner().EncodeUTF8AndSign(message, new EthECKey(_localAccount.PrivateKey));
                else if (_walletProvider == WalletProvider.SmartWallet)
                    return await _smartWallet.PersonalWeb3.Client.SendRequestAsync<T>("personal_sign", null, message, address);
            }
            else if (method == "eth_signTypedData_v4")
            {
                // Should only happen with non Local Wallet personal wallet
                if (_walletProvider == WalletProvider.SmartWallet)
                {
                    return await _smartWallet.PersonalWeb3.Client.SendRequestAsync<T>("eth_signTypedData_v4", null, paramList[0], paramList[1]);
                }
            }

            return await interceptedSendRequestAsync(method, route, paramList);
        }
    }
}
