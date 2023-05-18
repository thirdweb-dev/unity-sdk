using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace link.magic.unity.sdk.Provider
{
    [JsonObject]
    public class RelayerResponseNethereum
    {
        [JsonProperty("msgType", Required = Required.Default)]
        internal string MsgType;

        [JsonProperty("response", Required = Required.Default)]
        internal RpcResponseMessage Response;

        [JsonConstructor]
        internal RelayerResponseNethereum(string msgType, RpcResponseMessage response)
        {
            MsgType = msgType;
            Response = response;
        }
    }
}