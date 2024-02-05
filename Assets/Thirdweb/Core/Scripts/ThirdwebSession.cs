using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Siwe;
using Nethereum.Web3;
using UnityEngine;
using Thirdweb.Wallets;
using System.Linq;
using Newtonsoft.Json;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public class ThirdwebSession
    {
        #region Properties


        internal ThirdwebSDK.Options Options { get; private set; }
        internal BigInteger ChainId { get; private set; }
        internal string RPC { get; private set; }
        internal SiweMessageService SiweSession { get; private set; }
        internal Web3 Web3 { get; private set; }
        internal ThirdwebChainData CurrentChainData { get; private set; }

        internal IThirdwebWallet ActiveWallet { get; private set; }

        internal static int Nonce = 0;

        #endregion

        #region Constructors

        public ThirdwebSession(ThirdwebSDK.Options options, BigInteger chainId, string rpcUrl)
        {
            Options = options;
            ChainId = chainId;
            RPC = rpcUrl;
            SiweSession = new SiweMessageService();
            Web3 = new Web3(rpcUrl);
            CurrentChainData = options.supportedChains.ToList().Find(x => x.chainId == new HexBigInteger(chainId).HexValue);
        }

        #endregion

        #region Internal Methods

        internal async Task<string> Connect(WalletConnection walletConnection)
        {
            switch (walletConnection.provider)
            {
                case WalletProvider.LocalWallet:
                    ActiveWallet = new ThirdwebLocalWallet();
                    break;
                case WalletProvider.WalletConnect:
                    if (Options.wallet == null || string.IsNullOrEmpty(Options.wallet?.walletConnectProjectId))
                        throw new UnityException("Wallet connect project id is required for wallet connect connection method!");
                    ActiveWallet = new ThirdwebWalletConnect(Options.wallet?.walletConnectProjectId);
                    break;
                case WalletProvider.Metamask:
                    ActiveWallet = new ThirdwebMetamask();
                    break;
                case WalletProvider.SmartWallet:
                    if (Options.smartWalletConfig == null)
                        throw new UnityException("Smart wallet config is required for smart wallet connection method!");
                    if (ActiveWallet?.GetProvider() != walletConnection.personalWallet)
                    {
                        try
                        {
                            await Connect(
                                new WalletConnection(
                                    provider: walletConnection.personalWallet,
                                    chainId: walletConnection.chainId,
                                    password: walletConnection.password,
                                    email: walletConnection.email,
                                    authOptions: walletConnection.authOptions
                                )
                            );
                        }
                        catch
                        {
                            ActiveWallet = null;
                            throw;
                        }
                    }
                    else
                    {
                        ThirdwebDebug.Log("Already connected to personal wallet, skipping connection.");
                    }
                    ActiveWallet = new ThirdwebSmartWallet(ActiveWallet, Options.smartWalletConfig.Value);
                    break;
                case WalletProvider.Hyperplay:
                    ActiveWallet = new ThirdwebHyperplay(ChainId.ToString());
                    break;
                case WalletProvider.EmbeddedWallet:
                    if (string.IsNullOrEmpty(Options.clientId))
                        throw new UnityException("thirdweb client id is required for EmbeddedWallet connection method!");
                    ActiveWallet = new ThirdwebEmbeddedWallet(Options.clientId, Options.bundleId);
                    break;
                default:
                    throw new UnityException("This wallet connection method is not supported on this platform!");
            }

            await ActiveWallet.Connect(walletConnection, RPC);

            Web3 = await ActiveWallet.GetWeb3();
            Web3.Client.OverridingRequestInterceptor = new ThirdwebInterceptor(ActiveWallet);

            try
            {
                await EnsureCorrectNetwork(ChainId);
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogWarning("WalletProvider unable to switch chains, proceeding anyway. Error:" + e.Message);
            }

            var addy = await ActiveWallet.GetAddress();

            ThirdwebDebug.Log($"Connected wallet {walletConnection.provider} with address {addy} on chain {ChainId} with RPC {RPC}");

            return addy;
        }

        internal async Task Disconnect(bool endSession = true)
        {
            if (ActiveWallet != null)
            {
                await ActiveWallet.Disconnect(endSession);
            }
            else
            {
                ThirdwebDebug.LogWarning("No active wallet detected, unable to disconnect.");
            }
            ThirdwebManager.Instance.SDK.session = new ThirdwebSession(Options, ChainId, RPC);
        }

        internal async Task<T> Request<T>(string method, params object[] parameters)
        {
            var request = new RpcRequest(Nonce, method, parameters);
            Nonce++;
            return await Web3.Client.SendRequestAsync<T>(request);
        }

        internal async Task EnsureCorrectNetwork(BigInteger newChainId)
        {
            ThirdwebChainData newChainData = null;
            try
            {
                newChainData = Options.supportedChains.ToList().Find(x => x.chainId == new HexBigInteger(newChainId).HexValue);
            }
            catch
            {
                throw new UnityException("The chain you are trying to switch to is not part of the ThirdwebManager's supported chains.");
            }

            NetworkSwitchAction switchResult = await ActiveWallet.PrepareForNetworkSwitch(newChainId, newChainData.rpcUrls[0]);

            switch (switchResult)
            {
                case NetworkSwitchAction.ContinueSwitch:
                    var hexChainId = await Request<string>("eth_chainId");
                    var connectedChainId = hexChainId.HexToBigInteger(false);
                    if (connectedChainId != ChainId)
                    {
                        try
                        {
                            await SwitchNetwork(new ThirdwebChain() { chainId = newChainData.chainId });
                        }
                        catch (System.Exception e)
                        {
                            ThirdwebDebug.LogWarning("Switching chain error, attempting to add chain: " + e.Message);
                            try
                            {
                                await AddNetwork(newChainData);
                                await SwitchNetwork(new ThirdwebChain() { chainId = newChainData.chainId });
                            }
                            catch (System.Exception f)
                            {
                                throw new UnityException("Adding chain error: " + f.Message);
                            }
                        }
                    }
                    break;
                case NetworkSwitchAction.Handled:
                    break;
                case NetworkSwitchAction.Unsupported:
                    throw new UnityException("Network switching is not supported by the active wallet.");
            }

            ChainId = newChainId;
            CurrentChainData = newChainData;
            RPC = CurrentChainData.rpcUrls[0];
            Web3 = await ActiveWallet.GetWeb3();
            Web3.TransactionManager.UseLegacyAsDefault = !Utils.Supports1559(newChainId.ToString());
            Web3.Client.OverridingRequestInterceptor = new ThirdwebInterceptor(ActiveWallet);
        }

        #endregion

        #region Private Methods

        private async Task SwitchNetwork(ThirdwebChain newChain)
        {
            await Request<object>("wallet_switchEthereumChain", new object[] { newChain });
        }

        private async Task AddNetwork(ThirdwebChainData newChainData)
        {
            var sanitizedRpcs = newChainData.rpcUrls.Select(x => x.Contains("rpc.thirdweb.com") ? x[..(x.IndexOf(".com") + 4)] : x).ToArray();
            newChainData.rpcUrls = sanitizedRpcs;
            await Request<object>("wallet_addEthereumChain", new object[] { newChainData });
        }

        public static ThirdwebChainData FetchChainData(BigInteger chainId, string rpcOverride = null)
        {
            var allChainsJson = (TextAsset)Resources.Load("all_chains", typeof(TextAsset));
            var allChainsData = JsonConvert.DeserializeObject<List<ChainIDNetworkData>>(allChainsJson.text, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

            var additionalChainsJson = (TextAsset)Resources.Load("all_chains_additional", typeof(TextAsset));
            var additionalChainsData = JsonConvert.DeserializeObject<List<ChainIDNetworkData>>(additionalChainsJson.text, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

            allChainsData.AddRange(additionalChainsData);

            ChainIDNetworkData currentNetwork = allChainsData.Find(x => x.chainId == chainId.ToString());

            var explorerUrls = new List<string>();
            if (currentNetwork.explorers != null)
            {
                foreach (var explorer in currentNetwork.explorers)
                    explorerUrls.Add(explorer.url.Replace("http://", "https://"));
            }
            if (explorerUrls.Count == 0)
                explorerUrls.Add("https://etherscan.io");
            if (string.IsNullOrEmpty(currentNetwork.icon))
                currentNetwork.icon = "ipfs://QmdwQDr6vmBtXmK2TmknkEuZNoaDqTasFdZdu3DRw8b2wt";

            return new ThirdwebChainData()
            {
                chainId = BigInteger.Parse(currentNetwork.chainId).ToHex(false, true) ?? BigInteger.Parse(chainId.ToString()).ToHex(false, true),
                blockExplorerUrls = explorerUrls.ToArray(),
                chainName = currentNetwork.name ?? ThirdwebManager.Instance.activeChain,
                iconUrls = new string[] { currentNetwork.icon },
                nativeCurrency = new ThirdwebNativeCurrency()
                {
                    name = currentNetwork.nativeCurrency?.name ?? "Ether",
                    symbol = currentNetwork.nativeCurrency?.symbol ?? "ETH",
                    decimals = int.Parse(currentNetwork.nativeCurrency?.decimals ?? "18")
                },
                rpcUrls = rpcOverride != null ? new string[] { rpcOverride } : currentNetwork.rpc.ToArray()
            };
        }

        #endregion

        #region Nested Classes

        [System.Serializable]
        class ChainIDNetworkData
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
        class ChainIDNetworkNativeCurrency
        {
            public string name;
            public string symbol;
            public string decimals;
        }

        [System.Serializable]
        class ChainIDNetworkExplorer
        {
            public string name;
            public string url;
            public string standard;
        }

        struct ChaiNIDNetworkIcon
        {
            public string url;
            public int width;
            public int height;
            public string format;
        }

        #endregion
    }
}
