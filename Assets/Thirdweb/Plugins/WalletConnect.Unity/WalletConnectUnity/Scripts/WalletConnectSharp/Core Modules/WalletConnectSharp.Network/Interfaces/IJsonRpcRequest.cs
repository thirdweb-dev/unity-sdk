using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// An interface that describes a JSON RPC request with the given parameter type T
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the JSON RPC request</typeparam>
    public interface IJsonRpcRequest<T> : IRequestArguments<T>, IJsonRpcPayload { }
}