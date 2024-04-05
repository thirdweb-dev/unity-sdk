using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UnityEngine;

namespace Thirdweb
{
    public class Bridge
    {
        [System.Serializable]
        private struct Result<T>
        {
            public T result;
        }

        [System.Serializable]
        private struct RequestMessageBody
        {
            public RequestMessageBody(string[] arguments)
            {
                this.arguments = arguments;
            }

            public string[] arguments;
        }

        private struct GenericAction
        {
            public Type t;
            public Delegate d;

            public GenericAction(Type t, Delegate d)
            {
                this.t = t;
                this.d = d;
            }
        }

        private static readonly Dictionary<string, TaskCompletionSource<string>> taskMap = new();
        private static readonly Dictionary<string, GenericAction> taskActionMap = new();

        [AOT.MonoPInvokeCallback(typeof(Action<string, string, string>))]
        private static void jsCallback(string taskId, string result, string error)
        {
            if (taskMap.ContainsKey(taskId))
            {
                if (error != null)
                {
                    taskMap[taskId].TrySetException(new Exception(error));
                }
                else
                {
                    taskMap[taskId].TrySetResult(result);
                }
                taskMap.Remove(taskId);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string, string>))]
        private static void jsAction(string taskId, string result)
        {
            if (taskActionMap.ContainsKey(taskId))
            {
                Type tempType = taskActionMap[taskId].t;
                taskActionMap[taskId].d.DynamicInvoke(tempType == typeof(string) ? result : JsonConvert.DeserializeObject(result, tempType));
            }
        }

        public static void Initialize(string chainOrRPC, ThirdwebSDK.Options options)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Initializing the thirdweb SDK is not fully supported in the editor.");
                return;
            }
#if UNITY_WEBGL
            ThirdwebInitialize(chainOrRPC, Utils.ToJson(options));
#endif
        }

        public static async Task<string> Connect(WalletConnection walletConnection)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Connecting wallets is not fully supported in the editor.");
                return Utils.AddressZero;
            }
            var task = new TaskCompletionSource<string>();
            string taskId = Guid.NewGuid().ToString();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebConnect(
                taskId, 
                walletConnection.provider.ToString()[..1].ToLower() + walletConnection.provider.ToString()[1..], 
                walletConnection.chainId.ToString(), 
                string.IsNullOrEmpty(walletConnection.password) ? Utils.GetDeviceIdentifier() : walletConnection.password, 
                walletConnection.email, 
                walletConnection.phoneNumber,
                walletConnection.personalWallet.ToString()[..1].ToLower() + walletConnection.personalWallet.ToString()[1..],
                JsonConvert.SerializeObject(walletConnection.authOptions),
                walletConnection.smartWalletAccountOverride,
                jsCallback
            );
#endif
            string result = await task.Task;
            return result;
        }

        public static async Task Disconnect()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Disconnecting wallets is not fully supported in the editor.");
                return;
            }
            var task = new TaskCompletionSource<string>();
            string taskId = Guid.NewGuid().ToString();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebDisconnect(taskId, jsCallback);
#endif
            await task.Task;
        }

        public static async Task SwitchNetwork(string chainId)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Switching networks is not fully supported in the editor.");
                return;
            }
            var task = new TaskCompletionSource<string>();
            string taskId = Guid.NewGuid().ToString();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSwitchNetwork(taskId, chainId, jsCallback);
#endif
            await task.Task;
        }

        public static async Task<T> InvokeRoute<T>(string route, string[] body)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            var msg = Utils.ToJson(new RequestMessageBody(body));
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebInvoke(taskId, route, msg, jsCallback);
#endif
            string result = await task.Task;
            ThirdwebDebug.Log($"InvokeRoute Result: {result}");
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static string InvokeListener<T>(string route, string[] body, Action<T> action)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return null;
            }

            string taskId = Guid.NewGuid().ToString();
            taskActionMap[taskId] = new GenericAction(typeof(T), action);
            var msg = Utils.ToJson(new RequestMessageBody(body));
#if UNITY_WEBGL
            ThirdwebInvokeListener(taskId, route, msg, jsAction);
#endif
            return taskId;
        }

        public static async Task FundWallet(FundWalletOptions payload)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return;
            }
            var msg = Utils.ToJson(payload);
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebFundWallet(taskId, msg, jsCallback);
#endif
            await task.Task;
        }

        public static async Task<string> ExportWallet(string password)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return null;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebExportWallet(taskId, password, jsCallback);
