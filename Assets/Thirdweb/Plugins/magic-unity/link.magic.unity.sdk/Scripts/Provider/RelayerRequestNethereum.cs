using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace link.magic.unity.sdk.Provider
{
    [JsonObject]
    internal class RelayerRequestNethereum
    {
        [JsonProperty("msgType", Required = Required.Default)]
        internal string MsgType;

        [JsonProperty("payload", Required = Required.Default)]
        internal RpcRequestMessage Payload;

        internal RelayerRequestNethereum(string msgType, RpcRequestMessage payload)
        {
            MsgType = msgType;
            Payload = payload;
        }
    }
}