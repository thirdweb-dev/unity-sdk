using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;
using System.IO;
using UnityEngine;
using Nethereum.Signer;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Collections;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public static class Utils
    {
        public const string AddressZero = "0x0000000000000000000000000000000000000000";
        public const string NativeTokenAddress = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";
        public const double DECIMALS_18 = 1000000000000000000;

        public static string[] ToJsonStringArray(params object[] args)
        {
            var stringArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                // if array or list, check if bytes and convert to hex
                if (args[i].GetType().IsArray || args[i] is IList)
                {
                    var enumerable = args[i] as IEnumerable;
                    var enumerableArgs = new List<object>();
                    foreach (var item in enumerable)
                    {
                        if (item is byte[])
                        {
                            enumerableArgs.Add(ByteArrayToHexString(item as byte[]));
                        }
                        else
                        {
                            enumerableArgs.Add(item);
                        }
                    }
                    stringArgs.Add(ToJson(enumerableArgs));
                }
                // if bytes, make hex
                else if (args[i] is byte[])
                {
                    stringArgs.Add(ByteArrayToHexString(args[i] as byte[]));
                }
                // if value type, convert to string otherwise serialize to json
                else if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs.Add(args[i].ToString());
                }
                else
                {
                    stringArgs.Add(ToJson(args[i]));
                }
            }
            return stringArgs.ToArray();
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string ToBytes32HexString(byte[] bytes)
        {
            var hex = new StringBuilder(64);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return "0x" + hex.ToString().PadLeft(64, '0');
        }

        public static long UnixTimeNowMs()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        public static string ToWei(this string eth)
        {
            if (!double.TryParse(eth, NumberStyles.Number, CultureInfo.InvariantCulture, out double ethDouble))
                throw new ArgumentException("Invalid eth value.");
            BigInteger wei = (BigInteger)(ethDouble * DECIMALS_18);
            return wei.ToString();
        }

        public static string ToEth(this string wei, int decimalsToDisplay = 4, bool addCommas = true)
        {
            return FormatERC20(wei, decimalsToDisplay, 18, addCommas);
        }

        public static string FormatERC20(this string wei, int decimalsToDisplay = 4, int decimals = 18, bool addCommas = true)
        {
            decimals = decimals == 0 ? 18 : decimals;
            if (!BigInteger.TryParse(wei, out BigInteger weiBigInt))
                throw new ArgumentException("Invalid wei value.");
            double eth = (double)weiBigInt / Math.Pow(10.0, decimals);
            string format = addCommas ? "#,0" : "#0";
            if (decimalsToDisplay > 0)
                format += ".";
            for (int i = 0; i < decimalsToDisplay; i++)
                format += "#";
            return eth.ToString(format);
        }

        public static BigInteger AdjustDecimals(this BigInteger value, int fromDecimals, int toDecimals)
        {
            int differenceInDecimals = fromDecimals - toDecimals;

            if (differenceInDecimals > 0)
            {
                return value / BigInteger.Pow(10, differenceInDecimals);
            }
            else if (differenceInDecimals < 0)
            {
                return value * BigInteger.Pow(10, -differenceInDecimals);
            }

            return value;
        }

        public static string ShortenAddress(this string address)
        {
            if (address.Length != 42)
                throw new ArgumentException("Invalid Address Length.");
            return $"{address[..6]}...{address[38..]}";
        }

        public static bool IsWebGLBuild()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static string ReplaceIPFS(this string uri)
        {
            string gateway = ThirdwebManager.Instance.SDK.Storage.IPFSGateway;
            if (!string.IsNullOrEmpty(uri) && uri.StartsWith("ipfs://"))
                return uri.Replace("ipfs://", gateway);
            else
                return uri;
        }

        public static TransactionResult ToTransactionResult(this Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt)
        {
            var result = new TransactionResult();

            if (receipt != null)
            {
                result.receipt.to = receipt.To;
                result.receipt.from = receipt.From;
                result.receipt.contractAddress = receipt.ContractAddress;
                result.receipt.transactionIndex = receipt.TransactionIndex != null ? receipt.TransactionIndex.Value : -1;
                result.receipt.gasUsed = receipt.GasUsed != null ? receipt.GasUsed.Value : -1;
                result.receipt.logsBloom = receipt.LogsBloom;
                result.receipt.blockHash = receipt.BlockHash;
                result.receipt.transactionHash = receipt.TransactionHash;
                result.receipt.logs = receipt.Logs;
                result.receipt.blockNumber = receipt.BlockNumber != null ? receipt.BlockNumber.Value : -1;
                result.receipt.confirmations = -1;
                result.receipt.cumulativeGasUsed = receipt.CumulativeGasUsed != null ? receipt.CumulativeGasUsed.Value : -1;
                result.receipt.effectiveGasPrice = receipt.EffectiveGasPrice != null ? receipt.EffectiveGasPrice.Value : -1;
                result.receipt.status = receipt.Status != null ? receipt.Status.Value : -1;
                result.receipt.type = receipt.Type != null ? receipt.Type.Value : -1;
                result.receipt.byzantium = null;
                result.receipt.events = null;
                result.id = receipt.Status != null ? receipt.Status.ToString() : "-1";
            }

            return result;
        }

        public static List<Contracts.Pack.ContractDefinition.Token> ToPackTokenList(this NewPackInput packContents)
        {
            var tokenList = new List<Contracts.Pack.ContractDefinition.Token>();
            // Add ERC20 Rewards
            foreach (var erc20Reward in packContents.erc20Rewards)
            {
                tokenList.Add(
                    new Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc20Reward.contractAddress,
                        TokenType = 0,
                        TokenId = 0,
                        TotalAmount = BigInteger.Parse(erc20Reward.totalRewards.ToWei()),
                    }
                );
            }
            // Add ERC721 Rewards
            foreach (var erc721Reward in packContents.erc721Rewards)
            {
                tokenList.Add(
                    new Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc721Reward.contractAddress,
                        TokenType = 1,
                        TokenId = BigInteger.Parse(erc721Reward.tokenId),
                        TotalAmount = 1,
                    }
                );
            }
            // Add ERC1155 Rewards
            foreach (var erc1155Reward in packContents.erc1155Rewards)
            {
                tokenList.Add(
                    new Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc1155Reward.contractAddress,
                        TokenType = 2,
                        TokenId = BigInteger.Parse(erc1155Reward.tokenId),
                        TotalAmount = BigInteger.Parse(erc1155Reward.totalRewards),
                    }
                );
            }
            return tokenList;
        }

        public static List<BigInteger> ToPackRewardUnitsList(this PackContents packContents)
        {
            var rewardUnits = new List<BigInteger>();
            // Add ERC20 Rewards
            foreach (var content in packContents.erc20Rewards)
            {
                rewardUnits.Add(BigInteger.Parse(content.quantityPerReward.ToWei()));
            }
            // Add ERC721 Rewards
            foreach (var content in packContents.erc721Rewards)
            {
                rewardUnits.Add(1);
            }
            // Add ERC1155 Rewards
            foreach (var content in packContents.erc1155Rewards)
            {
                rewardUnits.Add(BigInteger.Parse(content.quantityPerReward));
            }
            return rewardUnits;
        }

        public static long GetUnixTimeStampNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long GetUnixTimeStampIn10Years()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60 * 60 * 24 * 365 * 10;
        }

        public static byte[] HexStringToByteArray(this string hex)
        {
            return hex.HexToByteArray();
        }

        public static string HexConcat(params string[] hexStrings)
        {
            var hex = new StringBuilder("0x");

            foreach (var hexStr in hexStrings)
                hex.Append(hexStr[2..]);

            return hex.ToString();
        }

        public static async Task<JToken> ToJToken(this object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return await Task.FromResult(JToken.Parse(json));
        }

        public static string ByteArrayToHexString(this byte[] hexBytes)
        {
            return hexBytes.ToHex(true);
        }

        public static BigInteger GetMaxUint256()
        {
            return BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935");
        }

        public static string GetDeviceIdentifier()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static bool HasStoredAccount()
        {
            return File.Exists(GetAccountPath());
        }

        public static string GetAccountPath()
        {
            return Application.persistentDataPath + "/account.json";
        }

        public static void DeleteLocalAccount()
        {
            if (File.Exists(GetAccountPath()))
                File.Delete(GetAccountPath());
        }

        public static Account UnlockOrGenerateLocalAccount(BigInteger chainId, string password = null, string privateKey = null)
        {
            password = string.IsNullOrEmpty(password) ? GetDeviceIdentifier() : password;

            var path = GetAccountPath();
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();

            if (privateKey != null)
            {
                return new Account(privateKey, chainId);
            }
            else
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var encryptedJson = File.ReadAllText(path);
                        var key = keyStoreService.DecryptKeyStoreFromJson(password, encryptedJson);
                        return new Account(key, chainId);
                    }
                    catch (System.Exception)
                    {
                        throw new UnityException("Incorrect Password!");
                    }
                }
                else
                {
                    byte[] seed = new byte[32];
                    using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(seed);
                    }
                    var ecKey = Nethereum.Signer.EthECKey.GenerateKey(seed);
                    File.WriteAllText(path, EncryptAndGenerateKeyStore(ecKey, password));
                    return new Account(ecKey, chainId);
                }
            }
        }

        public static string EncryptAndGenerateKeyStore(EthECKey ecKey, string password)
        {
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
            var scryptParams = new Nethereum.KeyStore.Model.ScryptParams
            {
                Dklen = 32,
                N = 262144,
                R = 1,
                P = 8
            };
            var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, ecKey.GetPrivateKeyAsBytes(), ecKey.GetPublicAddress(), scryptParams);
            return keyStoreService.SerializeKeyStoreToJson(keyStore);
        }

        public static Account GenerateRandomAccount(BigInteger chainId)
        {
            byte[] seed = new byte[32];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(seed);
            }
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey(seed);
            return new Account(ecKey, chainId);
        }

        public static string CidToIpfsUrl(this string cid, bool useGateway = false)
        {
            string ipfsRaw = $"ipfs://{cid}";
            return useGateway ? ipfsRaw.ReplaceIPFS() : ipfsRaw;
        }

        public async static Task<string> ResolveAddressFromENS(string ens)
        {
            if (string.IsNullOrEmpty(ens) || !ens.EndsWith(".eth"))
                return ens;

            try
            {
                string address = null;
                if (IsWebGLBuild())
                {
                    address = await Bridge.ResolveAddressFromENS(ens);
                }
                else
                {
                    var ensService = new Nethereum.Contracts.Standards.ENS.ENSService(new Web3("https://1.rpc.thirdweb.com/").Eth, "0x00000000000C2E074eC69A0dFb2997BA6C7d2e1e");
                    address = await ensService.ResolveAddressAsync(ens);
                }
                return string.IsNullOrEmpty(address) ? ens : address;
            }
            catch
            {
                return ens;
            }
        }

        public async static Task<string> ResolveENSFromAddress(string address)
        {
            if (string.IsNullOrEmpty(address) || address.Length != 42 || !address.StartsWith("0x"))
                return address;

            try
            {
                string ens = null;
                if (IsWebGLBuild())
                {
                    ens = await Bridge.ResolveENSFromAddress(address);
                }
                else
                {
                    var ensService = new Nethereum.Contracts.Standards.ENS.ENSService(new Web3("https://1.rpc.thirdweb.com/").Eth, "0x00000000000C2E074eC69A0dFb2997BA6C7d2e1e");
                    ens = await ensService.ReverseResolveAsync(address);
                }
                return string.IsNullOrEmpty(ens) ? address : ens;
            }
            catch
            {
                return address;
            }
        }

        public static string ToChecksumAddress(this string address)
        {
            return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address);
        }

        public static string GetClientId()
        {
            return ThirdwebManager.Instance.SDK?.Session?.Options.clientId ?? (string.IsNullOrEmpty(ThirdwebManager.Instance.clientId) ? null : ThirdwebManager.Instance.clientId);
        }

        public static string GetBundleId()
        {
            return ThirdwebManager.Instance.SDK?.Session?.Options.bundleId
                ?? (string.IsNullOrEmpty(ThirdwebManager.Instance.bundleIdOverride) ? Application.identifier.ToLower() : ThirdwebManager.Instance.bundleIdOverride);
        }

        public static string GetRuntimePlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android OS";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "Mac OS";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                default:
                    return Application.platform.ToString().ToLower();
            }
        }

        public static string AppendBundleIdQueryParam(this string uri)
        {
            if (IsWebGLBuild())
                return uri;

            uri += $"?bundleId={GetBundleId()}";
            return uri;
        }

        public static Web3 GetWeb3(BigInteger? chainId = null)
        {
            return new Web3(new ThirdwebClient(new Uri(chainId == null ? ThirdwebManager.Instance.SDK.Session.RPC : $"https://{chainId}.rpc.thirdweb.com")));
        }

        public static string GetNativeTokenWrapper(BigInteger chainId)
        {
            string id = chainId.ToString();
            return id switch
            {
                "1" => "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2", // mainnet
                "5" => "0xB4FBF271143F4FBf7B91A5ded31805e42b2208d6", // goerli
                "11155111" => "0x7b79995e5f793A07Bc00c21412e50Ecae098E7f9", // sepolia
                "137" => "0x0d500B1d8E8eF31E21C99d1Db9A6444d3ADf1270", // polygon
                "1101" => "0x4f9a0e7fd2bf6067db6994cf12e4495df938e6e9", // polygon zkevm
                "80001" => "0x9c3C9283D3e44854697Cd22D3Faa240Cfb032889", // mumbai
                "43114" => "0xB31f66AA3C1e785363F0875A1B74E27b85FD66c7", // avalanche
                "43113" => "0xd00ae08403B9bbb9124bB305C09058E32C39A48c", // avalanche fuji
                "250" => "0x21be370D5312f44cB42ce377BC9b8a0cEF1A4C83", // fantom
                "4002" => "0xf1277d1Ed8AD466beddF92ef448A132661956621", // fantom testnet
                "10" => "0x4200000000000000000000000000000000000006", // optimism
                "69" => "0xbC6F6b680bc61e30dB47721c6D1c5cde19C1300d", // optimism kovan
                "420" => "0x4200000000000000000000000000000000000006", // optimism goerli
                "42161" => "0x82af49447d8a07e3bd95bd0d56f35241523fbab1", // arbitrum
                "421611" => "0xEBbc3452Cc911591e4F18f3b36727Df45d6bd1f9", // arbitrum rinkeby
                "421613" => "0xe39Ab88f8A4777030A534146A9Ca3B52bd5D43A3", // arbitrum goerli
                "42170" => "0x722e8bdd2ce80a4422e880164f2079488e115365", // arbitrum nova
                "56" => "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c", // binance
                "97" => "0xae13d989daC2f0dEbFf460aC112a837C89BAa7cd", // binance testnet
                "84533" => "0x4200000000000000000000000000000000000006", // base
                "84531" => "0x4200000000000000000000000000000000000006", // base goerli
                "84532" => "0x4200000000000000000000000000000000000006", // base sepolia
                "324" => "0x5aea5775959fbc2557cc8789bc1bf90a239d9a91", // zksync era
                "280" => "0x5AEa5775959fBC2557Cc8789bC1bf90A239D9a91", // zksync era testnet
                "59144" => "0xe5d7c2a44ffddf6b295a15c148167daaaf5cf34f", // linea
                "534352" => "0x5300000000000000000000000000000000000004", // scroll
                "1030" => "0xa47f43de2f9623acb395ca4905746496d2014d57", // conflux
                _ => throw new UnityException($"WETH (or equivalent) contract address not known for chain id {id}."),
            };
        }

        public static bool Supports1559(string chainId)
        {
            switch (chainId)
            {
                // BNB Mainnet
                case "56":
                // BNB Testnet
                case "97":
                // opBNB Mainnet
                case "204":
                // opBNB Testnet
                case "5611":
                    return false;
                default:
                    return true;
            }
        }

        public static string JSDateToUnixTimestamp(string dateString)
        {
            DateTime dateTime = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixTimestamp = (long)(dateTime - unixEpoch).TotalSeconds;
            return unixTimestamp.ToString();
        }

        public async static Task<BigInteger> GetLegacyGasPriceAsync(BigInteger chainId)
        {
            var web3 = GetWeb3(chainId);
            var gasPrice = (await web3.Eth.GasPrice.SendRequestAsync()).Value;
            return BigInteger.Multiply(gasPrice, 10) / 9;
        }

        public async static Task<GasPriceParameters> GetGasPriceAsync(BigInteger chainId)
        {
            BigInteger? priorityOverride = null;
            if (chainId == 137 || chainId == 80001)
            {
                try
                {
                    return await GetPolygonGasPriceParameters((int)chainId);
                }
                catch (System.Exception e)
                {
                    ThirdwebDebug.LogWarning($"Failed to get gas price from Polygon gas station, using default method: {e.Message}");
                    priorityOverride = GweiToWei(chainId == 137 ? 40 : 1);
                }
            }

            var web3 = GetWeb3(chainId);
            var gasPrice = (await web3.Eth.GasPrice.SendRequestAsync()).Value;

            if (chainId == 42220) // celo mainnet
            {
                gasPrice = BigInteger.Multiply(gasPrice, 3) / 2;
                return new GasPriceParameters(gasPrice, gasPrice);
            }

            if (
                chainId == 1 // mainnet
                || chainId == 11155111 // sepolia
                || chainId == 42161 // arbitrum
                || chainId == 421614 // arbitrum sepolia
                || chainId == 534352 // scroll
                || chainId == 534351 // scroll sepolia
                || chainId == 5000 // mantle
                || chainId == 22222 // nautilus
                || chainId == 8453 // base
                || chainId == 53935 // dfk
                || chainId == 44787 // celo alfajores
                || chainId == 43114 // avalanche
                || chainId == 43113 // avalanche fuji
                || chainId == 8453 // base
                || chainId == 84532 // base sepolia
            )
            {
                gasPrice = BigInteger.Multiply(gasPrice, 10) / 9;
                return new GasPriceParameters(gasPrice, priorityOverride ?? gasPrice);
            }

            var maxPriorityFeePerGas = new BigInteger(2000000000) > gasPrice ? gasPrice : new BigInteger(2000000000);

            var feeHistory = await web3.Eth.FeeHistory.SendRequestAsync(new Nethereum.Hex.HexTypes.HexBigInteger(20), Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest(), new double[] { 20 });

            if (feeHistory.Reward == null)
            {
                gasPrice = BigInteger.Multiply(gasPrice, 3) / 2;
                maxPriorityFeePerGas = gasPrice;
            }
            else
            {
                var feeAverage = feeHistory.Reward.Select(r => r[0]).Aggregate(BigInteger.Zero, (acc, cur) => cur + acc) / 10;
                if (feeAverage > gasPrice)
                {
                    gasPrice = feeAverage;
                }
                maxPriorityFeePerGas = gasPrice;
            }

            return new GasPriceParameters(gasPrice, priorityOverride ?? maxPriorityFeePerGas);
        }

        public async static Task<GasPriceParameters> GetPolygonGasPriceParameters(int chainId)
        {
            using var httpClient = new HttpClient();
            string gasStationUrl;
            switch (chainId)
            {
                case 137:
                    gasStationUrl = "https://gasstation.polygon.technology/v2";
                    break;
                case 80001:
                    gasStationUrl = "https://gasstation-testnet.polygon.technology/v2";
                    break;
                default:
                    throw new UnityException("Unsupported chain id");
            }

            var response = await httpClient.GetAsync(gasStationUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<PolygonGasStationResult>(responseBody);
            return new GasPriceParameters(GweiToWei(data.fast.maxFee), GweiToWei(data.fast.maxPriorityFee));
        }

        public static BigInteger GweiToWei(double gweiAmount)
        {
            return new BigInteger(gweiAmount * 1e9);
        }

        public static string BigIntToHex(this BigInteger number)
        {
            return new HexBigInteger(number).HexValue;
        }

        public static async void TrackWalletAnalytics(string clientId, string source, string action, string walletType, string walletAddress)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(source) || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(walletType) || string.IsNullOrEmpty(walletAddress))
                return;

            try
            {
                var body = new
                {
                    source,
                    action,
                    walletAddress,
                    walletType,
                };
                var headers = new Dictionary<string, string>
                {
                    { "x-client-id", clientId },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-name", "UnitySDK" },
                    { "x-sdk-version", ThirdwebSDK.version },
                    { "x-sdk-os", GetRuntimePlatform() },
                    { "x-bundle-id", GetBundleId() },
                };
                var request = new HttpRequestMessage(HttpMethod.Post, "https://c.thirdweb.com/event")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                using var client = new HttpClient();
                await client.SendAsync(request);
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogWarning($"Failed to send wallet analytics: {e}");
            }
        }

        public static byte[] HashPrefixedMessage(this byte[] messageBytes)
        {
            var signer = new EthereumMessageSigner();
            return signer.HashPrefixedMessage(messageBytes);
        }

        public static string HashPrefixedMessage(this string message)
        {
            var signer = new EthereumMessageSigner();
            return signer.HashPrefixedMessage(System.Text.Encoding.UTF8.GetBytes(message)).ByteArrayToHexString();
        }

        public static byte[] HashMessage(this byte[] messageBytes)
        {
            var sha3 = new Nethereum.Util.Sha3Keccack();
            return sha3.CalculateHash(messageBytes);
        }

        public static string HashMessage(this string message)
        {
            var sha3 = new Nethereum.Util.Sha3Keccack();
            return sha3.CalculateHash(message);
        }

        public static bool IsValidEmail(string email)
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^\S+@\S+\.\S+$");
            return emailRegex.IsMatch(email.Replace("+", ""));
        }

        public static string GenerateRandomString(int v)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new System.Random();
            var result = new string(Enumerable.Repeat(chars, v).Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }

        public static async Task<bool> CopyToClipboard(string text)
        {
            try
            {
                if (IsWebGLBuild())
                    await Bridge.CopyBuffer(text);
                else
                    GUIUtility.systemCopyBuffer = text;
                return true;
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogWarning($"Failed to copy to clipboard: {e}");
                return false;
            }
        }
    }
}
