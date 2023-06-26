using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Crypto.Models;
using WalletConnectSharp.Network;

namespace WalletConnectSharp.Crypto.Interfaces
{
    /// <summary>
    /// A module that handles both key management and encoding/decoding of data with a given topic/keypair.
    ///
    /// A module holds one IKeyChain and stores generated keys in that keychain. The storage of generated keys
    /// is handled by the IKeyChain.
    /// </summary>
    public interface ICrypto : IModule
    {
        /// <summary>
        /// The IKeyChain this crypto module will use to store/retrieve key pairs
        /// </summary>
        IKeyChain KeyChain { get; }

        /// <summary>
        /// Initialize the crypto module. This should initialize the current KeyCHain if present.
        ///
        /// This call is asynchronous, since initializing the IKeyChain requires asynchronous initialization.
        /// </summary>
        /// <returns>The crypto module initialization task</returns>
        Task Init();

        /// <summary>
        /// Check if a keypair with a given tag is stored in this crypto module. This should
        /// check the backing keychain.
        /// </summary>
        /// <param name="tag">The tag of the keychain to look for</param>
        /// <returns>True if the backing KeyChain has a keypair for the given tag</returns>
        Task<bool> HasKeys(string tag);

        /// <summary>
        /// Generate a new keypair, storing the public/private key pair as the tag in the backing KeyChain. This will
        /// save the public/private keypair in the backing KeyChain
        /// </summary>
        /// <returns>The public key of the generated keypair</returns>
        Task<string> GenerateKeyPair();

        /// <summary>
        /// Generate a shared Sym key given two public keys. One of the public keys (selfPublicKey) is the public key
        /// we have generated a private key for in the backing KeyChain. The peer's public key (peerPublicKey) is used
        /// to generate the Sym key
        /// </summary>
        /// <param name="selfPublicKey">The public key to use, this keypair must be stored in the backing KeyChain</param>
        /// <param name="peerPublicKey">The Peer's public key. This public key does not exist in the backing KeyChain</param>
        /// <param name="overrideTopic"></param>
        /// <returns>The generated Sym key</returns>
        Task<string> GenerateSharedKey(string selfPublicKey, string peerPublicKey, string overrideTopic = null);

        /// <summary>
        /// Store the Sym key in the backing KeyChain, optionally for a given topic. If no topic is given,
        /// then the KeyChain tag for the Sym key will be the hash of the key.
        /// </summary>
        /// <param name="symKey">The Sym key to store</param>
        /// <param name="overrideTopic">An optional topic to use as the KeyChain tag</param>
        /// <returns>The tag used to store the Sym key in the KeyChain</returns>
        Task<string> SetSymKey(string symKey, string overrideTopic = null);

        /// <summary>
        /// Delete a keypair from the backing KeyChain
        /// </summary>
        /// <param name="publicKey">The public key of the keypair to delete</param>
        /// <returns>An async task</returns>
        Task DeleteKeyPair(string publicKey);

        /// <summary>
        /// Delete a Sym key with the given topic/tag from the backing KeyChain.
        /// </summary>
        /// <param name="topic">The topic/tag of the Sym key to delete</param>
        /// <returns>An async task</returns>
        Task DeleteSymKey(string topic);

        /// <summary>
        /// Encrypt a message with the given topic's Sym key. 
        /// </summary>
        /// <param name="@params">The encryption parameters to use</param>
        /// <returns>The encrypted message from an async task</returns>
        Task<string> Encrypt(EncryptParams @params);

        /// <summary>
        /// Decrypt an encrypted message using the given topic's Sym key.
        /// </summary>
        /// <param name="topic">The topic of the Sym key to use to decrypt the message</param>
        /// <param name="encoded">The message to decrypt</param>
        /// <returns>The decrypted message from an async task</returns>
        Task<string> Decrypt(string topic, string encoded);

        /// <summary>
        /// Encode a JsonRpcPayload message by encrypting the contents using the given topic's Sym key. If the topic
        /// has no Sym key, then the contents are not encrypted and instead are simply converted to Json -> Hex
        /// </summary>
        /// <param name="topic">The topic of the Sym key to use to encrypt the IJsonRpcPayload</param>
        /// <param name="payload">The payload to encode and encrypt</param>
        /// <returns>The encoded and encrypted IJsonRpcPayload from an async task</returns>
        Task<string> Encode(string topic, IJsonRpcPayload payload, EncodeOptions options = null);

        /// <summary>
        /// Decode an encoded/encrypted message to a IJsonRpcPayload using the given topic's Sym key. If the topic
        /// has no Sym key, then the contents are not decrypted and instead are simply converted Hex -> Json
        /// </summary>
        /// <param name="topic">The topic of the Sym key to use</param>
        /// <param name="encoded">The encoded/encrypted message to decrypt</param>
        /// <typeparam name="T">The type of the IJsonRpcPayload to convert the encoded Json to</typeparam>
        /// <returns>The decoded, decrypted and deserialized object of type T from an async task</returns>
        Task<T> Decode<T>(string topic, string encoded, DecodeOptions options = null) where T : IJsonRpcPayload;

        /// <summary>
        /// Given an AUD, sign and return a JWT
        /// </summary>
        /// <param name="aud">The AUD to sign</param>
        /// <returns>A JWT token</returns>
        Task<string> SignJwt(string aud);

        /// <summary>
        /// Get a unique client id for this client
        /// </summary>
        /// <returns>The client id as a string</returns>
        Task<string> GetClientId();

        /// <summary>
        /// Hash a hex key string using SHA256. The input key string must be a hex
        /// string and the returned hash is represented as a hex string
        /// </summary>
        /// <param name="key">The input hex key string to hash using SHA256</param>
        /// <returns>The hash of the given input as a hex string</returns>
        string HashKey(string key);
    }
}
