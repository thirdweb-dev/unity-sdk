using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Siwe;
using Nethereum.Web3;
using UnityEngine;
using Thirdweb.Wallets;

namespace Thirdweb
{
    public class ThirdwebSession
    {
        #region Properties


        public ThirdwebSDK.Options Options { get; private set; }
        public int ChainId { get; private set; }
        public string RPC { get; private set; }
        public SiweMessageService SiweSession { get; private set; }
        public Web3 Web3 { get; private set; }
        public ThirdwebChainData CurrentChainData { get; private set; }

        public IThirdwebWallet ActiveWallet { get; private set; }

        public static int Nonce = 0;

        #endregion

        #region Constructors

        public ThirdwebSession(ThirdwebSDK.Options options, int chainId, string rpcUrl)
        {
            Options = options;
            ChainId = chainId;
            RPC = rpcUrl;
            SiweSession = new SiweMessageService();
            Web3 = new Web3(rpcUrl);
            CurrentChainData = FetchChainData();
        }

        #endregion

        #region Public Methods

        public async Task<string> Connect(WalletConnection walletConnection)
        {
            switch (walletConnection.provider)
            {
                case WalletProvider.LocalWallet:
                    ActiveWallet = new ThirdwebLocalWallet();
                    break;
                case WalletProvider.WalletConnectV1:
                    ActiveWallet = new ThirdwebWalletConnect();
                    break;
                case WalletProvider.MagicLink:
                    if (Options.wallet == null || string.IsNullOrEmpty(Options.wallet?.magicLinkApiKey))
                        throw new UnityException("Magic link api key is required for magic link connection method!");
                    ActiveWallet = new ThirdwebMagicLink(Options.wallet?.magicLinkApiKey);
                    break;
                case WalletProvider.Metamask:
                    ActiveWallet = new ThirdwebMetamask();
                    break;
                case WalletProvider.SmartWallet:
                    await Connect(new WalletConnection(walletConnection.personalWallet, walletConnection.chainId, walletConnection.password, walletConnection.email));
                    if (Options.smartWalletConfig == null)
                        throw new UnityException("Smart wallet config is required for smart wallet connection method!");
                    ActiveWallet = new ThirdwebSmartWallet(ActiveWallet, Options.smartWalletConfig.Value);
                    break;
                default:
                    throw new UnityException("This wallet connection method is not supported on this platform!");
            }

            await ActiveWallet.Connect(walletConnection, RPC);

            Web3 = await ActiveWallet.GetWeb3();
            Web3.Client.OverridingRequestInterceptor = new ThirdwebInterceptor(ActiveWallet);

            await EnsureCorrectNetwork();

            var addy = await ActiveWallet.GetAddress();

            Debug.Log($"Connected wallet {walletConnection.provider} with address {addy} on chain {ChainId} with RPC {RPC}");

            return addy;
        }

        public async Task Disconnect()
        {
            await ActiveWallet.Disconnect();
            ThirdwebManager.Instance.SDK.session = new ThirdwebSession(Options, ChainId, RPC);
        }

        public async Task<T> Request<T>(string method, params object[] parameters)
        {
            var request = new RpcRequest(Nonce, method, parameters);
            Nonce++;
            return await Web3.Client.SendRequestAsync<T>(request);
        }

        #endregion

        #region Private Methods

        private async Task EnsureCorrectNetwork()
        {
            var hexChainId = await Request<string>("eth_chainId");
            var connectedChainId = (int)hexChainId.HexToBigInteger(false);
            if (connectedChainId != ChainId)
            {
                try
                {
                    await SwitchNetwork(new ThirdwebChain() { chainId = CurrentChainData.chainId });
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Switching chain error, attempting to add chain: " + e.Message);
                    try
                    {
                        await AddNetwork(CurrentChainData);
                        await SwitchNetwork(new ThirdwebChain() { chainId = CurrentChainData.chainId });
                    }
                    catch (System.Exception f)
                    {
                        Debug.LogWarning("Adding chain error: " + f.Message);
                    }
                }
            }
        }

        private async Task SwitchNetwork(ThirdwebChain newChain)
        {
            await Request<object>("wallet_switchEthereumChain", new object[] { newChain });
            CurrentChainData.chainId = newChain.chainId;
        }

        private async Task AddNetwork(ThirdwebChainData newChainData)
        {
            await Request<object>("wallet_addEthereumChain", new object[] { newChainData });
            CurrentChainData = newChainData;
        }

        private ThirdwebChainData FetchChainData()
        {
            var allChainsJson = (TextAsset)Resources.Load("all_chains", typeof(TextAsset));

            List<ChainIDNetworkData> allNetworkData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChainIDNetworkData>>(allChainsJson.text);

            ChainIDNetworkData currentNetwork = allNetworkData.Find(x => x.chainId == ChainId.ToString());

            List<string> explorerUrls = new List<string>();
            foreach (var explorer in currentNetwork.explorers)
                explorerUrls.Add(explorer.url);
            if (explorerUrls.Count == 0)
                explorerUrls.Add("https://etherscan.io");

            return new ThirdwebChainData()
            {
                chainId = BigInteger.Parse(currentNetwork.chainId).ToHex(false, true) ?? BigInteger.Parse(ChainId.ToString()).ToHex(false, true),
                blockExplorerUrls = explorerUrls.ToArray(),
                chainName = currentNetwork.name ?? ThirdwebManager.Instance.GetCurrentChainIdentifier(),
                iconUrls = new string[] { "ipfs://QmdwQDr6vmBtXmK2TmknkEuZNoaDqTasFdZdu3DRw8b2wt" },
                nativeCurrency = new ThirdwebNativeCurrency()
                {
                    name = currentNetwork.nativeCurrency?.name ?? "Ether",
                    symbol = currentNetwork.nativeCurrency?.symbol ?? "ETH",
                    decimals = int.Parse(currentNetwork.nativeCurrency?.decimals ?? "18")
                },
                rpcUrls = new string[] { RPC }
            };
        }

        #endregion

        #region Nested Classes

        public class ThirdwebChainData : ThirdwebChain
        {
            public string[] blockExplorerUrls;
            public string chainName;
            public string[] iconUrls;
            public ThirdwebNativeCurrency nativeCurrency;
            public string[] rpcUrls;
        }

        public class ThirdwebChain
        {
            public string chainId;
        }

        public class ThirdwebNativeCurrency
        {
            public string name;
            public string symbol;
            public int decimals;
        }

        [System.Serializable]
        public class ChainIDNetworkData
        {
            public string name;
            public string chain;
            public string icon;
            public List<string> rpc;
            public ChainIDNetworkNativeCurrency nativeCurrency;
            public string chainId;
            public List<ChainIDNetworkExplorer> explorers;
        }

        [System.Serializable]
        public class ChainIDNetworkNativeCurrency
        {
            public string name;
            public string symbol;
            public string decimals;
        }

        [System.Serializable]
        public class ChainIDNetworkExplorer
        {
            public string name;
            public string url;
            public string standard;
        }

        public struct ChaiNIDNetworkIcon
        {
            public string url;
            public int width;
            public int height;
            public string format;
        }

        #endregion
    }
}
