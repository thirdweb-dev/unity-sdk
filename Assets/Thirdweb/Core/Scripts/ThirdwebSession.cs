using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using link.magic.unity.sdk;
using MetaMask.NEthereum;
using MetaMask.Unity;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Siwe;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.NEthereum.Client;
using WalletConnectSharp.Unity;

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
        public WalletProvider WalletProvider { get; private set; }
        public Account LocalAccount { get; private set; }
        public Account PersonalAccount { get; private set; }
        public string Email { get; private set; }
        public ThirdwebChainData CurrentChainData { get; private set; }
        public ThirdwebInterceptor Interceptor { get; private set; }

        public bool IsConnected
        {
            get { return LocalAccount != null || ThirdwebManager.Instance.SDK.session.WalletProvider != WalletProvider.LocalWallet; }
        }

        private static int Nonce = 0;

        #endregion

        #region Constructors

        public ThirdwebSession(ThirdwebSDK.Options options, int chainId, string rpcUrl)
        {
            Options = options;
            ChainId = chainId;
            RPC = rpcUrl;
            SiweSession = new SiweMessageService();
            Web3 = new Web3(rpcUrl);
            WalletProvider = WalletProvider.LocalWallet;
            LocalAccount = null;
            PersonalAccount = null;
            Email = null;
            Interceptor = null;
            CurrentChainData = FetchChainData();
        }

        #endregion

        #region Public Methods

        public async Task<string> Connect(WalletProvider walletProvider, string password = null, string email = null)
        {
            WalletProvider = walletProvider;
            Email = email;

            switch (walletProvider)
            {
                case WalletProvider.LocalWallet:
                    InitializeLocalWallet(password);
                    break;
                case WalletProvider.WalletConnectV1:
                    await InitializeWalletConnect();
                    break;
                case WalletProvider.MagicLink:
                    await InitializeMagicLink();
                    break;
                case WalletProvider.Metamask:
                    await InitializeMetaMask();
                    break;
                default:
                    throw new UnityException("This wallet connection method is not supported on this platform!");
            }

            Interceptor = new ThirdwebInterceptor(this);
            Web3.Client.OverridingRequestInterceptor = Interceptor;

            var connectedChainId = await ThirdwebManager.Instance.SDK.wallet.GetChainId();
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

            return await GetAddress();
        }

        public void Disconnect()
        {
            switch (WalletProvider)
            {
                case WalletProvider.WalletConnectV1:
                    WalletConnect.Instance.DisableWalletConnect();
                    break;
                case WalletProvider.MagicLink:
                    MagicUnity.Instance.DisableMagicAuth();
                    break;
                case WalletProvider.Metamask:
                    MetaMaskUnity.Instance.Disconnect();
                    break;
                default:
                    break;
            }

            ThirdwebManager.Instance.SDK.session = new ThirdwebSession(Options, ChainId, RPC);
        }

        public async Task<string> GetAddress()
        {
            string address = null;
            switch (WalletProvider)
            {
                case WalletProvider.LocalWallet:
                    if (LocalAccount == null)
                        throw new UnityException("No Account Connected!");
                    address = LocalAccount.Address;
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
                default:
                    throw new UnityException("No Account Connected!");
            }
            return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address);
        }

        public async Task<T> Request<T>(string method, params object[] parameters)
        {
            var request = new RpcRequest(Nonce, method, parameters);
            Nonce++;
            return await Web3.Client.SendRequestAsync<T>(request);
        }

        #endregion

        #region Private Methods

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

        private void InitializeLocalWallet(string password)
        {
            LocalAccount = Utils.UnlockOrGenerateLocalAccount(ChainId, password);
            Web3 = new Web3(LocalAccount, RPC);
            PersonalAccount = null;
        }

        private async Task InitializeWalletConnect()
        {
            if (WalletConnect.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.WalletConnectPrefab);
                await new WaitForSeconds(0.5f);
                WalletConnect.Instance.Initialize();
            }

            await WalletConnect.Instance.EnableWalletConnect();
            Web3 = new Web3(new WalletConnectClient(WalletConnect.Instance.Session));
            LocalAccount = null;
            PersonalAccount = null;
        }

        private async Task InitializeMagicLink()
        {
            if (MagicUnity.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.MagicAuthPrefab);
                await new WaitForSeconds(0.5f);
                if (Options.wallet?.magicLinkApiKey == null)
                    throw new UnityException("MagicLink API Key is not set!");
                MagicUnity.Instance.Initialize(Options.wallet?.magicLinkApiKey, new link.magic.unity.sdk.Relayer.CustomNodeConfiguration(RPC, ChainId));
            }

            await MagicUnity.Instance.EnableMagicAuth(Email);
            Web3 = new Web3(Magic.Instance.Provider);
            LocalAccount = null;
            PersonalAccount = null;
        }

        private async Task InitializeMetaMask()
        {
            if (MetaMaskUnity.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.MetamaskPrefab);
                await new WaitForSeconds(0.5f);
                MetaMaskUnity.Instance.Initialize();
            }

            MetaMaskUnity.Instance.Connect();
            bool connected = false;
            MetaMaskUnity.Instance.Wallet.WalletAuthorized += (sender, e) =>
            {
                Web3 = MetaMaskUnity.Instance.Wallet.CreateWeb3();
                LocalAccount = null;
                PersonalAccount = null;
                connected = true;
            };
            await new WaitUntil(() => connected);
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
                nativeCurrency = new NativeCurrency()
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
            public NativeCurrency nativeCurrency;
            public string[] rpcUrls;
        }

        public class ThirdwebChain
        {
            public string chainId;
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
