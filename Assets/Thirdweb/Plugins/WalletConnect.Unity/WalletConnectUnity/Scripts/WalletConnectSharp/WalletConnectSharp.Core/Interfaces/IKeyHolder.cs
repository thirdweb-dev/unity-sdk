using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// An interface that represents a type that has a "key" field. A "key" field is a TKey value that is used to identify the object.
    /// Common types for TKey include string or long.
    /// </summary>
    /// <typeparam name="TKey">The type of key this object uses.</typeparam>
    public interface IKeyHolder<TKey>
    {
        /// <summary>
        /// The key field of this data element
        /// </summary>
        [JsonIgnore]
        public TKey Key { get; }
    }
}
