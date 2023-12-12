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
                // if value type, convert to string otherwise serialize to json
                if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs.Add(args[i].ToString());
                }
                else
                {
                    stringArgs.Add(Utils.ToJson(args[i]));
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
            string gateway = ThirdwebManager.Instance.SDK.storage.IPFSGateway;
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

        public static bool DeleteLocalAccount()
        {
            try
            {
                File.Delete(GetAccountPath());
                return true;
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogWarning("Error deleting account: " + e.Message);
                return false;
            }
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

        public async static Task<string> GetENSName(string address)
        {
            try
            {
                var ensService = new Nethereum.Contracts.Standards.ENS.ENSService(
                    new Nethereum.Web3.Web3("https://ethereum.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3").Eth,
                    "0x00000000000C2E074eC69A0dFb2997BA6C7d2e1e"
                );
                return await ensService.ReverseResolveAsync(address);
            }
            catch
            {
                return null;
            }
        }

        public static string ToChecksumAddress(this string address)
        {
            return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address);
        }

        public static string GetBundleId()
        {
            return ThirdwebManager.Instance.SDK?.session?.Options.bundleId
                ?? (string.IsNullOrEmpty(ThirdwebManager.Instance.bundleIdOverride) ? Application.identifier.ToLower() : ThirdwebManager.Instance.bundleIdOverride);
        }

        public static string GetRuntimePlatform()
        {
            return Application.platform.ToString();
        }

        public static string AppendBundleIdQueryParam(this string uri)
        {
            if (IsWebGLBuild())
                return uri;

            uri += $"?bundleId={GetBundleId()}";
            return uri;
        }

        public static Web3 GetWeb3()
        {
            return new Web3(new ThirdwebClient(new Uri(ThirdwebManager.Instance.SDK.session.RPC)));
        }

        public static string GetNativeTokenWrapper(BigInteger chainId)
        {
            string id = chainId.ToString();
            return id switch
            {
                "1" => "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2",
                "4" => "0xc778417E063141139Fce010982780140Aa0cD5Ab", // rinkeby
                "5" => "0xB4FBF271143F4FBf7B91A5ded31805e42b2208d6", // goerli
                "137" => "0x0d500B1d8E8eF31E21C99d1Db9A6444d3ADf1270",
                "80001" => "0x9c3C9283D3e44854697Cd22D3Faa240Cfb032889",
                "43114" => "0xB31f66AA3C1e785363F0875A1B74E27b85FD66c7",
                "43113" => "0xd00ae08403B9bbb9124bB305C09058E32C39A48c",
                "250" => "0x21be370D5312f44cB42ce377BC9b8a0cEF1A4C83",
                "4002" => "0xf1277d1Ed8AD466beddF92ef448A132661956621",
                "10" => "0x4200000000000000000000000000000000000006", // optimism
                "69" => "0xbC6F6b680bc61e30dB47721c6D1c5cde19C1300d", // optimism kovan
                "420" => "0x4200000000000000000000000000000000000006", // optimism goerli
                "42161" => "0x82af49447d8a07e3bd95bd0d56f35241523fbab1", // arbitrum
                "421611" => "0xEBbc3452Cc911591e4F18f3b36727Df45d6bd1f9", // arbitrum rinkeby
                "421613" => "0xe39Ab88f8A4777030A534146A9Ca3B52bd5D43A3", // arbitrum goerli
                "56" => "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c", // binance mainnet
                "97" => "0xae13d989daC2f0dEbFf460aC112a837C89BAa7cd", // binance testnet
                _ => throw new UnityException("Native Token Wrapper Unavailable For This Chain!"),
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
    }
}