#endif
            string result = await task.Task;
            return result;
        }

        public static async Task<T> SmartWalletAddAdmin<T>(string admin)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletAddAdmin(taskId, admin, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static async Task<T> SmartWalletRemoveAdmin<T>(string admin)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletRemoveAdmin(taskId, admin, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static async Task<T> SmartWalletCreateSessionKey<T>(string options)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletCreateSessionKey(taskId, options, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static async Task<T> SmartWalletRevokeSessionKey<T>(string signer)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletRevokeSessionKey(taskId, signer, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static async Task<TransactionReceipt> WaitForTransactionResult(string txHash)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebWaitForTransactionResult(taskId, txHash, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<TransactionReceipt>>(result).result;
        }

        public static async Task<T> SmartWalletGetAllActiveSigners<T>()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return default;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletGetAllActiveSigners(taskId, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<T>>(result).result;
        }

        public static async Task<BigInteger> GetLatestBlockNumber()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return BigInteger.Zero;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebGetLatestBlockNumber(taskId, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<BigInteger>>(result).result;
        }

        public static async Task<BlockWithTransactionHashes> GetBlock(BigInteger blockNumber)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return null;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebGetBlock(taskId, blockNumber.ToString(), jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<BlockWithTransactionHashes>>(result).result;
        }

        public static async Task<BlockWithTransactions> GetBlockWithTransactions(BigInteger blockNumber)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return null;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebGetBlockWithTransactions(taskId, blockNumber.ToString(), jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<BlockWithTransactions>>(result).result;
        }

        public static async Task<string> GetEmail()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return "";
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebGetEmail(taskId, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<string>>(result).result;
        }

        public static async Task<string> GetSigner()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return "";
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebGetSignerAddress(taskId, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<string>>(result).result;
        }

        public static async Task<bool> SmartWalletIsDeployed()
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return false;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebSmartWalletIsDeployed(taskId, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<bool>>(result).result;
        }

        public static async Task<string> ResolveENSFromAddress(string address)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return "";
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebResolveENSFromAddress(taskId, address, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<string>>(result).result;
        }

        public static async Task<string> ResolveAddressFromENS(string ens)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return "";
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebResolveAddressFromENS(taskId, ens, jsCallback);
#endif
            string result = await task.Task;
            return JsonConvert.DeserializeObject<Result<string>>(result).result;
        }

        public static async Task CopyBuffer(string text)
        {
            if (!Utils.IsWebGLBuild())
            {
                ThirdwebDebug.LogWarning("Interacting with the thirdweb SDK is not fully supported in the editor.");
                return;
            }
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
#if UNITY_WEBGL
            ThirdwebCopyBuffer(taskId, text, jsCallback);
#endif
            await task.Task;
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string ThirdwebInvoke(string taskId, string route, string payload, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebInvokeListener(string taskId, string route, string payload, Action<string, string> action);
        [DllImport("__Internal")]
        private static extern string ThirdwebInitialize(string chainOrRPC, string options);
        [DllImport("__Internal")]
        private static extern string ThirdwebConnect(string taskId, string wallet, string chainId, string password, string email, string phoneNumber, string personalWallet, string authOptions, string smartWalletAccountOverride, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebDisconnect(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSwitchNetwork(string taskId, string chainId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebFundWallet(string taskId, string payload, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebExportWallet(string taskId, string password, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletAddAdmin(string taskId, string admin, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletRemoveAdmin(string taskId, string admin, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletCreateSessionKey(string taskId, string options, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletRevokeSessionKey(string taskId, string signer, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletGetAllActiveSigners(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebWaitForTransactionResult(string taskId, string txHash, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebGetLatestBlockNumber(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebGetBlock(string taskId, string blockNumber, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebGetBlockWithTransactions(string taskId, string blockNumber, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebGetEmail(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebGetSignerAddress(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSmartWalletIsDeployed(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebResolveENSFromAddress(string taskId, string address, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebResolveAddressFromENS(string taskId, string ens, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebCopyBuffer(string taskId, string text, Action<string, string, string> cb);
#endif
    }
}
