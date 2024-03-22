using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Hyperplay
{
    public class Hyperplay
    {
        public string[] Accounts { get; private set; }
        public string ChainId { get; private set; }

        public Hyperplay(string chainId)
        {
            ChainId = chainId;
            Accounts = null;
        }

        internal async Task Initialize()
        {
            Accounts = (await Request(new RpcRequestMessage(-1, "eth_accounts"))).GetResult<string[]>();
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage message)
        {
            HyperplayRequest hyperplayRequest = new() { Method = message.Method, Params = message.RawParameters };
            string jsonString = JsonConvert.SerializeObject(hyperplayRequest);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            using var request = new UnityWebRequest("localhost:9680/rpcRaw", "POST");
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                ThirdwebDebug.LogError(request.error);
                throw new UnityException("RPC request failed: " + request.error);
            }
            var hyperplayResult = JsonConvert.DeserializeObject<HyperplayResult>(request.downloadHandler.text);
            try
            {
                return new RpcResponseMessage(message.Id, JsonConvert.DeserializeObject<JToken>(hyperplayResult.Result.ToString()));
            }
            catch
            {
                return new RpcResponseMessage(message.Id, hyperplayResult.Result.ToString());
            }
        }
    }

    [System.Serializable]
    public struct HyperplayRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Params { get; set; }
    }

    [System.Serializable]
    public struct HyperplayResult
    {
        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }
    }
}
