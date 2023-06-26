using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// An interface that represents a generic request with the parameter type of T
    /// </summary>
    /// <typeparam name="T">The type of the parameter for this request</typeparam>
    public interface IRequestArguments<T>
    {
        /// <summary>
        /// The method for this request
        /// </summary>
        [JsonProperty("method")]
        string Method { get; }
        
        /// <summary>
        /// The parameter for this request
        /// </summary>
        [JsonProperty("params")]
        T Params { get; }
    }
}
